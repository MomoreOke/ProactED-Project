using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FEENALOoFINALE.Models
{
    public class InventoryStock
    {
        [Key]
        public int StockId { get; set; }

        [Required]
        public int ItemId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit cost must be greater than 0")]
        [DataType(DataType.Currency)]
        [Display(Name = "Unit Cost")]
        public decimal UnitCost { get; set; }

        [Required]
        [Display(Name = "Date Received")]
        [DataType(DataType.Date)]
        public DateTime DateReceived { get; set; }

        [Required]
        [Display(Name = "Batch Number")]
        [StringLength(50)]
        public string? BatchNumber { get; set; }

        [ForeignKey("ItemId")]
        public virtual InventoryItem? InventoryItem { get; set; }
    }
}
