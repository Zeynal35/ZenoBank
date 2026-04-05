using Microsoft.EntityFrameworkCore;
using ZenoBank.Services.Customer.Application.Abstractions.Repositories;
using ZenoBank.Services.Customer.Domain.Entities;
using ZenoBank.Services.Customer.Infrastructure.Persistence;

namespace ZenoBank.Services.Customer.Infrastructure.Repositories;

public class KycDocumentRepository : IKycDocumentRepository
{
    private readonly CustomerDbContext _context;

    public KycDocumentRepository(CustomerDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(KycDocument entity, CancellationToken cancellationToken = default)
    {
        await _context.KycDocuments.AddAsync(entity, cancellationToken);
    }

    public void Update(KycDocument entity)
    {
        _context.KycDocuments.Update(entity);
    }

    public async Task<KycDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.KycDocuments.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<KycDocument>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.KycDocuments
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<KycDocument>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.KycDocuments
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<KycDocument?> GetLatestByCustomerProfileIdAsync(Guid customerProfileId, CancellationToken cancellationToken = default)
    {
        return await _context.KycDocuments
            .Where(x => x.CustomerProfileId == customerProfileId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
