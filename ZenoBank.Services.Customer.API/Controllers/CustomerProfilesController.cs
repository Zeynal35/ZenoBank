using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ZenoBank.BuildingBlocks.Shared.Common.Responses;
using ZenoBank.Services.Customer.Application.Abstractions.Services;
using ZenoBank.Services.Customer.Application.DTOs;

namespace ZenoBank.Services.Customer.API.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomerProfilesController : ControllerBase
{
    private readonly ICustomerProfileService _customerProfileService;

    public CustomerProfilesController(ICustomerProfileService customerProfileService)
    {
        _customerProfileService = customerProfileService;
    }

    // 🔥 USER ID TOKEN-DAN OXUNUR
    private Guid GetUserId()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            throw new Exception("UserId claim is missing in token");
        }

        return Guid.Parse(userId);
    }

    // 🔥 DEBUG üçün
    [HttpGet("debug-user")]
    public IActionResult DebugUser()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value });
        return Ok(claims);
    }

    // ✅ CREATE PROFILE
    [HttpPost("me")]
    public async Task<IActionResult> CreateMe(
        [FromBody] CreateCustomerProfileRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();

            var result = await _customerProfileService.CreateAsync(userId, request, cancellationToken);

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
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    // ✅ GET PROFILE
    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();

            var result = await _customerProfileService.GetMyProfileAsync(userId, cancellationToken);

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
                Data = result.Data
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }
}