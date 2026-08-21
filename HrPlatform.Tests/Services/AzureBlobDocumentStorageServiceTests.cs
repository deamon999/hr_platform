using System.Text;
using Azure.Storage.Blobs;
using HrPlatform.Data;
using HrPlatform.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HrPlatform.Tests.Services;

public class AzureBlobDocumentStorageServiceTests
{
    [Fact(Skip = "Requires local Azurite emulator running. Remove skip to run manually.")]
    public async Task Can_Upload_And_Get_Signed_Url()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:AzureBlobStorage", "UseDevelopmentStorage=true" },
                { "AzureBlobStorage:ContainerName", "test-documents-container" }
            })
            .Build();

        var services = new ServiceCollection();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("AzureBlobTestDb_" + Guid.NewGuid())
            .Options;

        services.AddScoped(sp => new ApplicationDbContext(options));
        var serviceProvider = services.BuildServiceProvider();

        var storageService = new AzureBlobDocumentStorageService(configuration, serviceProvider);

        var testContent = "Hello, Azure Blob Storage!";
        var fileStream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));
        var fileName = "test_document.txt";
        var contentType = "text/plain";
        var folder = "testfolder";

        // Act - Upload
        var documentId = await storageService.UploadAsync(fileStream, fileName, contentType, folder);

        // Assert - Upload
        Assert.NotNull(documentId);

        // Act - Get URL
        var signedUrl = await storageService.GetSignedUrlAsync(documentId);

        // Assert - URL
        Assert.NotNull(signedUrl);
        Assert.Contains("test-documents-container", signedUrl);
        Assert.Contains("sig=", signedUrl); // Indicates a SAS signature is present

        // Verify blob actually exists in Azurite
        var blobOptions = new BlobClientOptions(BlobClientOptions.ServiceVersion.V2024_11_04);
        var blobServiceClient = new BlobServiceClient("UseDevelopmentStorage=true", blobOptions);
        var containerClient = blobServiceClient.GetBlobContainerClient("test-documents-container");
        var db = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var doc = await db.DocumentFiles.FindAsync(documentId);

        Assert.NotNull(doc);
        var blobClient = containerClient.GetBlobClient(doc.FilePath);
        Assert.True(await blobClient.ExistsAsync());

        // Cleanup
        await storageService.DeleteAsync(documentId);
        Assert.False(await blobClient.ExistsAsync());
    }
}