namespace MotChecker.Models
{
    public class SavedVehicle
    {
        public string Registration { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public DateTime MotExpiryDate { get; set; }
        public DateTime DateSaved { get; set; }
        public DateTime LastChecked { get; set; }
    }
}
