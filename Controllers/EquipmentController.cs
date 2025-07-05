using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FEENALOoFINALE.Data;
using FEENALOoFINALE.Models;
using FEENALOoFINALE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace FEENALOoFINALE.Controllers
{
    [Authorize]
    public class EquipmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAdvancedAnalyticsService _analyticsService;

        public EquipmentController(ApplicationDbContext context, IAdvancedAnalyticsService analyticsService)
        {
            _context = context;
            _analyticsService = analyticsService;
        }

        // GET: Equipment
        public async Task<IActionResult> Index()
        {
            var equipment = await _context.Equipment
                .Include(e => e.EquipmentType)
                .Include(e => e.EquipmentModel)
                .Include(e => e.Building)
                .Include(e => e.Room)
                .Include(e => e.MaintenanceLogs)
                .Include(e => e.FailurePredictions)
                .Include(e => e.Alerts)
                .ToListAsync();

            // Add real-time analytics data
            ViewBag.SystemHealth = await _analyticsService.CalculateSystemHealthScoreAsync();
            ViewBag.MaintenanceEfficiency = await _analyticsService.CalculateMaintenanceEfficiencyAsync();
            ViewBag.CostEfficiency = await _analyticsService.CalculateCostEfficiencyAsync();

            return View(equipment);
        }

        // GET: Equipment/AIAnalysis
        [HttpGet]
        public async Task<IActionResult> AIAnalysis()
        {
            var insights = await _analyticsService.GetPredictiveMaintenanceInsightsAsync();
            var performanceMetrics = await _analyticsService.GetEquipmentPerformanceMetricsAsync();
            
            var result = new
            {
                insights = insights,
                performanceMetrics = performanceMetrics,
                systemHealth = await _analyticsService.CalculateSystemHealthScoreAsync(),
                recommendations = GenerateAIRecommendations(insights)
            };

            return Json(result);
        }

        // GET: Equipment/PredictiveMaintenance/{id}
        [HttpGet]
        public async Task<IActionResult> PredictiveMaintenance(int id)
        {
            var equipment = await _context.Equipment
                .Include(e => e.MaintenanceLogs)
                .Include(e => e.FailurePredictions)
                .FirstOrDefaultAsync(e => e.EquipmentId == id);

            if (equipment == null)
                return NotFound();

            var prediction = GenerateAdvancedPrediction(equipment);
            return Json(prediction);
        }

        // POST: Equipment/BulkScheduleMaintenance
        [HttpPost]
        public async Task<IActionResult> BulkScheduleMaintenance([FromBody] List<int> equipmentIds)
        {
            var scheduledTasks = new List<object>();

            foreach (var id in equipmentIds)
            {
                var equipment = await _context.Equipment.FindAsync(id);
                if (equipment != null)
                {
                    // In a real application, you would create actual maintenance tasks
                    scheduledTasks.Add(new
                    {
                        equipmentId = id,
                        scheduledDate = DateTime.Now.AddDays(7),
                        taskType = "Preventive Maintenance",
                        priority = "Medium"
                    });
                }
            }

            return Json(new { success = true, scheduledTasks = scheduledTasks });
        }

        // GET: Equipment/HealthScore/{id}
        [HttpGet]
        public async Task<IActionResult> HealthScore(int id)
        {
            var equipment = await _context.Equipment
                .Include(e => e.MaintenanceLogs)
                .Include(e => e.Alerts)
                .FirstOrDefaultAsync(e => e.EquipmentId == id);

            if (equipment == null)
                return NotFound();

            var healthScore = CalculateEquipmentHealthScore(equipment);
            return Json(new { healthScore = healthScore });
        }

        // GET: Equipment/RealTimeMetrics
        [HttpGet]
        public async Task<IActionResult> RealTimeMetrics()
        {
            var totalEquipment = await _context.Equipment.CountAsync();
            var activeEquipment = await _context.Equipment.CountAsync(e => e.Status == EquipmentStatus.Active);
            var inactiveEquipment = await _context.Equipment.CountAsync(e => e.Status == EquipmentStatus.Inactive);
            var retiredEquipment = await _context.Equipment.CountAsync(e => e.Status == EquipmentStatus.Retired);

            var metrics = new
            {
                total = totalEquipment,
                active = activeEquipment,
                inactive = inactiveEquipment,
                retired = retiredEquipment,
                systemHealth = await _analyticsService.CalculateSystemHealthScoreAsync(),
                maintenanceEfficiency = await _analyticsService.CalculateMaintenanceEfficiencyAsync(),
                timestamp = DateTime.Now
            };

            return Json(metrics);
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

        // Private helper methods for advanced AI functionality
        private List<string> GenerateAIRecommendations(List<PredictiveMaintenanceInsight> insights)
        {
            var recommendations = new List<string>();

            foreach (var insight in insights)
            {
                if (insight.ProbabilityPercentage > 70)
                {
                    recommendations.Add($"Immediate attention required for {insight.EquipmentName}: {insight.RecommendedAction}");
                }
                else if (insight.ProbabilityPercentage > 50)
                {
                    recommendations.Add($"Schedule maintenance for {insight.EquipmentName} within {insight.DaysUntilPredictedFailure} days");
                }
                else
                {
                    recommendations.Add($"Monitor {insight.EquipmentName} - {insight.PredictedIssue}");
                }
            }

            // Add general recommendations
            recommendations.Add("Consider implementing IoT sensors for real-time monitoring");
            recommendations.Add("Optimize maintenance schedules based on usage patterns");
            recommendations.Add("Implement predictive maintenance algorithms");

            return recommendations;
        }

        private object GenerateAdvancedPrediction(Equipment equipment)
        {
            var daysSinceInstall = (DateTime.Now - equipment.InstallationDate).Days;
            var maintenanceHistory = equipment.MaintenanceLogs?.Count ?? 0;
            var recentAlerts = equipment.Alerts?.Count(a => a.CreatedDate > DateTime.Now.AddDays(-30)) ?? 0;

            // Advanced prediction algorithm
            var failureRisk = Math.Min(95, (daysSinceInstall / 365.0 * 10) + (recentAlerts * 5) - (maintenanceHistory * 2));
            var healthScore = Math.Max(5, 100 - failureRisk);

            return new
            {
                equipmentId = equipment.EquipmentId,
                failureRisk = Math.Round(failureRisk, 1),
                healthScore = Math.Round(healthScore, 1),
                nextMaintenanceRecommended = DateTime.Now.AddDays(30 - (recentAlerts * 5)),
                criticalComponents = new[]
                {
                    "Motor bearings",
                    "Control circuits",
                    "Cooling system",
                    "Power supply"
                },
                recommendations = new[]
                {
                    "Schedule bearing lubrication",
                    "Test control circuit integrity",
                    "Clean cooling vents",
                    "Check power connections"
                },
                estimatedRemainingLife = new
                {
                    years = Math.Max(0.5, equipment.ExpectedLifespanMonths / 12.0 - daysSinceInstall / 365.0),
                    confidence = Math.Max(60, 95 - recentAlerts * 10)
                }
            };
        }

        private double CalculateEquipmentHealthScore(Equipment equipment)
        {
            var baseScore = 100.0;
            var daysSinceInstall = (DateTime.Now - equipment.InstallationDate).Days;
            var expectedLifeDays = equipment.ExpectedLifespanMonths * 30;
            var ageRatio = (double)daysSinceInstall / expectedLifeDays;

            // Age factor (0-30 points deduction)
            var ageDeduction = Math.Min(30, ageRatio * 30);
            baseScore -= ageDeduction;

            // Maintenance history factor
            var maintenanceCount = equipment.MaintenanceLogs?.Count ?? 0;
            var expectedMaintenanceCount = daysSinceInstall / 90; // Expected maintenance every 3 months
            var maintenanceRatio = maintenanceCount / Math.Max(1, expectedMaintenanceCount);

            if (maintenanceRatio < 0.5)
            {
                baseScore -= 20; // Poor maintenance
            }
            else if (maintenanceRatio > 1.5)
            {
                baseScore -= 10; // Over-maintenance might indicate problems
            }

            // Recent alerts factor
            var recentAlerts = equipment.Alerts?.Count(a => a.CreatedDate > DateTime.Now.AddDays(-30)) ?? 0;
            baseScore -= recentAlerts * 5;

            // Status factor
            switch (equipment.Status)
            {
                case EquipmentStatus.Active:
                    // No deduction
                    break;
                case EquipmentStatus.Inactive:
                    baseScore -= 30;
                    break;
                case EquipmentStatus.Retired:
                    baseScore = 0;
                    break;
            }

            return Math.Max(0, Math.Min(100, baseScore));
        }
    }
}
