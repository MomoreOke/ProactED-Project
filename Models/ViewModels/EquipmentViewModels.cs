using System.ComponentModel.DataAnnotations;
using FEENALOoFINALE.Models.ViewModels;

namespace FEENALOoFINALE.Models.ViewModels
{
    /// <summary>
    /// Enhanced equipment management view model
    /// </summary>
    public class EquipmentManagementViewModel : PaginatedViewModel
    {
        public List<EquipmentItemViewModel> Equipment { get; set; } = new();
        public EquipmentFilterOptions FilterOptions { get; set; } = new();
        public EquipmentStatistics Statistics { get; set; } = new();
        public List<EquipmentBulkAction> BulkActions { get; set; } = new();
        public bool CanExport { get; set; } = true;
        public bool CanBulkEdit { get; set; } = true;
        public string[] SelectedEquipmentIds { get; set; } = Array.Empty<string>();
    }

    public class EquipmentItemViewModel
    {
        public int EquipmentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Building { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
        public EquipmentStatus Status { get; set; }
        public DateTime? InstallationDate { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        public int MaintenanceOverdueDays { get; set; }
        public string StatusText => Status.ToString();
        public string StatusBadgeClass => GetStatusBadgeClass();
        public bool IsOverdue => MaintenanceOverdueDays > 0;
        public bool NeedsAttention => IsOverdue || Status == EquipmentStatus.Inactive;
        public int AlertCount { get; set; }
        public decimal? LastKnownEfficiency { get; set; }
        public string Notes { get; set; } = string.Empty;
        
        // Quick actions available for this equipment
        public List<QuickActionItem> AvailableActions { get; set; } = new();
        
        private string GetStatusBadgeClass()
        {
            return Status switch
            {
                EquipmentStatus.Active => "bg-success",
                EquipmentStatus.Inactive => "bg-warning",
                EquipmentStatus.Retired => "bg-danger",
                _ => "bg-secondary"
            };
        }
    }

    public class EquipmentFilterOptions
    {
        public List<BuildingOption> Buildings { get; set; } = new();
        public List<EquipmentTypeOption> EquipmentTypes { get; set; } = new();
        public List<StatusOption> Statuses { get; set; } = new();
        public List<MaintenanceStatusOption> MaintenanceStatuses { get; set; } = new();
    }

    public class BuildingOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int EquipmentCount { get; set; }
    }

    public class EquipmentTypeOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int EquipmentCount { get; set; }
    }

    public class StatusOption
    {
        public EquipmentStatus Status { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class MaintenanceStatusOption
    {
        public string Status { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class EquipmentStatistics
    {
        public int TotalEquipment { get; set; }
        public int OperationalCount { get; set; }
        public int MaintenanceCount { get; set; }
        public int OutOfServiceCount { get; set; }
        public int OverdueMaintenanceCount { get; set; }
        public double AverageEfficiency { get; set; }
        public decimal TotalMaintenanceCost { get; set; }
        public int CriticalAlertsCount { get; set; }
    }

    public class EquipmentBulkAction
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = "primary";
        public bool RequiresConfirmation { get; set; } = true;
        public string ConfirmationMessage { get; set; } = string.Empty;
        public string[] RequiredRoles { get; set; } = Array.Empty<string>();
    }

    /// <summary>
    /// Equipment creation/editing view model
    /// </summary>
    public class EquipmentFormViewModel : BaseViewModel
    {
        public int? EquipmentId { get; set; }
        public bool IsEdit => EquipmentId.HasValue;

        [Required(ErrorMessage = "Equipment type is required")]
        [Display(Name = "Equipment Type")]
        public int EquipmentTypeId { get; set; }

        [Required(ErrorMessage = "Equipment model is required")]
        [Display(Name = "Equipment Model")]
        public int EquipmentModelId { get; set; }

        [Display(Name = "Custom Model Name")]
        public string? CustomModelName { get; set; }

        [Required(ErrorMessage = "Building is required")]
        [Display(Name = "Building")]
        public int BuildingId { get; set; }

        [Required(ErrorMessage = "Room is required")]
        [Display(Name = "Room")]
        public int RoomId { get; set; }

        [Display(Name = "Installation Date")]
        [DataType(DataType.Date)]
        public DateTime? InstallationDate { get; set; }

        [Required(ErrorMessage = "Expected lifespan is required")]
        [Range(1, 600, ErrorMessage = "Expected lifespan must be between 1 and 600 months")]
        [Display(Name = "Expected Lifespan (Months)")]
        public int ExpectedLifespanMonths { get; set; } = 120;

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public EquipmentStatus Status { get; set; } = EquipmentStatus.Active;

        [Display(Name = "Notes")]
        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }

        // Form options
        public List<EquipmentTypeOption> EquipmentTypes { get; set; } = new();
        public List<EquipmentModelOption> EquipmentModels { get; set; } = new();
        public List<BuildingOption> Buildings { get; set; } = new();
        public List<RoomOption> Rooms { get; set; } = new();

        // Validation
        public Dictionary<string, List<string>> ValidationErrors { get; set; } = new();
        public bool HasValidationErrors => ValidationErrors.Any();
    }

    public class EquipmentModelOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int EquipmentTypeId { get; set; }
        public string Manufacturer { get; set; } = string.Empty;
        public string Specifications { get; set; } = string.Empty;
    }

    public class RoomOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int BuildingId { get; set; }
        public string BuildingName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Equipment details view model
    /// </summary>
    public class EquipmentDetailsViewModel : BaseViewModel
    {
        public Equipment Equipment { get; set; } = new();
        public List<MaintenanceLogSummary> RecentMaintenanceLogs { get; set; } = new();
        public List<Alert> RecentAlerts { get; set; } = new();
        public List<FailurePredictionSummary> Predictions { get; set; } = new();
        public EquipmentPerformanceMetrics Performance { get; set; } = new();
        public List<QuickActionItem> AvailableActions { get; set; } = new();
        public bool CanEdit { get; set; } = true;
        public bool CanDelete { get; set; } = true;
    }

    public class MaintenanceLogSummary
    {
        public int LogId { get; set; }
        public DateTime MaintenanceDate { get; set; }
        public string TaskType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TechnicianName { get; set; } = string.Empty;
        public MaintenanceStatus Status { get; set; }
        public decimal? Cost { get; set; }
        public int Duration { get; set; } // in minutes
    }

    public class FailurePredictionSummary
    {
        public int PredictionId { get; set; }
        public string PredictionType { get; set; } = string.Empty;
        public DateTime PredictedDate { get; set; }
        public double Probability { get; set; }
        public string Severity { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
    }

    public class EquipmentPerformanceMetrics
    {
        public double CurrentEfficiency { get; set; }
        public double AverageEfficiency { get; set; }
        public int UptimePercentage { get; set; }
        public int DowntimeHours { get; set; }
        public decimal MaintenanceCostMTD { get; set; }
        public decimal MaintenanceCostYTD { get; set; }
        public int MaintenanceFrequency { get; set; } // days between maintenance
        public DateTime? LastMaintenanceDate { get; set; }
        public DateTime? NextScheduledMaintenance { get; set; }
    }
}
