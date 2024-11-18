using MotChecker.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MotChecker.Api.Services
{
    public class DvsaApiProxy
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private string? _accessToken;

        public DvsaApiProxy(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<VehicleDetails> GetVehicleDetailsAsync(string registration)
        {
            await EnsureAccessTokenAsync();

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _configuration["DvsaApi:ApiKey"]);

            var response = await _httpClient.GetAsync($"{_configuration["DvsaApi:BaseUrl"]}trade/vehicles/mot-tests?registration={registration}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<VehicleDetails>()
                ?? throw new InvalidOperationException("Failed to deserialize response");
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
            _accessToken = tokenData.GetProperty("access_token").GetString();
        }
    }
}
