using ZenoBank.Services.Account.Application.Abstractions.Services;

namespace ZenoBank.Services.Account.Infrastructure.Services;

public class AccountNumberGenerator : IAccountNumberGenerator
{
    public string Generate()
    {
        var random = Random.Shared.Next(100000, 999999);
        var ticksPart = DateTime.UtcNow.Ticks.ToString()[^8..];
        return $"AZ{ticksPart}{random}";
    }
}
