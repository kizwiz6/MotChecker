using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MotChecker.Models;

namespace MotChecker.Services
{
    /// <summary>
    /// Service for retrieving vehicle details from the DVSA API with caching support
    /// Implements the Repository pattern with caching decorator
    /// </summary>
    public class DvsaVehicleService : IVehicleService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly ILogger<DvsaVehicleService> _logger;
        private string? _accessToken;

        /// <summary>
        /// Initialises a new instance of the DvsaVehicleService
        /// </summary>
        /// <param name="httpClient">HTTP client for API communication</param>
        /// <param name="configuration">Application configuration</param>
        /// <param name="cache">Memory cache for storing vehicle details</param>
        /// <param name="logger">Logger instance</param>
        public DvsaVehicleService(
            HttpClient httpClient,
            IConfiguration configuration,
            IMemoryCache cache,
            ILogger<DvsaVehicleService> logger
            )
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves vehicle details by registration number with caching
        /// </summary>
        /// <param name="registration">Vehicle registration number</param>
        /// <returns>Vehicle details if found</returns>
        /// <remarks>
        /// Implements a caching strategy with 30-minute expiration
        /// Handles token management and API authentication
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown when registration is invalid</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialisation fails</exception>
        public async Task<VehicleDetails> GetVehicleDetailsAsync(string registration)
        {
            if (string.IsNullOrWhiteSpace(registration))
            {
                throw new ArgumentException("Registration cannot be empty", nameof(registration));
            }

            // Check cache first
            var cacheKey = $"vehicle_{registration.ToUpper()}";
            if (_cache.TryGetValue(cacheKey, out VehicleDetails? cachedDetails))
            {
                _logger.LogInformation("Cache hit for registration {Registration}", registration);
                return cachedDetails!;
            }

            // Cache miss - fetch from API
            _logger.LogInformation("Cache miss for registration {Registration}", registration);
            await EnsureAccessTokenAsync();

            // Set up API request headers
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _configuration["DvsaApi:ApiKey"]);

            var response = await _httpClient.GetAsync($"trade/vehicles/mot-tests?registration={registration}");
            response.EnsureSuccessStatusCode();

            var details = await response.Content.ReadFromJsonAsync<VehicleDetails>();
            if (details == null)
            {
                throw new InvalidOperationException("Failed to deserialise vehicle details");
            }

            // Cache the results
            _cache.Set(cacheKey, details, TimeSpan.FromMinutes(30));
            return details;
        }

        /// <summary>
        /// Ensures a valid access token is available for API requests
        /// </summary>
        /// <remarks>
        /// Implements token caching to avoid unnecessary token requests
        /// Uses OAuth client credentials flow
        /// </remarks>
        private async Task EnsureAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken))
            {
                return;
            }

            // Prepare token request parameters
            var tokenParams = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _configuration["DvsaApi:ClientId"]!,
                ["client_secret"] = _configuration["DvsaApi:ClientSecret"]!,
                ["scope"] = _configuration["DvsaApi:ScopeUrl"]!
            };

            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, _configuration["DvsaApi:TokenUrl"])
            {
                Content = new FormUrlEncodedContent(tokenParams)
            };

            var tokenResponse = await _httpClient.SendAsync(tokenRequest);
            tokenResponse.EnsureSuccessStatusCode();

            var tokenData = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>();
            _accessToken = tokenData.GetProperty("access_token").GetString();
        }
    }
}
