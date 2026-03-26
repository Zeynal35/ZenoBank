using ZenoBank.Services.Account.Domain.Entities;

namespace ZenoBank.Services.Account.Application.Abstractions.Repositories;

public interface IBankAccountRepository
{
    Task AddAsync(BankAccount account, CancellationToken cancellationToken = default);
    void Update(BankAccount account);

    Task<BankAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BankAccount?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default);

    Task<List<BankAccount>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<BankAccount>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<bool> AnyByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}