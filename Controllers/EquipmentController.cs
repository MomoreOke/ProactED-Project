using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FEENALOoFINALE.Data;
using FEENALOoFINALE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FEENALOoFINALE.Controllers
{
    [Authorize]
    public class EquipmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        public EquipmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Equipment
        public async Task<IActionResult> Index()
        {
            return View(await _context.Equipment
                .Include(e => e.EquipmentType)
                .Include(e => e.EquipmentModel)
                .Include(e => e.Building)
                .Include(e => e.Room)
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
            ViewData["EquipmentTypeId"] = new SelectList(await _context.EquipmentTypes.ToListAsync(), "EquipmentTypeId", "EquipmentTypeName");
            ViewData["BuildingId"] = new SelectList(await _context.Buildings.ToListAsync(), "BuildingId", "BuildingName");
            ViewData["RoomId"] = new SelectList(await _context.Rooms.ToListAsync(), "RoomId", "RoomName");
            // Add this line to initialize EquipmentModelId with an empty SelectList
            ViewData["EquipmentModelId"] = new SelectList(new List<EquipmentModel>(), "EquipmentModelId", "ModelName");
            return View();
        }

        // POST: Equipment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EquipmentId,EquipmentTypeId,EquipmentModelId,BuildingId,RoomId,InstallationDate,ExpectedLifespanMonths,Status,Notes")] Equipment equipment)
        {
            if (ModelState.IsValid)
            {
                // Example: Check if IDs are valid
                var equipmentTypeExists = await _context.EquipmentTypes.AnyAsync(e => e.EquipmentTypeId == equipment.EquipmentTypeId);
                var equipmentModelExists = await _context.EquipmentModels.AnyAsync(e => e.EquipmentModelId == equipment.EquipmentModelId);
                var buildingExists = await _context.Buildings.AnyAsync(b => b.BuildingId == equipment.BuildingId);
                var roomExists = await _context.Rooms.AnyAsync(r => r.RoomId == equipment.RoomId);

                if (!equipmentTypeExists || !equipmentModelExists || !buildingExists || !roomExists)
                {
                    ModelState.AddModelError("", "One or more selected values are invalid.");
                    // Repopulate dropdowns and return the view as before
                    ViewData["EquipmentTypeId"] = new SelectList(await _context.EquipmentTypes.ToListAsync(), "EquipmentTypeId", "EquipmentTypeName", equipment.EquipmentTypeId);

                    // Repopulate EquipmentModelId based on selected EquipmentTypeId
                    var equipmentModels = await _context.EquipmentModels
                        .Where(m => m.EquipmentTypeId == equipment.EquipmentTypeId)
                        .ToListAsync();
                    ViewData["EquipmentModelId"] = new SelectList(equipmentModels, "EquipmentModelId", "ModelName", equipment.EquipmentModelId);

                    ViewData["BuildingId"] = new SelectList(await _context.Buildings.ToListAsync(), "BuildingId", "BuildingName", equipment.BuildingId);
                    ViewData["RoomId"] = new SelectList(await _context.Rooms.Where(r => r.BuildingId == equipment.BuildingId).ToListAsync(), "RoomId", "RoomName", equipment.RoomId);

                    return View(equipment);
                }

                try
                {
                    _context.Add(equipment);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Log ex.Message and ex.InnerException?.Message
                    ModelState.AddModelError("", "Error saving equipment: " + ex.Message);
                }
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            // Set a breakpoint here or log 'errors' to see what is failing

            ViewData["EquipmentTypeId"] = new SelectList(await _context.EquipmentTypes.ToListAsync(), "EquipmentTypeId", "EquipmentTypeName", equipment.EquipmentTypeId);

            // Repopulate EquipmentModelId based on selected EquipmentTypeId
            var models = await _context.EquipmentModels
                .Where(m => m.EquipmentTypeId == equipment.EquipmentTypeId)
                .ToListAsync();
            ViewData["EquipmentModelId"] = new SelectList(models, "EquipmentModelId", "ModelName", equipment.EquipmentModelId);

            ViewData["BuildingId"] = new SelectList(await _context.Buildings.ToListAsync(), "BuildingId", "BuildingName", equipment.BuildingId);
            ViewData["RoomId"] = new SelectList(await _context.Rooms.Where(r => r.BuildingId == equipment.BuildingId).ToListAsync(), "RoomId", "RoomName", equipment.RoomId);

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
            ViewData["EquipmentTypeId"] = new SelectList(await _context.EquipmentTypes.ToListAsync(), "EquipmentTypeId", "EquipmentTypeName", equipment.EquipmentTypeId);

            var models = await _context.EquipmentModels
                .Where(m => m.EquipmentTypeId == equipment.EquipmentTypeId)
                .ToListAsync();
            ViewData["EquipmentModelId"] = new SelectList(models, "EquipmentModelId", "ModelName", equipment.EquipmentModelId);

            ViewData["BuildingId"] = new SelectList(await _context.Buildings.ToListAsync(), "BuildingId", "BuildingName", equipment.BuildingId);
            ViewData["RoomId"] = new SelectList(await _context.Rooms.Where(r => r.BuildingId == equipment.BuildingId).ToListAsync(), "RoomId", "RoomName", equipment.RoomId);
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
                    if (!EquipmentExists(equipment.EquipmentId))
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
            ViewData["EquipmentTypeId"] = new SelectList(await _context.EquipmentTypes.ToListAsync(), "EquipmentTypeId", "EquipmentTypeName", equipment.EquipmentTypeId);

            var models = await _context.EquipmentModels
                .Where(m => m.EquipmentTypeId == equipment.EquipmentTypeId)
                .ToListAsync();
            ViewData["EquipmentModelId"] = new SelectList(models, "EquipmentModelId", "ModelName", equipment.EquipmentModelId);

            ViewData["BuildingId"] = new SelectList(await _context.Buildings.ToListAsync(), "BuildingId", "BuildingName", equipment.BuildingId);
            ViewData["RoomId"] = new SelectList(await _context.Rooms.Where(r => r.BuildingId == equipment.BuildingId).ToListAsync(), "RoomId", "RoomName", equipment.RoomId);
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
            var models = await _context.EquipmentModels
                .Where(em => em.EquipmentTypeId == equipmentTypeId)
                .Select(em => new 
                { 
                    equipmentModelId = em.EquipmentModelId, 
                    modelName = em.ModelName 
                })
                .ToListAsync();

            return Json(models);
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

        [HttpGet]
        public async Task<JsonResult> GetRoomsByBuilding(int buildingId)
        {
            var rooms = await _context.Rooms
                .Where(r => r.BuildingId == buildingId)
                .Select(r => new { r.RoomId, r.RoomName })
                .ToListAsync();
            return Json(rooms);
        }

        [HttpPost]
        public async Task<IActionResult> AddEquipmentType(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return Json(new { success = false });
            }

            var newEquipmentType = new EquipmentType { EquipmentTypeName = name };
            _context.EquipmentTypes.Add(newEquipmentType);
            await _context.SaveChangesAsync();

            return Json(new { success = true, newId = newEquipmentType.EquipmentTypeId });
        }

        [HttpPost]
        public async Task<IActionResult> AddEquipmentModel(int equipmentTypeId, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return Json(new { success = false });
            }

            // Look up the equipment type by id
            var equipmentType = await _context.EquipmentTypes
                .FirstOrDefaultAsync(et => et.EquipmentTypeId == equipmentTypeId);

            if (equipmentType == null)
            {
                return Json(new { success = false, error = "Invalid equipment type." });
            }

            var newEquipmentModel = new EquipmentModel 
            { 
                ModelName = name, 
                EquipmentTypeId = equipmentTypeId,
                EquipmentType = equipmentType // Set the required EquipmentType property
            };
            
            _context.EquipmentModels.Add(newEquipmentModel);
            await _context.SaveChangesAsync();

            return Json(new { success = true, newId = newEquipmentModel.EquipmentModelId });
        }

        [HttpPost]
        public async Task<IActionResult> AddBuilding(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return Json(new { success = false });
            }

            var newBuilding = new Building { BuildingName = name };
            _context.Buildings.Add(newBuilding);
            await _context.SaveChangesAsync();

            return Json(new { success = true, newId = newBuilding.BuildingId });
        }

        [HttpPost]
        public async Task<IActionResult> AddRoom(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return Json(new { success = false });
            }

            var building = await _context.Buildings.FirstOrDefaultAsync(b => b.BuildingId == 1) 
                           ?? new Building { BuildingName = "Default Building" };
            if (building.BuildingId == 0)
            {
                _context.Buildings.Add(building);
                await _context.SaveChangesAsync();
            }

            var newRoom = new Room { RoomName = name, Building = building }; // Ensure Building is set properly
            _context.Rooms.Add(newRoom);
            await _context.SaveChangesAsync();

            return Json(new { success = true, newId = newRoom.RoomId });
        }
        [HttpGet]
        public async Task<JsonResult> GetEquipmentTypes()
        {
            var equipmentTypes = await _context.EquipmentTypes
                .Select(et => new { et.EquipmentTypeId, et.EquipmentTypeName })
                .ToListAsync();
            return Json(equipmentTypes);
        }

        [HttpGet]
        public async Task<JsonResult> GetBuildings()
        {
            var buildings = await _context.Buildings
                .Select(b => new { b.BuildingId, b.BuildingName })
                .ToListAsync();
            return Json(buildings);
        }    
    }
}
