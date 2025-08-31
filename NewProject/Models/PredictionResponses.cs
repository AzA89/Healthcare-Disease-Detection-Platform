using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NewProject.Models
{
    public class PrecautionItem
    {
        [JsonPropertyName("disease")]
        public string Disease { get; set; }
        
        [JsonPropertyName("probability")]
        public float Probability { get; set; }
        
        [JsonPropertyName("description")]
        public string Description { get; set; }
        
        [JsonPropertyName("precautions")]
        public List<string> Precautions { get; set; }
    }

    public class SymptomPredictionRequest
    {
        public List<string> Symptoms { get; set; }
        public int? TopN { get; set; }
    }

    public class SymptomResponse
    {
        [JsonPropertyName("predictions")]
        public List<PrecautionItem> Predictions { get; set; }
    }

    public class ErrorResponse
    {
        [JsonPropertyName("error")]
        public string Error { get; set; }
        
        [JsonPropertyName("status")]
        public string Status { get; set; } = "error";
    }
}