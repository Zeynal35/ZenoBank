using ZenoBank.BuildingBlocks.Shared.Common.Entities;
using ZenoBank.Services.Transaction.Domain.Enums;

namespace ZenoBank.Services.Transaction.Domain.Entities;

public class TransactionRecord : AuditableEntity
{
    public Guid UserId { get; set; }

    public string ReferenceNumber { get; set; } = string.Empty;

    public TransactionType TransactionType { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

    public Guid? FromAccountId { get; set; }
    public Guid? ToAccountId { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AZN";

    public string Description { get; set; } = string.Empty;
}
