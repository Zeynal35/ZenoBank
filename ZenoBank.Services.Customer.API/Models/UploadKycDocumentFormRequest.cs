using Microsoft.AspNetCore.Http;

namespace ZenoBank.Services.Customer.API.Models;

public class UploadKycDocumentFormRequest
{
    public Guid CustomerProfileId { get; set; }
    public int DocumentType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public IFormFile File { get; set; } = null!;
}
