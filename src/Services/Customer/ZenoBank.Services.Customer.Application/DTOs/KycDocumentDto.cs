namespace ZenoBank.Services.Customer.Application.DTOs;

public class KycDocumentDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CustomerProfileId { get; set; }

    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
    public string? ReviewerNote { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}