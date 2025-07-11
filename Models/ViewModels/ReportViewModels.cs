using System.ComponentModel.DataAnnotations;
using FEENALOoFINALE.Models.ViewModels;

namespace FEENALOoFINALE.Models.ViewModels
{
    /// <summary>
    /// Enhanced Report Management ViewModels
    /// </summary>
    public class EnhancedReportDashboardViewModel : BaseViewModel
    {
        // Report Statistics
        public ReportStatistics Statistics { get; set; } = new();
        
        // Chart Data
        public ReportChartData ChartData { get; set; } = new();
        
        // Report Filters
        public ReportFilters Filters { get; set; } = new();
        
        // Recent Reports
        public List<ReportItemViewModel> RecentReports { get; set; } = new();
        
        // Scheduled Reports
        public List<ScheduledReportViewModel> ScheduledReports { get; set; } = new();
        
        // Quick Report Options
        public List<QuickReportOption> QuickReports { get; set; } = new();
        
        // Export Options
        public List<ExportOption> ExportOptions { get; set; } = new();
        
        public EnhancedReportDashboardViewModel()
        {
            PageTitle = "Advanced Reports & Analytics";
            PageDescription = "Comprehensive reporting dashboard with real-time analytics and predictive insights";
            
            Breadcrumbs = new List<BreadcrumbItem>
            {
                new() { Text = "Home", Controller = "Home", Action = "Index" },
                new() { Text = "Reports & Analytics", Controller = "Report", Action = "Enhanced", IsActive = true }
            };
        }
    }

    public class ReportStatistics
    {
        public int TotalReports { get; set; }
        public int ReportsThisMonth { get; set; }
        public int ScheduledReports { get; set; }
        public int AutomatedReports { get; set; }
        public decimal AverageReportGenerationTime { get; set; }
        public long TotalReportDownloads { get; set; }
        public int ReportsShared { get; set; }
        public DateTime LastReportGenerated { get; set; }
    }

    public class ReportChartData
    {
        // Equipment Performance Charts
        public List<ChartDataPoint> EquipmentUptimeChart { get; set; } = new();
        public List<ChartDataPoint> MaintenanceCostTrends { get; set; } = new();
        public List<ChartDataPoint> EquipmentEfficiencyChart { get; set; } = new();
        public List<ChartDataPoint> AlertFrequencyChart { get; set; } = new();
        
        // Predictive Analytics Charts
        public List<TrendDataPoint> PredictiveMaintenanceTrend { get; set; } = new();
        public List<TrendDataPoint> FailurePredictionTrend { get; set; } = new();
        public List<TrendDataPoint> CostProjectionTrend { get; set; } = new();
        
        // Time Series Data
        public List<TimeSeriesDataPoint> DailyMetrics { get; set; } = new();
        public List<TimeSeriesDataPoint> WeeklyTrends { get; set; } = new();
        public List<TimeSeriesDataPoint> MonthlyAnalytics { get; set; } = new();
        
        // Comparative Analysis
        public List<ChartDataPoint> YearOverYearComparison { get; set; } = new();
        public List<ChartDataPoint> DepartmentComparison { get; set; } = new();
        public List<ChartDataPoint> LocationComparison { get; set; } = new();
    }

    public class ReportFilters
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public List<string> SelectedDepartments { get; set; } = new();
        public List<string> SelectedEquipmentTypes { get; set; } = new();
        public List<string> SelectedLocations { get; set; } = new();
        public List<string> SelectedReportTypes { get; set; } = new();
        public ReportFrequency Frequency { get; set; } = ReportFrequency.Daily;
        public string SearchTerm { get; set; } = string.Empty;
        
        // Filter Options
        public List<FilterOption> DepartmentOptions { get; set; } = new();
        public List<FilterOption> EquipmentTypeOptions { get; set; } = new();
        public List<FilterOption> LocationOptions { get; set; } = new();
        public List<FilterOption> ReportTypeOptions { get; set; } = new();
    }

    public class ReportItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ReportType Type { get; set; }
        public string TypeDisplay => Type.ToString();
        public DateTime GeneratedDate { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;
        public ReportStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public string StatusColor => Status switch
        {
            ReportStatus.Completed => "success",
            ReportStatus.InProgress => "warning",
            ReportStatus.Failed => "danger",
            ReportStatus.Scheduled => "info",
            _ => "secondary"
        };
        public long FileSizeBytes { get; set; }
        public string FileSizeDisplay => FormatFileSize(FileSizeBytes);
        public string FileFormat { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
        public bool CanDelete { get; set; }
        public bool CanShare { get; set; }
        public bool IsScheduled { get; set; }
        public int DownloadCount { get; set; }
        
        private static string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F1} MB";
            return $"{bytes / (1024.0 * 1024 * 1024):F1} GB";
        }
    }

    public class ScheduledReportViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ReportType Type { get; set; }
        public ReportFrequency Frequency { get; set; }
        public string FrequencyDisplay => Frequency.ToString();
        public DateTime NextRunDate { get; set; }
        public DateTime LastRunDate { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public List<string> Recipients { get; set; } = new();
        public string RecipientsDisplay => string.Join(", ", Recipients);
        public Dictionary<string, object> Parameters { get; set; } = new();
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }

    public class QuickReportOption
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public ReportType Type { get; set; }
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
        public int EstimatedDuration { get; set; } // in seconds
        public string EstimatedDurationDisplay => EstimatedDuration < 60 ? 
            $"{EstimatedDuration}s" : 
            $"{EstimatedDuration / 60}m {EstimatedDuration % 60}s";
    }

    public class ExportOption
    {
        public string Format { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool RequiresConfiguration { get; set; }
        public List<string> SupportedReportTypes { get; set; } = new();
    }

    public class ExportResult
    {
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSizeBytes => Data.Length;
        public string FileSizeDisplay => FormatFileSize(FileSizeBytes);
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
        public bool Success { get; set; } = true;
        public string ErrorMessage { get; set; } = string.Empty;
        
        private static string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F1} MB";
            return $"{bytes / (1024.0 * 1024 * 1024):F1} GB";
        }
    }

    // Report Builder ViewModels
    public class ReportBuilderViewModel : BaseViewModel
    {
        public string ReportName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ReportType Type { get; set; } = ReportType.CustomReport;
        public List<ReportField> SelectedFields { get; set; } = new();
        public List<ReportField> AvailableFields { get; set; } = new();
        public List<ReportFilter> Filters { get; set; } = new();
        public List<ReportGrouping> Groupings { get; set; } = new();
        public List<ReportSorting> Sorting { get; set; } = new();
        public ReportLayoutOptions LayoutOptions { get; set; } = new();
        public ScheduleOptions? ScheduleOptions { get; set; }
        public bool IsScheduled { get; set; }
        public List<string> ExportFormats { get; set; } = new() { "pdf", "excel", "csv" };
    }

    public class ReportField
    {
        public string FieldName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public bool IsCalculated { get; set; }
        public string CalculationFormula { get; set; } = string.Empty;
        public ReportAggregationType AggregationType { get; set; } = ReportAggregationType.None;
    }

    public class ReportFilter
    {
        public string FieldName { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string LogicalOperator { get; set; } = "AND";
    }

    public class ReportGrouping
    {
        public string FieldName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool ShowSubtotals { get; set; }
        public int SortOrder { get; set; }
    }

    public class ReportSorting
    {
        public string FieldName { get; set; } = string.Empty;
        public string Direction { get; set; } = "ASC";
        public int Priority { get; set; }
    }

    public class ReportLayoutOptions
    {
        public string Orientation { get; set; } = "Portrait";
        public string PageSize { get; set; } = "A4";
        public bool ShowHeader { get; set; } = true;
        public bool ShowFooter { get; set; } = true;
        public bool ShowPageNumbers { get; set; } = true;
        public string CompanyLogo { get; set; } = string.Empty;
        public string CustomCss { get; set; } = string.Empty;
    }

    public class ScheduleOptions
    {
        public ReportFrequency Frequency { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string> Recipients { get; set; } = new();
        public string EmailSubject { get; set; } = string.Empty;
        public string EmailBody { get; set; } = string.Empty;
        public List<string> ExportFormats { get; set; } = new();
    }

    // Enums
    public enum ReportType
    {
        EquipmentPerformance,
        MaintenanceCosts,
        InventoryLevels,
        AlertSummary,
        PredictiveMaintenance,
        ComplianceReport,
        UtilizationReport,
        CostAnalysis,
        CustomReport
    }

    public enum ReportStatus
    {
        Scheduled,
        InProgress,
        Completed,
        Failed,
        Cancelled
    }

    public enum ReportFrequency
    {
        Daily,
        Weekly,
        Monthly,
        Quarterly,
        Yearly,
        Custom
    }

    public enum ReportAggregationType
    {
        None,
        Sum,
        Average,
        Count,
        Min,
        Max,
        GroupBy
    }
}
