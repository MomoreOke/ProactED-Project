using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FEENALOoFINALE.Services;
using FEENALOoFINALE.Models;
using FEENALOoFINALE.Data;
using Microsoft.EntityFrameworkCore;

namespace FEENALOoFINALE.Controllers
{
    [Authorize]
    public class PredictionTestController : Controller
    {
        private readonly IEquipmentPredictionService _predictionService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PredictionTestController> _logger;

        public PredictionTestController(
            IEquipmentPredictionService predictionService, 
            ApplicationDbContext context,
            ILogger<PredictionTestController> logger)
        {
            _predictionService = predictionService;
            _context = context;
            _logger = logger;
        }

        // GET: /PredictionTest
        public async Task<IActionResult> Index()
        {
            var viewModel = new PredictionTestViewModel
            {
                ApiHealthy = await _predictionService.IsApiHealthyAsync()
            };

            if (viewModel.ApiHealthy)
            {
                try
                {
                    viewModel.ModelInfo = await _predictionService.GetModelInfoAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting model info");
                }
            }

            return View(viewModel);
        }

        // POST: /PredictionTest/TestSinglePrediction
        [HttpPost]
        public async Task<IActionResult> TestSinglePrediction()
        {
            try
            {
                // Get a sample equipment from database
                var equipment = await _context.Equipment
                    .Include(e => e.EquipmentType)
                    .Include(e => e.EquipmentModel)
                    .FirstOrDefaultAsync();

                if (equipment == null)
                {
                    return Json(new { success = false, error = "No equipment found in database" });
                }

                // Convert to prediction data
                var predictionData = EquipmentPredictionData.FromEquipment(equipment);

                // Call prediction service
                var result = await _predictionService.PredictEquipmentFailureAsync(predictionData);

                return Json(new { 
                    success = true, 
                    equipmentName = $"{equipment.EquipmentModel?.ModelName} ({equipment.EquipmentType?.EquipmentTypeName})",
                    prediction = result 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing single prediction");
                return Json(new { success = false, error = ex.Message });
            }
        }

        // POST: /PredictionTest/TestBatchPrediction
        [HttpPost]
        public async Task<IActionResult> TestBatchPrediction()
        {
            try
            {
                // Get first 5 equipment from database
                var equipmentList = await _context.Equipment
                    .Include(e => e.EquipmentType)
                    .Include(e => e.EquipmentModel)
                    .Take(5)
                    .ToListAsync();

                if (!equipmentList.Any())
                {
                    return Json(new { success = false, error = "No equipment found in database" });
                }

                // Convert to prediction data
                var predictionDataList = equipmentList.Select(EquipmentPredictionData.FromEquipment).ToList();

                // Call batch prediction service
                var result = await _predictionService.PredictBatchEquipmentFailureAsync(predictionDataList);

                return Json(new { 
                    success = true, 
                    equipmentCount = equipmentList.Count,
                    batchResult = result 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing batch prediction");
                return Json(new { success = false, error = ex.Message });
            }
        }
    }

    // View Model for the test page
    public class PredictionTestViewModel
    {
        public bool ApiHealthy { get; set; }
        public ModelInfo? ModelInfo { get; set; }
    }
}
