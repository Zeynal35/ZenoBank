using Microsoft.AspNetCore.Mvc;
using ZenoBank.BuildingBlocks.Shared.Common.Responses;
using ZenoBank.Services.Identity.Application.Abstractions.Services;

namespace ZenoBank.Services.Identity.API.Controllers;

[ApiController]
[Route("internal/users")]
public class InternalUsersController : ControllerBase
{
    private readonly IIdentityService _identityService;

    public InternalUsersController(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    [HttpGet("{id:guid}/contact")]
    public async Task<IActionResult> GetContact(Guid id, CancellationToken cancellationToken)
    {
        var result = await _identityService.GetInternalUserContactByIdAsync(id, cancellationToken);

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
}
