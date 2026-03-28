namespace ZenoBank.BuildingBlocks.Shared.Contracts.Events;

public abstract class IntegrationEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; set; } = DateTime.UtcNow;
}
