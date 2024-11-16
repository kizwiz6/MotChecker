using Microsoft.Extensions.Caching.Memory;
using MotChecker.Models;

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
        }
    }
}
