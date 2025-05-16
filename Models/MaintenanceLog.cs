using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FEENALOoFINALE.Models
{
    public class MaintenanceLog
    {
        [Key]
        public int LogId { get; set; }
        [DataType(DataType.Date)]
        public DateTime MaintenanceDate { get; set; }  // Add this property
        public int EquipmentId { get; set; }
        public DateTime LogDate { get; set; }
        public MaintenanceType MaintenanceType { get; set; }
        public string? Description { get; set; }
        public required string Technician { get; set; } // 'required' is fine for value types or strings if needed for DTOs/creation
        public TimeSpan? DowntimeDuration { get; set; }

        [Column(TypeName = "decimal(18,2)")] // Example: Store as decimal with 2 decimal places
        public decimal Cost { get; set; }

        public MaintenanceStatus Status { get; set; } // Assuming you have a MaintenanceStatus enum

        // Navigation properties
        public Equipment Equipment { get; set; } = null!; // Changed from 'public required Equipment Equipment'
        public ICollection<MaintenanceInventoryLink> MaintenanceInventoryLinks { get; set; } = new List<MaintenanceInventoryLink>();
    }

    public enum MaintenanceType
    {
        Preventive,
        Corrective,
        Inspection
    }
}
