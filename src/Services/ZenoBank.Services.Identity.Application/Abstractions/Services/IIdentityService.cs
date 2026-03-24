using ZenoBank.BuildingBlocks.Shared.Common.Results;
using ZenoBank.Services.Identity.Application.DTOs;

namespace ZenoBank.Services.Identity.Application.Abstractions.Services;

public interface IIdentityService
{
    Task<Result<UserDto>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<Result> AssignRoleAsync(AssignRoleRequest request, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
}