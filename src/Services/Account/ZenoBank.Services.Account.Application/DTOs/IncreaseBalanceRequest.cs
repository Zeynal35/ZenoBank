namespace ZenoBank.Services.Account.Application.DTOs;

public class IncreaseBalanceRequest
{
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
}
