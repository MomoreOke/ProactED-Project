using FEENALOoFINALE.Models;

namespace FEENALOoFINALE.Models
{
    // View Models for the unified asset system
    public class UnifiedAssetDashboardViewModel
    {
        // Equipment metrics
        public int TotalEquipment { get; set; }
        public int OperationalEquipment { get; set; }
        public int EquipmentUnderMaintenance { get; set; }

        // Inventory metrics
        public int TotalInventoryItems { get; set; }
        public int LowStockItems { get; set; }
        public int OutOfStockItems { get; set; }

        // Activity data
        public List<MaintenanceLog> RecentMaintenanceLogs { get; set; } = new();
        public List<Alert> ActiveAlerts { get; set; } = new();
        public List<UnifiedAssetViewModel> LowStockAssets { get; set; } = new();
        public List<UnifiedAssetViewModel> RecentAssets { get; set; } = new();

        // Analytics
        public Dictionary<string, int> AssetsByCategory { get; set; } = new();
    }

    public class UnifiedAssetViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? Description { get; set; }
        public DateTime? InstallationDate { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
        public int CurrentStock { get; set; }
        public int MinimumStock { get; set; }
        public bool IsLowStock { get; set; }
        public int AlertCount { get; set; }
        public bool IsEquipment { get; set; }
        public string? CompatibleModels { get; set; }
        public string? Details { get; set; }
        public DateTime? LastUpdated { get; set; }
        
        // Navigation properties for detailed data
        public Equipment? EquipmentData { get; set; }
        public InventoryItem? InventoryData { get; set; }
    }

    public class UnifiedAssetReportViewModel
    {
        public int TotalAssets { get; set; }
        public int EquipmentCount { get; set; }
        public int InventoryItemCount { get; set; }
        public Dictionary<string, int> AssetsByCategory { get; set; } = new();
        public Dictionary<string, int> EquipmentByStatus { get; set; } = new();
        public Dictionary<string, int> InventoryByStatus { get; set; } = new();
    }
}
