using ZenoBank.BuildingBlocks.Shared.Common.DTOs;
using ZenoBank.BuildingBlocks.Shared.Common.Entities;

namespace ZenoBank.BuildingBlocks.Shared.Common.Abstractions;

public interface IAuditLogRepository
{
    Task<PagedResult<AuditLog>> GetPagedAsync(AuditLogFilterRequest filter, CancellationToken cancellationToken = default);
    Task<List<string>> GetDistinctEntityTypesAsync(CancellationToken cancellationToken = default);
    Task<List<string>> GetDistinctActionsAsync(CancellationToken cancellationToken = default);
}
