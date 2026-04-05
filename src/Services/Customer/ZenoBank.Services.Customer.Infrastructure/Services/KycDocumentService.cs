using Microsoft.AspNetCore.Http;
using ZenoBank.BuildingBlocks.Shared.Common.Abstractions;
using ZenoBank.BuildingBlocks.Shared.Common.DTOs;
using ZenoBank.BuildingBlocks.Shared.Common.Results;
using ZenoBank.Services.Customer.Application.Abstractions.Repositories;
using ZenoBank.Services.Customer.Application.Abstractions.Services;
using ZenoBank.Services.Customer.Application.DTOs;
using ZenoBank.Services.Customer.Domain.Entities;
using ZenoBank.Services.Customer.Domain.Enums;

namespace ZenoBank.Services.Customer.Infrastructure.Services;

public class KycDocumentService : IKycDocumentService
{
    private readonly ICustomerProfileRepository _customerProfileRepository;
    private readonly IKycDocumentRepository _kycDocumentRepository;
    private readonly IKycFileStorage _kycFileStorage;
    private readonly IAuditLogger _auditLogger;

    public KycDocumentService(
        ICustomerProfileRepository customerProfileRepository,
        IKycDocumentRepository kycDocumentRepository,
        IKycFileStorage kycFileStorage,
        IAuditLogger auditLogger)
    {
        _customerProfileRepository = customerProfileRepository;
        _kycDocumentRepository = kycDocumentRepository;
        _kycFileStorage = kycFileStorage;
        _auditLogger = auditLogger;
    }

    public async Task<Result<KycDocumentDto>> UploadAsync(
        Guid userId,
        Guid customerProfileId,
        UploadKycDocumentRequest request,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (customerProfileId == Guid.Empty)
            return Result<KycDocumentDto>.Failure("CustomerProfileId is required.");

        if (!Enum.IsDefined(typeof(KycDocumentType), request.DocumentType))
            return Result<KycDocumentDto>.Failure("Invalid document type.");

        if (string.IsNullOrWhiteSpace(request.DocumentNumber))
            return Result<KycDocumentDto>.Failure("Document number is required.");

        if (file is null || file.Length == 0)
            return Result<KycDocumentDto>.Failure("KYC file is required.");

        var profile = await _customerProfileRepository.GetByIdAsync(customerProfileId, cancellationToken);
        if (profile is null)
            return Result<KycDocumentDto>.Failure("Customer profile not found.");

        if (profile.UserId != userId)
            return Result<KycDocumentDto>.Failure("Customer profile does not belong to the current user.");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            return Result<KycDocumentDto>.Failure("Only .jpg, .jpeg, .png and .pdf files are allowed.");

        var (storedFileName, filePath) = await _kycFileStorage.SaveAsync(file, cancellationToken);

        var entity = new KycDocument
        {
            UserId = userId,
            CustomerProfileId = customerProfileId,
            DocumentType = (KycDocumentType)request.DocumentType,
            DocumentNumber = request.DocumentNumber.Trim(),
            OriginalFileName = file.FileName,
            StoredFileName = storedFileName,
            FilePath = filePath,
            Status = KycDocumentStatus.Pending
        };

        await _kycDocumentRepository.AddAsync(entity, cancellationToken);
        await _kycDocumentRepository.SaveChangesAsync(cancellationToken);

        await _auditLogger.WriteAsync(new CreateAuditLogRequest
        {
            UserId = userId,
            Action = "KycDocumentUploaded",
            EntityType = "KycDocument",
            EntityId = entity.Id.ToString(),
            Description = $"KYC document uploaded. Type: {entity.DocumentType}, File: {entity.OriginalFileName}",
            Status = "Success"
        }, cancellationToken);

        return Result<KycDocumentDto>.Success(Map(entity), "KYC document uploaded successfully.");
    }

    public async Task<Result<List<KycDocumentDto>>> GetMyDocumentsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var docs = await _kycDocumentRepository.GetByUserIdAsync(userId, cancellationToken);
        return Result<List<KycDocumentDto>>.Success(docs.Select(Map).ToList(), "KYC documents fetched successfully.");
    }

    public async Task<Result<List<KycDocumentDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var docs = await _kycDocumentRepository.GetAllAsync(cancellationToken);
        return Result<List<KycDocumentDto>>.Success(docs.Select(Map).ToList(), "KYC documents fetched successfully.");
    }

    public async Task<Result<KycDocumentDto>> ApproveAsync(
        Guid documentId,
        Guid reviewerUserId,
        string? reviewerNote,
        CancellationToken cancellationToken = default)
    {
        var doc = await _kycDocumentRepository.GetByIdAsync(documentId, cancellationToken);
        if (doc is null)
            return Result<KycDocumentDto>.Failure("KYC document not found.");

        if (doc.Status == KycDocumentStatus.Approved)
            return Result<KycDocumentDto>.Failure("KYC document is already approved.");

        doc.Status = KycDocumentStatus.Approved;
        doc.ReviewerNote = reviewerNote?.Trim();
        doc.ReviewedByUserId = reviewerUserId;
        doc.ReviewedAtUtc = DateTime.UtcNow;
        doc.UpdatedAtUtc = DateTime.UtcNow;

        _kycDocumentRepository.Update(doc);
        await _kycDocumentRepository.SaveChangesAsync(cancellationToken);

        await _auditLogger.WriteAsync(new CreateAuditLogRequest
        {
            UserId = doc.UserId,
            Action = "KycDocumentApproved",
            EntityType = "KycDocument",
            EntityId = doc.Id.ToString(),
            Description = $"KYC document approved. Type: {doc.DocumentType}",
            Status = "Success"
        }, cancellationToken);

        return Result<KycDocumentDto>.Success(Map(doc), "KYC document approved successfully.");
    }

    public async Task<Result<KycDocumentDto>> RejectAsync(
        Guid documentId,
        Guid reviewerUserId,
        string? reviewerNote,
        CancellationToken cancellationToken = default)
    {
        var doc = await _kycDocumentRepository.GetByIdAsync(documentId, cancellationToken);
        if (doc is null)
            return Result<KycDocumentDto>.Failure("KYC document not found.");

        if (string.IsNullOrWhiteSpace(reviewerNote))
            return Result<KycDocumentDto>.Failure("Reviewer note is required when rejecting a KYC document.");

        doc.Status = KycDocumentStatus.Rejected;
        doc.ReviewerNote = reviewerNote.Trim();
        doc.ReviewedByUserId = reviewerUserId;
        doc.ReviewedAtUtc = DateTime.UtcNow;
        doc.UpdatedAtUtc = DateTime.UtcNow;

        _kycDocumentRepository.Update(doc);
        await _kycDocumentRepository.SaveChangesAsync(cancellationToken);

        await _auditLogger.WriteAsync(new CreateAuditLogRequest
        {
            UserId = doc.UserId,
            Action = "KycDocumentRejected",
            EntityType = "KycDocument",
            EntityId = doc.Id.ToString(),
            Description = $"KYC document rejected. Note: {doc.ReviewerNote}",
            Status = "Success"
        }, cancellationToken);

        return Result<KycDocumentDto>.Success(Map(doc), "KYC document rejected successfully.");
    }

    private static KycDocumentDto Map(KycDocument entity)
    {
        return new KycDocumentDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            CustomerProfileId = entity.CustomerProfileId,
            DocumentType = entity.DocumentType.ToString(),
            DocumentNumber = entity.DocumentNumber,
            OriginalFileName = entity.OriginalFileName,
            FilePath = entity.FilePath,
            Status = entity.Status.ToString(),
            ReviewerNote = entity.ReviewerNote,
            ReviewedByUserId = entity.ReviewedByUserId,
            ReviewedAtUtc = entity.ReviewedAtUtc,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }
}