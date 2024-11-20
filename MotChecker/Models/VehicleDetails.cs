using System.Text.Json.Serialization;

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
        [JsonPropertyName("registration")]
        public string Registration { get; set; } = string.Empty;

        /// <summary>
        /// The manufacturer of the vehicle 
        /// </summary>
        [JsonPropertyName("make")]
        public string Make { get; set; } = string.Empty;

        /// <summary>
        /// The model name/number of the vehicle
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// The colour of the vehicle
        /// </summary>
        [JsonPropertyName("colour")]
        public string Colour { get; set; } = string.Empty;

        /// <summary>
        /// The date when the current MOT certificate expires
        /// </summary>
        [JsonPropertyName("motExpiryDate")]
        public DateTime MotExpiryDate { get; set; }

        /// <summary>
        /// The recorded mileage at the most recent MOT test
        /// </summary>
        [JsonPropertyName("mileageAtLastMot")]
        public int MileageAtLastMot { get; set; }

    }
}
