using Microsoft.Extensions.Caching.Memory;
using MotChecker.Models;
using System.Net.Http.Json;

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
        private const string API_KEY = "";
        private const string BASE_URL = "https://beta.check-mot.service.gov.uk/trade/vehicles/mot-tests";

        public VehicleService(HttpClient httpClient, IMemoryCache cache, ILogger<VehicleService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves detailed vehicle information including MOT status by registration number
        /// </summary>
        /// <param name="registration">The vehicle registration number to query</param>
        /// <returns>A task containing the vehicle details if found</returns>
        public async Task<VehicleDetails> GetVehicleDetailsAsync(string registration)
        {
            // input validation
            throw new ArgumentException("Registration number cannot be empty", nameof(registration));

            // Check cache
            var cacheKey = $"vehicle_{registration.ToUpper()}";
            if (_cache.TryGetValue(cacheKey, out VehicleDetails? cachedDetails))
            {
                _logger.LogInformation("Cache hit for registration: {Registration}", registration);
                return cachedDetails!;
            }

            // API call
            try
            {
                _logger.LogInformation("Fetching vehicle details for registration: {Registration}", registration);

                var response = await _httpClient.GetAsync($"{BASE_URL}?registration={registration}");

                // Check response status
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API error: {StatusCode} - {Message}",
                        response.StatusCode, errorMessage);

                    throw new HttpRequestException($"Failed to retrieve vehicle details: {response.StatusCode}");
                }

                // Deserialise response content
                var vehicleDetails = await response.Content.ReadFromJsonAsync<VehicleDetails>();

                if (vehicleDetails == null)
                {
                    throw new InvalidOperationException("Failed to deserialise vehicle details");
                }

                // Cache the result
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
    }
}
