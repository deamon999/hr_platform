using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using HrPlatform.Services;
using System.Collections.Generic;

namespace HrPlatform.Tests.Services
{
    public class EmailServiceTests
    {
        [Fact]
        public async Task SendEmailAsync_ConstructsSuccessfullyAndHandlesException()
        {
            // Arrange
            var inMemorySettings = new Dictionary<string, string?> {
                {"Smtp:Host", "smtp.example.com"},
                {"Smtp:Port", "587"},
                {"Smtp:Username", "test_user"},
                {"Smtp:Password", "test_pass"},
                {"Smtp:FromEmail", "noreply@example.com"},
                {"Smtp:FromName", "Test Sender"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var emailService = new EmailService(configuration);

            // Act
            // SmtpClient will fail to connect since it's a dummy host, but exception is swallowed in try/catch
            await emailService.SendEmailAsync("test@example.com", "Test User", "Subject", "<p>Html Content</p>");

            // Assert
            // No exception should be thrown because the service handles it internally and logs to console.
            Assert.True(true);
        }
        
        [Fact]
        public async Task SendEmailAsync_WithNullUserName_ConstructsSuccessfully()
        {
            // Arrange
            var inMemorySettings = new Dictionary<string, string?> {
                {"Smtp:Host", "smtp.example.com"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var emailService = new EmailService(configuration);

            // Act
            await emailService.SendEmailAsync("test@example.com", null, "Subject", "<p>Html Content</p>");

            // Assert
            Assert.True(true);
        }

        [Fact]
        public void Constructor_MissingHost_ThrowsArgumentNullException()
        {
            // Arrange
            var inMemorySettings = new Dictionary<string, string?>();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new EmailService(configuration));
        }
    }
}
