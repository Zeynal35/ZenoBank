namespace ZenoBank.BuildingBlocks.Shared.Contracts.Events;

public class UserLoggedOutEvent : IntegrationEvent
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime LoggedOutAtUtc { get; set; } = DateTime.UtcNow;
}
