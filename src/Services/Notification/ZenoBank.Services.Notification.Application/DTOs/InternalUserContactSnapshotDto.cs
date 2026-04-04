namespace ZenoBank.Services.Notification.Application.DTOs;

public class InternalUserContactSnapshotDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
}