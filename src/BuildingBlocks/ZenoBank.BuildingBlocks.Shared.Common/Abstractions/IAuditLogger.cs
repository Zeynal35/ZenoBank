using ZenoBank.BuildingBlocks.Shared.Common.DTOs;

namespace ZenoBank.BuildingBlocks.Shared.Common.Abstractions;

public interface IAuditLogger
{
    Task WriteAsync(CreateAuditLogRequest request, CancellationToken cancellationToken = default);
}
