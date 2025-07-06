using System.ComponentModel.DataAnnotations;

namespace FEENALOoFINALE.Models
{
    public class AssetDashboardViewModel
    {
        // Equipment data
        public List<Equipment> Equipment { get; set; } = new();
        
        // Inventory data
        public List<InventoryItem> InventoryItems { get; set; } = new();
        
        // Summary statistics
        public int TotalEquipment { get; set; }
        public int ActiveEquipment { get; set; }
        public int TotalInventoryItems { get; set; }
        public int LowStockItems { get; set; }
        
        // Recent activity
        public List<MaintenanceLog> RecentMaintenanceLogs { get; set; } = new();
        public List<Alert> CriticalAlerts { get; set; } = new();
        
        // Calculated properties
        public int InactiveEquipment => TotalEquipment - ActiveEquipment;
        public decimal EquipmentUtilization => TotalEquipment > 0 ? (decimal)ActiveEquipment / TotalEquipment * 100 : 0;
        public int NormalStockItems => TotalInventoryItems - LowStockItems;
        public decimal StockHealthPercentage => TotalInventoryItems > 0 ? (decimal)NormalStockItems / TotalInventoryItems * 100 : 0;
    }

    public class AssetInventoryViewModel
    {
        public List<AssetInventoryItemViewModel> Items { get; set; } = new();
    }

    public class AssetInventoryItemViewModel
    {
        public required InventoryItem Item { get; set; }
        public decimal TotalStock { get; set; }
        public decimal TotalValue { get; set; }
        public bool IsLowStock { get; set; }
    }

    public class AssetReportViewModel
    {
        // Equipment statistics
        public List<object> EquipmentByStatus { get; set; } = new();
        public List<object> EquipmentByType { get; set; } = new();
        
        // Inventory statistics
        public List<object> InventoryByCategory { get; set; } = new();
        public List<InventoryItem> LowStockItems { get; set; } = new();
        
        // Maintenance insights
        public decimal TotalMaintenanceCost { get; set; }
        public List<object> MaintenanceByEquipment { get; set; } = new();
    }
}
