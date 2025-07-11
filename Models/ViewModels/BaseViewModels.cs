using System.ComponentModel.DataAnnotations;

namespace FEENALOoFINALE.Models.ViewModels
{
    /// <summary>
    /// Base view model providing common functionality for all views
    /// </summary>
    public abstract class BaseViewModel
    {
        public string PageTitle { get; set; } = string.Empty;
        public string PageDescription { get; set; } = string.Empty;
        public List<BreadcrumbItem> Breadcrumbs { get; set; } = new();
        public List<NotificationMessage> Notifications { get; set; } = new();
        public Dictionary<string, object> MetaData { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public string? UserId { get; set; }
        public bool HasErrors => Notifications.Any(n => n.Type == NotificationType.Error);
        public bool HasWarnings => Notifications.Any(n => n.Type == NotificationType.Warning);
    }

    /// <summary>
    /// Base view model for paginated data
    /// </summary>
    public abstract class PaginatedViewModel : BaseViewModel
    {
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalRecords { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int StartRecord => ((CurrentPage - 1) * PageSize) + 1;
        public int EndRecord => Math.Min(CurrentPage * PageSize, TotalRecords);
        
        // Search and filtering
        public string SearchTerm { get; set; } = string.Empty;
        public string SortBy { get; set; } = string.Empty;
        public string SortDirection { get; set; } = "asc";
        public Dictionary<string, string> Filters { get; set; } = new();
        
        // UI State
        public bool ShowFilters { get; set; } = false;
        public string ViewMode { get; set; } = "table"; // table, grid, list
    }

    /// <summary>
    /// Enhanced dashboard view model with comprehensive analytics
    /// </summary>
    public class EnhancedDashboardViewModel : BaseViewModel
    {
        // Core Metrics
        public DashboardMetrics Metrics { get; set; } = new();
        
        // Chart Data
        public List<ChartDataPoint> EquipmentStatusChart { get; set; } = new();
        public List<ChartDataPoint> MaintenanceTrendChart { get; set; } = new();
        public List<ChartDataPoint> CostAnalysisChart { get; set; } = new();
        public List<ChartDataPoint> PerformanceChart { get; set; } = new();
        
        // Recent Activities
        public List<RecentActivity> RecentActivities { get; set; } = new();
        public List<Alert> CriticalAlerts { get; set; } = new();
        public List<MaintenanceTask> UpcomingTasks { get; set; } = new();
        public List<Equipment> EquipmentNeedingAttention { get; set; } = new();
        
        // Quick Actions
        public List<QuickActionItem> QuickActions { get; set; } = new();
        
        // Filtering and Customization
        public DashboardFilters Filters { get; set; } = new();
        public DashboardPreferences UserPreferences { get; set; } = new();
        
        // Real-time data
        public bool IsRealTimeEnabled { get; set; } = true;
        public DateTime LastRefresh { get; set; } = DateTime.UtcNow;
        public int RefreshIntervalSeconds { get; set; } = 30;
    }

    public class DashboardMetrics
    {
        public int TotalEquipment { get; set; }
        public int OperationalEquipment { get; set; }
        public int MaintenanceEquipment { get; set; }
        public int OutOfServiceEquipment { get; set; }
        
        public int ActiveMaintenanceTasks { get; set; }
        public int OverdueMaintenanceTasks { get; set; }
        public int CompletedTasksToday { get; set; }
        
        public int CriticalAlerts { get; set; }
        public int WarningAlerts { get; set; }
        public int InfoAlerts { get; set; }
        
        public decimal TotalMaintenanceCost { get; set; }
        public decimal MonthlyMaintenanceCost { get; set; }
        public decimal PredictedSavings { get; set; }
        
        public double SystemEfficiency { get; set; }
        public double PredictiveAccuracy { get; set; }
        public double EquipmentUtilization { get; set; }
        
        public int LowStockItems { get; set; }
        public int OutOfStockItems { get; set; }
    }

    public class ChartDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Color { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime? Date { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }

    public class TrendDataPoint
    {
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class TimeSeriesDataPoint
    {
        public DateTime Timestamp { get; set; }
        public decimal Value { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class RecentActivity
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string ActivityType { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string IconColor { get; set; } = string.Empty;
        public string? RelatedId { get; set; }
        public string? RelatedType { get; set; }
    }

    public class QuickActionItem
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = "primary";
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public Dictionary<string, string> RouteValues { get; set; } = new();
        public string Badge { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
        public int Priority { get; set; } = 0;
        public string[] RequiredRoles { get; set; } = Array.Empty<string>();
    }

    public class DashboardFilters
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public List<int> BuildingIds { get; set; } = new();
        public List<int> EquipmentTypeIds { get; set; } = new();
        public List<EquipmentStatus> EquipmentStatuses { get; set; } = new();
        public List<string> UserIds { get; set; } = new();
        public string Priority { get; set; } = string.Empty;
        public bool ShowOnlyMyTasks { get; set; } = false;
    }

    public class DashboardPreferences
    {
        public string UserId { get; set; } = string.Empty;
        public string Theme { get; set; } = "light";
        public List<string> VisibleWidgets { get; set; } = new();
        public Dictionary<string, int> WidgetPositions { get; set; } = new();
        public int RefreshInterval { get; set; } = 30;
        public string DefaultView { get; set; } = "overview";
        public bool EnableNotifications { get; set; } = true;
        public bool EnableSounds { get; set; } = false;
    }

    public class FilterOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool IsSelected { get; set; } = false;
        public int Count { get; set; } = 0;
        public string Category { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    // Supporting classes
    public class BreadcrumbItem
    {
        public string Text { get; set; } = string.Empty;
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public Dictionary<string, object>? RouteValues { get; set; }
        public bool IsActive { get; set; } = false;
    }

    public class NotificationMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; } = NotificationType.Info;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsDismissible { get; set; } = true;
        public int? AutoDismissAfterSeconds { get; set; }
        public string? ActionText { get; set; }
        public string? ActionUrl { get; set; }
    }

    public enum NotificationType
    {
        Success,
        Info,
        Warning,
        Error
    }

    public enum SortDirection
    {
        Ascending,
        Descending
    }
}
