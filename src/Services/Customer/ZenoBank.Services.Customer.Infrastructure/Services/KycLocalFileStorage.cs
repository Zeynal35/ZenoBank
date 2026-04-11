using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ZenoBank.Services.Customer.Application.Abstractions.Services;

namespace ZenoBank.Services.Customer.Infrastructure.Services;

public class KycLocalFileStorage : IKycFileStorage
{
    private readonly string _storageRoot;

    public KycLocalFileStorage(IConfiguration configuration)
    {
        _storageRoot = configuration["KycStorage:RootPath"]
                       ?? Path.Combine(AppContext.BaseDirectory, "kyc-files");

        if (!Directory.Exists(_storageRoot))
            Directory.CreateDirectory(_storageRoot);
    }

    public async Task<(string StoredFileName, string FilePath)> SaveAsync(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(_storageRoot, storedFileName);

        await using var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await file.CopyToAsync(stream, cancellationToken);

        return (storedFileName, fullPath);
    }
}