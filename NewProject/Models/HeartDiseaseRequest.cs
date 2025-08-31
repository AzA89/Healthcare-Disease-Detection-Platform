using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NewProject.Models
{
    public class HeartDiseaseRequest
    {
        [Range(25, 90, ErrorMessage = "Age must be between 25 and 90")]
        [JsonPropertyName("age")]
        public int Age { get; set; }
        
        [Range(0, 1, ErrorMessage = "Sex must be 0 (Female) or 1 (Male)")]
        [JsonPropertyName("sex")]
        public int Sex { get; set; }
        
        [Range(0, 3, ErrorMessage = "Chest pain type must be between 0 and 3")]
        [JsonPropertyName("cp")]
        public int Cp { get; set; }
        
        [Range(90, 200, ErrorMessage = "Resting blood pressure must be between 90 and 200 mm Hg")]
        [JsonPropertyName("trestbps")]
        public int Trestbps { get; set; }
        
        [Range(120, 570, ErrorMessage = "Cholesterol must be between 120 and 570 mg/dl")]
        [JsonPropertyName("chol")]
        public int Chol { get; set; }
        
        [Range(0, 1, ErrorMessage = "Fasting blood sugar must be 0 or 1")]
        [JsonPropertyName("fbs")]
        public int Fbs { get; set; }
        
        [Range(0, 2, ErrorMessage = "Resting ECG results must be between 0 and 2")]
        [JsonPropertyName("restecg")]
        public int Restecg { get; set; }
        
        [Range(60, 220, ErrorMessage = "Maximum heart rate must be between 60 and 220")]
        [JsonPropertyName("thalach")]
        public int Thalach { get; set; }
        
        [Range(0, 1, ErrorMessage = "Exercise induced angina must be 0 or 1")]
        [JsonPropertyName("exang")]
        public int Exang { get; set; }
        
        [Range(0.0, 6.0, ErrorMessage = "ST depression must be between 0.0 and 6.0")]
        [JsonPropertyName("oldpeak")]
        public float Oldpeak { get; set; }
        
        [Range(0, 2, ErrorMessage = "Slope must be between 0 and 2")]
        [JsonPropertyName("slope")]
        public int Slope { get; set; }
        
        [Range(0, 4, ErrorMessage = "Number of major vessels must be between 0 and 4")]
        [JsonPropertyName("ca")]
        public int Ca { get; set; }
        
        [Range(0, 3, ErrorMessage = "Thalassemia must be between 0 and 3")]
        [JsonPropertyName("thal")]
        public int Thal { get; set; }
    }
} 