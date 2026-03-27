namespace ZenoBank.Services.Transaction.Application.DTOs;

public class AccountBalanceSnapshotDto
{
    public Guid AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Currency { get; set; } = string.Empty;
}
