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

    /// <summary>
    /// Initialises a new instance of the VehicleService
    /// </summary>
    /// <param name="httpClient">HTTP client for API calls</param>
    /// <param name="logger">Logger instance</param>
    public VehicleService(HttpClient httpClient, ILogger<VehicleService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves vehicle details from the API, handling registration cleaning
    /// </summary>
    /// <param name="registration">Vehicle registration number (spaces allowed)</param>
    /// <returns>Vehicle details if found</returns>
    /// <exception cref="InvalidOperationException">Thrown when response deserialisation fails</exception>
    public async Task<VehicleDetails> GetVehicleDetailsAsync(string registration)
    {
        try
        {
            // Clean the registration by removing spaces and converting to uppercase
            var cleanRegistration = registration.Replace(" ", "").ToUpper();

            var request = new HttpRequestMessage(HttpMethod.Get,
                $"api/vehicles/{cleanRegistration}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);
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