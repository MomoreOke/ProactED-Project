namespace FEENALOoFINALE.Models
{
    public class DashboardViewModel
    {
        public int TotalEquipment { get; set; }
        public int ActiveMaintenanceTasks { get; set; }
        public int LowStockItems { get; set; }
        public List<Alert>? RecentAlerts { get; set; }
        public List<MaintenanceTask>? UpcomingMaintenances { get; set; }
        public required List<EquipmentStatusCount> EquipmentStatus { get; set; }
    }

    public class EquipmentStatusCount
    {
        public EquipmentStatus Status { get; set; }
        public int Count { get; set; }
    }
}