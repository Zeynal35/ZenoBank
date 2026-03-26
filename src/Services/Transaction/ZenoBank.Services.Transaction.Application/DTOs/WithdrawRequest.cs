namespace ZenoBank.Services.Transaction.Application.DTOs;

public class WithdrawRequest
{
    public Guid FromAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AZN";
    public string Description { get; set; } = string.Empty;
}
