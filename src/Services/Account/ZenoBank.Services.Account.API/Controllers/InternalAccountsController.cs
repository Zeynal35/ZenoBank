using Microsoft.AspNetCore.Mvc;
using ZenoBank.BuildingBlocks.Shared.Common.Responses;
using ZenoBank.Services.Account.Application.Abstractions.Services;
using ZenoBank.Services.Account.Application.DTOs;

namespace ZenoBank.Services.Account.API.Controllers;

[ApiController]
[Route("internal/accounts")]
public class InternalAccountsController : ControllerBase
{
    private readonly IBankAccountService _bankAccountService;

    public InternalAccountsController(IBankAccountService bankAccountService)
    {
        _bankAccountService = bankAccountService;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _bankAccountService.GetInternalByIdAsync(id, cancellationToken);

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

    [HttpPost("increase-balance")]
    public async Task<IActionResult> IncreaseBalance([FromBody] IncreaseBalanceRequest request, CancellationToken cancellationToken)
    {
        var result = await _bankAccountService.IncreaseBalanceAsync(request.AccountId, request.Amount, cancellationToken);

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

    [HttpPost("decrease-balance")]
    public async Task<IActionResult> DecreaseBalance([FromBody] DecreaseBalanceRequest request, CancellationToken cancellationToken)
    {
        var result = await _bankAccountService.DecreaseBalanceAsync(request.AccountId, request.Amount, cancellationToken);

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

    [HttpPost("transfer-balance")]
    public async Task<IActionResult> TransferBalance([FromBody] InternalTransferBalanceRequest request, CancellationToken cancellationToken)
    {
        var result = await _bankAccountService.TransferBalanceAsync(request.FromAccountId, request.ToAccountId, request.Amount, cancellationToken);

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
