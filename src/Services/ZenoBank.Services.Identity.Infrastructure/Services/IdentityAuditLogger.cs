using ZenoBank.BuildingBlocks.Shared.Common.Abstractions;
using ZenoBank.BuildingBlocks.Shared.Common.DTOs;
using ZenoBank.BuildingBlocks.Shared.Common.Entities;
using ZenoBank.Services.Identity.Infrastructure.Persistence;

namespace ZenoBank.Services.Identity.Infrastructure.Services;

public class IdentityAuditLogger : IAuditLogger
{
    private readonly IdentityDbContext _dbContext;

    public IdentityAuditLogger(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task WriteAsync(CreateAuditLogRequest request, CancellationToken cancellationToken = default)
    {
        var log = new AuditLog
        {
            UserId = request.UserId,
            Action = request.Action,
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            Description = request.Description,
            Status = request.Status,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _dbContext.AuditLogs.AddAsync(log, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
