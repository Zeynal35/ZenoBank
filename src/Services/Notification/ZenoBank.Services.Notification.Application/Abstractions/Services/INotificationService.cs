using ZenoBank.BuildingBlocks.Shared.Common.Results;
using ZenoBank.Services.Notification.Application.DTOs;

namespace ZenoBank.Services.Notification.Application.Abstractions.Services;

public interface INotificationService
{
    Task<Result<List<NotificationDto>>> GetMyNotificationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<List<NotificationDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result> MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default);
    Task<Result> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
}
