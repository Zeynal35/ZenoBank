using ZenoBank.BuildingBlocks.Shared.Common.Entities;
using ZenoBank.Services.Customer.Domain.Enums;

namespace ZenoBank.Services.Customer.Domain.Entities;

public class CustomerProfile : AuditableEntity
{
    public Guid UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public CustomerStatus Status { get; set; } = CustomerStatus.Active;

    public bool IsBlacklisted { get; set; } = false;
    public string? BlacklistReason { get; set; }

    public RiskLevel RiskLevel { get; set; } = RiskLevel.Low;
}