using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FEENALOoFINALE.Data;
using FEENALOoFINALE.Models;
using Microsoft.AspNetCore.Authorization;

namespace FEENALOoFINALE.Controllers
{
    [Authorize]
    public class AssetController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AssetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Main Unified Asset Management Dashboard
        public async Task<IActionResult> Index()
        {
            var viewModel = new UnifiedAssetDashboardViewModel
            {
                // Equipment Summary (from Equipment table)
                TotalEquipment = await _context.Equipment.CountAsync(),
                OperationalEquipment = await _context.Equipment
                    .CountAsync(e => e.Status == EquipmentStatus.Active),
                EquipmentUnderMaintenance = await _context.Equipment
                    .CountAsync(e => e.Status == EquipmentStatus.Inactive),

                // Inventory Summary (from InventoryItem table)
                TotalInventoryItems = await _context.InventoryItems.CountAsync(),
                LowStockItems = await GetLowStockCount(),
                OutOfStockItems = await GetOutOfStockCount(),

                // Recent maintenance activity
                RecentMaintenanceLogs = await _context.MaintenanceLogs
                    .Include(ml => ml.Equipment)
                    .OrderByDescending(ml => ml.LogDate)
                    .Take(5)
                    .ToListAsync(),

                // Active alerts (equipment-related only)
                ActiveAlerts = await _context.Alerts
                    .Include(a => a.Equipment)
                    .Where(a => a.Status == AlertStatus.Open && a.EquipmentId.HasValue)
                    .OrderByDescending(a => a.Priority)
                    .ThenByDescending(a => a.CreatedDate)
                    .Take(5)
                    .ToListAsync(),

                // Combined asset list (Equipment + Inventory as unified view)
                RecentAssets = await GetRecentUnifiedAssets(10)
            };

            return View(viewModel);
        }

        // Equipment Management - List all equipment
        public async Task<IActionResult> Equipment()
        {
            var equipment = await _context.Equipment
                .Include(e => e.EquipmentModel)
                .Include(e => e.EquipmentType)
                .Include(e => e.Building)
                .Include(e => e.Room)
                .ToListAsync();

            var viewModel = equipment.Select(e => new UnifiedAssetViewModel
            {
                Id = e.EquipmentId,
                Name = e.EquipmentModel?.ModelName ?? "Unknown Model",
                Type = "Equipment",
                Category = e.EquipmentType?.EquipmentTypeName ?? "Unknown Type",
                Status = e.Status.ToString(),
                Location = $"{e.Building?.BuildingName} - {e.Room?.RoomName}",
                Details = $"Installed: {e.InstallationDate:yyyy-MM-dd}",
                LastUpdated = e.InstallationDate
            }).ToList();

            return View("EquipmentNew", viewModel);
        }

        // Inventory Management - List all inventory items
        public async Task<IActionResult> Inventory()
        {
            var inventory = await _context.InventoryItems
                .Include(i => i.InventoryStocks)
                .ToListAsync();

            var viewModel = inventory.Select(i => new UnifiedAssetViewModel
            {
                Id = i.ItemId,
                Name = i.Name,
                Type = "Inventory",
                Category = i.Category.ToString(),
                Status = GetInventoryStatus(i),
                Location = "Warehouse"
            }).ToList();

            return View("InventoryNew", viewModel);
        }

        // Helper method to get recent unified assets (Equipment + Inventory)
        private async Task<List<UnifiedAssetViewModel>> GetRecentUnifiedAssets(int count)
        {
            var assets = new List<UnifiedAssetViewModel>();

            // Get recent equipment (by installation date)
            var recentEquipment = await _context.Equipment
                .Include(e => e.EquipmentModel)
                .Include(e => e.EquipmentType)
                .Include(e => e.Building)
                .Include(e => e.Room)
                .OrderByDescending(e => e.InstallationDate)
                .Take(count / 2)
                .ToListAsync();

            assets.AddRange(recentEquipment.Select(e => new UnifiedAssetViewModel
            {
                Id = e.EquipmentId,
                Name = e.EquipmentModel?.ModelName ?? "Unknown Model",
                Type = "Equipment",
                Category = e.EquipmentType?.EquipmentTypeName ?? "Unknown Type",
                Status = e.Status.ToString(),
                Location = $"{e.Building?.BuildingName} - {e.Room?.RoomName}",
                Details = $"Installed: {e.InstallationDate:yyyy-MM-dd}",
                LastUpdated = e.InstallationDate
            }));

            // Get recent inventory items (by ID since there's no created date)
            var recentInventory = await _context.InventoryItems
                .Include(i => i.InventoryStocks)
                .OrderByDescending(i => i.ItemId)
                .Take(count / 2)
                .ToListAsync();

            assets.AddRange(recentInventory.Select(i => new UnifiedAssetViewModel
            {
                Id = i.ItemId,
                Name = i.Name,
                Type = "Inventory",
                Category = i.Category.ToString(),
                Status = GetInventoryStatus(i),
                Location = "Warehouse",
                Details = $"Stock: {GetCurrentStock(i)} | Min: {i.MinimumStockLevel}",
                LastUpdated = DateTime.Now
            }));

            return assets.OrderByDescending(a => a.LastUpdated).Take(count).ToList();
        }

        // Helper method to get current stock for an inventory item
        private int GetCurrentStock(InventoryItem item)
        {
            return (int)(item.InventoryStocks?.Sum(s => s.Quantity) ?? 0);
        }

        // Helper method to determine inventory status
        private string GetInventoryStatus(InventoryItem item)
        {
            var currentStock = GetCurrentStock(item);
            
            if (currentStock == 0)
                return "Out of Stock";
            else if (currentStock <= item.MinimumStockLevel)
                return "Low Stock";
            else
                return "In Stock";
        }

        // Helper method to get low stock count
        private async Task<int> GetLowStockCount()
        {
            var inventoryItems = await _context.InventoryItems
                .Include(i => i.InventoryStocks)
                .ToListAsync();

            return inventoryItems.Count(i => GetCurrentStock(i) <= i.MinimumStockLevel);
        }

        // Helper method to get out of stock count
        private async Task<int> GetOutOfStockCount()
        {
            var inventoryItems = await _context.InventoryItems
                .Include(i => i.InventoryStocks)
                .ToListAsync();

            return inventoryItems.Count(i => GetCurrentStock(i) == 0);
        }

        // Asset details - handles both equipment and inventory
        public IActionResult Details(int id, string type)
        {
            if (type?.ToLower() == "equipment")
            {
                return RedirectToAction("Details", "Equipment", new { id = id });
            }
            else if (type?.ToLower() == "inventory")
            {
                return RedirectToAction("Details", "Inventory", new { id = id });
            }

            return NotFound();
        }

        // Quick Actions
        public IActionResult CreateEquipment()
        {
            return RedirectToAction("Create", "Equipment");
        }

        public IActionResult CreateInventory()
        {
            return RedirectToAction("Create", "Inventory");
        }

        public IActionResult ManageAlerts()
        {
            return RedirectToAction("Index", "Alert");
        }

        public IActionResult MaintenanceSchedule()
        {
            return RedirectToAction("Index", "MaintenanceLog");
        }

        public IActionResult Reports()
        {
            return RedirectToAction("Index", "Report");
        }
    }
}
