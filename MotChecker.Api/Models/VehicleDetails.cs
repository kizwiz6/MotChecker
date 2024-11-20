using System.Text.Json.Serialization;

namespace MotChecker.Api.Models
{
    /// <summary>
    /// Represents detailed information about a vehicle
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
        /// The model of the vehicle
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// The primary color of the vehicle
        /// </summary>
        [JsonPropertyName("primaryColour")]
        public string Colour { get; set; } = string.Empty;
        [JsonPropertyName("expiryDate")]

        /// <summary>
        /// The expiry date of the current MOT certificate
        /// </summary>
        public DateTime MotExpiryDate { get; set; }

        /// <summary>
        /// The recorded mileage at the last MOT test
        /// </summary>
        [JsonPropertyName("odometerValue")]
        public int MileageAtLastMot { get; set; }
    }
}
