using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NewProject.Models
{
    public class DiabetesRequest
    {
        [Range(0, 1, ErrorMessage = "Gender must be 0 (Male) or 1 (Female)")]
        [JsonPropertyName("Gender")]
        public int Gender { get; set; }
        
        [Range(1, 119, ErrorMessage = "Age must be between 1 and 119")]
        [JsonPropertyName("AGE")]
        public int Age { get; set; }
        
        [Range(0.01, 1000, ErrorMessage = "Urea must be a positive value")]
        [JsonPropertyName("Urea")]
        public float Urea { get; set; }
        
        [Range(0.01, 100, ErrorMessage = "Creatinine must be a positive value")]
        [JsonPropertyName("Cr")]
        public float Cr { get; set; }
        
        [Range(0.01, 15, ErrorMessage = "HbA1c must be between 0.01 and 15")]
        [JsonPropertyName("HbA1c")]
        public float HbA1c { get; set; }
        
        [Range(1, 800, ErrorMessage = "Cholesterol must be between 1 and 800")]
        [JsonPropertyName("Chol")]
        public int Chol { get; set; }
        
        [Range(1, 1000, ErrorMessage = "Triglycerides must be between 1 and 1000")]
        [JsonPropertyName("TG")]
        public int TG { get; set; }
        
        [Range(1, 200, ErrorMessage = "HDL must be between 1 and 200")]
        [JsonPropertyName("HDL")]
        public int HDL { get; set; }
        
        [Range(1, 600, ErrorMessage = "LDL must be between 1 and 600")]
        [JsonPropertyName("LDL")]
        public int LDL { get; set; }
        
        [Range(0.01, 200, ErrorMessage = "VLDL must be between 0.01 and 200")]
        [JsonPropertyName("VLDL")]
        public float VLDL { get; set; }
        
        [Range(0.01, 80, ErrorMessage = "BMI must be between 0.01 and 80")]
        [JsonPropertyName("BMI")]
        public float BMI { get; set; }
    }
} 