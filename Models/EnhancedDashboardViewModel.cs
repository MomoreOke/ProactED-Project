using System.ComponentModel.DataAnnotations;

namespace FEENALOoFINALE.Models
{
    public class EnhancedDashboardViewModel
    {
        // Basic Dashboard Metrics
        public int TotalEquipment { get; set; }
        public int ActiveEquipment { get; set; }
        public int CriticalAlerts { get; set; }
        public int ActiveMaintenanceTasks { get; set; }
        public int CompletedMaintenanceTasks { get; set; }
        public int PendingMaintenanceTasks { get; set; }
        public int OverdueMaintenances { get; set; }
        public int EquipmentNeedingAttention { get; set; }
        public int TotalInventoryItems { get; set; }
        public int LowStockItems { get; set; }
        public int OutOfStockItems { get; set; }

        // Equipment Status Distribution
        public List<EnhancedEquipmentStatusCount> EquipmentStatusCounts { get; set; } = new List<EnhancedEquipmentStatusCount>();
        
        // Recent Data
        public List<Alert> RecentAlerts { get; set; } = new List<Alert>();
        public List<MaintenanceTask> UpcomingMaintenanceTasks { get; set; } = new List<MaintenanceTask>();
        public List<MaintenanceTask> OverdueMaintenanceTasks { get; set; } = new List<MaintenanceTask>();
        public List<Equipment> CriticalEquipment { get; set; } = new List<Equipment>();
        public List<InventoryItem> LowStockInventory { get; set; } = new List<InventoryItem>();

        // Advanced Analytics
        public List<MaintenanceTrendData> MaintenanceTrends { get; set; } = new List<MaintenanceTrendData>();
        public List<CostAnalysisData> CostAnalysis { get; set; } = new List<CostAnalysisData>();
        public List<EquipmentPerformanceMetric> PerformanceMetrics { get; set; } = new List<EquipmentPerformanceMetric>();
        public List<PredictiveMaintenanceInsight> PredictiveInsights { get; set; } = new List<PredictiveMaintenanceInsight>();

        // KPI Indicators
        public List<KPIIndicator> KPIIndicators { get; set; } = new List<KPIIndicator>();
        
        // Performance Scores
        public double OverallSystemHealth { get; set; }
        public double MaintenanceEfficiency { get; set; }
        public double CostEfficiency { get; set; }
        public double EquipmentUtilization { get; set; }

        // Filter and Search Options
        public string? SearchTerm { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int? SelectedBuildingId { get; set; }
        public int? SelectedEquipmentTypeId { get; set; }
        public string? StatusFilter { get; set; }
        public string? PriorityFilter { get; set; }

        // Dropdown Options
        public List<Building> Buildings { get; set; } = new List<Building>();
        public List<EquipmentType> EquipmentTypes { get; set; } = new List<EquipmentType>();

        // Real-time Data
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        public bool IsRealTimeEnabled { get; set; } = true;

        // User Preferences
        public string? PreferredChartType { get; set; }
        public bool ShowAdvancedMetrics { get; set; } = true;
        public List<string> SelectedWidgets { get; set; } = new List<string>();
    }

    public class EnhancedEquipmentStatusCount
    {
        public EquipmentStatus Status { get; set; }
        public int Count { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string ColorClass { get; set; } = string.Empty;
        public double Percentage { get; set; }
    }

    public class MaintenanceTrendData
    {
        public DateTime Date { get; set; }
        public int CompletedCount { get; set; }
        public int ScheduledCount { get; set; }
        public int OverdueCount { get; set; }
        public decimal TotalCost { get; set; }
        public decimal PreventiveCost { get; set; }
        public decimal CorrectiveCost { get; set; }
    }

    public class CostAnalysisData
    {
        public string Category { get; set; } = string.Empty;
        public decimal CurrentPeriodCost { get; set; }
        public decimal PreviousPeriodCost { get; set; }
        public decimal BudgetAllocated { get; set; }
        public double VariancePercentage { get; set; }
        public string TrendDirection { get; set; } = string.Empty;
    }

    public class EquipmentPerformanceMetric
    {
        public string EquipmentName { get; set; } = string.Empty;
        public string EquipmentType { get; set; } = string.Empty;
        public double PerformanceScore { get; set; }
        public double UtilizationRate { get; set; }
        public int MaintenanceFrequency { get; set; }
        public decimal TotalMaintenanceCost { get; set; }
        public TimeSpan AverageDowntime { get; set; }
        public DateTime LastMaintenanceDate { get; set; }
        public DateTime NextScheduledMaintenance { get; set; }
        public string HealthStatus { get; set; } = string.Empty;
    }

    public class PredictiveMaintenanceInsight
    {
        public string EquipmentName { get; set; } = string.Empty;
        public string PredictedIssue { get; set; } = string.Empty;
        public double ProbabilityPercentage { get; set; }
        public DateTime PredictedFailureDate { get; set; }
        public string RecommendedAction { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = string.Empty;
        public decimal EstimatedRepairCost { get; set; }
        public int DaysUntilPredictedFailure { get; set; }
    }

    public class KPIIndicator
    {
        public string Name { get; set; } = string.Empty;
        public double CurrentValue { get; set; }
        public double TargetValue { get; set; }
        public double PreviousValue { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Good, Warning, Critical
        public string TrendDirection { get; set; } = string.Empty; // Up, Down, Stable
        public double PercentageChange { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }
}
