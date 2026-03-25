using ZenoBank.Services.Customer.Domain.Entities;

namespace ZenoBank.Services.Customer.Application.Abstractions.Repositories;

public interface ICustomerProfileRepository
{
    Task AddAsync(CustomerProfile profile, CancellationToken cancellationToken = default);
    void Update(CustomerProfile profile);
    Task<CustomerProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CustomerProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<CustomerProfile>> GetAllAsync(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
