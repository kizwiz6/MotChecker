using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using MotChecker.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace MotChecker.Services
{
    /// <summary>
    /// Implementation of IVehicleService that retrieves vehicle MOT data from the DVSA API
    /// </summary>
    public class VehicleService : IVehicleService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<VehicleService> _logger;
        private readonly IConfiguration _configuration;

        public VehicleService(HttpClient httpClient, IMemoryCache cache, ILogger<VehicleService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        private void ConfigureHttpClient()
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _configuration["DvsaApi:ApiKey"]);
        }

        /// <summary>
        /// Retrieves detailed vehicle information including MOT status by registration number
        /// </summary>
        /// <param name="registration">The vehicle registration number to query</param>
        /// <returns>A task containing the vehicle details if found</returns>
        public async Task<VehicleDetails> GetVehicleDetailsAsync(string registration)
        {
            if (string.IsNullOrWhiteSpace(registration))
            {
                throw new ArgumentException("Registration number cannot be empty", nameof(registration));
            }

            var cacheKey = $"vehicle_{registration.ToUpper()}";
            if (_cache.TryGetValue(cacheKey, out VehicleDetails? cachedDetails))
            {
                _logger.LogInformation("Cache hit for registration: {Registration}", registration);
                return cachedDetails!;
            }

            try
            {
                _logger.LogInformation("Fetching vehicle details for registration: {Registration}", registration);

                await EnsureAccessTokenAsync();
                ConfigureHttpClient();

                var response = await _httpClient.GetAsync($"vehicles/{registration}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API error: {StatusCode} - {Message}",
                        response.StatusCode, errorMessage);
                    throw new HttpRequestException($"Failed to retrieve vehicle details: {response.StatusCode}");
                }

                var vehicleDetails = await response.Content.ReadFromJsonAsync<VehicleDetails>();

                if (vehicleDetails == null)
                {
                    throw new InvalidOperationException("Failed to deserialise vehicle details");
                }

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30));

                _cache.Set(cacheKey, vehicleDetails, cacheEntryOptions);
                return vehicleDetails;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while fetching vehicle details for {Registration}", registration);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching vehicle details for {Registration}", registration);
                throw;
            }
        }

        private async Task EnsureAccessTokenAsync()
        {
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
            var accessToken = tokenData.GetProperty("access_token").GetString();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
    }
}
