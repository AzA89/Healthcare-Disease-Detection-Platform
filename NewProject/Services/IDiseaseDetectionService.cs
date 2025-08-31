using System.Threading.Tasks;
using System.Collections.Generic;
using NewProject.Models;

namespace NewProject.Services
{
    public interface IDiseaseDetectionService
    {
        Task<object> PredictDiseaseAsync(List<string> symptoms, int topN = 3);
        Task<List<string>> GetAllSymptomsAsync();
        Task<HeartDiseaseResponse> PredictHeartDiseaseAsync(HeartDiseaseRequest request);
        Task<DiabetesResponse> PredictDiabetesAsync(DiabetesRequest request);
        string GetApiBaseUrl();
    }
}
