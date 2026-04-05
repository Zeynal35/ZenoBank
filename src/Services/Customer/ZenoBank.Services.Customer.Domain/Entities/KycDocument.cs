using ZenoBank.BuildingBlocks.Shared.Common.Entities;
using ZenoBank.Services.Customer.Domain.Enums;

namespace ZenoBank.Services.Customer.Domain.Entities;

public class KycDocument : AuditableEntity
{
    public Guid UserId { get; set; }
    public Guid CustomerProfileId { get; set; }

    public KycDocumentType DocumentType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;

    public KycDocumentStatus Status { get; set; } = KycDocumentStatus.Pending;

    public string? ReviewerNote { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
}