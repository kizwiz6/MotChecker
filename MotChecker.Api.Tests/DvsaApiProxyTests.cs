using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq.Protected;
using Moq;
using MotChecker.Api.Services;
using System.Net;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

/// <summary>
/// Tests for the DVSA API proxy service
/// </summary>
public class DvsaApiProxyTests
{
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<ILogger<DvsaApiProxy>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly DvsaApiProxy _sut;

    /// <summary>
    /// Initialises test context and mocks required dependencies
    /// </summary>
    public DvsaApiProxyTests()
    {
        _configMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<DvsaApiProxy>>();
        _handlerMock = new Mock<HttpMessageHandler>();

        // Setup configuration section mock
        var configSection = new Mock<IConfigurationSection>();
        configSection.Setup(x => x.Value).Returns("mock-api-key");
        _configMock.Setup(x => x.GetSection("DvsaApi:ApiKey")).Returns(configSection.Object);

        // Setup other configuration values
        _configMock.Setup(x => x["DvsaApi:ApiKey"]).Returns("mock-api-key");
        _configMock.Setup(x => x["DvsaApi:BaseUrl"]).Returns("https://history.mot.api.gov.uk/v1/");
        _configMock.Setup(x => x["DvsaApi:ClientId"]).Returns("mock-client-id");
        _configMock.Setup(x => x["DvsaApi:ClientSecret"]).Returns("mock-client-secret");
        _configMock.Setup(x => x["DvsaApi:TokenUrl"]).Returns("https://mock.auth/token");
        _configMock.Setup(x => x["DvsaApi:ScopeUrl"]).Returns("https://mock.api/.default");

        var client = new HttpClient(_handlerMock.Object);
        _sut = new DvsaApiProxy(client, _configMock.Object, _loggerMock.Object);
    }

    /// <summary>
    /// Tests that attempting to get vehicle details with invalid registration throws ArgumentException
    /// </summary>
    /// <param name="registration">Invalid registration input to test</param>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task GetVehicleDetailsAsync_WithInvalidRegistration_ThrowsArgumentException(string registration)
    {
        // Act & Assert
        var action = () => _sut.GetVehicleDetailsAsync(registration);
        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*registration*");
    }

    /// <summary>
    /// Tests successful retrieval of vehicle details with valid registration
    /// </summary>
    [Fact]
    public async Task GetVehicleDetailsAsync_WithValidRegistration_ReturnsVehicleDetails()
    {
        // Arrange
        const string registration = "KW64JYJ";

        // Setup both token and vehicle response
        _handlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    JsonSerializer.Serialize(new { access_token = "test-token" }),
                    System.Text.Encoding.UTF8,
                    "application/json")
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    JsonSerializer.Serialize(new
                    {
                        registration = registration,
                        make = "SUZUKI",
                        model = "SWIFT",
                        primaryColour = "BLUE",
                        motTests = new[]
                        {
                            new { expiryDate = "2024-01-01", odometerValue = "50000" }
                        }
                    }),
                    System.Text.Encoding.UTF8,
                    "application/json")
            });

        // Act
        var result = await _sut.GetVehicleDetailsAsync(registration);

        // Assert
        result.Should().NotBeNull();
        result.Registration.Should().Be(registration);
        result.Make.Should().Be("SUZUKI");
        result.Model.Should().Be("SWIFT");

        // Verify the mock was called
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(2),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }
}