namespace ZenoBank.BuildingBlocks.Shared.Contracts.Events;

public class EmailVerificationRequestedEvent : IntegrationEvent
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string VerificationToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
}
