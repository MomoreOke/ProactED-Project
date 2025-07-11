using System.ComponentModel.DataAnnotations;

namespace FEENALOoFINALE.Models
{
    public class DashboardFilterViewModel
    {
        [Display(Name = "Date From")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "Date To")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Equipment Status")]
        public List<EquipmentStatus>? EquipmentStatuses { get; set; }

        [Display(Name = "Buildings")]
        public List<int>? BuildingIds { get; set; }

        [Display(Name = "Equipment Types")]
        public List<int>? EquipmentTypeIds { get; set; }

        [Display(Name = "Alert Priority")]
        public List<AlertPriority>? AlertPriorities { get; set; }

        [Display(Name = "Maintenance Status")]
        public List<MaintenanceStatus>? MaintenanceStatuses { get; set; }

        [Display(Name = "Users")]
        public List<string>? UserIds { get; set; }

        [Display(Name = "Search Term")]
        public string? SearchTerm { get; set; }

        [Display(Name = "Show Only Critical Items")]
        public bool ShowOnlyCritical { get; set; }

        [Display(Name = "Include Completed Items")]
        public bool IncludeCompleted { get; set; } = true;

        public string? SavedViewName { get; set; }

        // Constructor with default values
        public DashboardFilterViewModel()
        {
            DateFrom = DateTime.Now.AddDays(-30); // Default to last 30 days
            DateTo = DateTime.Now;
            EquipmentStatuses = new List<EquipmentStatus>();
            BuildingIds = new List<int>();
            EquipmentTypeIds = new List<int>();
            AlertPriorities = new List<AlertPriority>();
            MaintenanceStatuses = new List<MaintenanceStatus>();
            UserIds = new List<string>();
        }

        // Helper method to check if any filters are applied
        public bool HasActiveFilters()
        {
            return !string.IsNullOrEmpty(SearchTerm) ||
                   ShowOnlyCritical ||
                   !IncludeCompleted ||
                   (EquipmentStatuses?.Any() == true) ||
                   (BuildingIds?.Any() == true) ||
                   (EquipmentTypeIds?.Any() == true) ||
                   (AlertPriorities?.Any() == true) ||
                   (MaintenanceStatuses?.Any() == true) ||
                   (UserIds?.Any() == true) ||
                   (DateFrom.HasValue && DateFrom != DateTime.Now.AddDays(-30)) ||
                   (DateTo.HasValue && DateTo.Value.Date != DateTime.Now.Date);
        }

        // Convert to query string for URL persistence
        public string ToQueryString()
        {
            var parameters = new List<string>();

            if (DateFrom.HasValue)
                parameters.Add($"dateFrom={DateFrom:yyyy-MM-dd}");
            if (DateTo.HasValue)
                parameters.Add($"dateTo={DateTo:yyyy-MM-dd}");
            if (!string.IsNullOrEmpty(SearchTerm))
                parameters.Add($"search={Uri.EscapeDataString(SearchTerm)}");
            if (ShowOnlyCritical)
                parameters.Add("critical=true");
            if (!IncludeCompleted)
                parameters.Add("excludeCompleted=true");

            if (EquipmentStatuses?.Any() == true)
                parameters.AddRange(EquipmentStatuses.Select(s => $"equipmentStatus={s}"));
            if (BuildingIds?.Any() == true)
                parameters.AddRange(BuildingIds.Select(id => $"building={id}"));
            if (EquipmentTypeIds?.Any() == true)
                parameters.AddRange(EquipmentTypeIds.Select(id => $"equipmentType={id}"));
            if (AlertPriorities?.Any() == true)
                parameters.AddRange(AlertPriorities.Select(p => $"alertPriority={p}"));
            if (MaintenanceStatuses?.Any() == true)
                parameters.AddRange(MaintenanceStatuses.Select(s => $"maintenanceStatus={s}"));
            if (UserIds?.Any() == true)
                parameters.AddRange(UserIds.Select(id => $"user={Uri.EscapeDataString(id)}"));

            return string.Join("&", parameters);
        }
    }

    public class DashboardFilterOptions
    {
        public List<Building> Buildings { get; set; } = new();
        public List<EquipmentType> EquipmentTypes { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public List<SavedDashboardView> SavedViews { get; set; } = new();

        public static Dictionary<EquipmentStatus, string> EquipmentStatusOptions => new()
        {
            { EquipmentStatus.Active, "Active" },
            { EquipmentStatus.Inactive, "Inactive" },
            { EquipmentStatus.Retired, "Retired" }
        };

        public static Dictionary<AlertPriority, string> AlertPriorityOptions => new()
        {
            { AlertPriority.Low, "Low" },
            { AlertPriority.Medium, "Medium" },
            { AlertPriority.High, "High" }
        };

        public static Dictionary<MaintenanceStatus, string> MaintenanceStatusOptions => new()
        {
            { MaintenanceStatus.Pending, "Pending" },
            { MaintenanceStatus.InProgress, "In Progress" },
            { MaintenanceStatus.Completed, "Completed" },
            { MaintenanceStatus.Cancelled, "Cancelled" }
        };
    }

    public class EnhancedDashboardViewModel : DashboardViewModel
    {
        public DashboardFilterViewModel Filters { get; set; } = new();
        public DashboardFilterOptions FilterOptions { get; set; } = new();
        public List<SavedDashboardView> SavedViews { get; set; } = new();
        public bool IsFiltered => Filters.HasActiveFilters();
        public string CurrentViewName { get; set; } = "Default View";
        public long LoadTimeMs { get; set; }
        
        // Additional analytics data for filtered results
        public new Dictionary<string, object> FilteredAnalytics { get; set; } = new();
        public new int TotalRecordsBeforeFilter { get; set; }
        public new int TotalRecordsAfterFilter { get; set; }
    }
}
