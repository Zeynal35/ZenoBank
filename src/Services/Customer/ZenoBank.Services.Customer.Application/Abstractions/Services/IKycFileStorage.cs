using Microsoft.AspNetCore.Http;

namespace ZenoBank.Services.Customer.Application.Abstractions.Services;

public interface IKycFileStorage
{
    Task<(string StoredFileName, string FilePath)> SaveAsync(IFormFile file, CancellationToken cancellationToken = default);
}
