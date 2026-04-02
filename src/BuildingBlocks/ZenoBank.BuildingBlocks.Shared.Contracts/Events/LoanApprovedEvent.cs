namespace ZenoBank.BuildingBlocks.Shared.Contracts.Events;

public class LoanApprovedEvent : IntegrationEvent
{
    public Guid UserId { get; set; }
    public Guid LoanId { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal InterestRate { get; set; }
    public decimal MonthlyPayment { get; set; }
    public decimal TotalRepayment { get; set; }
    public int TermInMonths { get; set; }
    public string Currency { get; set; } = "AZN";
    public DateTime ApprovedAtUtc { get; set; } = DateTime.UtcNow;
}
