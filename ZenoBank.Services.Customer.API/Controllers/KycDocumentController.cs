using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZenoBank.BuildingBlocks.Shared.Common.Responses;
using ZenoBank.Services.Customer.API.Models;
using ZenoBank.Services.Customer.Application.Abstractions.Services;
using ZenoBank.Services.Customer.Application.DTOs;

namespace ZenoBank.Services.Customer.API.Controllers;

[ApiController]
[Route("api/customers/kyc")]
public class KycDocumentsController : ControllerBase
{
    private readonly IKycDocumentService _kycDocumentService;
    private readonly ICurrentUserService _currentUserService;

    public KycDocumentsController(
        IKycDocumentService kycDocumentService,
        ICurrentUserService currentUserService)
    {
        _kycDocumentService = kycDocumentService;
        _currentUserService = currentUserService;
    }

    [Authorize]
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(
        [FromForm] UploadKycDocumentFormRequest form,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
            return Unauthorized(new ApiResponse<object> { Success = false, Message = "User is not authenticated." });

        var request = new UploadKycDocumentRequest
        {
            DocumentType = form.DocumentType,
            DocumentNumber = form.DocumentNumber
        };

        var result = await _kycDocumentService.UploadAsync(
            _currentUserService.UserId.Value,
            form.CustomerProfileId,
            request,
            form.File,
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(new ApiResponse<object> { Success = false, Message = result.Message, Errors = result.Errors });

        return Ok(new ApiResponse<object> { Success = true, Message = result.Message, Data = result.Data });
    }

    [Authorize]
    [HttpGet("{id:guid}/file")]
    public async Task<IActionResult> GetFile(Guid id, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
            return Unauthorized(new ApiResponse<object> { Success = false, Message = "User is not authenticated." });

        var result = await _kycDocumentService.GetByIdAsync(id, cancellationToken);

        if (result.IsFailure || result.Data is null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "Document not found." });

        var isAdmin = _currentUserService.Roles?.Contains("Admin") == true ||
                      _currentUserService.Roles?.Contains("SuperAdmin") == true;

        if (result.Data.UserId != _currentUserService.UserId && !isAdmin)
            return Forbid();

        if (!System.IO.File.Exists(result.Data.FilePath))
            return NotFound(new ApiResponse<object> { Success = false, Message = "File not found on server." });

        var bytes = await System.IO.File.ReadAllBytesAsync(result.Data.FilePath, cancellationToken);
        var contentType = GetContentType(result.Data.FilePath);

        return File(bytes, contentType, Path.GetFileName(result.Data.FilePath));
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMy(CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
            return Unauthorized(new ApiResponse<object> { Success = false, Message = "User is not authenticated." });

        var result = await _kycDocumentService.GetMyDocumentsAsync(_currentUserService.UserId.Value, cancellationToken);

        return Ok(new ApiResponse<object> { Success = true, Message = result.Message, Data = result.Data });
    }

    [Authorize(Roles = "SuperAdmin,Admin,Operator")]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _kycDocumentService.GetAllAsync(cancellationToken);
        return Ok(new ApiResponse<object> { Success = true, Message = result.Message, Data = result.Data });
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPatch("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ReviewKycDocumentRequest request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
            return Unauthorized(new ApiResponse<object> { Success = false, Message = "User is not authenticated." });

        var result = await _kycDocumentService.ApproveAsync(id, _currentUserService.UserId.Value, request.ReviewerNote, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new ApiResponse<object> { Success = false, Message = result.Message, Errors = result.Errors });

        return Ok(new ApiResponse<object> { Success = true, Message = result.Message, Data = result.Data });
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPatch("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] ReviewKycDocumentRequest request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
            return Unauthorized(new ApiResponse<object> { Success = false, Message = "User is not authenticated." });

        var result = await _kycDocumentService.RejectAsync(id, _currentUserService.UserId.Value, request.ReviewerNote, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new ApiResponse<object> { Success = false, Message = result.Message, Errors = result.Errors });

        return Ok(new ApiResponse<object> { Success = true, Message = result.Message, Data = result.Data });
    }

    private static string GetContentType(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }
}
