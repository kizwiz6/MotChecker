using MotChecker.Models;
using MotChecker.Services;
using System.Net.Http.Headers;
using System.Net.Http.Json;

/// <summary>
/// Service to handle vehicle MOT data retrieval from API
/// </summary>
public class VehicleService : IVehicleService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VehicleService> _logger;

    public VehicleService(HttpClient httpClient, ILogger<VehicleService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves vehicle details from the API
    /// </summary>
    public async Task<VehicleDetails> GetVehicleDetailsAsync(string registration)
    {
        try
        {
            // Add Accept header
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"api/vehicles/{registration}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);

            // Log response
            var content = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<VehicleDetails>()
                            ?? throw new InvalidOperationException("Failed to deserialise vehicle details");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching vehicle details for {Registration}", registration);
            throw;
        }
    }
}