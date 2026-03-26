using ZenoBank.BuildingBlocks.Shared.Common.Results;
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

    public TransactionService(
        ITransactionRepository repository,
        ITransactionReferenceGenerator referenceGenerator)
    {
        _repository = repository;
        _referenceGenerator = referenceGenerator;
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

        var transaction = new TransactionRecord
        {
            UserId = userId,
            ReferenceNumber = GenerateUniqueReference(),
            TransactionType = TransactionType.Deposit,
            Status = TransactionStatus.Completed,
            ToAccountId = request.ToAccountId,
            Amount = request.Amount,
            Currency = request.Currency.Trim().ToUpper(),
            Description = request.Description?.Trim() ?? string.Empty
        };

        await _repository.AddAsync(transaction, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

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

        var transaction = new TransactionRecord
        {
            UserId = userId,
            ReferenceNumber = GenerateUniqueReference(),
            TransactionType = TransactionType.Withdraw,
            Status = TransactionStatus.Completed,
            FromAccountId = request.FromAccountId,
            Amount = request.Amount,
            Currency = request.Currency.Trim().ToUpper(),
            Description = request.Description?.Trim() ?? string.Empty
        };

        await _repository.AddAsync(transaction, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

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

        var transaction = new TransactionRecord
        {
            UserId = userId,
            ReferenceNumber = GenerateUniqueReference(),
            TransactionType = TransactionType.Transfer,
            Status = TransactionStatus.Completed,
            FromAccountId = request.FromAccountId,
            ToAccountId = request.ToAccountId,
            Amount = request.Amount,
            Currency = request.Currency.Trim().ToUpper(),
            Description = request.Description?.Trim() ?? string.Empty
        };

        await _repository.AddAsync(transaction, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

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

    private string GenerateUniqueReference()
    {
        return _referenceGenerator.Generate();
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
