using MotChecker.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MotChecker.Api.Services
{
    /// <summary>
    /// Proxy service for interacting with the DVSA API
    /// Handles authentication, requests, and response parsing
    /// </summary>
    public class DvsaApiProxy
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _tokenClient;
        private string? _accessToken;
        private readonly ILogger<DvsaApiProxy> _logger;

        /// <summary>
        /// Initialises a new instance of the DvsaApiProxy
        /// </summary>
        /// <param name="httpClient">HTTP client for API requests</param>
        /// <param name="configuration">Application configuration</param>
        /// <param name="logger">Logger instance</param>
        /// <exception cref="ArgumentNullException">Thrown when required dependencies are null</exception>
        /// <exception cref="ArgumentException">Thrown when API key is not configured</exception>
        public DvsaApiProxy(HttpClient httpClient, IConfiguration configuration, ILogger<DvsaApiProxy> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _tokenClient = new HttpClient();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var apiKey = configuration.GetSection("DvsaApi:ApiKey").Value;

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentException("DvsaApi:ApiKey configuration is required");
            }
        }

        /// <summary>
        /// Retrieves vehicle details from the DVSA API
        /// </summary>
        /// <param name="registration">Vehicle registration number</param>
        /// <returns>Vehicle details if found</returns>
        /// <exception cref="ArgumentException">Thrown when registration is invalid</exception>
        /// <exception cref="InvalidOperationException">Thrown when response parsing fails</exception>
        public async Task<VehicleDetails> GetVehicleDetailsAsync(string registration)
        {
            if (string.IsNullOrWhiteSpace(registration))
            {
                throw new ArgumentException("Registration cannot be empty or whitespace.", nameof(registration));
            }

            try
            {
                await EnsureAccessTokenAsync();

                // Build the request URL properly
                var baseUrl = "https://history.mot.api.gov.uk/v1/trade/vehicles/registration/";
                var fullUrl = $"{baseUrl}{registration}";

                var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);
                request.Headers.Add("X-API-Key", _configuration["DvsaApi:ApiKey"]);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Raw response: {Content}", content);

                // Parse the raw JSON to get the most recent MOT test
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                var vehicleDetails = new VehicleDetails
                {
                    Registration = root.GetProperty("registration").GetString() ?? string.Empty,
                    Make = root.GetProperty("make").GetString() ?? string.Empty,
                    Model = root.GetProperty("model").GetString() ?? string.Empty,
                    Colour = root.GetProperty("primaryColour").GetString() ?? string.Empty,
                };

                // Get the most recent MOT test
                if (root.TryGetProperty("motTests", out var motTests) &&
                    motTests.GetArrayLength() > 0)
                {
                    var latestTest = motTests[0];
                    if (latestTest.TryGetProperty("expiryDate", out var expiryDate))
                    {
                        vehicleDetails.MotExpiryDate = DateTime.Parse(expiryDate.GetString()!);
                    }
                    if (latestTest.TryGetProperty("odometerValue", out var odometer))
                    {
                        vehicleDetails.MileageAtLastMot = int.Parse(odometer.GetString()!);
                    }
                }

                return vehicleDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetVehicleDetailsAsync");
                throw;
            }
        }

        /// <summary>
        /// Ensures a valid access token is available for API requests
        /// Handles token acquisition and caching
        /// </summary>
        /// <returns>Task representing the token acquisition process</returns>
        /// <exception cref="HttpRequestException">Thrown when token request fails</exception>
        private async Task EnsureAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken))
            {
                return;
            }

            var tokenUrl = _configuration["DvsaApi:TokenUrl"];
            var tokenParams = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _configuration["DvsaApi:ClientId"]!,
                ["client_secret"] = _configuration["DvsaApi:ClientSecret"]!,
                ["scope"] = _configuration["DvsaApi:ScopeUrl"]!
            };

            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
            {
                Content = new FormUrlEncodedContent(tokenParams)
            };

            var response = await _httpClient.SendAsync(tokenRequest);
            response.EnsureSuccessStatusCode();

            var tokenData = await response.Content.ReadFromJsonAsync<JsonElement>();
            _accessToken = tokenData.GetProperty("access_token").GetString();
        }
    }
}
