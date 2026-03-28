using ZenoBank.BuildingBlocks.Shared.Common.Entities;
using ZenoBank.Services.Notification.Domain.Enums;

namespace ZenoBank.Services.Notification.Domain.Entities;

public class NotificationRecord : AuditableEntity
{
    public Guid UserId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public NotificationType NotificationType { get; set; }

    public bool IsRead { get; set; } = false;
    public DateTime? ReadAtUtc { get; set; }
}
