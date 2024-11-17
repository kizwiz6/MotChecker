using Xunit;
using Moq;
using FluentAssertions;
using MotChecker.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace MotChecker.Tests
{
    public class VehicleServiceTests
    {
        private readonly Mock<ILogger<MockVehicleService>> _loggerMock;
        private readonly IMemoryCache _memoryCache;

        public VehicleServiceTests()
        {
            _loggerMock = new Mock<ILogger<MockVehicleService>>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
        }

        [Fact]
        public async Task GetVehicleDetailsAsync_Should_Return_Vehicle_Details()
        {
            // Arrange
            var service = new MockVehicleService(_loggerMock.Object);
            var registration = "AB12CDE";

            // Act
            var result = await service.GetVehicleDetailsAsync(registration);

            // Assert
            result.Should().NotBeNull();
            result.Registration.Should().Be(registration);
            result.Make.Should().NotBeNullOrEmpty();
            result.Model.Should().NotBeNullOrEmpty();
            result.Colour.Should().NotBeNullOrEmpty();
            result.MotExpiryDate.Should().BeAfter(DateTime.Now);
            result.MileageAtLastMot.Should().BeGreaterThan(0);
        }
    }
}
