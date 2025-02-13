using Blazored.LocalStorage;
using MotChecker.Models;

namespace MotChecker.Services
{
    public class LocalStorageVehicleFavouritesService : IVehicleFavouritesService
    {
        private readonly ILocalStorageService _localStorage;
        private const string STORAGE_KEY = "saved_vehicles";

        public LocalStorageVehicleFavouritesService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task<IEnumerable<SavedVehicle>> GetSavedVehiclesAsync()
        {
            var vehicles = await _localStorage.GetItemAsync<List<SavedVehicle>>(STORAGE_KEY);
            return vehicles ?? new List<SavedVehicle>();
        }

        public async Task RemoveVehicleAsync(string registration)
        {
            var vehicles = (await GetSavedVehiclesAsync()).ToList();
            vehicles.RemoveAll(v => v.Registration == registration);
            await _localStorage.SetItemAsync(STORAGE_KEY, vehicles);
        }

        public async Task<bool> IsVehicleSavedAsync(string registration)
        {
            var vehicles = await GetSavedVehiclesAsync();
            return vehicles.Any(v => v.Registration == registration);
        }

        public async Task SaveVehicleAsync(SavedVehicle vehicle)
        {
            var vehicles = (await GetSavedVehiclesAsync()).ToList();
            var existing = vehicles.FirstOrDefault(v => v.Registration == vehicle.Registration);

            if (existing == null)
            {
                vehicle.DateSaved = DateTime.Now;
                vehicles.Add(vehicle);
            }
            else
            {
                existing.LastChecked = DateTime.Now;
            }

            await _localStorage.SetItemAsync(STORAGE_KEY, vehicles);
        }
    }
}
