using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewProject.Models;
using NewProject.Services;
using System.Text;

namespace NewProject.Controllers
{
    public class DiabetesController : Controller
    {
        private readonly IDiseaseDetectionService _diseaseDetectionService;
        private readonly ILogger<DiabetesController> _logger;

        public DiabetesController(IDiseaseDetectionService diseaseDetectionService, ILogger<DiabetesController> logger)
        {
            _diseaseDetectionService = diseaseDetectionService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View("~/Views/Diabetes/Index.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Predict(DiabetesRequest request)
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
                _logger.LogWarning($"Diabetes model validation failed: {errors}");
                return View("~/Views/Diabetes/Result.cshtml", request);
            }

            try
            {
                _logger.LogInformation("Processing diabetes prediction request");
                
                // Log request data for debugging
                _logger.LogInformation($"Diabetes request: Age={request.Age}, Gender={request.Gender}, HbA1c={request.HbA1c}");
                
                var result = await _diseaseDetectionService.PredictDiabetesAsync(request);
                
                // Create prediction result with additional user-friendly information
                var response = new DiabetesResponse
                {
                    Success = true,
                    OriginalClass = result.OriginalClass,
                    BinaryResult = result.BinaryResult,
                    RiskLevel = result.RiskLevel.Replace("_", " "),
                    ClinicalStatus = result.ClinicalStatus,
                    Probabilities = result.Probabilities,
                    Recommendation = result.Recommendation,
                    KeyIndicators = result.KeyIndicators,
                    Status = result.Status,
                    Message = result.RiskLevel == "HIGH_RISK"
                        ? "Your results suggest a higher risk of diabetes. Please consult with a healthcare professional."
                        : (result.RiskLevel == "MODERATE_RISK"
                            ? "Your results suggest a moderate risk of diabetes. Consider discussing with a healthcare professional."
                            : "Your results suggest a low risk of diabetes.")
                };
                
                // Store patient data in ViewBag for the Result view
                ViewBag.PatientData = request;
                
                // Calculate and store health status indicators
                ViewBag.HbA1cStatus = GetHbA1cStatus(request.HbA1c);
                ViewBag.BMIStatus = GetBMIStatus(request.BMI);
                ViewBag.CholesterolStatus = GetCholesterolStatus(request.Chol);
                ViewBag.HDLStatus = GetHDLStatus(request.HDL);
                ViewBag.LDLStatus = GetLDLStatus(request.LDL);
                ViewBag.TGStatus = GetTGStatus(request.TG);
                
                _logger.LogInformation("Diabetes prediction completed successfully");
                return View("~/Views/Diabetes/Result.cshtml", response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error predicting diabetes");
                ModelState.AddModelError("", $"Error communicating with the AI service: {ex.Message}");
                ViewBag.ApiErrorDetails = ex.StackTrace;
                return View("~/Views/Diabetes/Index.cshtml", request);
            }
        }

        // Helper methods to determine health parameter statuses
        private string GetHbA1cStatus(float hbA1c)
        {
            if (hbA1c < 5.7) return "Normal";
            if (hbA1c < 6.5) return "Prediabetic range";
            return "Diabetic range";
        }

        private string GetBMIStatus(float bmi)
        {
            if (bmi < 18.5) return "Underweight";
            if (bmi < 25) return "Normal weight";
            if (bmi < 30) return "Overweight";
            return "Obese";
        }

        private string GetCholesterolStatus(int chol)
        {
            if (chol < 200) return "Desirable";
            if (chol < 240) return "Borderline high";
            return "High";
        }

        private string GetHDLStatus(int hdl)
        {
            if (hdl < 40) return "Low (increased risk)";
            if (hdl >= 60) return "High (protective)";
            return "Average";
        }

        private string GetLDLStatus(int ldl)
        {
            if (ldl < 100) return "Optimal";
            if (ldl < 130) return "Near optimal";
            if (ldl < 160) return "Borderline high";
            if (ldl < 190) return "High";
            return "Very high";
        }

        private string GetTGStatus(int tg)
        {
            if (tg < 150) return "Normal";
            if (tg < 200) return "Borderline high";
            return "High";
        }
    }
} 