using ZenoBank.Services.Transaction.Domain.Entities;

namespace ZenoBank.Services.Transaction.Application.Abstractions.Repositories;

public interface ITransactionRepository
{
    Task AddAsync(TransactionRecord transaction, CancellationToken cancellationToken = default);
    Task<TransactionRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TransactionRecord?> GetByReferenceNumberAsync(string referenceNumber, CancellationToken cancellationToken = default);
    Task<List<TransactionRecord>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<TransactionRecord>> GetAllAsync(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
