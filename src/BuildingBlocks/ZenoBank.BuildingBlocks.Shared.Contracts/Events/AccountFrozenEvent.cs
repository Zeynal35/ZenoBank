namespace ZenoBank.BuildingBlocks.Shared.Contracts.Events;

public class AccountFrozenEvent : IntegrationEvent
{
    public Guid UserId { get; set; }
    public Guid AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public DateTime FrozenAtUtc { get; set; } = DateTime.UtcNow;
}
