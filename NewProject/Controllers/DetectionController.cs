using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.Json;
using NewProject.Models;
using NewProject.Services;
using System.Text;

namespace NewProject.Controllers
{
    public class DetectionController : Controller
    {
        private readonly IDiseaseDetectionService _diseaseDetectionService;
        private readonly ILogger<DetectionController> _logger;
        
        public DetectionController(IDiseaseDetectionService diseaseDetectionService, ILogger<DetectionController> logger)
        {
            _diseaseDetectionService = diseaseDetectionService;
            _logger = logger;
        }
        
            public IActionResult Index()
            {
                ViewBag.ApiBaseUrl = _diseaseDetectionService.GetApiBaseUrl();
                
                // Create a new ViewModel with properly initialized properties
                var viewModel = new DetectionViewModel
                {
                    ErrorMessage = "",
                    ApiErrorDetails = "",
                    HeartPredictionResult = new HeartDiseaseResponse(),
                    DiabetesPredictionResult = new DiabetesResponse()
                };
                
                return View("~/Views/Detection/Index.cshtml", viewModel);
            }
            // Handle a direct request to heart disease detection
            public IActionResult HeartDisease()
            {
                return RedirectToAction("Index", "HeartDisease");
            }

            // Handle a direct request to diabetes detection
            public IActionResult Diabetes() 
            {
                return RedirectToAction("Index", "Diabetes");
            }

        // Helper method to ensure model properties are never null
        private void EnsureModelInitialized(DetectionViewModel model)
        {
            if (model.HeartPredictionResult == null)
                model.HeartPredictionResult = new HeartDiseaseResponse();
                
            if (model.DiabetesPredictionResult == null)
                model.DiabetesPredictionResult = new DiabetesResponse();
                
            if (model.ErrorMessage == null)
                model.ErrorMessage = "";
                
            if (model.ApiErrorDetails == null)
                model.ApiErrorDetails = "";
        }

        [HttpPost]
        public async Task<IActionResult> Index(DetectionViewModel model, string submitButton)
        {
            // Always pass the API URL to the view for debugging purposes
            ViewBag.ApiBaseUrl = _diseaseDetectionService.GetApiBaseUrl();
            
            // Ensure model properties are never null
            EnsureModelInitialized(model);

            // Determine which form was submitted
            if (submitButton == "heart")
            {
                model.ActiveTab = "heart";
                
                if (!ModelState.IsValid)
                {
                    var errors = new StringBuilder();
                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {
                            errors.AppendLine($"{state.Key}: {error.ErrorMessage}");
                        }
                    }
                    _logger.LogWarning($"Heart model validation failed: {errors}");
                    model.ErrorMessage = $"Please correct the form errors: {errors}";
                    EnsureModelInitialized(model);
                    return View(model);
                }
                
                try
                {
                    _logger.LogInformation("Processing heart disease prediction request");
                    
                    // Create API request model
                    var request = new HeartDiseaseRequest
                    {
                        Age = model.HeartModel.Age,
                        Sex = model.HeartModel.Sex,
                        Cp = model.HeartModel.Cp,
                        Trestbps = model.HeartModel.Trestbps,
                        Chol = model.HeartModel.Chol,
                        Fbs = model.HeartModel.Fbs,
                        Restecg = model.HeartModel.Restecg,
                        Thalach = model.HeartModel.Thalach,
                        Exang = model.HeartModel.Exang,
                        Oldpeak = model.HeartModel.Oldpeak,
                        Slope = model.HeartModel.Slope,
                        Ca = model.HeartModel.Ca,
                        Thal = model.HeartModel.Thal
                    };
                    
                    // Log request data for debugging
                    var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                    _logger.LogInformation($"Heart disease request data: {requestJson}");
                    
                    // Store submitted data for display
                    TempData["HeartPredictionData"] = requestJson;
                    
                    // Call API
                    _logger.LogInformation("Calling heart disease prediction API");
                    var result = await _diseaseDetectionService.PredictHeartDiseaseAsync(request);
                    _logger.LogInformation($"Heart disease API response: Prediction={result.Prediction}, Probability={result.Probability}");
                    
                    // Create prediction result
                    model.HeartPredictionResult = new HeartDiseaseResponse
                    {
                        Success = true,
                        Prediction = result.Prediction,
                        Probability = result.Probability,
                        Message = result.Probability >= 0.5 
                            ? "Your results suggest a higher risk of heart disease. Please consult with a healthcare professional."
                            : (result.Probability >= 0.25 
                                ? "Your results suggest a moderate risk of heart disease. Consider discussing with a healthcare professional."
                                : "Your heart appears to be healthy based on the provided information.")
                    };
                    
                    _logger.LogInformation("Heart disease prediction completed successfully");
                    EnsureModelInitialized(model);
                    return View(model);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "API error during heart disease prediction");
                    model.ErrorMessage = $"Error communicating with the AI service: {ex.Message}";
                    model.ApiErrorDetails = "Please ensure the API server is running at the correct URL and check the logs for more details.";
                    EnsureModelInitialized(model);
                    return View(model);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "JSON parsing error during heart disease prediction");
                    model.ErrorMessage = "Error processing the AI service response. The data format was unexpected.";
                    model.ApiErrorDetails = ex.Message;
                    EnsureModelInitialized(model);
                    return View(model);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "General error during heart disease prediction");
                    model.ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                    model.ApiErrorDetails = ex.StackTrace;
                    EnsureModelInitialized(model);
                    return View(model);
                }
            }
            else if (submitButton == "diabetes")
            {
                model.ActiveTab = "diabetes";
                
                if (!ModelState.IsValid)
                {
                    var errors = new StringBuilder();
                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {
                            errors.AppendLine($"{state.Key}: {error.ErrorMessage}");
                        }
                    }
                    _logger.LogWarning($"Diabetes model validation failed: {errors}");
                    model.ErrorMessage = $"Please correct the form errors: {errors}";
                    EnsureModelInitialized(model);
                    return View(model);
                }
                
                try
                {
                    _logger.LogInformation("Processing diabetes prediction request");
                    
                    // Create API request model
                    var request = new DiabetesRequest
                    {
                        Gender = model.DiabetesModel.Gender,
                        Age = model.DiabetesModel.Age,
                        Urea = model.DiabetesModel.Urea,
                        Cr = model.DiabetesModel.Cr,
                        HbA1c = model.DiabetesModel.HbA1c,
                        Chol = model.DiabetesModel.Chol,
                        TG = model.DiabetesModel.TG,
                        HDL = model.DiabetesModel.HDL,
                        LDL = model.DiabetesModel.LDL,
                        VLDL = model.DiabetesModel.VLDL,
                        BMI = model.DiabetesModel.BMI
                    };
                    
                    // Log request data for debugging
                    var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                    _logger.LogInformation($"Diabetes request data: {requestJson}");
                    
                    // Store submitted data for display
                    TempData["DiabetesPredictionData"] = requestJson;
                    
                    // Call API
                    _logger.LogInformation("Calling diabetes prediction API");
                    var result = await _diseaseDetectionService.PredictDiabetesAsync(request);
                    _logger.LogInformation($"Diabetes API response: Prediction={result.Prediction}, Probability={result.Probability}, RiskLevel={result.RiskLevel}");
                    
                    // Create prediction result
                    model.DiabetesPredictionResult = new DiabetesResponse
                    {
                        Success = true,
                        Prediction = result.Prediction,
                        Probability = result.Probability,
                        RiskLevel = result.RiskLevel.Replace("_", " ").ToTitleCase(),
                        Message = result.RiskLevel == "HIGH_RISK"
                            ? "Your results suggest a higher risk of diabetes. Please consult with a healthcare professional."
                            : (result.RiskLevel == "MODERATE_RISK"
                                ? "Your results suggest a moderate risk of diabetes. Consider discussing with a healthcare professional."
                                : "Your results suggest a low risk of diabetes.")
                    };
                    
                    _logger.LogInformation("Diabetes prediction completed successfully");
                    EnsureModelInitialized(model);
                    return View(model);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "API error during diabetes prediction");
                    model.ErrorMessage = $"Error communicating with the AI service: {ex.Message}";
                    model.ApiErrorDetails = "Please ensure the API server is running at the correct URL and check the logs for more details.";
                    EnsureModelInitialized(model);
                    return View(model);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "JSON parsing error during diabetes prediction");
                    model.ErrorMessage = "Error processing the AI service response. The data format was unexpected.";
                    model.ApiErrorDetails = ex.Message;
                    EnsureModelInitialized(model);
                    return View(model);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "General error during diabetes prediction");
                    model.ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                    model.ApiErrorDetails = ex.StackTrace;
                    EnsureModelInitialized(model);
                    return View(model);
                }
            }
            
            // Default view
            return View(model);
        }
        
        [HttpPost]
        public async Task<IActionResult> PredictHeartDisease([FromBody] HeartDiseaseRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = new StringBuilder();
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        errors.AppendLine($"{state.Key}: {error.ErrorMessage}");
                    }
                }
                _logger.LogWarning($"Heart disease API validation failed: {errors}");
                return BadRequest(new { error = errors.ToString() });
            }
            
            try
            {
                _logger.LogInformation("API endpoint: Predicting heart disease");
                var result = await _diseaseDetectionService.PredictHeartDiseaseAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API endpoint: Error predicting heart disease");
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}", details = ex.StackTrace });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> PredictDiabetes([FromBody] DiabetesRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = new StringBuilder();
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        errors.AppendLine($"{state.Key}: {error.ErrorMessage}");
                    }
                }
                _logger.LogWarning($"Diabetes API validation failed: {errors}");
                return BadRequest(new { error = errors.ToString() });
            }
            
            try
            {
                _logger.LogInformation("API endpoint: Predicting diabetes");
                var result = await _diseaseDetectionService.PredictDiabetesAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API endpoint: Error predicting diabetes");
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}", details = ex.StackTrace });
            }
        }
    }
    
    public static class StringExtensions
    {
        public static string ToTitleCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;
                
            var words = str.ToLower().Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (string.IsNullOrEmpty(words[i]))
                    continue;
                    
                words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
            }
            
            return string.Join(" ", words);
        }
    }
}