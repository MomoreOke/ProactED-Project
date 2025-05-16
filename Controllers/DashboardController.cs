using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FEENALOoFINALE.Data;
using FEENALOoFINALE.Models; // Ensures MaintenanceTask and MaintenanceStatus enum are in scope
using Microsoft.AspNetCore.Authorization;

namespace FEENALOoFINALE.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel
            {
                TotalEquipment = await _context.Equipment.CountAsync(),
                ActiveMaintenanceTasks = await _context.MaintenanceTasks
                    .Where(t => t.Status != MaintenanceStatus.Completed) // Accessing t.Status
                    .CountAsync(),
                LowStockItems = await _context.InventoryItems
                    .Where(i => i.InventoryStocks != null && i.InventoryStocks.Sum(s => s.Quantity) <= i.MinStockLevel)
                    .CountAsync(),
                RecentAlerts = await _context.Alerts
                    .Include(a => a.Equipment) 
                    .Include(a => a.AssignedTo)
                    .OrderByDescending(a => a.CreatedDate)
                    .Take(5)
                    .ToListAsync(),
                UpcomingMaintenances = await _context.MaintenanceTasks
                    .Include(m => m.Equipment) 
                    .Include(m => m.AssignedTo)
                    .Where(m => m.Status != MaintenanceStatus.Completed) // Accessing m.Status
                    .OrderBy(m => m.ScheduledDate)
                    .Take(5)
                    .ToListAsync(),
                EquipmentStatus = await _context.Equipment
                    .GroupBy(e => e.Status)                    
                    .Select(g => new EquipmentStatusCount { Status = g.Key, Count = g.Count() })
                    .ToListAsync()
            };

            return View(viewModel);
        }
    }

    internal class DashboardViewModel
    {
        public int TotalEquipment { get; set; }
        public int ActiveMaintenanceTasks { get; set; }
        public int LowStockItems { get; set; }
        public List<Alert> RecentAlerts { get; set; } = new List<Alert>();
        public List<MaintenanceTask> UpcomingMaintenances { get; set; } = new List<MaintenanceTask>();
        public List<EquipmentStatusCount> EquipmentStatus { get; set; } = new List<EquipmentStatusCount>();
    }
}