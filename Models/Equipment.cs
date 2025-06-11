using System.ComponentModel.DataAnnotations;

namespace FEENALOoFINALE.Models
{
    public class Equipment
    {
        [Key]
        public int EquipmentId { get; set; }
        
        // Remove Name and Model properties
        // public required string Name { get; set; }
        // public required string Model { get; set; }
        
        // New foreign keys and navigation properties
        public int EquipmentTypeId { get; set; }
        public EquipmentType? EquipmentType { get; set; }

        [Required]
        public int EquipmentModelId { get; set; }
        public EquipmentModel? EquipmentModel { get; set; }
        
        // Remove Location property
        // public required string Location { get; set; }
        
        // New foreign keys and navigation properties for location
        public int BuildingId { get; set; }
        public Building? Building { get; set; }

        public int RoomId { get; set; }
        public Room? Room { get; set; }

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
