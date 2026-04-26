using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZenoBank.BuildingBlocks.Shared.Common.Abstractions;
using ZenoBank.BuildingBlocks.Shared.Common.DTOs;
using ZenoBank.BuildingBlocks.Shared.Common.Responses;

namespace ZenoBank.Services.Identity.API.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Roles = "SuperAdmin,Admin,Operator")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogRepository _repository;

    public AuditLogsController(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Filtrlənmiş, səhifələnmiş audit logları qaytarır.
    /// GET /api/audit-logs?page=1&pageSize=20&entityType=Transaction&status=Success
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? userId,
        [FromQuery] string? entityType,
        [FromQuery] string? action,
        [FromQuery] string? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var filter = new AuditLogFilterRequest
        {
            UserId = userId,
            EntityType = entityType,
            Action = action,
            Status = status,
            FromDate = fromDate,
            ToDate = toDate,
            Page = page,
            PageSize = pageSize
        };

        var result = await _repository.GetPagedAsync(filter, cancellationToken);

        var dtos = result.Items.Select(x => new AuditLogDto
        {
            Id = x.Id,
            UserId = x.UserId,
            Action = x.Action,
            EntityType = x.EntityType,
            EntityId = x.EntityId,
            Description = x.Description,
            Status = x.Status,
            CreatedAtUtc = x.CreatedAtUtc
        }).ToList();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Audit logs fetched successfully.",
            Data = new
            {
                items = dtos,
                result.TotalCount,
                result.Page,
                result.PageSize,
                result.TotalPages,
                result.HasNext,
                result.HasPrev
            }
        });
    }

    /// <summary>
    /// Filter dropdown-ları üçün distinct entity type-ları qaytarır.
    /// GET /api/audit-logs/entity-types
    /// </summary>
    [HttpGet("entity-types")]
    public async Task<IActionResult> GetEntityTypes(CancellationToken cancellationToken)
    {
        var types = await _repository.GetDistinctEntityTypesAsync(cancellationToken);
        return Ok(new ApiResponse<object> { Success = true, Message = "OK", Data = types });
    }

    /// <summary>
    /// Filter dropdown-ları üçün distinct action-ları qaytarır.
    /// GET /api/audit-logs/actions
    /// </summary>
    [HttpGet("actions")]
    public async Task<IActionResult> GetActions(CancellationToken cancellationToken)
    {
        var actions = await _repository.GetDistinctActionsAsync(cancellationToken);
        return Ok(new ApiResponse<object> { Success = true, Message = "OK", Data = actions });
    }
}