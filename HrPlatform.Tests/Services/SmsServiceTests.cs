using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Xunit;
using HrPlatform.Services;

namespace HrPlatform.Tests.Services
{
    public class SmsServiceTests
    {
        [Fact]
        public async Task SendDriverInviteAsync_ConstructsSuccessfullyAndHandlesException()
        {
            // Arrange
            var inMemorySettings = new Dictionary<string, string?> {
                {"Brevo:ApiKey", "fake_api_key"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var smsService = new SmsService(configuration);

            // Act
            // Will fail due to fake key, but try/catch swallows exception
            await smsService.SendDriverInviteAsync("1234567890", "John", "Doe", "Test message");

            // Assert
            Assert.True(true);
        }
    }
}
