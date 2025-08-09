using FEENALOoFINALE.Models;
using FEENALOoFINALE.Services;

namespace FEENALOoFINALE.ViewModels
{
    public class EquipmentPredictionDashboardViewModel
    {
        public List<EquipmentPredictionItemViewModel> Equipment { get; set; } = new();
        public EquipmentPredictionStatistics Statistics { get; set; } = new();
        public bool ApiHealthy { get; set; }
        public ModelInfo? ModelInfo { get; set; }
    }

    public class EquipmentPredictionItemViewModel
    {
        public int EquipmentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime? InstallationDate { get; set; }
        public int AgeMonths { get; set; }
        public double WeeklyUsageHours { get; set; }
        public double OperatingTemperature { get; set; }
        public double VibrationLevel { get; set; }
        public double PowerConsumption { get; set; }
        public string Status { get; set; } = string.Empty;
        public PredictionResult? LastPrediction { get; set; }
    }

    public class EquipmentPredictionStatistics
    {
        public int TotalEquipment { get; set; }
        public int PendingPredictions { get; set; }
        public bool ApiConnected { get; set; }
        public double ModelAccuracy { get; set; }
    }

    public class EquipmentRiskAnalysisViewModel
    {
        public List<EquipmentRiskAssessment> RiskAssessments { get; set; } = new();
        public RiskAnalysisSummary Summary { get; set; } = new();
    }

    public class EquipmentRiskAssessment
    {
        public int EquipmentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int AgeMonths { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public double FailureProbability { get; set; }
        public double ConfidenceScore { get; set; }
        public DateTime PredictionDate { get; set; }
        public string RecommendedAction { get; set; } = string.Empty;
    }

    public class RiskAnalysisSummary
    {
        public int TotalEquipment { get; set; }
        public int HighRiskCount { get; set; }
        public int MediumRiskCount { get; set; }
        public int LowRiskCount { get; set; }
        public double AverageFailureProbability { get; set; }
        public int RecommendedImmediateActions { get; set; }
    }
}
