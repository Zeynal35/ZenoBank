namespace ZenoBank.Services.Notification.Application.Abstractions.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserName { get; }
    List<string> Roles { get; }
}
