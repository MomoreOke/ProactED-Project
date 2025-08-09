using FEENALOoFINALE.Models;

namespace FEENALOoFINALE.ViewModels
{
    public class MLDashboardViewModel
    {
        public string ModelVersion { get; set; } = "Unknown";
        public double ModelAccuracy { get; set; }
        public bool ApiHealthy { get; set; }
        
        public int TotalEquipmentAnalyzed { get; set; }
        public int HighRiskEquipment { get; set; }
        public int MediumRiskEquipment { get; set; }
        public int LowRiskEquipment { get; set; }
        
        public double AverageFailureProbability { get; set; }
        public DateTime LastPredictionUpdate { get; set; }
        
        // Additional properties that views are expecting
        public string ModelStatus { get; set; } = "Unknown";
        public List<FailurePrediction> RecentPredictions { get; set; } = new();
        public int CriticalPredictionsCount { get; set; }
        public double AverageConfidence { get; set; }
        
        public Dictionary<string, int> RiskLevelDistribution { get; set; } = new();
        public List<EquipmentWithMLPrediction> HighRiskEquipmentList { get; set; } = new();
        
        // Chart data
        public List<ChartDataPoint> FailureProbabilityTrend { get; set; } = new();
        public List<ChartDataPoint> RiskDistributionChart { get; set; } = new();
        
        // Real-time status
        public bool IsLiveMode { get; set; } = true;
        public int RefreshInterval { get; set; } = 30; // seconds
    }

    public class EquipmentWithMLPrediction
    {
        public Equipment Equipment { get; set; } = new();
        public PredictionResult? Prediction { get; set; }
        
        // Convenience properties for easier access in views
        public int EquipmentId => Equipment.EquipmentId;
        public EquipmentType? EquipmentType => Equipment.EquipmentType;
        public Building? Building => Equipment.Building;
        public Room? Room => Equipment.Room;
        public EquipmentModel? EquipmentModel => Equipment.EquipmentModel;
        
        public string RiskLevelDisplay => Prediction?.RiskLevel switch
        {
            "High" => "🔴 High Risk",
            "Medium" => "🟡 Medium Risk", 
            "Low" => "🟢 Low Risk",
            _ => "⚪ Unknown"
        };

        public string FailureProbabilityDisplay => Prediction != null 
            ? $"{Prediction.FailureProbability:P1}" 
            : "N/A";

        public string ConfidenceDisplay => Prediction != null 
            ? $"{Prediction.ConfidenceScore:P1}" 
            : "N/A";
    }

    public class ChartDataPoint
    {
        public string Label { get; set; } = "";
        public double Value { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}
