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
            // No longer need EquipmentModelId dropdown since we'll use text input
            return View();
        }

        // POST: Equipment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EquipmentId,EquipmentTypeId,EquipmentModelName,BuildingId,RoomId,InstallationDate,ExpectedLifespanMonths,Status,Notes")] Equipment equipment)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(equipment.EquipmentModelName))
            {
                ModelState.AddModelError("EquipmentModelName", "Equipment Model is required.");
            }

            if (ModelState.IsValid)
            {
                // Validate foreign keys
                var equipmentTypeExists = await _context.EquipmentTypes.AnyAsync(e => e.EquipmentTypeId == equipment.EquipmentTypeId);
                var buildingExists = await _context.Buildings.AnyAsync(b => b.BuildingId == equipment.BuildingId);
                var roomExists = await _context.Rooms.AnyAsync(r => r.RoomId == equipment.RoomId);

                if (!equipmentTypeExists || !buildingExists || !roomExists)
                {
                    ModelState.AddModelError("", "One or more selected values are invalid.");
                    // Repopulate dropdowns and return view
                    ViewData["EquipmentTypeId"] = new SelectList(await _context.EquipmentTypes.ToListAsync(), "EquipmentTypeId", "EquipmentTypeName", equipment.EquipmentTypeId);
                    ViewData["BuildingId"] = new SelectList(await _context.Buildings.ToListAsync(), "BuildingId", "BuildingName", equipment.BuildingId);
                    ViewData["RoomId"] = new SelectList(await _context.Rooms.Where(r => r.BuildingId == equipment.BuildingId).ToListAsync(), "RoomId", "RoomName", equipment.RoomId);
                    return View(equipment);
                }

                try
                {
                    // Find or create the equipment model
                    var existingModel = await _context.EquipmentModels
                        .FirstOrDefaultAsync(m => m.ModelName.ToLower() == equipment.EquipmentModelName!.ToLower() 
                                                && m.EquipmentTypeId == equipment.EquipmentTypeId);

                    if (existingModel != null)
                    {
                        // Use existing model
                        equipment.EquipmentModelId = existingModel.EquipmentModelId;
                    }
                    else
                    {
                        // Create new model
                        var newModel = new EquipmentModel
                        {
                            ModelName = equipment.EquipmentModelName!.Trim(),
                            EquipmentTypeId = equipment.EquipmentTypeId
                        };
                        
                        _context.EquipmentModels.Add(newModel);
                        await _context.SaveChangesAsync(); // Save to get the ID
                        equipment.EquipmentModelId = newModel.EquipmentModelId;
                    }

                    // Clear the non-mapped property before saving equipment
                    equipment.EquipmentModelName = null;
                    
                    _context.Add(equipment);
                    await _context.SaveChangesAsync();

                    // Generate alert for new equipment if status is problematic
                    await GenerateNewEquipmentAlert(equipment);

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

            var equipment = await _context.Equipment
                .Include(e => e.EquipmentModel)
                .FirstOrDefaultAsync(e => e.EquipmentId == id);
            if (equipment == null)
            {
                return NotFound();
            }

            // Set the model name for the text input
            equipment.EquipmentModelName = equipment.EquipmentModel?.ModelName;

            ViewData["EquipmentTypeId"] = new SelectList(await _context.EquipmentTypes.ToListAsync(), "EquipmentTypeId", "EquipmentTypeName", equipment.EquipmentTypeId);
            ViewData["BuildingId"] = new SelectList(await _context.Buildings.ToListAsync(), "BuildingId", "BuildingName", equipment.BuildingId);
            ViewData["RoomId"] = new SelectList(await _context.Rooms.Where(r => r.BuildingId == equipment.BuildingId).ToListAsync(), "RoomId", "RoomName", equipment.RoomId);
            return View(equipment);
        }

        // POST: Equipment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EquipmentId,EquipmentTypeId,EquipmentModelName,BuildingId,RoomId,InstallationDate,ExpectedLifespanMonths,Status,Notes")] Equipment equipment)
        {
            if (id != equipment.EquipmentId)
            {
                return NotFound();
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(equipment.EquipmentModelName))
            {
                ModelState.AddModelError("EquipmentModelName", "Equipment Model is required.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the original equipment status for comparison
                    var originalEquipment = await _context.Equipment
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.EquipmentId == id);

                    // Find or create the equipment model
                    var existingModel = await _context.EquipmentModels
                        .FirstOrDefaultAsync(m => m.ModelName.ToLower() == equipment.EquipmentModelName!.ToLower() 
                                                && m.EquipmentTypeId == equipment.EquipmentTypeId);

                    if (existingModel != null)
                    {
                        // Use existing model
                        equipment.EquipmentModelId = existingModel.EquipmentModelId;
                    }
                    else
                    {
                        // Create new model
                        var newModel = new EquipmentModel
                        {
                            ModelName = equipment.EquipmentModelName!.Trim(),
                            EquipmentTypeId = equipment.EquipmentTypeId
                        };
                        
                        _context.EquipmentModels.Add(newModel);
                        await _context.SaveChangesAsync(); // Save to get the ID
                        equipment.EquipmentModelId = newModel.EquipmentModelId;
                    }

                    // Clear the non-mapped property before saving equipment
                    equipment.EquipmentModelName = null;

                    _context.Update(equipment);
                    await _context.SaveChangesAsync();

                    // Generate condition-based alerts only when status changes to problematic states
                    if (originalEquipment != null && originalEquipment.Status != equipment.Status)
                    {
                        await GenerateConditionBasedAlert(equipment, originalEquipment.Status);
                    }
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
                .Include(e => e.EquipmentType)
                .Include(e => e.EquipmentModel)
                .Include(e => e.Building)
                .Include(e => e.Room)
                .FirstOrDefaultAsync(m => m.EquipmentId == id);
            
            if (equipment == null)
            {
                return NotFound();
            }

            // Count related data that will be deleted
            var relatedAlertsCount = await _context.Alerts.CountAsync(a => a.EquipmentId == id);
            var relatedMaintenanceLogsCount = await _context.MaintenanceLogs.CountAsync(ml => ml.EquipmentId == id);
            var relatedFailurePredictionsCount = await _context.FailurePredictions.CountAsync(fp => fp.EquipmentId == id);

            ViewBag.RelatedAlertsCount = relatedAlertsCount;
            ViewBag.RelatedMaintenanceLogsCount = relatedMaintenanceLogsCount;
            ViewBag.RelatedFailurePredictionsCount = relatedFailurePredictionsCount;
            ViewBag.HasRelatedData = relatedAlertsCount > 0 || relatedMaintenanceLogsCount > 0 || relatedFailurePredictionsCount > 0;

            return View(equipment);
        }

        // POST: Equipment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var equipment = await _context.Equipment.FindAsync(id);
                if (equipment == null)
                {
                    TempData["ErrorMessage"] = "Equipment not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Delete related alerts first
                var relatedAlerts = await _context.Alerts
                    .Where(a => a.EquipmentId == id)
                    .ToListAsync();
                
                if (relatedAlerts.Any())
                {
                    _context.Alerts.RemoveRange(relatedAlerts);
                    await _context.SaveChangesAsync();
                }

                // Delete related maintenance logs
                var relatedMaintenanceLogs = await _context.MaintenanceLogs
                    .Where(ml => ml.EquipmentId == id)
                    .ToListAsync();
                
                if (relatedMaintenanceLogs.Any())
                {
                    _context.MaintenanceLogs.RemoveRange(relatedMaintenanceLogs);
                    await _context.SaveChangesAsync();
                }

                // Delete related failure predictions
                var relatedFailurePredictions = await _context.FailurePredictions
                    .Where(fp => fp.EquipmentId == id)
                    .ToListAsync();
                
                if (relatedFailurePredictions.Any())
                {
                    _context.FailurePredictions.RemoveRange(relatedFailurePredictions);
                    await _context.SaveChangesAsync();
                }

                // Finally, delete the equipment
                _context.Equipment.Remove(equipment);
                await _context.SaveChangesAsync();
                
                await transaction.CommitAsync();
                
                TempData["SuccessMessage"] = $"Equipment '{equipment.EquipmentModel?.ModelName ?? "Unknown"}' and all related data have been successfully deleted.";
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = $"Database error while deleting equipment: {dbEx.InnerException?.Message ?? dbEx.Message}";
                return RedirectToAction(nameof(Delete), new { id = id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = $"Unexpected error deleting equipment: {ex.Message}";
                return RedirectToAction(nameof(Delete), new { id = id });
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

        // Generate condition-based alerts when equipment status changes
        private async Task GenerateConditionBasedAlert(Equipment equipment, EquipmentStatus previousStatus)
        {
            string? alertTitle = null;
            string? alertDescription = null;
            AlertPriority priority = AlertPriority.Low;

            // Only generate alerts for problematic status changes
            switch (equipment.Status)
            {
                case EquipmentStatus.Inactive:
                    if (previousStatus == EquipmentStatus.Active)
                    {
                        alertTitle = "Equipment Became Inactive";
                        alertDescription = $"Equipment has changed from Active to Inactive status. Immediate attention may be required.";
                        priority = AlertPriority.High;
                    }
                    break;

                case EquipmentStatus.Retired:
                    if (previousStatus != EquipmentStatus.Retired)
                    {
                        alertTitle = "Equipment Retired";
                        alertDescription = $"Equipment has been retired from service. Consider replacement planning.";
                        priority = AlertPriority.Low;
                    }
                    break;

                case EquipmentStatus.Active:
                    // Good news - equipment is back to active, no alert needed
                    // But we could optionally create a "resolved" type notification
                    if (previousStatus == EquipmentStatus.Inactive)
                    {
                        alertTitle = "Equipment Restored to Active";
                        alertDescription = $"Equipment has been successfully restored to active status.";
                        priority = AlertPriority.Low;
                    }
                    break;
            }

            // Only create alert if we determined one is needed
            if (!string.IsNullOrEmpty(alertTitle) && !string.IsNullOrEmpty(alertDescription))
            {
                // Check if a similar alert already exists for this equipment
                var existingAlert = await _context.Alerts
                    .Where(a => a.EquipmentId == equipment.EquipmentId && 
                               a.Status == AlertStatus.Open && 
                               a.Title == alertTitle)
                    .FirstOrDefaultAsync();

                if (existingAlert == null)
                {
                    var alert = new Alert
                    {
                        Title = alertTitle,
                        Description = alertDescription,
                        Priority = priority,
                        Status = AlertStatus.Open,
                        CreatedDate = DateTime.Now,
                        EquipmentId = equipment.EquipmentId,
                        AssignedToUserId = null // Can be assigned later
                    };

                    _context.Alerts.Add(alert);
                    await _context.SaveChangesAsync();
                }
            }
        }

        private async Task GenerateNewEquipmentAlert(Equipment equipment)
        {
            string? alertTitle = null;
            string? alertDescription = null;
            AlertPriority priority = AlertPriority.Low;

            // Generate alerts for newly added equipment with problematic status
            switch (equipment.Status)
            {
                case EquipmentStatus.Inactive:
                    alertTitle = "New Equipment Added with Inactive Status";
                    alertDescription = $"New equipment has been added with Inactive status. Please verify if this is correct and take appropriate action.";
                    priority = AlertPriority.High;
                    break;

                case EquipmentStatus.Retired:
                    alertTitle = "New Equipment Added with Retired Status";
                    alertDescription = $"New equipment has been added with Retired status. Please verify if this is correct.";
                    priority = AlertPriority.Medium;
                    break;

                case EquipmentStatus.Active:
                    // Optional: Create a welcome/tracking alert for new active equipment
                    alertTitle = "New Active Equipment Added";
                    alertDescription = $"New equipment has been successfully added to the system with Active status.";
                    priority = AlertPriority.Low;
                    break;
            }

            // Create alert if one is needed
            if (!string.IsNullOrEmpty(alertTitle) && !string.IsNullOrEmpty(alertDescription))
            {
                var alert = new Alert
                {
                    Title = alertTitle,
                    Description = alertDescription,
                    Priority = priority,
                    Status = AlertStatus.Open,
                    CreatedDate = DateTime.Now,
                    EquipmentId = equipment.EquipmentId,
                    AssignedToUserId = null // Can be assigned later
                };

                _context.Alerts.Add(alert);
                await _context.SaveChangesAsync();
            }
        }

        // Check for overdue maintenance and generate condition-based alerts
        public async Task<IActionResult> CheckMaintenanceOverdue()
        {
            var alertsGenerated = 0;
            
            // Get all active equipment with their maintenance logs
            var equipmentList = await _context.Equipment
                .Where(e => e.Status == EquipmentStatus.Active)
                .Include(e => e.MaintenanceLogs)
                .Include(e => e.EquipmentType)
                .ToListAsync();

            foreach (var equipment in equipmentList)
            {
                var lastMaintenance = equipment.MaintenanceLogs?
                    .OrderByDescending(ml => ml.LogDate)
                    .FirstOrDefault();

                // Consider equipment overdue if no maintenance in last 90 days for active equipment
                var daysSinceLastMaintenance = lastMaintenance != null ? 
                    (DateTime.Now - lastMaintenance.LogDate).Days : 
                    equipment.InstallationDate.HasValue ? (DateTime.Now - equipment.InstallationDate.Value).Days : 365;

                if (daysSinceLastMaintenance > 90)
                {
                    // Check if alert already exists
                    var existingAlert = await _context.Alerts
                        .Where(a => a.EquipmentId == equipment.EquipmentId && 
                                   a.Status == AlertStatus.Open && 
                                   a.Title!.Contains("Maintenance Overdue"))
                        .FirstOrDefaultAsync();

                    if (existingAlert == null)
                    {
                        var alert = new Alert
                        {
                            Title = "Maintenance Overdue",
                            Description = $"Equipment {equipment.EquipmentType?.EquipmentTypeName} has not received maintenance for {daysSinceLastMaintenance} days. Recommended maintenance interval exceeded.",
                            Priority = daysSinceLastMaintenance > 180 ? AlertPriority.High : AlertPriority.Medium,
                            Status = AlertStatus.Open,
                            CreatedDate = DateTime.Now,
                            EquipmentId = equipment.EquipmentId,
                            AssignedToUserId = null
                        };

                        _context.Alerts.Add(alert);
                        alertsGenerated++;
                    }
                }
            }

            if (alertsGenerated > 0)
            {
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = $"Maintenance check completed. {alertsGenerated} alerts generated for overdue maintenance.";
            return RedirectToAction("Index", "Alert");
        }

        // TEST ACTION: Create test equipment for delete testing
        // TODO: Remove this action in production
        [HttpGet]
        public async Task<IActionResult> CreateTestEquipment()
        {
            try
            {
                // Get first available equipment type, building, and room
                var equipmentType = await _context.EquipmentTypes.FirstAsync();
                var building = await _context.Buildings.FirstAsync();
                var room = await _context.Rooms.FirstAsync();

                // Create or get test equipment model
                var testModel = await _context.EquipmentModels
                    .FirstOrDefaultAsync(m => m.ModelName == "TEST-DELETE-MODEL");
                
                if (testModel == null)
                {
                    testModel = new EquipmentModel
                    {
                        ModelName = "TEST-DELETE-MODEL",
                        EquipmentTypeId = equipmentType.EquipmentTypeId
                    };
                    _context.EquipmentModels.Add(testModel);
                    await _context.SaveChangesAsync();
                }

                // Create test equipment
                var testEquipment = new Equipment
                {
                    EquipmentTypeId = equipmentType.EquipmentTypeId,
                    EquipmentModelId = testModel.EquipmentModelId,
                    BuildingId = building.BuildingId,
                    RoomId = room.RoomId,
                    Status = EquipmentStatus.Active,
                    InstallationDate = DateTime.Now.AddDays(-30),
                    ExpectedLifespanMonths = 60,
                    Notes = "TEST EQUIPMENT - Safe to delete"
                };

                _context.Equipment.Add(testEquipment);
                await _context.SaveChangesAsync();

                // Create test alerts
                var alert1 = new Alert
                {
                    EquipmentId = testEquipment.EquipmentId,
                    Title = "Test Alert 1",
                    Description = "Test alert for delete functionality - will be deleted with equipment",
                    Priority = AlertPriority.Medium,
                    Status = AlertStatus.Open,
                    CreatedDate = DateTime.Now.AddDays(-5)
                };

                var alert2 = new Alert
                {
                    EquipmentId = testEquipment.EquipmentId,
                    Title = "Test Alert 2",
                    Description = "Another test alert - will be deleted with equipment",
                    Priority = AlertPriority.High,
                    Status = AlertStatus.InProgress,
                    CreatedDate = DateTime.Now.AddDays(-3)
                };

                _context.Alerts.AddRange(alert1, alert2);

                // Create test maintenance log
                var maintenanceLog = new MaintenanceLog
                {
                    EquipmentId = testEquipment.EquipmentId,
                    LogDate = DateTime.Now.AddDays(-10),
                    MaintenanceType = MaintenanceType.Preventive,
                    Description = "Test maintenance log - will be deleted with equipment",
                    Cost = 150.00m,
                    Technician = "Test Technician",
                    Status = MaintenanceStatus.Completed
                };

                _context.MaintenanceLogs.Add(maintenanceLog);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Test equipment created successfully! Equipment ID: {testEquipment.EquipmentId} with 2 alerts and 1 maintenance log. You can now test the delete functionality.";
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating test equipment: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
