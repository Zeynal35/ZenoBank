using Microsoft.EntityFrameworkCore;
using ZenoBank.Services.Loan.Application.Abstractions.Repositories;
using ZenoBank.Services.Loan.Domain.Entities;
using ZenoBank.Services.Loan.Infrastructure.Persistence;

namespace ZenoBank.Services.Loan.Infrastructure.Repositories;

public class LoanRepository : ILoanRepository
{
    private readonly LoanDbContext _context;

    public LoanRepository(LoanDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(LoanApplication loan, CancellationToken cancellationToken = default)
    {
        await _context.LoanApplications.AddAsync(loan, cancellationToken);
    }

    public void Update(LoanApplication loan)
    {
        _context.LoanApplications.Update(loan);
    }

    public async Task<LoanApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.LoanApplications.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<LoanApplication>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.LoanApplications
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<LoanApplication>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.LoanApplications
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
