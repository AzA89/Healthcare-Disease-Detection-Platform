namespace NewProject.Models
{
    public class PredictionResult
    {
        public bool Success { get; set; }
        public string Prediction { get; set; }
        public float Probability { get; set; }
        public string RiskLevel { get; set; }
        public string Message { get; set; }
    }
}