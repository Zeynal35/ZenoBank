using ZenoBank.BuildingBlocks.Shared.Common.Results;
using ZenoBank.Services.Notification.Application.DTOs;

namespace ZenoBank.Services.Notification.Application.Abstractions.Services;

public interface IIdentityServiceClient
{
    Task<Result<InternalUserContactSnapshotDto>> GetUserContactAsync(Guid userId, CancellationToken cancellationToken = default);
}
