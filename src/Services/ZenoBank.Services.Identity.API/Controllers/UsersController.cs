using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ZenoBank.BuildingBlocks.Shared.Common.Responses;
using ZenoBank.Services.Identity.Application.Abstractions.Services;

namespace ZenoBank.Services.Identity.API.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IIdentityService _identityService;

    public UsersController(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = "User is not authenticated."
            });
        }

        var result = await _identityService.GetCurrentUserAsync(userId, cancellationToken);

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