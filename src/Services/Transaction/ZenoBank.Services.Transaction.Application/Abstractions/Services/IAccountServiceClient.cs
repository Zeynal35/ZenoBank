using ZenoBank.BuildingBlocks.Shared.Common.Results;
using ZenoBank.Services.Transaction.Application.DTOs;

namespace ZenoBank.Services.Transaction.Application.Abstractions.Services;

public interface IAccountServiceClient
{
    Task<Result<InternalAccountSnapshotDto>> GetAccountByIdAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<Result<AccountBalanceSnapshotDto>> IncreaseBalanceAsync(Guid accountId, decimal amount, CancellationToken cancellationToken = default);
    Task<Result<AccountBalanceSnapshotDto>> DecreaseBalanceAsync(Guid accountId, decimal amount, CancellationToken cancellationToken = default);
    Task<Result> TransferBalanceAsync(Guid fromAccountId, Guid toAccountId, decimal amount, CancellationToken cancellationToken = default);
}
