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

        public string Description { get; set; }

        public string? AssignedToUserId { get; set; }

        [ForeignKey("AssignedToUserId")]
        public User AssignedTo { get; set; }
    }

    public enum MaintenanceStatus
    {
        Pending,
        InProgress,
        Completed,
        Cancelled
    }
}