namespace ZenoBank.Services.Account.Application.DTOs;

public class AccountBalanceDto
{
    public Guid AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Currency { get; set; } = string.Empty;
}
