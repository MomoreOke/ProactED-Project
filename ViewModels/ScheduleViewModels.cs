using FEENALOoFINALE.Models;
using System.ComponentModel.DataAnnotations;

namespace FEENALOoFINALE.ViewModels
{
    public class ScheduleViewModel
    {
        public string PageTitle { get; set; } = "Maintenance Schedule";
        public string PageDescription { get; set; } = "View and manage maintenance schedules for all equipment";

        public List<MaintenanceTask> UpcomingTasks { get; set; } = new();
        public List<MaintenanceTask> OverdueTasks { get; set; } = new();
        public List<MaintenanceTask> InProgressTasks { get; set; } = new();
        public List<MaintenanceTask> RecentlyCompleted { get; set; } = new();

        public ScheduleStatistics Statistics { get; set; } = new();
    }

    public class ScheduleCalendarViewModel
    {
        public string PageTitle { get; set; } = "Maintenance Calendar";
        public string PageDescription { get; set; } = "Calendar view of all scheduled maintenance tasks";

        public List<MaintenanceTask> MaintenanceTasks { get; set; } = new();
    }

    public class MaintenanceTaskDetailsViewModel
    {
        public MaintenanceTask MaintenanceTask { get; set; } = null!;
        public List<MaintenanceLog> MaintenanceHistory { get; set; } = new();
    }

    public class CreateMaintenanceTaskViewModel
    {
        [Required]
        [Display(Name = "Equipment")]
        public int EquipmentId { get; set; }

        [Required]
        [Display(Name = "Description")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Priority")]
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        [Display(Name = "Scheduled Date")]
        [DataType(DataType.DateTime)]
        public DateTime? ScheduledDate { get; set; }

        [Display(Name = "Assigned To")]
        public string? AssignedToUserId { get; set; }
    }

    public class ScheduleStatistics
    {
        public int TotalUpcoming { get; set; }
        public int TotalOverdue { get; set; }
        public int TotalInProgress { get; set; }
        public int TotalCompletedThisWeek { get; set; }
        public int HighPriorityCount { get; set; }
        public int MediumPriorityCount { get; set; }
        public int LowPriorityCount { get; set; }

        public double CompletionRate => TotalCompletedThisWeek > 0 && (TotalCompletedThisWeek + TotalOverdue) > 0
            ? Math.Round((double)TotalCompletedThisWeek / (TotalCompletedThisWeek + TotalOverdue) * 100, 1)
            : 0;

        public string OverallHealthStatus
        {
            get
            {
                if (TotalOverdue > 10) return "Critical";
                if (TotalOverdue > 5 || HighPriorityCount > 5) return "Warning";
                if (TotalInProgress > 0 || TotalUpcoming > 0) return "Active";
                return "Good";
            }
        }

        public string HealthStatusColor => OverallHealthStatus switch
        {
            "Critical" => "danger",
            "Warning" => "warning",
            "Active" => "primary",
            "Good" => "success",
            _ => "secondary"
        };
    }

    public class ScheduleFilterOptions
    {
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public string? EquipmentType { get; set; }
        public string? Building { get; set; }
        public string? AssignedTo { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? SearchTerm { get; set; }
    }
}
