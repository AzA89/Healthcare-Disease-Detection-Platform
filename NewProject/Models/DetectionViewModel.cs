using System.ComponentModel.DataAnnotations;

namespace NewProject.Models
{
    public class DetectionViewModel
    {
        public string ActiveTab { get; set; } = "heart";
        public string ErrorMessage { get; set; } = "";
        
        // Heart disease form model
        public HeartDiseaseRequest HeartModel { get; set; } = new HeartDiseaseRequest();
        
        // Diabetes form model
        public DiabetesRequest DiabetesModel { get; set; } = new DiabetesRequest();
        
        // Prediction results
        public HeartDiseaseResponse HeartPredictionResult { get; set; } = new HeartDiseaseResponse();
        public DiabetesResponse DiabetesPredictionResult { get; set; } = new DiabetesResponse();
        
        // For UI display
        public bool ShowDiabetesResults { get; set; } = false;
        public DiabetesRequest DiabetesRequest { get; set; } = new DiabetesRequest();
        public DiabetesResponse DiabetesResult { get; set; } = new DiabetesResponse();
        
        // Error handling properties
        public string ApiErrorDetails { get; set; } = "";
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    }
}