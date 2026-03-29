using ZenoBank.BuildingBlocks.Shared.Common.Abstractions;
using ZenoBank.BuildingBlocks.Shared.Common.DTOs;
using ZenoBank.BuildingBlocks.Shared.Common.Entities;
using ZenoBank.Services.Account.Infrastructure.Persistence;

namespace ZenoBank.Services.Account.Infrastructure.Services;

public class AccountAuditLogger : IAuditLogger
{
    private readonly AccountDbContext _dbContext;

    public AccountAuditLogger(AccountDbContext dbContext)
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
