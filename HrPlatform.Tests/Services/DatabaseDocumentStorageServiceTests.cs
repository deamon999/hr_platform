using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using HrPlatform.Data;
using HrPlatform.Services;
using HrPlatform.Data.Entities;

namespace HrPlatform.Tests.Services
{
    public class DatabaseDocumentStorageServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            return new ApplicationDbContext(options);
        }

        private IServiceProvider GetServiceProvider(ApplicationDbContext dbContext)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(dbContext);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            var scopeMock = new Mock<IServiceScope>();
            scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProvider);
            scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

            var rootProviderMock = new Mock<IServiceProvider>();
            rootProviderMock.Setup(p => p.GetService(typeof(IServiceScopeFactory))).Returns(scopeFactoryMock.Object);
            
            return rootProviderMock.Object;
        }

        [Fact]
        public async Task UploadAsync_SavesDocumentAndReturnsId()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var serviceProvider = GetServiceProvider(dbContext);
            
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var service = new DatabaseDocumentStorageService(serviceProvider, httpContextAccessorMock.Object);

            var fileContent = "dummy content";
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            await writer.WriteAsync(fileContent);
            await writer.FlushAsync();
            stream.Position = 0;

            // Act
            var documentId = await service.UploadAsync(stream, "test.txt", "text/plain", "folder");

            // Assert
            Assert.NotNull(documentId);
            var savedDoc = await dbContext.DocumentFiles.FirstOrDefaultAsync(d => d.Id == documentId);
            Assert.NotNull(savedDoc);
            Assert.Equal("test.txt", savedDoc.FileName);
            Assert.Equal("text/plain", savedDoc.ContentType);
            Assert.NotEmpty(savedDoc.Data);
        }

        [Fact]
        public async Task GetSignedUrlAsync_ReturnsConstructedUrl()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var serviceProvider = GetServiceProvider(dbContext);
            
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            context.Request.Scheme = "https";
            context.Request.Host = new HostString("example.com");
            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

            var service = new DatabaseDocumentStorageService(serviceProvider, httpContextAccessorMock.Object);
            var documentId = "doc123";

            // Act
            var url = await service.GetSignedUrlAsync(documentId);

            // Assert
            Assert.Equal("https://example.com/api/documents/doc123", url);
        }

        [Fact]
        public async Task DeleteAsync_RemovesDocument()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var document = new DocumentFile
            {
                Id = "doc-delete",
                FileName = "delete.txt",
                ContentType = "text/plain",
                Data = new byte[] { 1, 2, 3 },
                UploadedAt = DateTime.UtcNow
            };
            dbContext.DocumentFiles.Add(document);
            await dbContext.SaveChangesAsync();

            var serviceProvider = GetServiceProvider(dbContext);
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var service = new DatabaseDocumentStorageService(serviceProvider, httpContextAccessorMock.Object);

            // Act
            await service.DeleteAsync("doc-delete");

            // Assert
            var deletedDoc = await dbContext.DocumentFiles.FirstOrDefaultAsync(d => d.Id == "doc-delete");
            Assert.Null(deletedDoc);
        }
    }
}
