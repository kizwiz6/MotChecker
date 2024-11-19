using System.Text.Json.Serialization;

namespace MotChecker.Api.Models
{
    public class VehicleDetails
    {
        [JsonPropertyName("registration")]
        public string Registration { get; set; } = string.Empty;
        [JsonPropertyName("make")]
        public string Make { get; set; } = string.Empty;
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;
        [JsonPropertyName("primaryColour")]
        public string Colour { get; set; } = string.Empty;
        [JsonPropertyName("expiryDate")]
        public DateTime MotExpiryDate { get; set; }
        [JsonPropertyName("odometerValue")]
        public int MileageAtLastMot { get; set; }
    }
}
