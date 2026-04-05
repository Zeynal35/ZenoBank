namespace ZenoBank.Services.Customer.Application.DTOs;

public class UploadKycDocumentRequest
{
    public int DocumentType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
}
