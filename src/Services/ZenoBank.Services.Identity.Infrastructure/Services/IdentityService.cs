using ZenoBank.BuildingBlocks.Shared.Common.Results;
using ZenoBank.Services.Identity.Application.Abstractions.Repositories;
using ZenoBank.Services.Identity.Application.Abstractions.Services;
using ZenoBank.Services.Identity.Application.DTOs;

namespace ZenoBank.Services.Identity.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public IdentityService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public Task<Result<UserDto>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<UserDto>.Failure("Register logic will be implemented in the next step."));
    }

    public Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<AuthResponse>.Failure("Login logic will be implemented in the next step."));
    }

    public Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<AuthResponse>.Failure("Refresh token logic will be implemented in the next step."));
    }

    public Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result.Failure("Logout logic will be implemented in the next step."));
    }

    public Task<Result> AssignRoleAsync(AssignRoleRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result.Failure("Assign role logic will be implemented in the next step."));
    }

    public Task<Result<UserDto>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<UserDto>.Failure("Get current user logic will be implemented in the next step."));
    }
}
