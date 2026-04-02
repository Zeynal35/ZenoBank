using ZenoBank.BuildingBlocks.Shared.Common.Results;
using ZenoBank.Services.Loan.Application.DTOs;

namespace ZenoBank.Services.Loan.Application.Abstractions.Services;

public interface ICustomerServiceClient
{
    Task<Result<InternalCustomerComplianceSnapshotDto>> GetCustomerComplianceAsync(Guid customerProfileId, CancellationToken cancellationToken = default);
}
