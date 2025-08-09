using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FEENALOoFINALE.Data;
using FEENALOoFINALE.Models;
using Microsoft.AspNetCore.Authorization;

namespace FEENALOoFINALE.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EquipmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EquipmentController> _logger;

        public EquipmentController(ApplicationDbContext context, ILogger<EquipmentController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetEquipment()
        {
            try
            {
                var equipment = await _context.Equipment
                    .Include(e => e.EquipmentType)
                    .Include(e => e.EquipmentModel)
                    .Include(e => e.Room)
                    .Include(e => e.Building)
                    .Where(e => e.Status == EquipmentStatus.Active)
                    .Select(e => new
                    {
                        equipmentId = e.EquipmentId,
                        equipmentType = new
                        {
                            equipmentTypeName = e.EquipmentType!.EquipmentTypeName
                        },
                        equipmentModel = new
                        {
                            modelName = e.EquipmentModel!.ModelName
                        },
                        room = new
                        {
                            roomName = e.Room!.RoomName
                        },
                        building = new
                        {
                            buildingName = e.Building!.BuildingName
                        },
                        status = e.Status.ToString()
                    })
                    .OrderBy(e => e.equipmentModel.modelName)
                    .ToListAsync();

                return Ok(equipment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment list");
                return StatusCode(500, new { message = "Error retrieving equipment list" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEquipment(int id)
        {
            try
            {
                var equipment = await _context.Equipment
                    .Include(e => e.EquipmentType)
                    .Include(e => e.EquipmentModel)
                    .Include(e => e.Room)
                    .Include(e => e.Building)
                    .Include(e => e.MaintenanceLogs)
                    .FirstOrDefaultAsync(e => e.EquipmentId == id);

                if (equipment == null)
                {
                    return NotFound(new { message = "Equipment not found" });
                }

                var result = new
                {
                    equipmentId = equipment.EquipmentId,
                    equipmentType = new
                    {
                        equipmentTypeName = equipment.EquipmentType?.EquipmentTypeName
                    },
                    equipmentModel = new
                    {
                        modelName = equipment.EquipmentModel?.ModelName
                    },
                    room = new
                    {
                        roomName = equipment.Room?.RoomName
                    },
                    building = new
                    {
                        buildingName = equipment.Building?.BuildingName
                    },
                    installationDate = equipment.InstallationDate,
                    expectedLifespanMonths = equipment.ExpectedLifespanMonths,
                    status = equipment.Status.ToString(),
                    notes = equipment.Notes,
                    averageWeeklyUsageHours = equipment.AverageWeeklyUsageHours,
                    maintenanceCount = equipment.MaintenanceLogs?.Count ?? 0
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment with ID: {EquipmentId}", id);
                return StatusCode(500, new { message = "Error retrieving equipment details" });
            }
        }
    }
}
