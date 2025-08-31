using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using NewProject.Models;
using Microsoft.Extensions.Logging;

namespace NewProject.Services
{
    public class DiseaseDetectionService : IDiseaseDetectionService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly string _apiBaseUrl;
        private readonly ILogger<DiseaseDetectionService> _logger;

        public DiseaseDetectionService(IHttpClientFactory clientFactory, IConfiguration configuration, ILogger<DiseaseDetectionService> logger)
        {
            _clientFactory = clientFactory;
            _apiBaseUrl = configuration.GetValue<string>("DetectionApi:BaseUrl") ?? "http://localhost:8000";
            _logger = logger;
        }

        public string GetApiBaseUrl() => _apiBaseUrl;

        public async Task<object> PredictDiseaseAsync(List<string> symptoms, int topN = 3)
        {
            if (symptoms == null || symptoms.Count == 0)
            {
                throw new ArgumentException("Symptoms list cannot be empty");
            }

            var client = _clientFactory.CreateClient("DiseaseAPI");

            var requestData = new
            {
                symptoms = symptoms,
                top_n = topN
            };

            var jsonContent = JsonSerializer.Serialize(requestData);
            var content = new StringContent(
                jsonContent,
                Encoding.UTF8,
                "application/json");

            try
            {
                _logger.LogInformation($"Sending request to {_apiBaseUrl}/api/predict with symptoms: {string.Join(", ", symptoms)}");
                var response = await client.PostAsync($"{_apiBaseUrl}/api/predict", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"API Error: {response.StatusCode}, Content: {errorContent}");
                    throw new HttpRequestException($"API returned status code {response.StatusCode}: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"API Response: {responseContent}");
                return JsonSerializer.Deserialize<SymptomResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception in PredictDiseaseAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<List<string>> GetAllSymptomsAsync()
        {
            try
            {
                var client = _clientFactory.CreateClient("DiseaseAPI");
                _logger.LogInformation($"Getting symptoms from {_apiBaseUrl}/api/symptoms");
                var response = await client.GetAsync($"{_apiBaseUrl}/api/symptoms");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"API Error: {response.StatusCode}, Content: {errorContent}");
                    throw new HttpRequestException($"API returned status code {response.StatusCode}: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"API Response received, length: {responseContent.Length}");
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                try
                {
                    // First try to deserialize as a dictionary with symptoms key
                    var result = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(responseContent, options);
                    if (result != null && result.ContainsKey("symptoms"))
                    {
                        _logger.LogInformation($"Found {result["symptoms"].Count} symptoms in API response");
                        return result["symptoms"];
                    }
                    
                    // If that fails, try to deserialize as a list directly
                    return JsonSerializer.Deserialize<List<string>>(responseContent, options);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error deserializing symptoms response: {ex.Message}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception in GetAllSymptomsAsync: {ex.Message}");
                throw;
            }
        }
        
        public async Task<HeartDiseaseResponse> PredictHeartDiseaseAsync(HeartDiseaseRequest request)
        {
            var client = _clientFactory.CreateClient("DiseaseAPI");
            
            // Format data as a nested array as expected by the Python model
            // This matches how the model processes: input_data = [[age, sex, cp, ...]]
            var jsonContent = JsonSerializer.Serialize(new
            {
                data = new object[]
                {
                    new object[]
                    {
                        request.Age,
                        request.Sex,
                        request.Cp,
                        request.Trestbps,
                        request.Chol,
                        request.Fbs,
                        request.Restecg,
                        request.Thalach,
                        request.Exang,
                        request.Oldpeak,
                        request.Slope,
                        request.Ca,
                        request.Thal
                    }
                }
            }, new JsonSerializerOptions { 
                WriteIndented = true 
            });
            
            _logger.LogInformation($"Heart disease request data: {jsonContent}");
            
            var content = new StringContent(
                jsonContent,
                Encoding.UTF8,
                "application/json");
                
            try
            {
                _logger.LogInformation($"Sending request to {_apiBaseUrl}/api/heart-disease-prediction");
                
                var response = await client.PostAsync($"{_apiBaseUrl}/api/heart-disease-prediction", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"API Error: {response.StatusCode}, Content: {errorContent}");
                    throw new HttpRequestException($"API returned status code {response.StatusCode}: {errorContent}");
                }
                
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"API Heart disease response received: {responseContent}");
                
                return JsonSerializer.Deserialize<HeartDiseaseResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception in PredictHeartDiseaseAsync: {ex.Message}");
                throw;
            }
        }
        
        public async Task<DiabetesResponse> PredictDiabetesAsync(DiabetesRequest request)
        {
            var client = _clientFactory.CreateClient("DiseaseAPI");
            
            // Format data as a nested array as expected by the Python model
            // This matches how the model processes: input_data = [[Gender, AGE, ...]]
            var jsonContent = JsonSerializer.Serialize(new
            {
                data = new object[]
                {
                    new object[]
                    {
                        request.Gender,
                        request.Age,
                        request.Urea,
                        request.Cr,
                        request.HbA1c,
                        request.Chol,
                        request.TG,
                        request.HDL,
                        request.LDL,
                        request.VLDL,
                        request.BMI
                    }
                }
            }, new JsonSerializerOptions { 
                WriteIndented = true
            });
            
            _logger.LogInformation($"Diabetes request data: {jsonContent}");
            
            var content = new StringContent(
                jsonContent,
                Encoding.UTF8,
                "application/json");
                
            try
            {
                _logger.LogInformation($"Sending request to {_apiBaseUrl}/api/diabetes-prediction");
                
                var response = await client.PostAsync($"{_apiBaseUrl}/api/diabetes-prediction", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"API Error: {response.StatusCode}, Content: {errorContent}");
                    throw new HttpRequestException($"API returned status code {response.StatusCode}: {errorContent}");
                }
                
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"API Diabetes response received: {responseContent}");
                
                DiabetesResponse result;
                try
                {
                    result = JsonSerializer.Deserialize<DiabetesResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    if (result == null)
                    {
                        throw new JsonException("Failed to deserialize response");
                    }
                    
                    // Convert risk level format from "Low"/"Medium"/"High" to "LOW_RISK"/"MODERATE_RISK"/"HIGH_RISK" for compatibility
                    if (!string.IsNullOrEmpty(result.RiskLevel))
                    {
                        string riskLevel = result.RiskLevel.Trim().ToUpper();
                        if (riskLevel == "LOW" || riskLevel == "NEGATIVE")
                            result.RiskLevel = "LOW_RISK";
                        else if (riskLevel == "MEDIUM")
                            result.RiskLevel = "MODERATE_RISK";
                        else if (riskLevel == "HIGH")
                            result.RiskLevel = "HIGH_RISK";
                    }
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError($"Error parsing API response: {jsonEx.Message}. Content: {responseContent}");
                    
                    // Create a fallback response with error information
                    result = new DiabetesResponse
                    {
                        Status = "error",
                        BinaryResult = "ERROR",
                        OriginalClass = "N",
                        RiskLevel = "UNKNOWN",
                        ClinicalStatus = "Unknown",
                        Recommendation = "Error processing data, please try again",
                        Probabilities = new Dictionary<string, float> { { "N", 0 }, { "P", 0 }, { "Y", 0 } },
                        KeyIndicators = new Dictionary<string, float> { { "HbA1c", request.HbA1c }, { "BMI", request.BMI } }
                    };
                }
                
                // Set a default risk level if it's not provided or is empty
                if (string.IsNullOrEmpty(result.RiskLevel))
                {
                    _logger.LogWarning("Risk level was missing in API response, determining based on HbA1c value");
                    result.RiskLevel = GetRiskLevelBasedOnHbA1c(request.HbA1c);
                }
                
                // Override the risk level based on HbA1c values
                if (request.HbA1c >= 6.5)
                {
                    result.RiskLevel = "HIGH_RISK";
                    if (result.Probabilities != null && result.Probabilities.ContainsKey("Y"))
                    {
                        result.Probabilities["Y"] = Math.Max(result.Probabilities["Y"], 0.75f);
                    }
                }
                else if (request.HbA1c >= 5.7)
                {
                    // If prediabetic and model predicted low risk, adjust to moderate
                    if (result.RiskLevel == "LOW_RISK")
                    {
                        result.RiskLevel = "MODERATE_RISK";
                        if (result.Probabilities != null && result.Probabilities.ContainsKey("P"))
                        {
                            result.Probabilities["P"] = Math.Max(result.Probabilities["P"], 0.5f);
                        }
                    }
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception in PredictDiabetesAsync: {ex.Message}");
                throw;
            }
        }
        
        private string GetRiskLevelBasedOnHbA1c(float hbA1c)
        {
            if (hbA1c >= 6.5) return "HIGH_RISK";
            if (hbA1c >= 5.7) return "MODERATE_RISK";
            return "LOW_RISK";
        }
    }
}