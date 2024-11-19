using Microsoft.AspNetCore.Components;
using MotChecker.Models;
using MotChecker.Services;
using System.ComponentModel.DataAnnotations;

namespace MotChecker.Pages;

/// <summary>
/// Home page component for MOT History Checker
/// </summary>
public partial class Home : ComponentBase
{
    [Inject]
    public IVehicleService VehicleService { get; set; } = default!;

    private readonly SearchModel searchModel = new();
    private VehicleDetails? vehicleDetails;
    private string? errorMessage;
    private bool isLoading;

    /// <summary>
    /// Handles input changes for the registration field
    /// </summary>
    /// <param name="e">Change event arguments containing the new value</param>

    private void HandleInput(ChangeEventArgs e)
    {
        if (e.Value is string value)
        {
            searchModel.Registration = value.ToUpper();
            StateHasChanged();
        }
    }

    /// <summary>
    /// Handles the search form submission
    /// </summary>
    private async Task HandleSearch()
    {
        try
        {
            isLoading = true;
            errorMessage = null;
            vehicleDetails = null;

            vehicleDetails = await VehicleService.GetVehicleDetailsAsync(searchModel.CleanRegistration);
        }
        catch (Exception ex)
        {
            errorMessage = "Unable to retrieve vehicle details. Please try again.";
        }
        finally
        {
            isLoading = false;
        }
    }

    /// <summary>
    /// Model class for vehicle registration search
    /// </summary>
    public class SearchModel
    {
        private string _registration = string.Empty;

        [Required(ErrorMessage = "Please enter a registration number")]
        [RegularExpression(@"^[A-Z0-9 ]{1,11}$",
            ErrorMessage = "Please enter a valid UK registration number")]
        public string Registration
        {
            get => _registration;
            set => _registration = value?.ToUpper() ?? string.Empty;
        }

        public string CleanRegistration => Registration.Replace(" ", "");
    }
}