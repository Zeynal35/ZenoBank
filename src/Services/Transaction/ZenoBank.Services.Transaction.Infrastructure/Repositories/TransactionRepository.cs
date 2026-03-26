using Microsoft.EntityFrameworkCore;
using ZenoBank.Services.Transaction.Application.Abstractions.Repositories;
using ZenoBank.Services.Transaction.Domain.Entities;
using ZenoBank.Services.Transaction.Infrastructure.Persistence;

namespace ZenoBank.Services.Transaction.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly TransactionDbContext _context;

    public TransactionRepository(TransactionDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(TransactionRecord transaction, CancellationToken cancellationToken = default)
    {
        await _context.Transactions.AddAsync(transaction, cancellationToken);
    }

    public async Task<TransactionRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<TransactionRecord?> GetByReferenceNumberAsync(string referenceNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions.FirstOrDefaultAsync(x => x.ReferenceNumber == referenceNumber, cancellationToken);
    }

    public async Task<List<TransactionRecord>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TransactionRecord>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
