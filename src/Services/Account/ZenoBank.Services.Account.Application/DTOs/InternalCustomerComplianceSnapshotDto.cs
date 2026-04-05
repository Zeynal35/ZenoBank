namespace ZenoBank.Services.Account.Application.DTOs;

public class InternalCustomerComplianceSnapshotDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public DateTime DateOfBirth { get; set; }
    public int Age { get; set; }

    public string Status { get; set; } = string.Empty;

    public bool IsBlacklisted { get; set; }
    public string? BlacklistReason { get; set; }

    public string RiskLevel { get; set; } = string.Empty;

    public bool IsKycApproved { get; set; }
    public string KycStatus { get; set; } = string.Empty;

    public bool IsEligibleForBanking { get; set; }
}