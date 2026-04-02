using ZenoBank.Services.Loan.Domain.Entities;

namespace ZenoBank.Services.Loan.Application.Abstractions.Repositories;

public interface ILoanRepository
{
    Task AddAsync(LoanApplication loan, CancellationToken cancellationToken = default);
    void Update(LoanApplication loan);

    Task<LoanApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<LoanApplication>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<LoanApplication>> GetAllAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
