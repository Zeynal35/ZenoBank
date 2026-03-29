using Microsoft.AspNetCore.Mvc;
using ZenoBank.BuildingBlocks.Shared.Common.Responses;
using ZenoBank.Services.Customer.Application.Abstractions.Services;

namespace ZenoBank.Services.Customer.API.Controllers;

[ApiController]
[Route("internal/customers")]
public class InternalCustomersController : ControllerBase
{
    private readonly ICustomerProfileService _customerProfileService;

    public InternalCustomersController(ICustomerProfileService customerProfileService)
    {
        _customerProfileService = customerProfileService;
    }

    [HttpGet("{id:guid}/compliance")]
    public async Task<IActionResult> GetCompliance(Guid id, CancellationToken cancellationToken)
    {
        var result = await _customerProfileService.GetInternalComplianceByIdAsync(id, cancellationToken);

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
