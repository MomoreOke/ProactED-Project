using System.ComponentModel.DataAnnotations;

namespace FEENALOoFINALE.Models
{
    public class InventoryViewModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public ItemCategory Category { get; set; }

        // This is the amount to add at creation time.
        [Range(0, int.MaxValue, ErrorMessage = "Initial stock must be non-negative.")]
        public int InitialStock { get; set; }

        // Equipment linking properties
        public int? EquipmentTypeId { get; set; }
        public string EquipmentModelName { get; set; } = string.Empty;
        
        // Stock management properties - Updated for cost removal
        public string? CompatibleModels { get; set; }
        
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Minimum stock level must be non-negative.")]
        public int MinimumStockLevel { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "Maximum stock level must be non-negative.")]
        public int? MaxStockLevel { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "Reorder point must be non-negative.")]
        public int? ReorderPoint { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "Reorder quantity must be non-negative.")]
        public int? ReorderQuantity { get; set; }
    }
}