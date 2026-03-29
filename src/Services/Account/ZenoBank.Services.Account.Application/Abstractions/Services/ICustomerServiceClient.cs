using ZenoBank.BuildingBlocks.Shared.Common.Results;
using ZenoBank.Services.Account.Application.DTOs;

namespace ZenoBank.Services.Account.Application.Abstractions.Services;

public interface ICustomerServiceClient
{
    Task<Result<InternalCustomerComplianceSnapshotDto>> GetCustomerComplianceAsync(Guid customerProfileId, CancellationToken cancellationToken = default);
}
