using System.ComponentModel.DataAnnotations;

namespace FEENALOoFINALE.Models
{
    public class MaintenanceLog
    {
        [Key]
        public int LogId { get; set; }
        public int EquipmentId { get; set; }
        public DateTime LogDate { get; set; }
        public MaintenanceType MaintenanceType { get; set; }
        public string? Description { get; set; }
        public required string Technician { get; set; } // 'required' is fine for value types or strings if needed for DTOs/creation
        public int DowntimeDuration { get; set; }

        // Navigation properties
        public Equipment Equipment { get; set; } = null!; // Changed from 'public required Equipment Equipment'
        public ICollection<MaintenanceInventoryLink>? MaintenanceInventoryLinks { get; set; }
    }

    public enum MaintenanceType
    {
        Preventive,
        Corrective,
        Inspection
    }
}
