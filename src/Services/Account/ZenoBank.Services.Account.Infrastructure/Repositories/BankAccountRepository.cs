using Microsoft.EntityFrameworkCore;
using ZenoBank.Services.Account.Application.Abstractions.Repositories;
using ZenoBank.Services.Account.Domain.Entities;
using ZenoBank.Services.Account.Infrastructure.Persistence;

namespace ZenoBank.Services.Account.Infrastructure.Repositories;

public class BankAccountRepository : IBankAccountRepository
{
    private readonly AccountDbContext _context;

    public BankAccountRepository(AccountDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(BankAccount account, CancellationToken cancellationToken = default)
    {
        await _context.BankAccounts.AddAsync(account, cancellationToken);
    }

    public void Update(BankAccount account)
    {
        _context.BankAccounts.Update(account);
    }

    public async Task<BankAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.BankAccounts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<BankAccount?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default)
    {
        return await _context.BankAccounts.FirstOrDefaultAsync(x => x.AccountNumber == accountNumber, cancellationToken);
    }

    public async Task<List<BankAccount>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.BankAccounts
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<BankAccount>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.BankAccounts
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> AnyByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default)
    {
        return await _context.BankAccounts.AnyAsync(x => x.AccountNumber == accountNumber, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
