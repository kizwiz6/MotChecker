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
    public class DvsaVehicleService : IVehicleService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly ILogger<DvsaVehicleService> _logger;
        private string? _accessToken;

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

        public async Task<VehicleDetails> GetVehicleDetailsAsync(string registration)
        {
            if (string.IsNullOrWhiteSpace(registration))
            {
                throw new ArgumentException("Registration cannot be empty", nameof(registration));
            }

            var cacheKey = $"vehicle_{registration.ToUpper()}";
            if (_cache.TryGetValue(cacheKey, out VehicleDetails? cachedDetails))
            {
                return cachedDetails!;
            }

            await EnsureAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _configuration["DvsaApi:ApiKey"]);

            var response = await _httpClient.GetAsync($"trade/vehicles/mot-tests?registration={registration}");
            response.EnsureSuccessStatusCode();

            var details = await response.Content.ReadFromJsonAsync<VehicleDetails>();
            if (details == null)
            {
                throw new InvalidOperationException("Failed to deserialise vehicle details");
            }

            _cache.Set(cacheKey, details, TimeSpan.FromMinutes(30));
            return details;
        }

        private async Task EnsureAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken))
            {
                return;
            }

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
