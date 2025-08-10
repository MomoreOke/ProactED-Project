namespace FEENALOoFINALE.ViewModels
{
    // View Models for Predictive Maintenance - Updated for cost removal
    public class PredictiveMaintenanceViewModel
    {
        public int TotalActiveEquipment { get; set; }
        public int HighRiskEquipmentCount { get; set; }
        public int PredictedFailuresNext30Days { get; set; }
        public int TotalPredictions { get; set; }
        public List<MaintenanceRecommendation> MaintenanceRecommendations { get; set; } = new();
        public List<PredictedFailure> PredictedFailures { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }

    public class EquipmentRiskScore
    {
        public int EquipmentId { get; set; }
        public string EquipmentName { get; set; } = string.Empty;
        public string EquipmentType { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public double RiskScore { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public List<string> RiskFactors { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public DateTime? NextMaintenanceDue { get; set; }
        public DateTime EstimatedFailureDate { get; set; }
    }

    public class PredictedFailure
    {
        public int EquipmentId { get; set; }
        public string EquipmentName { get; set; } = string.Empty;
        public string EquipmentType { get; set; } = string.Empty;
        public DateTime EstimatedFailureDate { get; set; }
        public double FailureProbability { get; set; }
    }

    public class MaintenanceRecommendation
    {
        public int EquipmentId { get; set; }
        public string EquipmentName { get; set; } = string.Empty;
        public string RecommendationType { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Urgency { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new();
    }

    public class MaintenancePrediction
    {
        public int EquipmentId { get; set; }
        public string EquipmentName { get; set; } = string.Empty;
        public DateTime PredictedMaintenanceDate { get; set; }
        public string MaintenanceType { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }

    public class MaintenancePlanningData
    {
        public List<MaintenancePrediction> UpcomingMaintenance { get; set; } = new();
        public List<MaintenanceRecommendation> MaintenanceRecommendations { get; set; } = new();
        public Dictionary<string, int> MonthlyPredictions { get; set; } = new();
        public Dictionary<string, object> ResourceRequirements { get; set; } = new();
    }
}
