namespace ZenoBank.Services.Account.Application.DTOs;

public class DecreaseBalanceRequest
{
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
}
