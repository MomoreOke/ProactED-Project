using FEENALOoFINALE.Models;

namespace FEENALOoFINALE.ViewModels
{
    public class MLPredictionDashboardViewModel
    {
        public int TotalEquipmentAnalyzed { get; set; }
        public int HighRiskEquipment { get; set; }
        public double ModelAccuracy { get; set; }
        public double AverageFailureProbability { get; set; }
        public bool ApiHealthy { get; set; }
        public string ModelVersion { get; set; } = "v1.0";
        public DateTime LastPredictionUpdate { get; set; }
        
        public Dictionary<string, int> RiskLevelDistribution { get; set; } = new();
        public List<EquipmentPredictionViewModel> HighRiskEquipmentList { get; set; } = new();
        public List<EquipmentPredictionViewModel> RecentPredictions { get; set; } = new();
    }

    public class EquipmentPredictionViewModel
    {
        public Equipment Equipment { get; set; } = new();
        public FailurePrediction? Prediction { get; set; }
        
        public string RiskLevelDisplay => GetRiskLevelFromPrediction();
        public string RiskLevelClass => GetRiskLevelClass();
        public string FailureProbabilityDisplay => GetFailureProbabilityDisplay();
        public string ConfidenceDisplay => GetConfidenceDisplay();

        private string GetFailureProbabilityDisplay()
        {
            if (Prediction == null) return "N/A";
            
            // Calculate probability based on confidence level and status
            var baseProb = Prediction.Status switch
            {
                PredictionStatus.High => 0.8,
                PredictionStatus.Medium => 0.5,
                PredictionStatus.Low => 0.2,
                _ => 0.1
            };
            
            // Adjust by confidence
            var adjustedProb = baseProb * (Prediction.ConfidenceLevel / 100.0);
            return adjustedProb.ToString("P1");
        }

        private string GetConfidenceDisplay()
        {
            return Prediction?.ConfidenceLevel.ToString() + "%" ?? "N/A";
        }

        private string GetRiskLevelFromPrediction()
        {
            return Prediction?.Status.ToString() ?? "Unknown";
        }

        private string GetRiskLevelClass()
        {
            var riskLevel = GetRiskLevelFromPrediction();
            return riskLevel switch
            {
                "High" => "danger",
                "Medium" => "warning", 
                "Low" => "success",
                _ => "secondary"
            };
        }
    }

    public class PredictionResultViewModel
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
        public object? Data { get; set; }
        public int Count { get; set; }
    }

    public class ApiHealthViewModel
    {
        public bool Healthy { get; set; }
        public ModelInfo? ModelInfo { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
