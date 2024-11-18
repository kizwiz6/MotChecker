using Xunit;
using Moq;
using FluentAssertions;
using MotChecker.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace MotChecker.Tests
{
    /// <summary>
    /// Tests for the MockVehicleService implementation
    /// </summary>
    public class VehicleServiceTests
    {
        private readonly Mock<ILogger<MockVehicleService>> _loggerMock;
        private readonly MockVehicleService _service;
        private const string VALID_REGISTRATION = "LB11WXA";

        public VehicleServiceTests()
        {
            _loggerMock = new Mock<ILogger<MockVehicleService>>();
            _service = new MockVehicleService(_loggerMock.Object);
        }

        /// <summary>
        /// Tests that the service returns correct vehicle details for a valid registration
        /// </summary>
        [Fact]
        public async Task GetVehicleDetailsAsync_Should_Return_Vehicle_Details()
        {
            // Act
            var result = await _service.GetVehicleDetailsAsync(VALID_REGISTRATION);

            // Assert
            result.Should().NotBeNull();
            result.Registration.Should().Be(VALID_REGISTRATION);
            result.Make.Should().Be("Kia");
            result.Model.Should().Be("Rio");
        }

        /// <summary>
        /// Tests that the service throws ArgumentException for invalid registration inputs
        /// </summary>
        /// <param name="registration">The invalid registration to test</param>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task GetVehicleDetailsAsync_Should_Throw_ArgumentException_For_Invalid_Registration(string registration)
        {
            // Act & Assert
            await _service.Invoking(s => s.GetVehicleDetailsAsync(registration))
                .Should().ThrowAsync<ArgumentException>();
        }

        /// <summary>
        /// Tests that the service throws an exception when querying an unknown registration
        /// </summary>
        [Fact]
        public async Task GetVehicleDetailsAsync_Should_Throw_For_Unknown_Registration()
        {
            // Act & Assert
            await _service.Invoking(s => s.GetVehicleDetailsAsync("XX99XXX"))
                .Should().ThrowAsync<Exception>().WithMessage("Vehicle not found");
        }

        /// <summary>
        /// Tests that the service logs information when retrieving vehicle details
        /// </summary>
        [Fact]
        public async Task GetVehicleDetailsAsync_Should_Log_Information()
        {
            // Act
            await _service.GetVehicleDetailsAsync(VALID_REGISTRATION);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(VALID_REGISTRATION)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
