using MotChecker.Models;

namespace MotChecker.Services
{
    /// <summary>
    /// Mock implementation of IVehicleService for development and testing
    /// </summary>
    public class MockVehicleService : IVehicleService
    {
        private readonly ILogger<MockVehicleService> _logger;
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

        public MockVehicleService(ILogger<MockVehicleService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

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
