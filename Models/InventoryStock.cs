using System.ComponentModel.DataAnnotations;

namespace FEENALOoFINALE.Models
{
    public class InventoryStock
    {
        [Key]
        public int StockId { get; set; }
        public int ItemId { get; set; }
        public int CurrentQuantity { get; set; }
        public DateTime LastRestockDate { get; set; }
        public string Location { get; set; }

        // Navigation property
        public InventoryItem InventoryItem { get; set; }
    }
}
