namespace ZenoBank.BuildingBlocks.Shared.Contracts.Events;

public class UserLoggedInEvent : IntegrationEvent
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime LoggedInAtUtc { get; set; } = DateTime.UtcNow;
}