namespace ZenoBank.Services.Transaction.Application.DTOs;

public class DepositRequest
{
    public Guid ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AZN";
    public string Description { get; set; } = string.Empty;
}
