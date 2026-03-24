using ZenoBank.Services.Identity.Application.Abstractions.Services;
using ZenoBank.Services.Identity.Domain.Entities;

namespace ZenoBank.Services.Identity.Infrastructure.Services;

public class TokenService : ITokenService
{
    public string GenerateAccessToken(User user, List<string> roles)
    {
        return "access-token-will-be-generated-here";
    }

    public string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
    }

    public DateTime GetAccessTokenExpirationUtc()
    {
        return DateTime.UtcNow.AddMinutes(15);
    }
}
