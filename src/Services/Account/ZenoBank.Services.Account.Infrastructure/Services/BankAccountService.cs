using ZenoBank.BuildingBlocks.Shared.Common.Abstractions;
using ZenoBank.BuildingBlocks.Shared.Common.DTOs;
using ZenoBank.BuildingBlocks.Shared.Common.Results;
using ZenoBank.BuildingBlocks.Shared.Contracts.Events;
using ZenoBank.BuildingBlocks.Shared.Messaging.Abstractions;
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
    private readonly IAuditLogger _auditLogger;
    private readonly ICustomerServiceClient _customerServiceClient;
    private readonly IEventPublisher _eventPublisher;

    public BankAccountService(
        IBankAccountRepository repository,
        IAccountNumberGenerator accountNumberGenerator,
        IAuditLogger auditLogger,
        ICustomerServiceClient customerServiceClient,
        IEventPublisher eventPublisher)
    {
        _repository = repository;
        _accountNumberGenerator = accountNumberGenerator;
        _auditLogger = auditLogger;
        _customerServiceClient = customerServiceClient;
        _eventPublisher = eventPublisher;
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

        var complianceResult = await _customerServiceClient.GetCustomerComplianceAsync(request.CustomerProfileId, cancellationToken);
        if (complianceResult.IsFailure || complianceResult.Data is null)
            return Result<BankAccountDto>.Failure(complianceResult.Message, complianceResult.Errors);

        if (complianceResult.Data.UserId != userId)
            return Result<BankAccountDto>.Failure("Customer profile does not belong to the current user.");

        if (!complianceResult.Data.IsEligibleForBanking)
        {
            if (complianceResult.Data.Age < 18)
                return Result<BankAccountDto>.Failure("Customer must be at least 18 years old to open a bank account.");

            if (!complianceResult.Data.IsKycApproved)
                return Result<BankAccountDto>.Failure($"KYC is not approved. Current KYC status: {complianceResult.Data.KycStatus}");

            if (complianceResult.Data.IsBlacklisted)
                return Result<BankAccountDto>.Failure($"Customer is blacklisted. Reason: {complianceResult.Data.BlacklistReason}");

            return Result<BankAccountDto>.Failure("Customer is not eligible for banking operations.");
        }

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

        await _auditLogger.WriteAsync(new CreateAuditLogRequest
        {
            UserId = userId,
            Action = "BankAccountCreated",
            EntityType = "BankAccount",
            EntityId = account.Id.ToString(),
            Description = $"Bank account {account.AccountNumber} created.",
            Status = "Success"
        }, cancellationToken);

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

        await _auditLogger.WriteAsync(new CreateAuditLogRequest
        {
            UserId = account.UserId,
            Action = "BankAccountFrozen",
            EntityType = "BankAccount",
            EntityId = account.Id.ToString(),
            Description = $"Bank account {account.AccountNumber} frozen.",
            Status = "Success"
        }, cancellationToken);

        await _eventPublisher.PublishAsync(new AccountFrozenEvent
        {
            UserId = account.UserId,
            AccountId = account.Id,
            AccountNumber = account.AccountNumber,
            FrozenAtUtc = DateTime.UtcNow
        }, cancellationToken);

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

        await _auditLogger.WriteAsync(new CreateAuditLogRequest
        {
            UserId = account.UserId,
            Action = "BankAccountUnfrozen",
            EntityType = "BankAccount",
            EntityId = account.Id.ToString(),
            Description = $"Bank account {account.AccountNumber} unfrozen.",
            Status = "Success"
        }, cancellationToken);

        await _eventPublisher.PublishAsync(new AccountUnfrozenEvent
        {
            UserId = account.UserId,
            AccountId = account.Id,
            AccountNumber = account.AccountNumber,
            UnfrozenAtUtc = DateTime.UtcNow
        }, cancellationToken);

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

        await _auditLogger.WriteAsync(new CreateAuditLogRequest
        {
            UserId = account.UserId,
            Action = "BalanceIncreased",
            EntityType = "BankAccount",
            EntityId = account.Id.ToString(),
            Description = $"Balance increased by {amount} {account.Currency} for account {account.AccountNumber}.",
            Status = "Success"
        }, cancellationToken);

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

        await _auditLogger.WriteAsync(new CreateAuditLogRequest
        {
            UserId = account.UserId,
            Action = "BalanceDecreased",
            EntityType = "BankAccount",
            EntityId = account.Id.ToString(),
            Description = $"Balance decreased by {amount} {account.Currency} for account {account.AccountNumber}.",
            Status = "Success"
        }, cancellationToken);

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

        await _auditLogger.WriteAsync(new CreateAuditLogRequest
        {
            UserId = fromAccount.UserId,
            Action = "BalanceTransferred",
            EntityType = "BankAccount",
            EntityId = fromAccount.Id.ToString(),
            Description = $"Transferred {amount} {fromAccount.Currency} from {fromAccount.AccountNumber} to {toAccount.AccountNumber}.",
            Status = "Success"
        }, cancellationToken);

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