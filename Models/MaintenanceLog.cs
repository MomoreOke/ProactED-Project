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
        public string Description { get; set; }
        public string Technician { get; set; }
        public int DowntimeDuration { get; set; }

        // Navigation properties
        public Equipment Equipment { get; set; }
        public ICollection<MaintenanceInventoryLink> MaintenanceInventoryLinks { get; set; }
    }

    public enum MaintenanceType
    {
        Preventive,
        Corrective,
        Inspection
    }
}
