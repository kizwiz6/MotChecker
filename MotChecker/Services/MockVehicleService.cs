﻿using MotChecker.Models;

namespace MotChecker.Services
{
    /// <summary>
    /// Mock implementation of IVehicleService for development and testing
    /// </summary>
    public class MockVehicleService : IVehicleService
    {
        private readonly ILogger<MockVehicleService> _logger;

        public MockVehicleService(ILogger<MockVehicleService> logger)
        {
            _logger = logger;
        }

        public async Task<VehicleDetails> GetVehicleDetailsAsync(string registration)
        {
            _logger.LogInformation("Mock service retrieving details for: {Registration}", registration);

            // Simulate API delay
            await Task.Delay(1000);

            // Return mock data
            return new VehicleDetails
            {
                Registration = registration.ToUpper(),
                Make = "Kia",
                Model = "Rio",
                Colour = "Silver",
                MotExpiryDate = DateTime.Now.AddMonths(6),
                MileageAtLastMot = 97420
            };
        }
    }
}
