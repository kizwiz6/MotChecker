using Xunit;
using Moq;
using FluentAssertions;
using MotChecker.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using FluentAssertions.Common;

namespace MotChecker.Tests
{
    public class VehicleServiceTests
    {
        private readonly Mock<ILogger<MockVehicleService>> _loggerMock;
        private readonly MockVehicleService _service;

        public VehicleServiceTests()
        {
            _loggerMock = new Mock<ILogger<MockVehicleService>>();
            _service = new MockVehicleService(_loggerMock.Object);
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

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task GetVehicleDetailsAsync_Should_Throw_ArgumentException_For_Invalid_Registration(string registration)
        {
            // Arrange & Act
            var action = () => _service.GetVehicleDetailsAsync(registration);

            // Assert
            await action.Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("Registration number cannot be empty or whitespace.*");
        }

        [Fact]
        public async Task GetVehicleDetailsAsync_Should_Log_Information()
        {
            // Arrange
            var registration = "AB12CDE";

            // Act
            await _service.GetVehicleDetailsAsync(registration);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(registration)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
