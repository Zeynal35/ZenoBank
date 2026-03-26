using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZenoBank.BuildingBlocks.Shared.Common.Responses;
using ZenoBank.Services.Transaction.Application.Abstractions.Services;
using ZenoBank.Services.Transaction.Application.DTOs;

namespace ZenoBank.Services.Transaction.API.Controllers;

[ApiController]
[Route("api/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ICurrentUserService _currentUserService;

    public TransactionsController(
        ITransactionService transactionService,
        ICurrentUserService currentUserService)
    {
        _transactionService = transactionService;
        _currentUserService = currentUserService;
    }

    [Authorize]
    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] DepositRequest request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = "User is not authenticated."
            });
        }

        var result = await _transactionService.DepositAsync(_currentUserService.UserId.Value, request, cancellationToken);

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
    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawRequest request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = "User is not authenticated."
            });
        }

        var result = await _transactionService.WithdrawAsync(_currentUserService.UserId.Value, request, cancellationToken);

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
    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = "User is not authenticated."
            });
        }

        var result = await _transactionService.TransferAsync(_currentUserService.UserId.Value, request, cancellationToken);

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
    public async Task<IActionResult> GetMyTransactions(CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = "User is not authenticated."
            });
        }

        var result = await _transactionService.GetMyTransactionsAsync(_currentUserService.UserId.Value, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = result.Message,
            Data = result.Data
        });
    }

    [Authorize]
    [HttpGet("my/{id:guid}")]
    public async Task<IActionResult> GetMyTransactionById(Guid id, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = "User is not authenticated."
            });
        }

        var result = await _transactionService.GetMyTransactionByIdAsync(_currentUserService.UserId.Value, id, cancellationToken);

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
        var result = await _transactionService.GetAllAsync(cancellationToken);

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
        var result = await _transactionService.GetByIdAsync(id, cancellationToken);

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
