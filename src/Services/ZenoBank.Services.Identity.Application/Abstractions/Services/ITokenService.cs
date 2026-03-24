using ZenoBank.Services.Identity.Domain.Entities;

namespace ZenoBank.Services.Identity.Application.Abstractions.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user, List<string> roles);
    string GenerateRefreshToken();
    DateTime GetAccessTokenExpirationUtc();
}