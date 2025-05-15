using System.ComponentModel.DataAnnotations;

namespace FEENALOoFINALE.Models
{
    public class Alert
    {
        [Key]
        public int AlertId { get; set; }
        public int? EquipmentId { get; set; }
        public int? InventoryItemId { get; set; }
        public string? Title { get; set; }
        public required string Description { get; set; }
        public AlertPriority Priority { get; set; }
        public AlertStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? AssignedToUserId { get; set; }

        // Navigation properties
        public Equipment? Equipment { get; set; }
        public InventoryItem? InventoryItem { get; set; }
        public User? AssignedTo { get; set; }
    }

    public enum AlertPriority
    {
        Low,
        Medium,
        High
    }

    public enum AlertStatus
    {
        Open,
        InProgress,
        Closed
    }
}
