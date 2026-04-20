namespace ZenoBank.Services.Customer.Application.DTOs;

public class CustomerProfileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public bool IsBlacklisted { get; set; }
    public string? BlacklistReason { get; set; }

    public string RiskLevel { get; set; } = string.Empty;

    public int Age { get; set; }

    // ✅ Frontend OnboardingGuard buna baxır - olmadan həmişə onboarding-ə yönləndirirdi
    public bool ProfileCompleted { get; set; }
}