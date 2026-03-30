using ZenoBank.BuildingBlocks.Shared.Common.Results;
using ZenoBank.Services.Customer.Application.DTOs;

namespace ZenoBank.Services.Customer.Application.Abstractions.Services;

public interface ICustomerProfileService
{
    Task<Result<CustomerProfileDto>> CreateAsync(Guid userId, CreateCustomerProfileRequest request, CancellationToken cancellationToken = default);
    Task<Result<CustomerProfileDto>> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<CustomerProfileDto>> UpdateMyProfileAsync(Guid userId, UpdateCustomerProfileRequest request, CancellationToken cancellationToken = default);

    Task<Result<List<CustomerProfileDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<CustomerProfileDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<InternalCustomerComplianceDto>> GetInternalComplianceByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<CustomerProfileDto>> BlacklistAsync(Guid id, string reason, CancellationToken cancellationToken = default);
    Task<Result<CustomerProfileDto>> RemoveFromBlacklistAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<CustomerProfileDto>> UpdateRiskLevelAsync(Guid id, int riskLevel, CancellationToken cancellationToken = default);
    Task<Result<CustomerProfileDto>> UpdateStatusAsync(Guid id, int status, CancellationToken cancellationToken = default);
}
