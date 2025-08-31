using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewProject.Models;
using NewProject.Services;
using System.Text;

namespace NewProject.Controllers
{
    public class HeartDiseaseController : Controller
    {
        private readonly IDiseaseDetectionService _diseaseDetectionService;
        private readonly ILogger<HeartDiseaseController> _logger;

        public HeartDiseaseController(IDiseaseDetectionService diseaseDetectionService, ILogger<HeartDiseaseController> logger)
        {
            _diseaseDetectionService = diseaseDetectionService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Use absolute path to ensure view is found regardless of routing
            return View("~/Views/Heart/Index.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Predict(HeartDiseaseRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state in Heart Disease prediction");
                    return View("~/Views/Heart/Index.cshtml", request);
                }

                _logger.LogInformation("Processing heart disease prediction");
                
                // Store the patient data in ViewBag for display on the results page
                ViewBag.PatientData = request;
                
                // Calculate status values for display
                ViewBag.BPStatus = GetBloodPressureStatus(request.Trestbps);
                ViewBag.CholesterolStatus = GetCholesterolStatus(request.Chol);
                ViewBag.HeartRateStatus = GetHeartRateStatus(request.Thalach);

                var result = await _diseaseDetectionService.PredictHeartDiseaseAsync(request);
                
                // Add a custom message based on prediction
                if (result.Prediction == "POSITIVE")
                {
                    result.Message = "Your results indicate an elevated risk of heart disease. We recommend consulting with a healthcare professional.";
                }
                else
                {
                    result.Message = "Your results suggest a lower risk of heart disease. Continue maintaining a heart-healthy lifestyle.";
                }

                _logger.LogInformation($"Heart disease prediction result: {result.Prediction} with probability {result.Probability:F2}");
                return View("~/Views/Heart/Result.cshtml", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing heart disease prediction");
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return View("~/Views/Heart/Result.cshtml", request);
            }
        }
        
        // Helper methods for status indicators
        private string GetBloodPressureStatus(int bp)
        {
            return bp < 120 ? "Normal" :
                   bp < 130 ? "Elevated" :
                   bp < 140 ? "Stage 1 Hypertension" : 
                   "Stage 2 Hypertension";
        }
        
        private string GetCholesterolStatus(int chol)
        {
            return chol < 200 ? "Desirable" :
                   chol < 240 ? "Borderline High" : 
                   "High";
        }
        
        private string GetHeartRateStatus(int heartRate)
        {
            // Simplified heart rate status (in a real app this would account for age)
            return heartRate < 100 ? "Normal" :
                   heartRate < 150 ? "Elevated" : 
                   "High";
        }
    }
}