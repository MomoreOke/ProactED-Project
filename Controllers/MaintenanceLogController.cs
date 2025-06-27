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
        public IActionResult Create(int? equipmentId, int? alertId = null)
        {
            ViewBag.EquipmentId = equipmentId;
            ViewBag.AlertId = alertId;
            ViewBag.Equipment = _context.Equipment.ToList(); // Ensure this is always a non-null list
            return View();
        }

        // POST: MaintenanceLog/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EquipmentId,LogDate,MaintenanceType,Description,DowntimeDuration,AlertId")] MaintenanceLog maintenanceLog)
        {
            if (ModelState.IsValid)
            {
                if (User != null && User.Identity != null)
                {
                    maintenanceLog.Technician = User?.Identity?.Name ?? "Unknown";
                }
                else
                {
                    maintenanceLog.Technician = "Unknown";
                }

                _context.Add(maintenanceLog);
                await _context.SaveChangesAsync();

                // Mark alert as resolved if AlertId is present
                if (maintenanceLog.AlertId.HasValue)
                {
                    var alert = await _context.Alerts.FindAsync(maintenanceLog.AlertId.Value);
                    if (alert != null)
                    {
                        alert.Status = AlertStatus.Resolved; // Use your enum or string as appropriate
                        _context.Update(alert);
                        await _context.SaveChangesAsync();
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            ViewBag.Equipment = _context.Equipment.ToList(); // Ensure this is always set on error
            return View(maintenanceLog);
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
            ViewBag.Equipment = _context.Equipment.ToList();
            return View(maintenanceLog);
        }

        // POST: MaintenanceLog/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LogId,EquipmentId,LogDate,MaintenanceType,Description,Technician,DowntimeDuration")] MaintenanceLog maintenanceLog)
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
            ViewBag.Equipment = _context.Equipment.ToList();
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