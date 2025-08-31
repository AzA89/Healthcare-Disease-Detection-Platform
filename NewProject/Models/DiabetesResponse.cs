using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace NewProject.Models
{
    public class DiabetesResponse
    {
        [JsonPropertyName("original_class")]
        public string OriginalClass { get; set; }
        
        [JsonPropertyName("binary_result")]
        public string BinaryResult { get; set; }
        
        [JsonPropertyName("risk_level")]
        public string RiskLevel { get; set; }
        
        [JsonPropertyName("clinical_status")]
        public string ClinicalStatus { get; set; }
        
        [JsonPropertyName("probabilities")]
        public Dictionary<string, float> Probabilities { get; set; }
        
        [JsonPropertyName("recommendation")]
        public string Recommendation { get; set; }
        
        [JsonPropertyName("key_indicators")]
        public Dictionary<string, float> KeyIndicators { get; set; }
        
        [JsonPropertyName("status")]
        public string Status { get; set; }
        
        [JsonIgnore]
        public bool Success { get; set; }
        
        [JsonIgnore]
        public string Message { get; set; } = string.Empty;
        
        [JsonIgnore]
        public string Prediction 
        { 
            get => BinaryResult ?? (RiskLevel == "Negative" || RiskLevel == "Low" ? "NEGATIVE" : "POSITIVE");
            set { /* Compatibility setter */ }
        }
        
        [JsonIgnore]
        public float Probability
        {
            get
            {
                if (Probabilities != null && Probabilities.ContainsKey("Y"))
                {
                    return Probabilities["Y"];
                }
                if (RiskLevel == "High") return 0.85f;
                if (RiskLevel == "Medium") return 0.65f;
                if (RiskLevel == "Low") return 0.25f;
                return 0.1f;
            }
            set { /* Compatibility setter */ }
        }
    }
}