using Microsoft.AspNetCore.Mvc;
using FEENALOoFINALE.Models;
using System.Linq;

namespace FEENALOoFINALE.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Inventory Report
        public IActionResult Inventory()
        {
            var items = _context.InventoryItems.ToList();
            return View(items);
        }

        // Alerts Report
        public IActionResult Alerts()
        {
            var alerts = _context.Alerts.ToList();
            return View(alerts);
        }

        // Maintenance Logs Report
        public IActionResult MaintenanceLogs()
        {
            var logs = _context.MaintenanceLogs.ToList();
            return View(logs);
        }
    }
}