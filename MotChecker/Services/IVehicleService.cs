namespace MotChecker.Services
{
    /// <summary>
    /// Defines the contract for retrieving vehicle MOT information
    /// </summary>
    public interface IVehicleService
    {
        /// <summary>
        /// Retrieves detailed vehicle information including MOT status by registration number
        /// </summary>
        /// <param name="registration">The vehicle registration number to query</param>
        /// <returns>A task containing the vehicle details if found</returns>
        Task<Models.VehicleDetails> GetVehicleDetailsAsync(string registration);
    }
}
