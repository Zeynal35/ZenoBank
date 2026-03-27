using ZenoBank.BuildingBlocks.Shared.Common.Results;
using ZenoBank.Services.Account.Application.Abstractions.Repositories;
using ZenoBank.Services.Account.Application.Abstractions.Services;
using ZenoBank.Services.Account.Application.DTOs;
using ZenoBank.Services.Account.Domain.Entities;
using ZenoBank.Services.Account.Domain.Enums;

namespace ZenoBank.Services.Account.Infrastructure.Services;

public class BankAccountService : IBankAccountService
{
    private readonly IBankAccountRepository _repository;
    private readonly IAccountNumberGenerator _accountNumberGenerator;

    public BankAccountService(
        IBankAccountRepository repository,
        IAccountNumberGenerator accountNumberGenerator)
    {
        _repository = repository;
        _accountNumberGenerator = accountNumberGenerator;
    }

    public async Task<Result<BankAccountDto>> CreateAsync(Guid userId, CreateBankAccountRequest request, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        if (request.CustomerProfileId == Guid.Empty)
            errors.Add("CustomerProfileId is required.");

        if (!Enum.IsDefined(typeof(AccountType), request.AccountType))
            errors.Add("Invalid account type.");

        if (string.IsNullOrWhiteSpace(request.Currency))
            errors.Add("Currency is required.");

        if (errors.Count > 0)
            return Result<BankAccountDto>.Failure("Validation failed.", errors);

        string accountNumber;
        do
        {
            accountNumber = _accountNumberGenerator.Generate();
        }
        while (await _repository.AnyByAccountNumberAsync(accountNumber, cancellationToken));

        var account = new BankAccount
        {
            UserId = userId,
            CustomerProfileId = request.CustomerProfileId,
            AccountNumber = accountNumber,
            AccountType = (AccountType)request.AccountType,
            Currency = request.Currency.Trim().ToUpper(),
            Balance = 0m,
            Status = AccountStatus.Active
        };

        await _repository.AddAsync(account, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result<BankAccountDto>.Success(Map(account), "Bank account created successfully.");
    }

    public async Task<Result<List<BankAccountDto>>> GetMyAccountsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var accounts = await _repository.GetByUserIdAsync(userId, cancellationToken);
        var data = accounts.Select(Map).ToList();

        return Result<List<BankAccountDto>>.Success(data, "Accounts fetched successfully.");
    }

    public async Task<Result<BankAccountDto>> GetMyAccountByIdAsync(Guid userId, Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await _repository.GetByIdAsync(accountId, cancellationToken);
        if (account is null)
            return Result<BankAccountDto>.Failure("Account not found.");

        if (account.UserId != userId)
            return Result<BankAccountDto>.Failure("You are not allowed to access this account.");

        return Result<BankAccountDto>.Success(Map(account), "Account fetched successfully.");
    }

    public async Task<Result<AccountBalanceDto>> GetMyAccountBalanceAsync(Guid userId, Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await _repository.GetByIdAsync(accountId, cancellationToken);
        if (account is null)
            return Result<AccountBalanceDto>.Failure("Account not found.");

        if (account.UserId != userId)
            return Result<AccountBalanceDto>.Failure("You are not allowed to access this account.");

        return Result<AccountBalanceDto>.Success(MapBalance(account), "Account balance fetched successfully.");
    }

    public async Task<Result<List<BankAccountDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var accounts = await _repository.GetAllAsync(cancellationToken);
        var data = accounts.Select(Map).ToList();

        return Result<List<BankAccountDto>>.Success(data, "Accounts fetched successfully.");
    }

    public async Task<Result<BankAccountDto>> GetByIdAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await _repository.GetByIdAsync(accountId, cancellationToken);
        if (account is null)
            return Result<BankAccountDto>.Failure("Account not found.");

        return Result<BankAccountDto>.Success(Map(account), "Account fetched successfully.");
    }

    public async Task<Result> FreezeAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await _repository.GetByIdAsync(accountId, cancellationToken);
        if (account is null)
            return Result.Failure("Account not found.");

        if (account.Status == AccountStatus.Frozen)
            return Result.Failure("Account is already frozen.");

        if (account.Status == AccountStatus.Closed)
            return Result.Failure("Closed account cannot be frozen.");

        account.Status = AccountStatus.Frozen;
        account.UpdatedAtUtc = DateTime.UtcNow;

        _repository.Update(account);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success("Account frozen successfully.");
    }

    public async Task<Result> UnfreezeAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await _repository.GetByIdAsync(accountId, cancellationToken);
        if (account is null)
            return Result.Failure("Account not found.");

        if (account.Status != AccountStatus.Frozen)
            return Result.Failure("Account is not frozen.");

        account.Status = AccountStatus.Active;
        account.UpdatedAtUtc = DateTime.UtcNow;

        _repository.Update(account);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success("Account unfrozen successfully.");
    }

    public async Task<Result<InternalAccountDto>> GetInternalByIdAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await _repository.GetByIdAsync(accountId, cancellationToken);
        if (account is null)
            return Result<InternalAccountDto>.Failure("Account not found.");

        return Result<InternalAccountDto>.Success(MapInternal(account), "Internal account fetched successfully.");
    }

    public async Task<Result<AccountBalanceDto>> IncreaseBalanceAsync(Guid accountId, decimal amount, CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
            return Result<AccountBalanceDto>.Failure("Amount must be greater than zero.");

        var account = await _repository.GetByIdAsync(accountId, cancellationToken);
        if (account is null)
            return Result<AccountBalanceDto>.Failure("Account not found.");

        if (account.Status == AccountStatus.Closed)
            return Result<AccountBalanceDto>.Failure("Closed account cannot be updated.");

        if (account.Status == AccountStatus.Frozen)
            return Result<AccountBalanceDto>.Failure("Frozen account cannot be updated.");

        account.Balance += amount;
        account.UpdatedAtUtc = DateTime.UtcNow;

        _repository.Update(account);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result<AccountBalanceDto>.Success(MapBalance(account), "Balance increased successfully.");
    }

    public async Task<Result<AccountBalanceDto>> DecreaseBalanceAsync(Guid accountId, decimal amount, CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
            return Result<AccountBalanceDto>.Failure("Amount must be greater than zero.");

        var account = await _repository.GetByIdAsync(accountId, cancellationToken);
        if (account is null)
            return Result<AccountBalanceDto>.Failure("Account not found.");

        if (account.Status == AccountStatus.Closed)
            return Result<AccountBalanceDto>.Failure("Closed account cannot be updated.");

        if (account.Status == AccountStatus.Frozen)
            return Result<AccountBalanceDto>.Failure("Frozen account cannot be updated.");

        if (account.Balance < amount)
            return Result<AccountBalanceDto>.Failure("Insufficient balance.");

        account.Balance -= amount;
        account.UpdatedAtUtc = DateTime.UtcNow;

        _repository.Update(account);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result<AccountBalanceDto>.Success(MapBalance(account), "Balance decreased successfully.");
    }

    public async Task<Result> TransferBalanceAsync(Guid fromAccountId, Guid toAccountId, decimal amount, CancellationToken cancellationToken = default)
    {
        if (fromAccountId == Guid.Empty || toAccountId == Guid.Empty)
            return Result.Failure("Account ids are required.");

        if (fromAccountId == toAccountId)
            return Result.Failure("From and to accounts cannot be the same.");

        if (amount <= 0)
            return Result.Failure("Amount must be greater than zero.");

        var fromAccount = await _repository.GetByIdAsync(fromAccountId, cancellationToken);
        if (fromAccount is null)
            return Result.Failure("Sender account not found.");

        var toAccount = await _repository.GetByIdAsync(toAccountId, cancellationToken);
        if (toAccount is null)
            return Result.Failure("Receiver account not found.");

        if (fromAccount.Status == AccountStatus.Closed || toAccount.Status == AccountStatus.Closed)
            return Result.Failure("Closed account cannot participate in transfer.");

        if (fromAccount.Status == AccountStatus.Frozen || toAccount.Status == AccountStatus.Frozen)
            return Result.Failure("Frozen account cannot participate in transfer.");

        if (fromAccount.Balance < amount)
            return Result.Failure("Insufficient balance.");

        fromAccount.Balance -= amount;
        toAccount.Balance += amount;

        fromAccount.UpdatedAtUtc = DateTime.UtcNow;
        toAccount.UpdatedAtUtc = DateTime.UtcNow;

        _repository.Update(fromAccount);
        _repository.Update(toAccount);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success("Transfer applied successfully.");
    }

    private static BankAccountDto Map(BankAccount account)
    {
        return new BankAccountDto
        {
            Id = account.Id,
            CustomerProfileId = account.CustomerProfileId,
            UserId = account.UserId,
            AccountNumber = account.AccountNumber,
            AccountType = account.AccountType.ToString(),
            Currency = account.Currency,
            Balance = account.Balance,
            Status = account.Status.ToString()
        };
    }

    private static InternalAccountDto MapInternal(BankAccount account)
    {
        return new InternalAccountDto
        {
            Id = account.Id,
            CustomerProfileId = account.CustomerProfileId,
            UserId = account.UserId,
            AccountNumber = account.AccountNumber,
            AccountType = account.AccountType.ToString(),
            Currency = account.Currency,
            Balance = account.Balance,
            Status = account.Status.ToString(),
            IsFrozen = account.IsFrozen
        };
    }

    private static AccountBalanceDto MapBalance(BankAccount account)
    {
        return new AccountBalanceDto
        {
            AccountId = account.Id,
            AccountNumber = account.AccountNumber,
            Balance = account.Balance,
            Currency = account.Currency
        };
    }
}
