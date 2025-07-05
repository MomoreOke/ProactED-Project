namespace FEENALOoFINALE.Models
{
    public class DashboardViewModel
    {
        public int TotalEquipment { get; set; }
        public int ActiveMaintenanceTasks { get; set; }
        public int LowStockItems { get; set; }
        public int CriticalAlerts { get; set; }
        public int OverdueMaintenances { get; set; }
        public int EquipmentNeedingAttention { get; set; }
        public int CompletedMaintenanceTasks { get; set; }
        public int TotalInventoryItems { get; set; }
        public List<Alert>? RecentAlerts { get; set; }
        public List<MaintenanceTask>? UpcomingMaintenances { get; set; }
        public List<MaintenanceTask>? UpcomingMaintenanceTasks { get; set; }
        public List<EquipmentStatusCount> EquipmentStatus { get; set; } = new List<EquipmentStatusCount>();
    }

    public class EquipmentStatusCount
    {
        public EquipmentStatus Status { get; set; }
        public int Count { get; set; }
    }
}