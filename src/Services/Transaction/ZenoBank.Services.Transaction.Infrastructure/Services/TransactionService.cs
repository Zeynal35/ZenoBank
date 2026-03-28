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

    public TransactionService(
        ITransactionRepository repository,
        ITransactionReferenceGenerator referenceGenerator,
        IAccountServiceClient accountServiceClient,
        IEventPublisher eventPublisher)
    {
        _repository = repository;
        _referenceGenerator = referenceGenerator;
        _accountServiceClient = accountServiceClient;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result<TransactionRecordDto>> DepositAsync(Guid userId, DepositRequest request, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        if (request.ToAccountId == Guid.Empty)
            errors.Add("ToAccountId is required.");

        if (request.Amount <= 0)
            errors.Add("Amount must be greater than zero.");

        if (string.IsNullOrWhiteSpace(request.Currency))
            errors.Add("Currency is required.");

        if (errors.Count > 0)
            return Result<TransactionRecordDto>.Failure("Validation failed.", errors);

        var increaseResult = await _accountServiceClient.IncreaseBalanceAsync(request.ToAccountId, request.Amount, cancellationToken);

        if (increaseResult.IsFailure)
        {
            var failedTransaction = CreateTransaction(
                userId,
                TransactionType.Deposit,
                TransactionStatus.Failed,
                null,
                request.ToAccountId,
                request.Amount,
                request.Currency,
                request.Description);

            await _repository.AddAsync(failedTransaction, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return Result<TransactionRecordDto>.Failure(increaseResult.Message, increaseResult.Errors);
        }

        var transaction = CreateTransaction(
            userId,
            TransactionType.Deposit,
            TransactionStatus.Completed,
            null,
            request.ToAccountId,
            request.Amount,
            request.Currency,
            request.Description);

        await _repository.AddAsync(transaction, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new DepositCompletedEvent
        {
            TransactionId = transaction.Id,
            TransactionReference = transaction.ReferenceNumber,
            UserId = transaction.UserId,
            AccountId = transaction.ToAccountId!.Value,
            Amount = transaction.Amount,
            Currency = transaction.Currency,
            Description = transaction.Description
        }, cancellationToken);

        return Result<TransactionRecordDto>.Success(Map(transaction), "Deposit transaction created successfully.");
    }

    public async Task<Result<TransactionRecordDto>> WithdrawAsync(Guid userId, WithdrawRequest request, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        if (request.FromAccountId == Guid.Empty)
            errors.Add("FromAccountId is required.");

        if (request.Amount <= 0)
            errors.Add("Amount must be greater than zero.");

        if (string.IsNullOrWhiteSpace(request.Currency))
            errors.Add("Currency is required.");

        if (errors.Count > 0)
            return Result<TransactionRecordDto>.Failure("Validation failed.", errors);

        var accountResult = await _accountServiceClient.GetAccountByIdAsync(request.FromAccountId, cancellationToken);
        if (accountResult.IsFailure || accountResult.Data is null)
            return Result<TransactionRecordDto>.Failure(accountResult.Message, accountResult.Errors);

        if (accountResult.Data.UserId != userId)
            return Result<TransactionRecordDto>.Failure("You are not allowed to withdraw from this account.");

        var decreaseResult = await _accountServiceClient.DecreaseBalanceAsync(request.FromAccountId, request.Amount, cancellationToken);

        if (decreaseResult.IsFailure)
        {
            var failedTransaction = CreateTransaction(
                userId,
                TransactionType.Withdraw,
                TransactionStatus.Failed,
                request.FromAccountId,
                null,
                request.Amount,
                request.Currency,
                request.Description);

            await _repository.AddAsync(failedTransaction, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return Result<TransactionRecordDto>.Failure(decreaseResult.Message, decreaseResult.Errors);
        }

        var transaction = CreateTransaction(
            userId,
            TransactionType.Withdraw,
            TransactionStatus.Completed,
            request.FromAccountId,
            null,
            request.Amount,
            request.Currency,
            request.Description);

        await _repository.AddAsync(transaction, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new WithdrawCompletedEvent
        {
            TransactionId = transaction.Id,
            TransactionReference = transaction.ReferenceNumber,
            UserId = transaction.UserId,
            AccountId = transaction.FromAccountId!.Value,
            Amount = transaction.Amount,
            Currency = transaction.Currency,
            Description = transaction.Description
        }, cancellationToken);

        return Result<TransactionRecordDto>.Success(Map(transaction), "Withdraw transaction created successfully.");
    }

    public async Task<Result<TransactionRecordDto>> TransferAsync(Guid userId, TransferRequest request, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        if (request.FromAccountId == Guid.Empty)
            errors.Add("FromAccountId is required.");

        if (request.ToAccountId == Guid.Empty)
            errors.Add("ToAccountId is required.");

        if (request.FromAccountId == request.ToAccountId)
            errors.Add("FromAccountId and ToAccountId cannot be the same.");

        if (request.Amount <= 0)
            errors.Add("Amount must be greater than zero.");

        if (string.IsNullOrWhiteSpace(request.Currency))
            errors.Add("Currency is required.");

        if (errors.Count > 0)
            return Result<TransactionRecordDto>.Failure("Validation failed.", errors);

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
            var failedTransaction = CreateTransaction(
                userId,
                TransactionType.Transfer,
                TransactionStatus.Failed,
                request.FromAccountId,
                request.ToAccountId,
                request.Amount,
                request.Currency,
                request.Description);

            await _repository.AddAsync(failedTransaction, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return Result<TransactionRecordDto>.Failure(transferResult.Message, transferResult.Errors);
        }

        var transaction = CreateTransaction(
            userId,
            TransactionType.Transfer,
            TransactionStatus.Completed,
            request.FromAccountId,
            request.ToAccountId,
            request.Amount,
            request.Currency,
            request.Description);

        await _repository.AddAsync(transaction, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new TransferCompletedEvent
        {
            TransactionId = transaction.Id,
            TransactionReference = transaction.ReferenceNumber,
            UserId = transaction.UserId,
            FromAccountId = transaction.FromAccountId!.Value,
            ToAccountId = transaction.ToAccountId!.Value,
            Amount = transaction.Amount,
            Currency = transaction.Currency,
            Description = transaction.Description
        }, cancellationToken);

        return Result<TransactionRecordDto>.Success(Map(transaction), "Transfer transaction created successfully.");
    }

    public async Task<Result<List<TransactionRecordDto>>> GetMyTransactionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var transactions = await _repository.GetByUserIdAsync(userId, cancellationToken);
        var data = transactions.Select(Map).ToList();

        return Result<List<TransactionRecordDto>>.Success(data, "Transactions fetched successfully.");
    }

    public async Task<Result<TransactionRecordDto>> GetMyTransactionByIdAsync(Guid userId, Guid transactionId, CancellationToken cancellationToken = default)
    {
        var transaction = await _repository.GetByIdAsync(transactionId, cancellationToken);
        if (transaction is null)
            return Result<TransactionRecordDto>.Failure("Transaction not found.");

        if (transaction.UserId != userId)
            return Result<TransactionRecordDto>.Failure("You are not allowed to access this transaction.");

        return Result<TransactionRecordDto>.Success(Map(transaction), "Transaction fetched successfully.");
    }

    public async Task<Result<List<TransactionRecordDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var transactions = await _repository.GetAllAsync(cancellationToken);
        var data = transactions.Select(Map).ToList();

        return Result<List<TransactionRecordDto>>.Success(data, "Transactions fetched successfully.");
    }

    public async Task<Result<TransactionRecordDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var transaction = await _repository.GetByIdAsync(id, cancellationToken);
        if (transaction is null)
            return Result<TransactionRecordDto>.Failure("Transaction not found.");

        return Result<TransactionRecordDto>.Success(Map(transaction), "Transaction fetched successfully.");
    }

    private TransactionRecord CreateTransaction(
        Guid userId,
        TransactionType transactionType,
        TransactionStatus status,
        Guid? fromAccountId,
        Guid? toAccountId,
        decimal amount,
        string currency,
        string? description)
    {
        return new TransactionRecord
        {
            UserId = userId,
            ReferenceNumber = _referenceGenerator.Generate(),
            TransactionType = transactionType,
            Status = status,
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId,
            Amount = amount,
            Currency = currency.Trim().ToUpper(),
            Description = description?.Trim() ?? string.Empty
        };
    }

    private static TransactionRecordDto Map(TransactionRecord transaction)
    {
        return new TransactionRecordDto
        {
            Id = transaction.Id,
            UserId = transaction.UserId,
            ReferenceNumber = transaction.ReferenceNumber,
            TransactionType = transaction.TransactionType.ToString(),
            Status = transaction.Status.ToString(),
            FromAccountId = transaction.FromAccountId,
            ToAccountId = transaction.ToAccountId,
            Amount = transaction.Amount,
            Currency = transaction.Currency,
            Description = transaction.Description,
            CreatedAtUtc = transaction.CreatedAtUtc
        };
    }
}
