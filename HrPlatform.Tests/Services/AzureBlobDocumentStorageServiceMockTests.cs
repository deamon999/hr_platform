using System.Text;
using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using HrPlatform.Data;
using HrPlatform.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace HrPlatform.Tests.Services;

public class AzureBlobDocumentStorageServiceMockTests
{
    [Fact]
    public async Task UploadAsync_SavesToDb_And_UploadsToBlob()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "AzureBlobStorage:ContainerName", "test-container" }
            })
            .Build();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("AzureBlobMockDb_" + Guid.NewGuid())
            .Options;

        var services = new ServiceCollection();
        services.AddScoped(sp => new ApplicationDbContext(options));
        var serviceProvider = services.BuildServiceProvider();

        // Setup Azure Mocks
        var mockBlobClient = new Mock<BlobClient>();
        mockBlobClient.Setup(x => x.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobHttpHeaders>(), It.IsAny<IDictionary<string, string>>(),
                It.IsAny<BlobRequestConditions>(), It.IsAny<IProgress<long>>(), It.IsAny<AccessTier?>(), It.IsAny<StorageTransferOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<Response<BlobContentInfo>>());

        var mockContainerClient = new Mock<BlobContainerClient>();
        mockContainerClient.Setup(x => x.CreateIfNotExistsAsync(It.IsAny<PublicAccessType>(), It.IsAny<IDictionary<string, string>>(),
                It.IsAny<BlobContainerEncryptionScopeOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<Response<BlobContainerInfo>>());

        mockContainerClient.Setup(x => x.GetBlobClient(It.IsAny<string>()))
            .Returns(mockBlobClient.Object);

        var mockServiceClient = new Mock<BlobServiceClient>();
        mockServiceClient.Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
            .Returns(mockContainerClient.Object);

        // Inject the mocked service client
        var storageService = new AzureBlobDocumentStorageService(configuration, serviceProvider, mockServiceClient.Object);

        var testContent = "Hello, Azure Blob Storage!";
        var fileStream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));

        // Act
        var documentId = await storageService.UploadAsync(fileStream, "test.txt", "text/plain", "folder");

        // Assert
        Assert.NotNull(documentId);
        mockContainerClient.Verify(x => x.CreateIfNotExistsAsync(PublicAccessType.None, null, null, It.IsAny<CancellationToken>()), Times.Once);
        mockBlobClient.Verify(
            x => x.UploadAsync(fileStream, It.IsAny<BlobHttpHeaders>(), null, null, null, null, default, It.IsAny<CancellationToken>()), Times.Once);
    }
}