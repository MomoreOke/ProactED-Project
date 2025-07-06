using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FEENALOoFINALE.Data;
using FEENALOoFINALE.Models;
using Microsoft.AspNetCore.Authorization;

namespace FEENALOoFINALE.Controllers
{
    [Authorize]
    public class MaintenanceLogController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;

        // GET: MaintenanceLog
        public async Task<IActionResult> Index()
        {
            return View(await _context.MaintenanceLogs
                .Include(m => m.Equipment)
                .OrderByDescending(m => m.LogDate)
                .ToListAsync());
        }

        // GET: MaintenanceLog/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var maintenanceLog = await _context.MaintenanceLogs
                .Include(m => m.Equipment)
                .Include(m => m.MaintenanceInventoryLinks)
                    .ThenInclude(mil => mil.InventoryItem)
                .FirstOrDefaultAsync(m => m.LogId == id);

            if (maintenanceLog == null)
            {
                return NotFound();
            }

            return View(maintenanceLog);
        }

        // GET: MaintenanceLog/Create
        public async Task<IActionResult> Create(int? equipmentId, int? alertId = null)
        {
            await LoadEquipmentViewBag(equipmentId, alertId);
            return View();
        }

        // POST: MaintenanceLog/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EquipmentId,LogDate,MaintenanceType,Description,Technician,DowntimeHours,AlertId,TaskId")] MaintenanceLog maintenanceLog)
        {
            // Remove any validation errors for DowntimeDuration since we're using DowntimeHours
            ModelState.Remove("DowntimeDuration");
            
            if (ModelState.IsValid)
            {
                try
                {
                    // Ensure Technician is set if not provided
                    if (string.IsNullOrEmpty(maintenanceLog.Technician))
                    {
                        maintenanceLog.Technician = User?.Identity?.Name ?? "Unknown";
                    }

                    // Set default values for required properties
                    maintenanceLog.Status = MaintenanceStatus.Completed;
                    maintenanceLog.Cost = 0;

                    _context.Add(maintenanceLog);
                    await _context.SaveChangesAsync();

                    // Enhanced workflow completion logic
                    // Complete linked maintenance task
                    if (maintenanceLog.TaskId.HasValue)
                    {
                        var task = await _context.MaintenanceTasks.FindAsync(maintenanceLog.TaskId.Value);
                        if (task != null)
                        {
                            task.Status = MaintenanceStatus.Completed;
                            task.CompletedDate = DateTime.Now;
                            _context.Update(task);
                        }
                    }

                    // Mark alert as resolved if AlertId is present
                    if (maintenanceLog.AlertId.HasValue)
                    {
                        var alert = await _context.Alerts.FindAsync(maintenanceLog.AlertId.Value);
                        if (alert != null)
                        {
                            alert.Status = AlertStatus.Resolved;
                            _context.Update(alert);
                        }
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Database error: {ex.Message}");
                    ModelState.AddModelError("", $"Database error: {ex.Message}");
                }
            }
            
            // Reload equipment data for the view in case of validation errors
            await LoadEquipmentViewBag(maintenanceLog.EquipmentId, maintenanceLog.AlertId);
            return View(maintenanceLog);
        }

        private async Task LoadEquipmentViewBag(int? selectedEquipmentId = null, int? alertId = null)
        {
            var equipment = await _context.Equipment
                .Include(e => e.EquipmentModel)
                .Include(e => e.EquipmentType)
                .Include(e => e.Building)
                .Include(e => e.Room)
                .ToListAsync();
                
            ViewBag.Equipment = equipment;
            ViewBag.EquipmentId = selectedEquipmentId;
            ViewBag.AlertId = alertId;
        }

        // GET: MaintenanceLog/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var maintenanceLog = await _context.MaintenanceLogs.FindAsync(id);
            if (maintenanceLog == null)
            {
                return NotFound();
            }
            
            await LoadEquipmentViewBag(maintenanceLog.EquipmentId);
            return View(maintenanceLog);
        }

        // POST: MaintenanceLog/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LogId,EquipmentId,LogDate,MaintenanceType,Description,Technician,DowntimeHours")] MaintenanceLog maintenanceLog)
        {
            if (id != maintenanceLog.LogId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(maintenanceLog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaintenanceLogExists(maintenanceLog.LogId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            await LoadEquipmentViewBag(maintenanceLog.EquipmentId);
            return View(maintenanceLog);
        }

        // GET: MaintenanceLog/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var maintenanceLog = await _context.MaintenanceLogs
                .Include(m => m.Equipment)
                .FirstOrDefaultAsync(m => m.LogId == id);
            if (maintenanceLog == null)
            {
                return NotFound();
            }

            return View(maintenanceLog);
        }

        // POST: MaintenanceLog/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var maintenanceLog = await _context.MaintenanceLogs.FindAsync(id);
            if (maintenanceLog != null)
            {
                _context.MaintenanceLogs.Remove(maintenanceLog);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: MaintenanceLog/ByEquipment/5
        public async Task<IActionResult> ByEquipment(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var equipment = await _context.Equipment
                .Include(e => e.MaintenanceLogs)
                .FirstOrDefaultAsync(e => e.EquipmentId == id);

            if (equipment == null)
            {
                return NotFound();
            }

            return View(equipment);
        }

        private bool MaintenanceLogExists(int id)
        {
            return _context.MaintenanceLogs.Any(e => e.LogId == id);
        }
    }
}