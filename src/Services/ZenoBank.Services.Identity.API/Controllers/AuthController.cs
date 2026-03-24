using Microsoft.AspNetCore.Mvc;
using ZenoBank.BuildingBlocks.Shared.Common.Responses;
using ZenoBank.Services.Identity.Application.Abstractions.Services;
using ZenoBank.Services.Identity.Application.DTOs;

namespace ZenoBank.Services.Identity.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IIdentityService _identityService;

    public AuthController(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _identityService.RegisterAsync(request, cancellationToken);

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

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _identityService.LoginAsync(request, cancellationToken);

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

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _identityService.RefreshTokenAsync(request, cancellationToken);

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

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _identityService.LogoutAsync(request.RefreshToken, cancellationToken);

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
            Message = result.Message
        });
    }
}
