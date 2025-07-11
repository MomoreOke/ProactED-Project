using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FEENALOoFINALE.Models
{
    public class MaintenanceLog
    {
        [Key]
        public int LogId { get; set; }
        
        [Required]
        [Display(Name = "Equipment")]
        public int EquipmentId { get; set; }
        
        [Required]
        public DateTime LogDate { get; set; }
        
        [Required]
        public MaintenanceType MaintenanceType { get; set; }
        
        public string? Description { get; set; }
        
        [Required]
        public string Technician { get; set; } = string.Empty;
        
        public TimeSpan? DowntimeDuration { get; set; }

        // Helper property for form input (hours as decimal)
        [NotMapped]
        public double? DowntimeHours
        {
            get => DowntimeDuration?.TotalHours;
            set => DowntimeDuration = value.HasValue ? TimeSpan.FromHours(value.Value) : null;
        }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; } = 0;

        public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Completed;

        public int? AlertId { get; set; }
        public Alert? Alert { get; set; }

        // New property for linking to maintenance task
        public int? MaintenanceTaskId { get; set; }
        public MaintenanceTask? Task { get; set; }

        // Navigation properties - explicitly exclude from validation
        [ValidateNever]
        public Equipment? Equipment { get; set; }
        public ICollection<MaintenanceInventoryLink> MaintenanceInventoryLinks { get; set; } = new List<MaintenanceInventoryLink>();
    }

    public enum MaintenanceType
    {
        Preventive,
        Corrective,
        Inspection,
        Emergency
    }
}
