namespace ZenoBank.BuildingBlocks.Shared.Contracts.Events;

public class AccountUnfrozenEvent : IntegrationEvent
{
    public Guid UserId { get; set; }
    public Guid AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public DateTime UnfrozenAtUtc { get; set; } = DateTime.UtcNow;
}
