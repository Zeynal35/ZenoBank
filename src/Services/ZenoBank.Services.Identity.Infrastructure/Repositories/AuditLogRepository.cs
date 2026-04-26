using Microsoft.EntityFrameworkCore;
using ZenoBank.BuildingBlocks.Shared.Common.Abstractions;
using ZenoBank.BuildingBlocks.Shared.Common.DTOs;
using ZenoBank.BuildingBlocks.Shared.Common.Entities;
using ZenoBank.Services.Identity.Infrastructure.Persistence;

namespace ZenoBank.Services.Identity.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly IdentityDbContext _context;

    public AuditLogRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AuditLog>> GetPagedAsync(
        AuditLogFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (filter.UserId.HasValue)
            query = query.Where(x => x.UserId == filter.UserId.Value);

        if (!string.IsNullOrWhiteSpace(filter.EntityType))
            query = query.Where(x => x.EntityType == filter.EntityType);

        if (!string.IsNullOrWhiteSpace(filter.Action))
            query = query.Where(x => x.Action.Contains(filter.Action));

        if (!string.IsNullOrWhiteSpace(filter.Status))
            query = query.Where(x => x.Status == filter.Status);

        if (filter.FromDate.HasValue)
            query = query.Where(x => x.CreatedAtUtc >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(x => x.CreatedAtUtc <= filter.ToDate.Value.AddDays(1));

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditLog>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<List<string>> GetDistinctEntityTypesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Select(x => x.EntityType)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<string>> GetDistinctActionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Select(x => x.Action)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);
    }
}
