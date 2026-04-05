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
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = "User is not authenticated."
            });
        }

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
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = result.Message,
                Errors = result.Errors
            });
        }

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = result.Message,
            Data = result.Data
        });
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMy(CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = "User is not authenticated."
            });
        }

        var result = await _kycDocumentService.GetMyDocumentsAsync(_currentUserService.UserId.Value, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = result.Message,
            Data = result.Data
        });
    }

    [Authorize(Roles = "SuperAdmin,Admin,Operator")]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _kycDocumentService.GetAllAsync(cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = result.Message,
            Data = result.Data
        });
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPatch("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ReviewKycDocumentRequest request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = "User is not authenticated."
            });
        }

        var result = await _kycDocumentService.ApproveAsync(id, _currentUserService.UserId.Value, request.ReviewerNote, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = result.Message,
                Errors = result.Errors
            });
        }

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = result.Message,
            Data = result.Data
        });
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPatch("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] ReviewKycDocumentRequest request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = "User is not authenticated."
            });
        }

        var result = await _kycDocumentService.RejectAsync(id, _currentUserService.UserId.Value, request.ReviewerNote, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = result.Message,
                Errors = result.Errors
            });
        }

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = result.Message,
            Data = result.Data
        });
    }
}
