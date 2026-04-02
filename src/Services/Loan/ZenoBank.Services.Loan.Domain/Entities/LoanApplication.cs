using ZenoBank.BuildingBlocks.Shared.Common.Entities;
using ZenoBank.Services.Loan.Domain.Enums;

namespace ZenoBank.Services.Loan.Domain.Entities;

public class LoanApplication : AuditableEntity
{
    public Guid UserId { get; set; }
    public Guid CustomerProfileId { get; set; }

    public decimal PrincipalAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int TermInMonths { get; set; }

    public decimal MonthlyPayment { get; set; }
    public decimal TotalRepayment { get; set; }

    public string Currency { get; set; } = "AZN";
    public string Purpose { get; set; } = string.Empty;

    public LoanStatus Status { get; set; } = LoanStatus.Pending;

    public string? RejectionReason { get; set; }

    public DateTime? ApprovedAtUtc { get; set; }
    public DateTime? RejectedAtUtc { get; set; }
}
