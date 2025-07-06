using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FEENALOoFINALE.Models
{
    public class MaintenanceTask
    {
        [Key]
        public int TaskId { get; set; }

        [Required]
        public int EquipmentId { get; set; }

        [ForeignKey("EquipmentId")]
        public Equipment Equipment { get; set; } = null!;

        [Required]
        public DateTime ScheduledDate { get; set; }

        [Required]
        public MaintenanceStatus Status { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty; // Initialize or make nullable

        public string? AssignedToUserId { get; set; }

        [ForeignKey("AssignedToUserId")]
        public User? AssignedTo { get; set; } // Make nullable if it can be null

        // New properties for enhanced workflow
        public int? CreatedFromAlertId { get; set; }  // Link to originating alert
        public Alert? OriginatingAlert { get; set; }  // Navigation property
        public DateTime? CompletedDate { get; set; }  // When task was completed
        public TaskPriority Priority { get; set; } = TaskPriority.Medium; // Task priority
    }

    public enum TaskPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum MaintenanceStatus
    {
        Pending,
        InProgress,
        Completed,
        Cancelled
        // Add other statuses if needed
    }
}