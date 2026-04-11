using Microsoft.AspNetCore.Http;
using ZenoBank.BuildingBlocks.Shared.Common.Results;
using ZenoBank.Services.Customer.Application.DTOs;

namespace ZenoBank.Services.Customer.Application.Abstractions.Services;

public interface IKycDocumentService
{
    Task<Result<KycDocumentDto>> UploadAsync(
        Guid userId,
        Guid customerProfileId,
        UploadKycDocumentRequest request,
        IFormFile file,
        CancellationToken cancellationToken = default);

    Task<Result<KycDocumentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<List<KycDocumentDto>>> GetMyDocumentsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<List<KycDocumentDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<KycDocumentDto>> ApproveAsync(
        Guid documentId,
        Guid reviewerUserId,
        string? reviewerNote,
        CancellationToken cancellationToken = default);

    Task<Result<KycDocumentDto>> RejectAsync(
        Guid documentId,
        Guid reviewerUserId,
        string? reviewerNote,
        CancellationToken cancellationToken = default);
}
