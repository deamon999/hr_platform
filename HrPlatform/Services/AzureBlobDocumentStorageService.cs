using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using HrPlatform.Data;
using Microsoft.EntityFrameworkCore;

namespace HrPlatform.Services;

public class AzureBlobDocumentStorageService : IDocumentStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;
    private readonly IServiceProvider _serviceProvider;

    public AzureBlobDocumentStorageService(IConfiguration configuration, IServiceProvider serviceProvider,
        BlobServiceClient? blobServiceClient = null)
    {
        _containerName = configuration["AzureBlobStorage:ContainerName"] ?? "hrplatform-documents";
        _serviceProvider = serviceProvider;

        if (blobServiceClient != null)
        {
            _blobServiceClient = blobServiceClient;
        }
        else
        {
            var connectionString = configuration.GetConnectionString("AzureBlobStorage")
                                   ?? throw new InvalidOperationException("AzureBlobStorage connection string is missing.");

            var options = new BlobClientOptions(BlobClientOptions.ServiceVersion.V2024_11_04);
            _blobServiceClient = new BlobServiceClient(connectionString, options);
        }
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string folder)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync();

        var blobId = Guid.NewGuid().ToString("N");
        var blobName = string.IsNullOrWhiteSpace(folder) ? blobId : $"{folder}/{blobId}";

        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType });

        // Return the generated blobId. 
        // The calling code (e.g. ProfileDocumentsForm) is responsible for creating the DocumentFile entity 
        // and saving it to the database with this Id and the blobName as FilePath.
        return blobId;
    }

    public async Task<string> GetSignedUrlAsync(string documentId, TimeSpan? expiry = null)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var document = await db.DocumentFiles.FindAsync(documentId);
        if (document == null || string.IsNullOrEmpty(document.FilePath)) return string.Empty;

        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(document.FilePath);

        if (!await blobClient.ExistsAsync()) return string.Empty;

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerName,
            BlobName = document.FilePath,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.Add(expiry ?? TimeSpan.FromHours(1))
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sasUri = blobClient.GenerateSasUri(sasBuilder);
        return sasUri.ToString();
    }

    public async Task DeleteAsync(string documentId)
    {
        // documentId is usually the blobId, which we can use to figure out the FilePath
        // But wait! If we only have documentId, we can't get the FilePath without the DB!
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Just get the file path, but DO NOT delete it from DB here, let the caller handle DB deletion.
        var document = await db.DocumentFiles.AsNoTracking().FirstOrDefaultAsync(d => d.Id == documentId);
        if (document != null && !string.IsNullOrEmpty(document.FilePath))
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(document.FilePath);
            await blobClient.DeleteIfExistsAsync();
        }
    }
}