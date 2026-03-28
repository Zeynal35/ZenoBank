using ZenoBank.BuildingBlocks.Shared.Common.Results;
using ZenoBank.Services.Notification.Application.Abstractions.Repositories;
using ZenoBank.Services.Notification.Application.Abstractions.Services;
using ZenoBank.Services.Notification.Application.DTOs;
using ZenoBank.Services.Notification.Domain.Entities;

namespace ZenoBank.Services.Notification.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;

    public NotificationService(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<NotificationDto>>> GetMyNotificationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var notifications = await _repository.GetByUserIdAsync(userId, cancellationToken);
        return Result<List<NotificationDto>>.Success(notifications.Select(Map).ToList(), "Notifications fetched successfully.");
    }

    public async Task<Result<List<NotificationDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var notifications = await _repository.GetAllAsync(cancellationToken);
        return Result<List<NotificationDto>>.Success(notifications.Select(Map).ToList(), "Notifications fetched successfully.");
    }

    public async Task<Result> MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _repository.GetByIdAsync(notificationId, cancellationToken);
        if (notification is null)
            return Result.Failure("Notification not found.");

        if (notification.UserId != userId)
            return Result.Failure("You are not allowed to access this notification.");

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAtUtc = DateTime.UtcNow;
            notification.UpdatedAtUtc = DateTime.UtcNow;

            _repository.Update(notification);
            await _repository.SaveChangesAsync(cancellationToken);
        }

        return Result.Success("Notification marked as read.");
    }

    public async Task<Result> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var notifications = await _repository.GetByUserIdAsync(userId, cancellationToken);

        foreach (var notification in notifications.Where(x => !x.IsRead))
        {
            notification.IsRead = true;
            notification.ReadAtUtc = DateTime.UtcNow;
            notification.UpdatedAtUtc = DateTime.UtcNow;
            _repository.Update(notification);
        }

        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success("All notifications marked as read.");
    }

    private static NotificationDto Map(NotificationRecord notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Title = notification.Title,
            Message = notification.Message,
            NotificationType = notification.NotificationType.ToString(),
            IsRead = notification.IsRead,
            ReadAtUtc = notification.ReadAtUtc,
            CreatedAtUtc = notification.CreatedAtUtc
        };
    }
}
