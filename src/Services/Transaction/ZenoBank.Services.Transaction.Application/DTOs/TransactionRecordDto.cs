namespace ZenoBank.Services.Transaction.Application.DTOs;

public class TransactionRecordDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public string ReferenceNumber { get; set; } = string.Empty;

    public string TransactionType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public Guid? FromAccountId { get; set; }
    public Guid? ToAccountId { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
