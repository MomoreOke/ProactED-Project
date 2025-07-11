using System.ComponentModel.DataAnnotations;
using FEENALOoFINALE.Models.ViewModels;

namespace FEENALOoFINALE.Models.ViewModels
{
    /// <summary>
    /// Enhanced Asset Dashboard ViewModel with comprehensive metrics and analytics
    /// </summary>
    public class EnhancedAssetDashboardViewModel : BaseViewModel
    {
        // Asset Statistics
        public AssetStatistics Statistics { get; set; } = new();
        
        // Chart Data for visualization
        public AssetChartData ChartData { get; set; } = new();
        
        // Filter Options
        public AssetFilterOptions FilterOptions { get; set; } = new();
        
        // Recent Activity
        public List<AssetActivityViewModel> RecentActivity { get; set; } = new();
        
        // Critical Items requiring attention
        public List<AssetItemViewModel> CriticalAssets { get; set; } = new();
        
        // Low Stock Items
        public List<AssetItemViewModel> LowStockItems { get; set; } = new();
        
        // Asset Performance Metrics
        public AssetPerformanceMetrics Performance { get; set; } = new();
        
        // Quick Actions
        public List<AssetQuickAction> QuickActions { get; set; } = new();
        
        // Summary Cards Data
        public List<AssetSummaryCard> SummaryCards { get; set; } = new();
        
        public EnhancedAssetDashboardViewModel()
        {
            PageTitle = "Asset Management Dashboard";
            PageDescription = "Comprehensive view of all equipment and inventory assets";
            
            Breadcrumbs = new List<BreadcrumbItem>
            {
                new() { Text = "Home", Controller = "Home", Action = "Index" },
                new() { Text = "Asset Management", Controller = "Asset", Action = "Index", IsActive = true }
            };
        }
    }

    /// <summary>
    /// Enhanced Asset Management ViewModel with advanced features
    /// </summary>
    public class EnhancedAssetManagementViewModel : PaginatedViewModel
    {
        // Asset Data
        public List<AssetItemViewModel> Assets { get; set; } = new();
        
        // Statistics
        public AssetStatistics Statistics { get; set; } = new();
        
        // Filter and Search
        public AssetFilterOptions FilterOptions { get; set; } = new();
        new public string SearchTerm { get; set; } = string.Empty;
        public AssetSortOptions SortOptions { get; set; } = new();
        
        // Bulk Operations
        public List<AssetBulkAction> AvailableBulkActions { get; set; } = new();
        public bool BulkActionsEnabled { get; set; } = true;
        
        // Export Options
        public List<AssetExportOption> ExportOptions { get; set; } = new();
        
        // View Options
        new public AssetViewMode ViewMode { get; set; } = AssetViewMode.Table;
        public List<AssetColumnOption> ColumnOptions { get; set; } = new();
        
        public EnhancedAssetManagementViewModel()
        {
            PageTitle = "Asset Management";
            PageDescription = "Manage equipment and inventory assets";
            PageSize = 25;
            
            Breadcrumbs = new List<BreadcrumbItem>
            {
                new() { Text = "Home", Controller = "Home", Action = "Index" },
                new() { Text = "Asset Management", Controller = "Asset", Action = "Index" },
                new() { Text = "Manage Assets", Controller = "Asset", Action = "Enhanced", IsActive = true }
            };
        }
    }

    /// <summary>
    /// Individual Asset Item ViewModel
    /// </summary>
    public class AssetItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Equipment, Inventory
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        // Equipment-specific properties
        public DateTime? InstallationDate { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        public string Manufacturer { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public decimal? PurchasePrice { get; set; }
        public decimal? CurrentValue { get; set; }
        
        // Inventory-specific properties
        public int CurrentStock { get; set; }
        public int MinimumStock { get; set; }
        public int MaximumStock { get; set; }
        public bool IsLowStock { get; set; }
        public bool IsOutOfStock { get; set; }
        public decimal UnitPrice { get; set; }
        public string Unit { get; set; } = string.Empty;
        
        // Common properties
        public int AlertCount { get; set; }
        public bool HasCriticalAlerts { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<string> Tags { get; set; } = new();
        public string StatusColor => GetStatusColor();
        public string StatusIcon => GetStatusIcon();
        public bool CanEdit { get; set; } = true;
        public bool CanDelete { get; set; } = true;
        
        private string GetStatusColor()
        {
            return Status?.ToLower() switch
            {
                "active" or "operational" or "in stock" => "success",
                "inactive" or "maintenance" or "low stock" => "warning",
                "critical" or "out of stock" or "failed" => "danger",
                "pending" or "under review" => "info",
                _ => "secondary"
            };
        }
        
        private string GetStatusIcon()
        {
            return Type?.ToLower() switch
            {
                "equipment" => Status?.ToLower() switch
                {
                    "active" or "operational" => "bi-gear-fill",
                    "maintenance" => "bi-wrench",
                    "inactive" => "bi-gear",
                    "critical" => "bi-exclamation-triangle-fill",
                    _ => "bi-gear"
                },
                "inventory" => Status?.ToLower() switch
                {
                    "in stock" => "bi-box-fill",
                    "low stock" => "bi-box",
                    "out of stock" => "bi-box2",
                    _ => "bi-box"
                },
                _ => "bi-collection"
            };
        }
    }

    /// <summary>
    /// Asset Statistics ViewModel
    /// </summary>
    public class AssetStatistics
    {
        // Total Assets
        public int TotalAssets { get; set; }
        public int TotalEquipment { get; set; }
        public int TotalInventoryItems { get; set; }
        
        // Equipment Statistics
        public int OperationalEquipment { get; set; }
        public int EquipmentUnderMaintenance { get; set; }
        public int CriticalEquipment { get; set; }
        public int OverdueMaintenanceEquipment { get; set; }
        
        // Inventory Statistics
        public int InStockItems { get; set; }
        public int LowStockItems { get; set; }
        public int OutOfStockItems { get; set; }
        public int CriticalInventoryItems { get; set; }
        
        // Financial Metrics
        public decimal TotalAssetValue { get; set; }
        public decimal MonthlyMaintenanceCost { get; set; }
        public decimal YearlyMaintenanceCost { get; set; }
        public decimal InventoryValue { get; set; }
        
        // Performance Metrics
        public double EquipmentUptime { get; set; }
        public double MaintenanceEfficiency { get; set; }
        public double InventoryTurnover { get; set; }
        public int MaintenanceTasksCompleted { get; set; }
        
        // Trend Data
        public List<TrendDataPoint> AssetGrowthTrend { get; set; } = new();
        public List<TrendDataPoint> MaintenanceCostTrend { get; set; } = new();
        public List<TrendDataPoint> UptimeTrend { get; set; } = new();
    }

    /// <summary>
    /// Asset Chart Data for visualizations
    /// </summary>
    public class AssetChartData
    {
        // Asset Distribution Charts
        public List<ChartDataPoint> AssetsByType { get; set; } = new();
        public List<ChartDataPoint> AssetsByCategory { get; set; } = new();
        public List<ChartDataPoint> AssetsByStatus { get; set; } = new();
        public List<ChartDataPoint> AssetsByLocation { get; set; } = new();
        
        // Performance Charts
        public List<TimeSeriesDataPoint> UptimeHistory { get; set; } = new();
        public List<TimeSeriesDataPoint> MaintenanceCosts { get; set; } = new();
        public List<TimeSeriesDataPoint> AssetUtilization { get; set; } = new();
        
        // Inventory Charts
        public List<ChartDataPoint> StockLevels { get; set; } = new();
        public List<ChartDataPoint> InventoryTurnover { get; set; } = new();
        public List<TimeSeriesDataPoint> StockMovement { get; set; } = new();
    }

    /// <summary>
    /// Asset Filter Options
    /// </summary>
    public class AssetFilterOptions
    {
        public List<AssetTypeOption> Types { get; set; } = new();
        public List<AssetCategoryOption> Categories { get; set; } = new();
        public List<AssetStatusOption> Statuses { get; set; } = new();
        public List<AssetLocationOption> Locations { get; set; } = new();
        public List<AssetManufacturerOption> Manufacturers { get; set; } = new();
        
        // Selected Filters
        public List<string> SelectedTypes { get; set; } = new();
        public List<string> SelectedCategories { get; set; } = new();
        public List<string> SelectedStatuses { get; set; } = new();
        public List<string> SelectedLocations { get; set; } = new();
        public List<string> SelectedManufacturers { get; set; } = new();
        
        // Date Range Filters
        public DateTime? InstallationDateFrom { get; set; }
        public DateTime? InstallationDateTo { get; set; }
        public DateTime? LastMaintenanceDateFrom { get; set; }
        public DateTime? LastMaintenanceDateTo { get; set; }
        
        // Value Range Filters
        public decimal? PriceFrom { get; set; }
        public decimal? PriceTo { get; set; }
        public int? StockFrom { get; set; }
        public int? StockTo { get; set; }
        
        // Boolean Filters
        public bool? ShowLowStock { get; set; }
        public bool? ShowCriticalOnly { get; set; }
        public bool? ShowMaintenanceDue { get; set; }
        public bool? ShowWithAlerts { get; set; }
    }

    /// <summary>
    /// Asset Sort Options
    /// </summary>
    public class AssetSortOptions
    {
        public AssetSortField SortBy { get; set; } = AssetSortField.Name;
        public SortDirection Direction { get; set; } = SortDirection.Ascending;
        
        public List<AssetSortOption> AvailableOptions { get; set; } = new()
        {
            new() { Value = "name_asc", Text = "Name (A-Z)", Field = AssetSortField.Name, Direction = SortDirection.Ascending },
            new() { Value = "name_desc", Text = "Name (Z-A)", Field = AssetSortField.Name, Direction = SortDirection.Descending },
            new() { Value = "type_asc", Text = "Type (A-Z)", Field = AssetSortField.Type, Direction = SortDirection.Ascending },
            new() { Value = "status_asc", Text = "Status", Field = AssetSortField.Status, Direction = SortDirection.Ascending },
            new() { Value = "location_asc", Text = "Location", Field = AssetSortField.Location, Direction = SortDirection.Ascending },
            new() { Value = "date_newest", Text = "Newest First", Field = AssetSortField.InstallationDate, Direction = SortDirection.Descending },
            new() { Value = "date_oldest", Text = "Oldest First", Field = AssetSortField.InstallationDate, Direction = SortDirection.Ascending },
            new() { Value = "value_high", Text = "Value (High to Low)", Field = AssetSortField.Value, Direction = SortDirection.Descending },
            new() { Value = "value_low", Text = "Value (Low to High)", Field = AssetSortField.Value, Direction = SortDirection.Ascending }
        };
    }

    /// <summary>
    /// Asset Activity ViewModel
    /// </summary>
    public class AssetActivityViewModel
    {
        public int Id { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public string AssetType { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty; // Maintenance, Alert, Stock Change, etc.
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    /// <summary>
    /// Asset Performance Metrics
    /// </summary>
    public class AssetPerformanceMetrics
    {
        public double OverallEfficiency { get; set; }
        public double MaintenanceCompliance { get; set; }
        public double AssetUtilization { get; set; }
        public double CostEfficiency { get; set; }
        public double InventoryAccuracy { get; set; }
        
        public List<AssetKPI> KPIs { get; set; } = new();
        public List<AssetBenchmark> Benchmarks { get; set; } = new();
    }

    /// <summary>
    /// Supporting classes for Asset ViewModels
    /// </summary>
    public class AssetTypeOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public int Count { get; set; }
        public string Icon { get; set; } = string.Empty;
    }

    public class AssetCategoryOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public int Count { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class AssetStatusOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public int Count { get; set; }
        public string Color { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }

    public class AssetLocationOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public int Count { get; set; }
        public string Building { get; set; } = string.Empty;
    }

    public class AssetManufacturerOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class AssetSortOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public AssetSortField Field { get; set; }
        public SortDirection Direction { get; set; }
    }

    public class AssetQuickAction
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = "primary";
        public int Count { get; set; }
        public bool IsEnabled { get; set; } = true;
    }

    public class AssetSummaryCard
    {
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = "primary";
        public string Trend { get; set; } = string.Empty; // up, down, stable
        public double TrendPercentage { get; set; }
        public string Url { get; set; } = string.Empty;
    }

    public class AssetBulkAction
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = "primary";
        public bool RequiresConfirmation { get; set; } = true;
        public string ConfirmationMessage { get; set; } = string.Empty;
        public List<string> ApplicableTypes { get; set; } = new(); // Equipment, Inventory, or both
    }

    public class AssetExportOption
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty; // CSV, PDF, Excel
        public string Icon { get; set; } = string.Empty;
        public bool IncludesImages { get; set; }
        public List<string> AvailableFields { get; set; } = new();
    }

    public class AssetColumnOption
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsVisible { get; set; } = true;
        public bool IsRequired { get; set; }
        public int Order { get; set; }
        public string Width { get; set; } = "auto";
    }

    public class AssetKPI
    {
        public string Name { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // good, warning, critical
        public double Trend { get; set; }
    }

    public class AssetBenchmark
    {
        public string Category { get; set; } = string.Empty;
        public double InternalValue { get; set; }
        public double IndustryAverage { get; set; }
        public double BestInClass { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    /// <summary>
    /// Enumerations for Asset Management
    /// </summary>
    public enum AssetViewMode
    {
        Table,
        Grid,
        Cards
    }

    public enum AssetSortField
    {
        Name,
        Type,
        Category,
        Status,
        Location,
        InstallationDate,
        LastMaintenance,
        Value,
        Stock
    }
}
