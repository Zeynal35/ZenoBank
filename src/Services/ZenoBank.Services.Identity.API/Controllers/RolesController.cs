using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZenoBank.BuildingBlocks.Shared.Common.Responses;
using ZenoBank.Services.Identity.Application.DTOs;
using ZenoBank.Services.Identity.Application.Abstractions.Services;

namespace ZenoBank.Services.Identity.API.Controllers;

[ApiController]
[Route("api/roles")]
public class RolesController : ControllerBase
{
    private readonly IIdentityService _identityService;

    public RolesController(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost("assign")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _identityService.AssignRoleAsync(request, cancellationToken);

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