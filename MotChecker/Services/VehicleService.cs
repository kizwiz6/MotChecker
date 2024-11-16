using MotChecker.Models;

namespace MotChecker.Services
{
    /// <summary>
    /// Implementation of IVehicleService that retrieves vehicle MOT data from the DVSA API
    /// </summary>
    public class VehicleService : IVehicleService
    {
        private readonly ILogger<VehicleService> _logger;

        public VehicleService(ILogger<VehicleService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves detailed vehicle information including MOT status by registration number
        /// </summary>
        /// <param name="registration">The vehicle registration number to query</param>
        /// <returns>A task containing the vehicle details if found</returns>
        public Task<VehicleDetails> GetVehicleDetailsAsync(string registration)
        {
            throw new NotImplementedException();
        }
    }
}
