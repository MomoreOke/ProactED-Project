using System.ComponentModel.DataAnnotations;

namespace FEENALOoFINALE.Models
{
    public class Alert
    {
        [Key]
        public int AlertId { get; set; }
        
        [Display(Name = "Equipment")]
        public int? EquipmentId { get; set; }
        
        [Display(Name = "Inventory Item")]
        public int? InventoryItemId { get; set; }
        
        [Display(Name = "Title")]
        public string? Title { get; set; }
        
        [Required]
        [Display(Name = "Description")]
        public required string Description { get; set; }
        
        [Display(Name = "Priority")]
        public AlertPriority Priority { get; set; }
        
        [Display(Name = "Status")]
        public AlertStatus Status { get; set; }
        
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }
        
        [Display(Name = "Assigned To")]
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
        Resolved,   // <-- Add this line
        Closed
    }
}
