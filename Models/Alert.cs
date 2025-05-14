using System.ComponentModel.DataAnnotations;

namespace FEENALOoFINALE.Models
{
    public class Alert
    {
        [Key]
        public int AlertId { get; set; }
        public int? EquipmentId { get; set; }
        public int? InventoryItemId { get; set; }
        public AlertType AlertType { get; set; }
        public DateTime AlertDate { get; set; }
        public AlertStatus Status { get; set; }
        public AlertPriority Priority { get; set; }
        public int? AssignedToUserId { get; set; }

        // Navigation properties
        public Equipment Equipment { get; set; }
        public InventoryItem InventoryItem { get; set; }
        public User AssignedTo { get; set; }
    }

    public enum AlertType
    {
        FailurePrediction,
        LowStock,
        MaintenanceDue
    }

    public enum AlertStatus
    {
        Pending,
        Acknowledged,
        Resolved
    }

    public enum AlertPriority
    {
        Low,
        Medium,
        High
    }
}
