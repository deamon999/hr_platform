namespace HrPlatform.Services;

public interface IDocumentStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string folder);
    Task<string> GetSignedUrlAsync(string documentId, TimeSpan? expiry = null);
    Task DeleteAsync(string documentId);
}
