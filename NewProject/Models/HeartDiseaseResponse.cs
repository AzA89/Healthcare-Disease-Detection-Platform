using System.Text.Json.Serialization;

namespace NewProject.Models
{
    public class HeartDiseaseResponse
    {
        [JsonPropertyName("prediction")]
        public string Prediction { get; set; } = string.Empty;
        
        [JsonPropertyName("probability")]
        public double Probability { get; set; }
        
        [JsonIgnore]
        public bool Success { get; set; }
        
        [JsonIgnore]
        public string Message { get; set; } = string.Empty;
    }
}