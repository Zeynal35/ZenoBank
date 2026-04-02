using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZenoBank.BuildingBlocks.Shared.Common.Responses;
using ZenoBank.Services.Loan.Application.Abstractions.Services;
using ZenoBank.Services.Loan.Application.DTOs;

namespace ZenoBank.Services.Loan.API.Controllers;

[ApiController]
[Route("api/loans")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;
    private readonly ICurrentUserService _currentUserService;

    public LoansController(
        ILoanService loanService,
        ICurrentUserService currentUserService)
    {
        _loanService = loanService;
        _currentUserService = currentUserService;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLoanRequest request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = "User is not authenticated."
            });
        }

        var result = await _loanService.CreateAsync(_currentUserService.UserId.Value, request, cancellationToken);

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
    public async Task<IActionResult> GetMyLoans(CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = "User is not authenticated."
            });
        }

        var result = await _loanService.GetMyLoansAsync(_currentUserService.UserId.Value, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = result.Message,
            Data = result.Data
        });
    }

    [Authorize]
    [HttpGet("my/{id:guid}")]
    public async Task<IActionResult> GetMyLoanById(Guid id, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = "User is not authenticated."
            });
        }

        var result = await _loanService.GetMyLoanByIdAsync(_currentUserService.UserId.Value, id, cancellationToken);

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

    [Authorize(Roles = "SuperAdmin,Admin,Operator")]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _loanService.GetAllAsync(cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = result.Message,
            Data = result.Data
        });
    }

    [Authorize(Roles = "SuperAdmin,Admin,Operator")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _loanService.GetByIdAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new ApiResponse<object>
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
    [HttpPatch("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveLoanRequest request, CancellationToken cancellationToken)
    {
        var result = await _loanService.ApproveAsync(id, request.InterestRate, cancellationToken);

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
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectLoanRequest request, CancellationToken cancellationToken)
    {
        var result = await _loanService.RejectAsync(id, request.Reason, cancellationToken);

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
