using System.ComponentModel.DataAnnotations;

namespace FEENALOoFINALE.Models
{
    public class InventoryItem
    {   
        [Key]
        public int ItemId { get; set; }
        public string Name { get; set; }
        public ItemCategory Category { get; set; }
        public int MinStockLevel { get; set; }
        public string CompatibleModels { get; set; } // Stored as JSON

        // Navigation properties
        public ICollection<InventoryStock> InventoryStocks { get; set; }
        public ICollection<MaintenanceInventoryLink> MaintenanceInventoryLinks { get; set; }
        public ICollection<Alert> Alerts { get; set; }
    }

    public enum ItemCategory
    {
        Electrical,
        Mechanical,
        Consumable
    }
}
