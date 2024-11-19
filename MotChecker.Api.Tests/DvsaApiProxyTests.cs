using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq.Protected;
using Moq;
using MotChecker.Api.Services;
using System.Net;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace MotChecker.Api.Tests;

public class DvsaApiProxyTests
{
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<ILogger<DvsaApiProxy>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly DvsaApiProxy _sut;
    private readonly HttpClient _httpClient;

    public DvsaApiProxyTests()
    {
        _configMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<DvsaApiProxy>>();
        _handlerMock = new Mock<HttpMessageHandler>();

        // Setup config
        _configMock.Setup(x => x["DvsaApi:ApiKey"]).Returns("test-api-key");
        _configMock.Setup(x => x["DvsaApi:BaseUrl"]).Returns("https://test.api/");
        _configMock.Setup(x => x["DvsaApi:ClientId"]).Returns("test-client-id");
        _configMock.Setup(x => x["DvsaApi:ClientSecret"]).Returns("test-client-secret");
        _configMock.Setup(x => x["DvsaApi:TokenUrl"]).Returns("https://test.auth/token");
        _configMock.Setup(x => x["DvsaApi:ScopeUrl"]).Returns("https://test.api/.default");

        // Setup mock HTTP handler
        _httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("https://test.api/")
        };

        _sut = new DvsaApiProxy(_httpClient, _configMock.Object, _loggerMock.Object);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task GetVehicleDetailsAsync_WithInvalidRegistration_ThrowsArgumentException(string registration)
    {
        // Act & Assert
        var action = () => _sut.GetVehicleDetailsAsync(registration);
        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Registration number cannot be empty or whitespace.*");
    }

    [Fact]
    public async Task GetVehicleDetailsAsync_WithValidRegistration_ReturnsVehicleDetails()
    {
        // Arrange
        var registration = "AB12CDE";

        // Setup mock responses
        var tokenResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(@"{""access_token"":""test-token""}")
        };

        var vehicleResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(@"{
                ""registration"":""AB12CDE"",
                ""make"":""TOYOTA"",
                ""model"":""COROLLA"",
                ""primaryColour"":""SILVER"",
                ""motTests"":[{
                    ""expiryDate"":""2024-01-01"",
                    ""odometerValue"":""50000""
                }]
            }")
        };

        _handlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(tokenResponse)
            .ReturnsAsync(vehicleResponse);

        // Act
        var result = await _sut.GetVehicleDetailsAsync(registration);

        // Assert
        result.Should().NotBeNull();
        result.Registration.Should().Be("AB12CDE");
        result.Make.Should().Be("TOYOTA");
    }

    [Fact]
    public async Task GetVehicleDetailsAsync_WhenApiReturnsError_ThrowsHttpRequestException()
    {
        // Arrange
        var errorResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.Unauthorized,
            Content = new StringContent(@"{""error"":""unauthorized""}")
        };

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(errorResponse);

        // Act & Assert
        var action = () => _sut.GetVehicleDetailsAsync("AB12CDE");
        await action.Should().ThrowAsync<HttpRequestException>();
    }
}