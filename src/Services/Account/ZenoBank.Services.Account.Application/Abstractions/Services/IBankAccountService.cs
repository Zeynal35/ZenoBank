using ZenoBank.BuildingBlocks.Shared.Common.Results;
using ZenoBank.Services.Account.Application.DTOs;

namespace ZenoBank.Services.Account.Application.Abstractions.Services;

public interface IBankAccountService
{
    Task<Result<BankAccountDto>> CreateAsync(Guid userId, CreateBankAccountRequest request, CancellationToken cancellationToken = default);
    Task<Result<List<BankAccountDto>>> GetMyAccountsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<BankAccountDto>> GetMyAccountByIdAsync(Guid userId, Guid accountId, CancellationToken cancellationToken = default);
    Task<Result<AccountBalanceDto>> GetMyAccountBalanceAsync(Guid userId, Guid accountId, CancellationToken cancellationToken = default);

    Task<Result<List<BankAccountDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<BankAccountDto>> GetByIdAsync(Guid accountId, CancellationToken cancellationToken = default);

    Task<Result> FreezeAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<Result> UnfreezeAsync(Guid accountId, CancellationToken cancellationToken = default);
}