using System.ComponentModel.DataAnnotations;

namespace FEENALOoFINALE.Models
{
    public class ReportDashboardViewModel
    {
        public int TotalEquipment { get; set; }
        public int TotalAlerts { get; set; }
        public int TotalMaintenanceLogs { get; set; }
        public int TotalInventoryItems { get; set; }
        public List<MaintenanceLog>? RecentMaintenanceLogs { get; set; }
        public List<Alert>? TopAlerts { get; set; }
    }
}
