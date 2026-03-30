using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZenoBank.BuildingBlocks.Shared.Common.Responses;
using ZenoBank.Services.Customer.Application.Abstractions.Services;
using ZenoBank.Services.Customer.Application.DTOs;

namespace ZenoBank.Services.Customer.API.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomerProfilesController : ControllerBase
{
    private readonly ICustomerProfileService _customerProfileService;
    private readonly ICurrentUserService _currentUserService;

    public CustomerProfilesController(
        ICustomerProfileService customerProfileService,
        ICurrentUserService currentUserService)
    {
        _customerProfileService = customerProfileService;
        _currentUserService = currentUserService;
    }

    [Authorize]
    [HttpPost("me")]
    public async Task<IActionResult> CreateMyProfile([FromBody] CreateCustomerProfileRequest request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = "User is not authenticated."
            });
        }

        var result = await _customerProfileService.CreateAsync(_currentUserService.UserId.Value, request, cancellationToken);

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
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = "User is not authenticated."
            });
        }

        var result = await _customerProfileService.GetMyProfileAsync(_currentUserService.UserId.Value, cancellationToken);

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

    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateCustomerProfileRequest request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = "User is not authenticated."
            });
        }

        var result = await _customerProfileService.UpdateMyProfileAsync(_currentUserService.UserId.Value, request, cancellationToken);

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
        var result = await _customerProfileService.GetAllAsync(cancellationToken);

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
        var result = await _customerProfileService.GetByIdAsync(id, cancellationToken);

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
    [HttpPatch("{id:guid}/blacklist")]
    public async Task<IActionResult> Blacklist(Guid id, [FromBody] BlacklistCustomerRequest request, CancellationToken cancellationToken)
    {
        var result = await _customerProfileService.BlacklistAsync(id, request.Reason, cancellationToken);

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
    [HttpPatch("{id:guid}/unblacklist")]
    public async Task<IActionResult> Unblacklist(Guid id, CancellationToken cancellationToken)
    {
        var result = await _customerProfileService.RemoveFromBlacklistAsync(id, cancellationToken);

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
    [HttpPatch("{id:guid}/risk")]
    public async Task<IActionResult> UpdateRisk(Guid id, [FromBody] UpdateCustomerRiskRequest request, CancellationToken cancellationToken)
    {
        var result = await _customerProfileService.UpdateRiskLevelAsync(id, request.RiskLevel, cancellationToken);

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
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateCustomerStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _customerProfileService.UpdateStatusAsync(id, request.Status, cancellationToken);

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