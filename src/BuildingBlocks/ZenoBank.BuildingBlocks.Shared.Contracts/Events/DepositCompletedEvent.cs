namespace ZenoBank.BuildingBlocks.Shared.Contracts.Events;

public class DepositCompletedEvent : IntegrationEvent
{
    public Guid TransactionId { get; set; }
    public string TransactionReference { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AZN";
    public string Description { get; set; } = string.Empty;
}
