using HrPlatform.Data;
using HrPlatform.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HrPlatform.Services;

public class DatabaseDocumentStorageService : IDocumentStorageService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DatabaseDocumentStorageService(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor)
    {
        _serviceProvider = serviceProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string folder)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);

        var document = new DocumentFile
        {
            FileName = fileName,
            ContentType = contentType,
            Data = memoryStream.ToArray(),
            UploadedAt = DateTime.UtcNow
        };

        db.DocumentFiles.Add(document);
        await db.SaveChangesAsync();

        return document.Id;
    }

    public Task<string> GetSignedUrlAsync(string documentId, TimeSpan? expiry = null)
    {
        // Instead of a SAS token, we provide a secure endpoint.
        var request = _httpContextAccessor.HttpContext?.Request;
        var baseUrl = $"{request?.Scheme}://{request?.Host}";
        
        // This endpoint will be protected by standard authentication.
        return Task.FromResult($"{baseUrl}/api/documents/{documentId}");
    }

    public async Task DeleteAsync(string documentId)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var document = await db.DocumentFiles.FirstOrDefaultAsync(d => d.Id == documentId);
        if (document != null)
        {
            db.DocumentFiles.Remove(document);
            await db.SaveChangesAsync();
        }
    }
}
