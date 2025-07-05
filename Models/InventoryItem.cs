using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FEENALOoFINALE.Models
{
    public class InventoryItem
    {
        public int ItemId { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        
        [Range(0, int.MaxValue)]
        public int MinimumStockLevel { get; set; }
        public ItemCategory Category { get; set; }
        public int MinStockLevel { get; set; }
        public int MaxStockLevel { get; set; }
        public int ReorderPoint { get; set; }
        public int ReorderQuantity { get; set; }
        public string? CompatibleModels { get; set; } // Stored as JSON

        [NotMapped] // This ensures EF Core does NOT create a column for this in the database
        public int InitialStock { get; set; }

        // Navigation properties
        public ICollection<InventoryStock>? InventoryStocks { get; set; }
        public ICollection<MaintenanceInventoryLink> MaintenanceInventoryLinks { get; set; } = new List<MaintenanceInventoryLink>();
        public ICollection<Alert>? Alerts { get; set; }
    }

    public enum ItemCategory
    {
        Electrical,
        Mechanical,
        Consumable
    }
}
