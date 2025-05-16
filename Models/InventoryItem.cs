using System.ComponentModel.DataAnnotations;

namespace FEENALOoFINALE.Models
{
    public class InventoryItem
    {
        public int ItemId { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        
        [Range(0, int.MaxValue)]
        public int MinimumStockLevel { get; set; }  // Add this property
        public ItemCategory Category { get; set; }
        public int MinStockLevel { get; set; }
        public int MaxStockLevel { get; set; }
        public int ReorderPoint { get; set; }
        public int ReorderQuantity { get; set; }
        public string? UnitOfMeasure { get; set; }
        public string? CompatibleModels { get; set; } // Stored as JSON

        // Navigation properties
        public ICollection<InventoryStock>? InventoryStocks { get; set; }
        public ICollection<MaintenanceInventoryLink>? MaintenanceInventoryLinks { get; set; }
        public ICollection<Alert>? Alerts { get; set; }
    }

    public enum ItemCategory
    {
        Electrical,
        Mechanical,
        Consumable
    }
}
