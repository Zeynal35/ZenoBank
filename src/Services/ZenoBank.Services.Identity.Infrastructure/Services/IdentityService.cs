using ZenoBank.BuildingBlocks.Shared.Common.Abstractions;
using ZenoBank.BuildingBlocks.Shared.Common.DTOs;
using ZenoBank.BuildingBlocks.Shared.Common.Results;
using ZenoBank.Services.Identity.Application.Abstractions.Repositories;
using ZenoBank.Services.Identity.Application.Abstractions.Services;
using ZenoBank.Services.Identity.Application.DTOs;
using ZenoBank.Services.Identity.Domain.Constants;
using ZenoBank.Services.Identity.Domain.Entities;

namespace ZenoBank.Services.Identity.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IAuditLogger _auditLogger;

    public IdentityService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IAuditLogger auditLogger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _auditLogger = auditLogger;
    }

    public async Task<Result<UserDto>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.UserName))
            errors.Add("Username is required.");

        if (string.IsNullOrWhiteSpace(request.Email))
            errors.Add("Email is required.");

        if (string.IsNullOrWhiteSpace(request.Password))
            errors.Add("Password is required.");

        if (request.Password.Length < 6)
            errors.Add("Password must be at least 6 characters.");

        if (errors.Count > 0)
            return Result<UserDto>.Failure("Validation failed.", errors);

        var existingByUserName = await _userRepository.GetByUserNameAsync(request.UserName, cancellationToken);
        if (existingByUserName is not null)
            return Result<UserDto>.Failure("Username already exists.");

        var existingByEmail = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingByEmail is not null)
            return Result<UserDto>.Failure("Email already exists.");

        var customerRole = await _roleRepository.GetByNameAsync(RoleNames.Customer, cancellationToken);
        if (customerRole is null)
            return Result<UserDto>.Failure("Customer role was not found. Please seed roles first.");

        var user = new User
        {
            UserName = request.UserName.Trim(),
            Email = request.Email.Trim().ToLower(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            IsActive = true
        };

        user.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = customerRole.Id,
            User = user,
            Role = customerRole
        });

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        await _auditLogger.WriteAsync(new CreateAuditLogRequest
        {
            UserId = user.Id,
            Action = "UserRegistered",
            EntityType = "User",
            EntityId = user.Id.ToString(),
            Description = $"User {user.UserName} registered successfully.",
            Status = "Success"
        }, cancellationToken);

        var userDto = new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Roles = user.UserRoles.Select(x => x.Role.Name).ToList()
        };

        return Result<UserDto>.Success(userDto, "User registered successfully.");
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.UserNameOrEmail) || string.IsNullOrWhiteSpace(request.Password))
            return Result<AuthResponse>.Failure("Username/email and password are required.");

        var user = await _userRepository.GetByUserNameOrEmailAsync(request.UserNameOrEmail, cancellationToken);
        if (user is null)
            return Result<AuthResponse>.Failure("Invalid username/email or password.");

        if (!user.IsActive)
            return Result<AuthResponse>.Failure("User is inactive.");

        var passwordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!passwordValid)
            return Result<AuthResponse>.Failure("Invalid username/email or password.");

        var roles = user.UserRoles.Select(x => x.Role.Name).ToList();

        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshTokenValue = _tokenService.GenerateRefreshToken();
        var accessTokenExpiresAtUtc = _tokenService.GetAccessTokenExpirationUtc();
        var refreshTokenExpiresAtUtc = _tokenService.GetRefreshTokenExpirationUtc();

        var refreshToken = new RefreshToken
        {
            Token = refreshTokenValue,
            ExpiresAtUtc = refreshTokenExpiresAtUtc,
            IsRevoked = false,
            UserId = user.Id
        };

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        await _auditLogger.WriteAsync(new CreateAuditLogRequest
        {
            UserId = user.Id,
            Action = "UserLoggedIn",
            EntityType = "User",
            EntityId = user.Id.ToString(),
            Description = $"User {user.UserName} logged in successfully.",
            Status = "Success"
        }, cancellationToken);

        var response = new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            AccessTokenExpiresAtUtc = accessTokenExpiresAtUtc
        };

        return Result<AuthResponse>.Success(response, "Login successful.");
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Result<AuthResponse>.Failure("Refresh token is required.");

        var existingRefreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
        if (existingRefreshToken is null)
            return Result<AuthResponse>.Failure("Refresh token is invalid.");

        if (existingRefreshToken.IsRevoked)
            return Result<AuthResponse>.Failure("Refresh token has been revoked.");

        if (existingRefreshToken.ExpiresAtUtc <= DateTime.UtcNow)
            return Result<AuthResponse>.Failure("Refresh token has expired.");

        var user = existingRefreshToken.User;
        if (!user.IsActive)
            return Result<AuthResponse>.Failure("User is inactive.");

        existingRefreshToken.IsRevoked = true;
        _refreshTokenRepository.Update(existingRefreshToken);

        var roles = user.UserRoles.Select(x => x.Role.Name).ToList();

        var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
        var newRefreshTokenValue = _tokenService.GenerateRefreshToken();
        var accessTokenExpiresAtUtc = _tokenService.GetAccessTokenExpirationUtc();
        var refreshTokenExpiresAtUtc = _tokenService.GetRefreshTokenExpirationUtc();

        var newRefreshToken = new RefreshToken
        {
            Token = newRefreshTokenValue,
            ExpiresAtUtc = refreshTokenExpiresAtUtc,
            IsRevoked = false,
            UserId = user.Id
        };

        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        await _auditLogger.WriteAsync(new CreateAuditLogRequest
        {
            UserId = user.Id,
            Action = "TokenRefreshed",
            EntityType = "RefreshToken",
            EntityId = existingRefreshToken.Id.ToString(),
            Description = $"Refresh token used for user {user.UserName}.",
            Status = "Success"
        }, cancellationToken);

        var response = new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            AccessTokenExpiresAtUtc = accessTokenExpiresAtUtc
        };

        return Result<AuthResponse>.Success(response, "Token refreshed successfully.");
    }

    public async Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Result.Failure("Refresh token is required.");

        var existingRefreshToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken);
        if (existingRefreshToken is null)
            return Result.Failure("Refresh token is invalid.");

        if (existingRefreshToken.IsRevoked)
            return Result.Failure("Refresh token is already revoked.");

        existingRefreshToken.IsRevoked = true;
        _refreshTokenRepository.Update(existingRefreshToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        await _auditLogger.WriteAsync(new CreateAuditLogRequest
        {
            UserId = existingRefreshToken.UserId,
            Action = "UserLoggedOut",
            EntityType = "RefreshToken",
            EntityId = existingRefreshToken.Id.ToString(),
            Description = $"User logged out successfully.",
            Status = "Success"
        }, cancellationToken);

        return Result.Success("Logout successful.");
    }

    public async Task<Result> AssignRoleAsync(AssignRoleRequest request, CancellationToken cancellationToken = default)
    {
        if (request.UserId == Guid.Empty)
            return Result.Failure("UserId is required.");

        if (string.IsNullOrWhiteSpace(request.RoleName))
            return Result.Failure("Role name is required.");

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result.Failure("User not found.");

        var role = await _roleRepository.GetByNameAsync(request.RoleName, cancellationToken);
        if (role is null)
            return Result.Failure("Role not found.");

        var hasRole = user.UserRoles.Any(x => x.RoleId == role.Id);
        if (hasRole)
            return Result.Failure("User already has this role.");

        user.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            User = user,
            Role = role
        });

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        await _auditLogger.WriteAsync(new CreateAuditLogRequest
        {
            UserId = user.Id,
            Action = "RoleAssigned",
            EntityType = "UserRole",
            EntityId = user.Id.ToString(),
            Description = $"Role {role.Name} assigned to user {user.UserName}.",
            Status = "Success"
        }, cancellationToken);

        return Result.Success("Role assigned successfully.");
    }

    public async Task<Result<UserDto>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result<UserDto>.Failure("User not found.");

        var userDto = new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Roles = user.UserRoles.Select(x => x.Role.Name).ToList()
        };

        return Result<UserDto>.Success(userDto, "User fetched successfully.");
    }
}
