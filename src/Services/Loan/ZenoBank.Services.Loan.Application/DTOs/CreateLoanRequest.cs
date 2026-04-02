namespace ZenoBank.Services.Loan.Application.DTOs;

public class CreateLoanRequest
{
    public Guid CustomerProfileId { get; set; }
    public decimal PrincipalAmount { get; set; }
    public int TermInMonths { get; set; }
    public string Currency { get; set; } = "AZN";
    public string Purpose { get; set; } = string.Empty;
}
