using ZenoBank.BuildingBlocks.Shared.Common.Results;

namespace ZenoBank.Services.Loan.Application.Abstractions.Services;

public interface IAccountServiceClient
{
    Task<Result<decimal>> IncreaseBalanceAsync(Guid accountId, decimal amount, CancellationToken cancellationToken = default);
}