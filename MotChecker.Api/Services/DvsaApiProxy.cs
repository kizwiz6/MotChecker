﻿using MotChecker.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MotChecker.Api.Services
{
    public class DvsaApiProxy
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _tokenClient;
        private string? _accessToken;
        private readonly ILogger<DvsaApiProxy> _logger;

        public DvsaApiProxy(HttpClient httpClient, IConfiguration configuration, ILogger<DvsaApiProxy> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _tokenClient = new HttpClient();
            _logger = logger;
        }

        public async Task<VehicleDetails> GetVehicleDetailsAsync(string registration)
        {
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

        private async Task EnsureAccessTokenAsync()
        {
            var tokenParams = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _configuration["DvsaApi:ClientId"]!,
                ["client_secret"] = _configuration["DvsaApi:ClientSecret"]!,
                ["scope"] = _configuration["DvsaApi:ScopeUrl"]!
            };

            var tokenRequest = new FormUrlEncodedContent(tokenParams);
            var response = await _tokenClient.PostAsync(_configuration["DvsaApi:TokenUrl"], tokenRequest);
            response.EnsureSuccessStatusCode();

            var tokenData = await response.Content.ReadFromJsonAsync<JsonElement>();
            _accessToken = tokenData.GetProperty("access_token").GetString();
        }
    }
}
