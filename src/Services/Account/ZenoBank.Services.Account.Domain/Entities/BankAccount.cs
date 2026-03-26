using ZenoBank.BuildingBlocks.Shared.Common.Entities;
using ZenoBank.Services.Account.Domain.Enums;

namespace ZenoBank.Services.Account.Domain.Entities;

public class BankAccount : AuditableEntity
{
    public Guid CustomerProfileId { get; set; }
    public Guid UserId { get; set; }

    public string AccountNumber { get; set; } = string.Empty;

    public AccountType AccountType { get; set; }
    public string Currency { get; set; } = "AZN";

    public decimal Balance { get; set; } = 0m;

    public AccountStatus Status { get; set; } = AccountStatus.Active;

    public bool IsFrozen => Status == AccountStatus.Frozen;
}
