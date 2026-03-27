namespace ZenoBank.Services.Account.Application.DTOs;

public class InternalTransferBalanceRequest
{
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public decimal Amount { get; set; }
}
