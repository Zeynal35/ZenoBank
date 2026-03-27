namespace ZenoBank.Services.Account.Application.DTOs;

public class InternalAccountDto
{
    public Guid Id { get; set; }
    public Guid CustomerProfileId { get; set; }
    public Guid UserId { get; set; }

    public string AccountNumber { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;

    public decimal Balance { get; set; }
    public string Status { get; set; } = string.Empty;

    public bool IsFrozen { get; set; }
}
