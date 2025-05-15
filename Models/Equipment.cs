using System.ComponentModel.DataAnnotations;

namespace FEENALOoFINALE.Models
{
    public class Equipment
    {
        [Key]
        public int EquipmentId { get; set; }
        public required string EquipmentName { get; set; }
        public string? Model { get; set; }
        public required string Location { get; set; }
        public DateTime InstallationDate { get; set; }
        public int ExpectedLifespanMonths { get; set; }
        public EquipmentStatus Status { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public ICollection<MaintenanceLog>? MaintenanceLogs { get; set; }
        public ICollection<FailurePrediction>? FailurePredictions { get; set; }
        public ICollection<Alert>? Alerts { get; set; }
    }

    public enum EquipmentStatus
    {
        Active,
        Inactive,
        Retired
    }
}
