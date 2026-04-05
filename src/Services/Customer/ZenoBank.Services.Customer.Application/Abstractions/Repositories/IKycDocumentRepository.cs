using ZenoBank.Services.Customer.Domain.Entities;

namespace ZenoBank.Services.Customer.Application.Abstractions.Repositories;

public interface IKycDocumentRepository
{
    Task AddAsync(KycDocument entity, CancellationToken cancellationToken = default);
    void Update(KycDocument entity);

    Task<KycDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<KycDocument>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<KycDocument>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<KycDocument?> GetLatestByCustomerProfileIdAsync(Guid customerProfileId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
