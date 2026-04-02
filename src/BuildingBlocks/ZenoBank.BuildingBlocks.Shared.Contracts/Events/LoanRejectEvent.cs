namespace ZenoBank.BuildingBlocks.Shared.Contracts.Events;

public class LoanRejectedEvent : IntegrationEvent
{
    public Guid UserId { get; set; }
    public Guid LoanId { get; set; }
    public decimal PrincipalAmount { get; set; }
    public int TermInMonths { get; set; }
    public string Currency { get; set; } = "AZN";
    public string Reason { get; set; } = string.Empty;
    public DateTime RejectedAtUtc { get; set; } = DateTime.UtcNow;
}
