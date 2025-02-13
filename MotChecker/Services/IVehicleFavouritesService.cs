using MotChecker.Models;

namespace MotChecker.Services
{
    public interface IVehicleFavouritesService
    {
        /// <summary>
        /// Get all saved vehicles
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<SavedVehicle>> GetSavedVehiclesAsync();

        /// <summary>
        /// Save a vehicle to favourites
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        Task SaveVehicleAsync(SavedVehicle vehicle);

        /// <summary>
        /// Remove a vehicle from favourites
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        Task RemoveVehicleAsync(string registration);

        /// <summary>
        /// Check if a vehicle is saved
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        Task<bool> IsVehicleSavedAsync(string registration);
    }
}
