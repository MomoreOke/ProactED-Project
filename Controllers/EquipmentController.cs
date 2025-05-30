using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FEENALOoFINALE.Data;
using FEENALOoFINALE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FEENALOoFINALE.Controllers
{
    [Authorize]
    public class EquipmentController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;

        // GET: Equipment
        public async Task<IActionResult> Index()
        {
            return View(await _context.Equipment
                .Include(e => e.EquipmentType)      // Add this line
                .Include(e => e.EquipmentModel)     // Add this line
                .Include(e => e.Building)           // Add this line
                .Include(e => e.Room)               // Add this line
                .Include(e => e.MaintenanceLogs)
                .Include(e => e.FailurePredictions)
                .Include(e => e.Alerts)
                .ToListAsync());
        }

        // GET: Equipment/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var equipment = await _context.Equipment
                .Include(e => e.MaintenanceLogs)
                .Include(e => e.FailurePredictions)
                .Include(e => e.Alerts)
                .FirstOrDefaultAsync(m => m.EquipmentId == id);

            if (equipment == null)
            {
                return NotFound();
            }

            return View(equipment);
        }

        // GET: Equipment/Create
        public async Task<IActionResult> Create()
        {
            ViewData["EquipmentTypeId"] = new SelectList(await _context.EquipmentTypes.ToListAsync(), "EquipmentTypeId", "Name");
            ViewData["BuildingId"] = new SelectList(await _context.Buildings.ToListAsync(), "BuildingId", "Name");
            return View();
        }

        // POST: Equipment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EquipmentTypeId,EquipmentModelId,BuildingId,RoomId,InstallationDate,ExpectedLifespanMonths,Status,Notes")] Equipment equipment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(equipment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EquipmentTypeId"] = new SelectList(await _context.EquipmentTypes.ToListAsync(), "EquipmentTypeId", "Name", equipment.EquipmentTypeId);
            ViewData["BuildingId"] = new SelectList(await _context.Buildings.ToListAsync(), "BuildingId", "Name", equipment.BuildingId);
            // You will need to populate EquipmentModelId and RoomId SelectLists based on selected EquipmentTypeId and BuildingId via AJAX in the view
            return View(equipment);
        }

        // GET: Equipment/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var equipment = await _context.Equipment.FindAsync(id);
            if (equipment == null)
            {
                return NotFound();
            }
            ViewData["EquipmentTypeId"] = new SelectList(await _context.EquipmentTypes.ToListAsync(), "EquipmentTypeId", "Name", equipment.EquipmentTypeId);
            ViewData["EquipmentModelId"] = new SelectList(await _context.EquipmentModels.Where(em => em.EquipmentTypeId == equipment.EquipmentTypeId).ToListAsync(), "EquipmentModelId", "Name", equipment.EquipmentModelId);
            ViewData["BuildingId"] = new SelectList(await _context.Buildings.ToListAsync(), "BuildingId", "Name", equipment.BuildingId);
            ViewData["RoomId"] = new SelectList(await _context.Rooms.Where(r => r.BuildingId == equipment.BuildingId).ToListAsync(), "RoomId", "Name", equipment.RoomId);
            return View(equipment);
        }

        // POST: Equipment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EquipmentId,EquipmentTypeId,EquipmentModelId,BuildingId,RoomId,InstallationDate,ExpectedLifespanMonths,Status,Notes")] Equipment equipment)
        {
            if (id != equipment.EquipmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(equipment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Equipment.Any(e => e.EquipmentId == equipment.EquipmentId))
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
            ViewData["EquipmentTypeId"] = new SelectList(await _context.EquipmentTypes.ToListAsync(), "EquipmentTypeId", "Name", equipment.EquipmentTypeId);
            ViewData["EquipmentModelId"] = new SelectList(await _context.EquipmentModels.Where(em => em.EquipmentTypeId == equipment.EquipmentTypeId).ToListAsync(), "EquipmentModelId", "Name", equipment.EquipmentModelId);
            ViewData["BuildingId"] = new SelectList(await _context.Buildings.ToListAsync(), "BuildingId", "Name", equipment.BuildingId);
            ViewData["RoomId"] = new SelectList(await _context.Rooms.Where(r => r.BuildingId == equipment.BuildingId).ToListAsync(), "RoomId", "Name", equipment.RoomId);
            return View(equipment);
        }

        // GET: Equipment/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var equipment = await _context.Equipment
                .FirstOrDefaultAsync(m => m.EquipmentId == id);
            if (equipment == null)
            {
                return NotFound();
            }

            return View(equipment);
        }

        // POST: Equipment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var equipment = await _context.Equipment.FindAsync(id);
            if (equipment != null)
            {
                _context.Equipment.Remove(equipment);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Equipment/MaintenanceHistory/5
        public async Task<IActionResult> MaintenanceHistory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var equipment = await _context.Equipment
                .Include(e => e.MaintenanceLogs)
                .FirstOrDefaultAsync(m => m.EquipmentId == id);

            if (equipment == null)
            {
                return NotFound();
            }

            return View(equipment);
        }

        // GET: Equipment/PredictionHistory/5
        public async Task<IActionResult> PredictionHistory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var equipment = await _context.Equipment
                .Include(e => e.FailurePredictions)
                .FirstOrDefaultAsync(m => m.EquipmentId == id);

            if (equipment == null)
            {
                return NotFound();
            }

            return View(equipment);
        }

        private bool EquipmentExists(int id)
        {
            return _context.Equipment.Any(e => e.EquipmentId == id);
        }

        [HttpGet]
        public async Task<JsonResult> GetEquipmentModels(int equipmentTypeId)
        {
            var equipmentModels = await _context.EquipmentModels
                .Where(em => em.EquipmentTypeId == equipmentTypeId)
                .Select(em => new { em.EquipmentModelId, em.ModelName })
                .ToListAsync();
            return Json(equipmentModels);
        }

        [HttpGet]
        public async Task<JsonResult> GetRooms(int buildingId)
        {
            var rooms = await _context.Rooms
                .Where(r => r.BuildingId == buildingId)
                .Select(r => new { r.RoomId, r.RoomName })
                .ToListAsync();
            return Json(rooms);
        }
    }
}