using Microsoft.AspNetCore.Mvc;
using FEENALOoFINALE.Models;
using System.Linq;
using FEENALOoFINALE.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using FEENALOoFINALE.Services;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;

namespace FEENALOoFINALE.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAdvancedAnalyticsService _analyticsService;
        private readonly IExportService _exportService;
        private readonly UserManager<User> _userManager;

        public ReportController(ApplicationDbContext context, IAdvancedAnalyticsService analyticsService, IExportService exportService, UserManager<User> userManager)
        {
            _context = context;
            _analyticsService = analyticsService;
            _exportService = exportService;
            _userManager = userManager;
        }

        // Main Reports Dashboard
        public async Task<IActionResult> Index()
        {
            var reportSummary = new ReportDashboardViewModel
            {
                TotalEquipment = await _context.Equipment.CountAsync(),
                TotalAlerts = await _context.Alerts.CountAsync(),
                TotalMaintenanceLogs = await _context.MaintenanceLogs.CountAsync(),
                TotalInventoryItems = await _context.InventoryItems.CountAsync(),
                RecentMaintenanceLogs = await _context.MaintenanceLogs
                    .Include(m => m.Equipment)
                        .ThenInclude(e => e!.EquipmentModel)
                    .OrderByDescending(m => m.LogDate)
                    .Take(5)
                    .ToListAsync(),
                TopAlerts = await _context.Alerts
                    .Include(a => a.Equipment)
                        .ThenInclude(e => e!.EquipmentModel)
                    .Where(a => a.Priority == AlertPriority.High)
                    .OrderByDescending(a => a.CreatedDate)
                    .Take(5)
                    .ToListAsync()
            };

            return View(reportSummary);
        }

        // Inventory Report
        public async Task<IActionResult> Inventory()
        {
            var items = await _context.InventoryItems
                .Include(i => i.InventoryStocks)
                .ToListAsync();
            return View(items);
        }

        // Alerts Report
        public async Task<IActionResult> Alerts()
        {
            var alerts = await _context.Alerts
                .Include(a => a.Equipment)
                    .ThenInclude(e => e!.EquipmentModel)
                .Include(a => a.AssignedTo)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
            return View(alerts);
        }

        // Maintenance Logs Report
        public async Task<IActionResult> MaintenanceLogs()
        {
            var logs = await _context.MaintenanceLogs
                .Include(m => m.Equipment)
                    .ThenInclude(e => e!.EquipmentModel)
                .OrderByDescending(m => m.LogDate)
                .ToListAsync();
            return View(logs);
        }
    }
}