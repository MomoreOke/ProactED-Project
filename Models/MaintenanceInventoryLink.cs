using System.ComponentModel.DataAnnotations;

namespace FEENALOoFINALE.Models
{
    public class MaintenanceInventoryLink
    {
        [Key]
        public int LinkId { get; set; }
        public int LogId { get; set; }
        public int ItemId { get; set; }
        public int QuantityUsed { get; set; }

        // Navigation properties
        public MaintenanceLog? MaintenanceLog { get; set; }
        public InventoryItem? InventoryItem { get; set; }
    }
}
