using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using NewProject.Models;
using NewProject.Services;
using System.Linq;

namespace NewProject.Controllers
{
    public class SymptomController : Controller
    {
        private readonly IDiseaseDetectionService _diseaseDetectionService;
        private readonly ILogger<SymptomController> _logger;

        public SymptomController(IDiseaseDetectionService diseaseDetectionService, ILogger<SymptomController> logger)
        {
            _diseaseDetectionService = diseaseDetectionService;
            _logger = logger;
        }

        public async Task<IActionResult> Checker()
        {
            // Pre-fetch symptoms for the view
            try
            {
                var symptoms = await _diseaseDetectionService.GetAllSymptomsAsync();
                ViewBag.AllSymptoms = symptoms;
                
                // Log symptoms for debugging
                _logger.LogInformation($"Loaded {symptoms?.Count ?? 0} symptoms");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching symptoms");
                ViewBag.ErrorMessage = "Unable to load symptoms. The API server may not be running. Please make sure the API is accessible at " + 
                                       _diseaseDetectionService.GetApiBaseUrl();
                ViewBag.AllSymptoms = new List<string>();
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Predict([FromBody] SymptomPredictionRequest request)
        {
            if (request == null || request.Symptoms == null || request.Symptoms.Count == 0)
            {
                return BadRequest(new { error = "No symptoms provided" });
            }

            try
            {
                _logger.LogInformation($"Predicting disease based on {request.Symptoms.Count} symptoms");
                Debug.WriteLine($"Predicting disease with symptoms: {string.Join(", ", request.Symptoms)}");

                // Check for any invalid symptoms that might cause API errors
                var validSymptoms = await _diseaseDetectionService.GetAllSymptomsAsync();
                var invalidSymptoms = request.Symptoms.Where(s => !validSymptoms.Contains(s)).ToList();
                
                if (invalidSymptoms.Count > 0)
                {
                    string errorMessage = $"Invalid symptoms: {string.Join(", ", invalidSymptoms)}";
                    _logger.LogWarning(errorMessage);
                    return BadRequest(new { 
                        error = errorMessage,
                        detail = new { 
                            error = errorMessage, 
                            status = "error"
                        }
                    });
                }

                var result = await _diseaseDetectionService.PredictDiseaseAsync(request.Symptoms, request.TopN ?? 3);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error predicting disease");
                
                // Return more specific error message for debugging
                var errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += " - " + ex.InnerException.Message;
                }
                
                return StatusCode(500, new { 
                    error = $"API error: {errorMessage}",
                    apiUrl = _diseaseDetectionService.GetApiBaseUrl() 
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSymptoms()
        {
            try
            {
                _logger.LogInformation("Fetching all symptoms");
                var symptoms = await _diseaseDetectionService.GetAllSymptomsAsync();
                return Ok(new { symptoms });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching symptoms");
                
                // Return more specific error message for debugging
                var errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += " - " + ex.InnerException.Message;
                }
                
                return StatusCode(500, new { 
                    error = $"API error: {errorMessage}",
                    apiUrl = _diseaseDetectionService.GetApiBaseUrl() 
                });
            }
        }
    }
}