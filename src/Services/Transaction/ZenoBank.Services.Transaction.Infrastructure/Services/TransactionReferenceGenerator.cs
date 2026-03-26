using ZenoBank.Services.Transaction.Application.Abstractions.Services;

namespace ZenoBank.Services.Transaction.Infrastructure.Services;

public class TransactionReferenceGenerator : ITransactionReferenceGenerator
{
    public string Generate()
    {
        var random = Random.Shared.Next(100000, 999999);
        var ticksPart = DateTime.UtcNow.Ticks.ToString()[^8..];
        return $"TXN{ticksPart}{random}";
    }
}
