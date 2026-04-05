using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using ZenoBank.Services.Customer.Application.Abstractions.Services;

namespace ZenoBank.Services.Customer.Infrastructure.Services;

public class KycLocalFileStorage : IKycFileStorage
{
    private readonly IWebHostEnvironment _environment;

    public KycLocalFileStorage(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<(string StoredFileName, string FilePath)> SaveAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        var uploadsRoot = Path.Combine(_environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"), "kyc");

        if (!Directory.Exists(uploadsRoot))
            Directory.CreateDirectory(uploadsRoot);

        var extension = Path.GetExtension(file.FileName);
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(uploadsRoot, storedFileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream, cancellationToken);

        var relativePath = $"/kyc/{storedFileName}";
        return (storedFileName, relativePath);
    }
}
