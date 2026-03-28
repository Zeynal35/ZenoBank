using ZenoBank.Services.Notification.Domain.Entities;

namespace ZenoBank.Services.Notification.Application.Abstractions.Repositories;

public interface INotificationRepository
{
    Task AddAsync(NotificationRecord notification, CancellationToken cancellationToken = default);
    void Update(NotificationRecord notification);

    Task<NotificationRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<NotificationRecord>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<NotificationRecord>> GetAllAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
