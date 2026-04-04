using ZenoBank.Services.Identity.Domain.Entities;

namespace ZenoBank.Services.Identity.Application.Abstractions.Repositories;

public interface IEmailVerificationTokenRepository
{
    Task AddAsync(EmailVerificationToken token, CancellationToken cancellationToken = default);
    Task<EmailVerificationToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<EmailVerificationToken?> GetLatestActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    void Update(EmailVerificationToken token);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
