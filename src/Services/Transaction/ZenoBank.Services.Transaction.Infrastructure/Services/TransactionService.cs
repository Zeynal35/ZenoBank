using ZenoBank.BuildingBlocks.Shared.Common.Abstractions;
using ZenoBank.BuildingBlocks.Shared.Common.DTOs;
using ZenoBank.BuildingBlocks.Shared.Common.Results;
using ZenoBank.BuildingBlocks.Shared.Contracts.Events;
using ZenoBank.BuildingBlocks.Shared.Messaging.Abstractions;
using ZenoBank.Services.Transaction.Application.Abstractions.Repositories;
using ZenoBank.Services.Transaction.Application.Abstractions.Services;
using ZenoBank.Services.Transaction.Application.DTOs;
using ZenoBank.Services.Transaction.Domain.Entities;
using ZenoBank.Services.Transaction.Domain.Enums;

namespace ZenoBank.Services.Transaction.Infrastructure.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _repository;
    private readonly ITransactionReferenceGenerator _referenceGenerator;
    private readonly IAccountServiceClient _accountServiceClient;
    private readonly IEventPublisher _eventPublisher;
    private readonly IAuditLogger _auditLogger;

    public TransactionService(
        ITransactionRepository repository,
        ITransactionReferenceGenerator referenceGenerator,
        IAccountServiceClient accountServiceClient,
        IEventPublisher eventPublisher,
        IAuditLogger auditLogger)
    {
        _repository = repository;
        _referenceGenerator = referenceGenerator;
        _accountServiceClient = accountServiceClient;
        _eventPublisher = eventPublisher;
        _auditLogger = auditLogger;
    }

    public async Task<Result<TransactionRecordDto>> DepositAsync(Guid userId, DepositRequest request, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        if (request.ToAccountId == Guid.Empty) errors.Add("ToAccountId is required.");
        if (request.Amount <= 0) errors.Add("Amount must be greater than zero.");
        if (string.IsNullOrWhiteSpace(request.Currency)) errors.Add("Currency is required.");
        if (errors.Count > 0) return Result<TransactionRecordDto>.Failure("Validation failed.", errors);

        var accountResult = await _accountServiceClient.GetAccountByIdAsync(request.ToAccountId, cancellationToken);
        if (accountResult.IsFailure || accountResult.Data is null)
            return Result<TransactionRecordDto>.Failure(accountResult.Message, accountResult.Errors);

        if (accountResult.Data.UserId != userId)
            return Result<TransactionRecordDto>.Failure("You are not allowed to deposit to this account.");

        var increaseResult = await _accountServiceClient.IncreaseBalanceAsync(request.ToAccountId, request.Amount, cancellationToken);

        if (increaseResult.IsFailure)
        {
            var failedTx = CreateTransaction(userId, TransactionType.Deposit, TransactionStatus.Failed, null, request.ToAccountId, request.Amount, request.Currency, request.Description);
            await _repository.AddAsync(failedTx, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            await _auditLogger.WriteAsync(new CreateAuditLogRequest { UserId = userId, Action = "DepositFailed", EntityType = "Transaction", EntityId = failedTx.Id.ToString(), Description = $"Deposit failed for account {request.ToAccountId}. Reason: {increaseResult.Message}", Status = "Failed" }, cancellationToken);
            return Result<TransactionRecordDto>.Failure(increaseResult.Message, increaseResult.Errors);
        }

        var transaction = CreateTransaction(userId, TransactionType.Deposit, TransactionStatus.Completed, null, request.ToAccountId, request.Amount, request.Currency, request.Description);
        await _repository.AddAsync(transaction, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        await _auditLogger.WriteAsync(new CreateAuditLogRequest { UserId = userId, Action = "DepositCompleted", EntityType = "Transaction", EntityId = transaction.Id.ToString(), Description = $"Deposit completed for account {request.ToAccountId}. Amount: {request.Amount} {request.Currency}.", Status = "Success" }, cancellationToken);
        await _eventPublisher.PublishAsync(new DepositCompletedEvent { TransactionId = transaction.Id, TransactionReference = transaction.ReferenceNumber, UserId = transaction.UserId, AccountId = transaction.ToAccountId!.Value, Amount = transaction.Amount, Currency = transaction.Currency, Description = transaction.Description }, cancellationToken);

        return Result<TransactionRecordDto>.Success(Map(transaction), "Deposit transaction created successfully.");
    }

    public async Task<Result<TransactionRecordDto>> WithdrawAsync(Guid userId, WithdrawRequest request, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        if (request.FromAccountId == Guid.Empty) errors.Add("FromAccountId is required.");
        if (request.Amount <= 0) errors.Add("Amount must be greater than zero.");
        if (string.IsNullOrWhiteSpace(request.Currency)) errors.Add("Currency is required.");
        if (errors.Count > 0) return Result<TransactionRecordDto>.Failure("Validation failed.", errors);

        var accountResult = await _accountServiceClient.GetAccountByIdAsync(request.FromAccountId, cancellationToken);
        if (accountResult.IsFailure || accountResult.Data is null)
            return Result<TransactionRecordDto>.Failure(accountResult.Message, accountResult.Errors);

        if (accountResult.Data.UserId != userId)
            return Result<TransactionRecordDto>.Failure("You are not allowed to withdraw from this account.");

        var decreaseResult = await _accountServiceClient.DecreaseBalanceAsync(request.FromAccountId, request.Amount, cancellationToken);

        if (decreaseResult.IsFailure)
        {
            var failedTx = CreateTransaction(userId, TransactionType.Withdraw, TransactionStatus.Failed, request.FromAccountId, null, request.Amount, request.Currency, request.Description);
            await _repository.AddAsync(failedTx, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            await _auditLogger.WriteAsync(new CreateAuditLogRequest { UserId = userId, Action = "WithdrawFailed", EntityType = "Transaction", EntityId = failedTx.Id.ToString(), Description = $"Withdraw failed for account {request.FromAccountId}. Reason: {decreaseResult.Message}", Status = "Failed" }, cancellationToken);
            return Result<TransactionRecordDto>.Failure(decreaseResult.Message, decreaseResult.Errors);
        }

        var transaction = CreateTransaction(userId, TransactionType.Withdraw, TransactionStatus.Completed, request.FromAccountId, null, request.Amount, request.Currency, request.Description);
        await _repository.AddAsync(transaction, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        await _auditLogger.WriteAsync(new CreateAuditLogRequest { UserId = userId, Action = "WithdrawCompleted", EntityType = "Transaction", EntityId = transaction.Id.ToString(), Description = $"Withdraw completed from account {request.FromAccountId}. Amount: {request.Amount} {request.Currency}.", Status = "Success" }, cancellationToken);
        await _eventPublisher.PublishAsync(new WithdrawCompletedEvent { TransactionId = transaction.Id, TransactionReference = transaction.ReferenceNumber, UserId = transaction.UserId, AccountId = transaction.FromAccountId!.Value, Amount = transaction.Amount, Currency = transaction.Currency, Description = transaction.Description }, cancellationToken);

        return Result<TransactionRecordDto>.Success(Map(transaction), "Withdraw transaction created successfully.");
    }

    public async Task<Result<TransactionRecordDto>> TransferAsync(Guid userId, TransferRequest request, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        if (request.FromAccountId == Guid.Empty) errors.Add("FromAccountId is required.");
        if (request.ToAccountId == Guid.Empty) errors.Add("ToAccountId is required.");
        if (request.FromAccountId == request.ToAccountId) errors.Add("FromAccountId and ToAccountId cannot be the same.");
        if (request.Amount <= 0) errors.Add("Amount must be greater than zero.");
        if (string.IsNullOrWhiteSpace(request.Currency)) errors.Add("Currency is required.");
        if (errors.Count > 0) return Result<TransactionRecordDto>.Failure("Validation failed.", errors);

        var fromAccountResult = await _accountServiceClient.GetAccountByIdAsync(request.FromAccountId, cancellationToken);
        if (fromAccountResult.IsFailure || fromAccountResult.Data is null)
            return Result<TransactionRecordDto>.Failure(fromAccountResult.Message, fromAccountResult.Errors);

        if (fromAccountResult.Data.UserId != userId)
            return Result<TransactionRecordDto>.Failure("You are not allowed to transfer from this account.");

        var toAccountResult = await _accountServiceClient.GetAccountByIdAsync(request.ToAccountId, cancellationToken);
        if (toAccountResult.IsFailure || toAccountResult.Data is null)
            return Result<TransactionRecordDto>.Failure(toAccountResult.Message, toAccountResult.Errors);

        var transferResult = await _accountServiceClient.TransferBalanceAsync(request.FromAccountId, request.ToAccountId, request.Amount, cancellationToken);

        if (transferResult.IsFailure)
        {
            var failedTx = CreateTransaction(userId, TransactionType.Transfer, TransactionStatus.Failed, request.FromAccountId, request.ToAccountId, request.Amount, request.Currency, request.Description);
            await _repository.AddAsync(failedTx, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            await _auditLogger.WriteAsync(new CreateAuditLogRequest { UserId = userId, Action = "TransferFailed", EntityType = "Transaction", EntityId = failedTx.Id.ToString(), Description = $"Transfer failed from {request.FromAccountId} to {request.ToAccountId}. Reason: {transferResult.Message}", Status = "Failed" }, cancellationToken);
            return Result<TransactionRecordDto>.Failure(transferResult.Message, transferResult.Errors);
        }

        var transaction = CreateTransaction(userId, TransactionType.Transfer, TransactionStatus.Completed, request.FromAccountId, request.ToAccountId, request.Amount, request.Currency, request.Description);
        await _repository.AddAsync(transaction, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        await _auditLogger.WriteAsync(new CreateAuditLogRequest { UserId = userId, Action = "TransferCompleted", EntityType = "Transaction", EntityId = transaction.Id.ToString(), Description = $"Transfer completed from {request.FromAccountId} to {request.ToAccountId}. Amount: {request.Amount} {request.Currency}.", Status = "Success" }, cancellationToken);
        await _eventPublisher.PublishAsync(new TransferCompletedEvent { TransactionId = transaction.Id, TransactionReference = transaction.ReferenceNumber, UserId = transaction.UserId, FromAccountId = transaction.FromAccountId!.Value, ToAccountId = transaction.ToAccountId!.Value, Amount = transaction.Amount, Currency = transaction.Currency, Description = transaction.Description }, cancellationToken);

        return Result<TransactionRecordDto>.Success(Map(transaction), "Transfer transaction created successfully.");
    }

    public async Task<Result<List<TransactionRecordDto>>> GetMyTransactionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var transactions = await _repository.GetByUserIdAsync(userId, cancellationToken);
        return Result<List<TransactionRecordDto>>.Success(transactions.Select(Map).ToList(), "Transactions fetched successfully.");
    }

    public async Task<Result<TransactionRecordDto>> GetMyTransactionByIdAsync(Guid userId, Guid transactionId, CancellationToken cancellationToken = default)
    {
        var transaction = await _repository.GetByIdAsync(transactionId, cancellationToken);
        if (transaction is null) return Result<TransactionRecordDto>.Failure("Transaction not found.");
        if (transaction.UserId != userId) return Result<TransactionRecordDto>.Failure("You are not allowed to access this transaction.");
        return Result<TransactionRecordDto>.Success(Map(transaction), "Transaction fetched successfully.");
    }

    public async Task<Result<List<TransactionRecordDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var transactions = await _repository.GetAllAsync(cancellationToken);
        return Result<List<TransactionRecordDto>>.Success(transactions.Select(Map).ToList(), "Transactions fetched successfully.");
    }

    public async Task<Result<TransactionRecordDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var transaction = await _repository.GetByIdAsync(id, cancellationToken);
        if (transaction is null) return Result<TransactionRecordDto>.Failure("Transaction not found.");
        return Result<TransactionRecordDto>.Success(Map(transaction), "Transaction fetched successfully.");
    }

    public async Task<Result<DashboardAnalyticsDto>> GetMyAnalyticsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var since = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-5);
        var transactions = await _repository.GetByUserIdSinceAsync(userId, since, cancellationToken);
        return Result<DashboardAnalyticsDto>.Success(BuildAnalytics(transactions, since), "Analytics fetched successfully.");
    }

    public async Task<Result<DashboardAnalyticsDto>> GetAllAnalyticsAsync(CancellationToken cancellationToken = default)
    {
        var since = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-5);
        var transactions = await _repository.GetAllSinceAsync(since, cancellationToken);
        return Result<DashboardAnalyticsDto>.Success(BuildAnalytics(transactions, since), "Analytics fetched successfully.");
    }

    private static DashboardAnalyticsDto BuildAnalytics(List<TransactionRecord> transactions, DateTime since)
    {
        var now = DateTime.UtcNow;
        var completed = transactions.Where(t => t.Status == TransactionStatus.Completed).ToList();

        var monthlySummary = new List<MonthlyTransactionSummaryDto>();
        for (var i = 0; i < 6; i++)
        {
            var monthStart = since.AddMonths(i);
            var year = monthStart.Year;
            var month = monthStart.Month;
            var monthTxs = completed.Where(t => t.CreatedAtUtc.Year == year && t.CreatedAtUtc.Month == month).ToList();

            monthlySummary.Add(new MonthlyTransactionSummaryDto
            {
                Year = year,
                Month = month,
                MonthLabel = monthStart.ToString("MMM"),
                TotalDeposited = monthTxs.Where(t => t.TransactionType == TransactionType.Deposit).Sum(t => t.Amount),
                TotalWithdrawn = monthTxs.Where(t => t.TransactionType == TransactionType.Withdraw).Sum(t => t.Amount),
                TotalTransferred = monthTxs.Where(t => t.TransactionType == TransactionType.Transfer).Sum(t => t.Amount),
            });
        }

        var totalCount = completed.Count == 0 ? 1 : completed.Count;
        var typeBreakdown = completed
            .GroupBy(t => t.TransactionType)
            .Select(g => new TransactionTypeBreakdownDto
            {
                Type = g.Key.ToString(),
                Count = g.Count(),
                TotalAmount = g.Sum(t => t.Amount),
                Percentage = Math.Round((decimal)g.Count() / totalCount * 100, 1)
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        var currentMonthTxs = completed.Where(t => t.CreatedAtUtc.Year == now.Year && t.CreatedAtUtc.Month == now.Month).ToList();

        return new DashboardAnalyticsDto
        {
            MonthlySummary = monthlySummary,
            TypeBreakdown = typeBreakdown,
            TotalTransactionCount = transactions.Count,
            TotalVolumeAllTime = completed.Sum(t => t.Amount),
            AverageTransactionAmount = completed.Count > 0 ? Math.Round(completed.Average(t => t.Amount), 2) : 0,
            CurrentMonthDeposited = currentMonthTxs.Where(t => t.TransactionType == TransactionType.Deposit).Sum(t => t.Amount),
            CurrentMonthWithdrawn = currentMonthTxs.Where(t => t.TransactionType == TransactionType.Withdraw).Sum(t => t.Amount),
        };
    }

    private TransactionRecord CreateTransaction(Guid userId, TransactionType type, TransactionStatus status, Guid? fromAccountId, Guid? toAccountId, decimal amount, string currency, string? description)
    {
        return new TransactionRecord
        {
            UserId = userId,
            ReferenceNumber = _referenceGenerator.Generate(),
            TransactionType = type,
            Status = status,
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId,
            Amount = amount,
            Currency = currency.Trim().ToUpper(),
            Description = description?.Trim() ?? string.Empty
        };
    }

    private static TransactionRecordDto Map(TransactionRecord t) => new()
    {
        Id = t.Id,
        UserId = t.UserId,
        ReferenceNumber = t.ReferenceNumber,
        TransactionType = t.TransactionType.ToString(),
        Status = t.Status.ToString(),
        FromAccountId = t.FromAccountId,
        ToAccountId = t.ToAccountId,
        Amount = t.Amount,
        Currency = t.Currency,
        Description = t.Description,
        CreatedAtUtc = t.CreatedAtUtc
    };
}