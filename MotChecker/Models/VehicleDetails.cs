namespace MotChecker.Models
{
    /// <summary>
    /// Represents detailed information about a vehicle and its MOT status
    /// </summary>
    public class VehicleDetails
    {
        /// <summary>
        /// The vehicle's registration number
        /// </summary>
        public string Registration { get; set; } = string.Empty;
        
        /// <summary>
        /// The manufacturer of the vehicle 
        /// </summary>
        public string Make { get; set; } = string.Empty;
        
        /// <summary>
        /// The model name/number of the vehicle
        /// </summary>
        public string Model { get; set; } = string.Empty;
        
        /// <summary>
        /// The colour of the vehicle
        /// </summary>
        public string Colour { get; set; } = string.Empty;
        
        /// <summary>
        /// The date when the current MOT certificate expires
        /// </summary>
        public DateTime NotExpiryDate { get; set; }

        /// <summary>
        /// The recorded mileage at the most recent MOT test
        /// </summary>
        public int MileageAtLastMot { get; set; }

    }
}
