using Microsoft.EntityFrameworkCore;
using ZenoBank.Services.Customer.Application.Abstractions.Repositories;
using ZenoBank.Services.Customer.Domain.Entities;
using ZenoBank.Services.Customer.Infrastructure.Persistence;

namespace ZenoBank.Services.Customer.Infrastructure.Repositories;

public class CustomerProfileRepository : ICustomerProfileRepository
{
    private readonly CustomerDbContext _context;

    public CustomerProfileRepository(CustomerDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(CustomerProfile profile, CancellationToken cancellationToken = default)
    {
        await _context.CustomerProfiles.AddAsync(profile, cancellationToken);
    }

    public void Update(CustomerProfile profile)
    {
        _context.CustomerProfiles.Update(profile);
    }

    public async Task<CustomerProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.CustomerProfiles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<CustomerProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.CustomerProfiles.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<List<CustomerProfile>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CustomerProfiles
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
