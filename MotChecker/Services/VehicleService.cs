using MotChecker.Models;
using MotChecker.Services;

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
            // Remove all whitespace from registration
            var cleanRegistration = registration.Replace(" ", "");

            var response = await _httpClient.GetAsync($"api/vehicles/{registration}");
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