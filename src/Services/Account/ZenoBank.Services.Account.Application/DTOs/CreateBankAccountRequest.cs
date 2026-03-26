namespace ZenoBank.Services.Account.Application.DTOs;

public class CreateBankAccountRequest
{
    public Guid CustomerProfileId { get; set; }
    public int AccountType { get; set; }
    public string Currency { get; set; } = "AZN";
}
