using MotChecker.Models;

namespace MotChecker.Services
{
    /// <summary>
    /// Mock implementation of IVehicleService for development and testing purposes
    /// Provides predefined vehicle data without requiring external API calls
    /// </summary>
    public class MockVehicleService : IVehicleService
    {
        private readonly ILogger<MockVehicleService> _logger;

        /// <summary>
        /// In-memory mock database of vehicle details
        /// </summary>
        private readonly Dictionary<string, VehicleDetails> _mockDatabase = new()
        {
            ["LB11WXA"] = new VehicleDetails
            {
                Registration = "LB11WXA",
                Make = "Kia",
                Model = "Rio",
                Colour = "Silver",
                MotExpiryDate = DateTime.Now.AddMonths(6),
                MileageAtLastMot = 97988
            },
            ["BP71MLX"] = new VehicleDetails
            {
                Registration = "BP71MLX",
                Make = "Tesla",
                Model = "Model 3",
                Colour = "Red",
                MotExpiryDate = DateTime.Now.AddMonths(3),
                MileageAtLastMot = 44205
            }
        };

        /// <summary>
        /// Initialises a new instance of the MockVehicleService
        /// </summary>
        /// <param name="logger">Logger instance for diagnostic information</param>
        /// <exception cref="ArgumentNullException">Thrown when logger is null</exception>
        public MockVehicleService(ILogger<MockVehicleService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Simulates retrieving vehicle details by registration number
        /// </summary>
        /// <param name="registration">Vehicle registration number to look up</param>
        /// <returns>Vehicle details if found in mock database</returns>
        /// <remarks>
        /// Includes a simulated delay to mimic real API behavior
        /// Normalizes registration numbers by trimming whitespace and converting to uppercase
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown when registration is null or empty</exception>
        /// <exception cref="Exception">Thrown when vehicle is not found in mock database</exception>
        public async Task<VehicleDetails> GetVehicleDetailsAsync(string registration)
        {
            if (string.IsNullOrWhiteSpace(registration))
            {
                throw new ArgumentException("Registration number cannot be empty or whitespace.", nameof(registration));
            }

            _logger.LogInformation("Mock service retrieving details for: {Registration}", registration);

            // Simulate API delay
            await Task.Delay(1000);

            // Return mock data
            string normalisedReg = registration.ToUpper().Trim();
            if (!_mockDatabase.ContainsKey(normalisedReg))
            {
                throw new Exception("Vehicle not found");
            }

            return _mockDatabase[normalisedReg];
        }
    }
}
