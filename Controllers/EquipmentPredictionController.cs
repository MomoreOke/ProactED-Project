using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FEENALOoFINALE.Data;
using FEENALOoFINALE.Models;
using FEENALOoFINALE.Services;
using FEENALOoFINALE.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace FEENALOoFINALE.Controllers
{
    public class EquipmentPredictionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEquipmentPredictionService _predictionService;
        private readonly ILogger<EquipmentPredictionController> _logger;

        public EquipmentPredictionController(
            ApplicationDbContext context,
            IEquipmentPredictionService predictionService,
            ILogger<EquipmentPredictionController> logger)
        {
            _context = context;
            _predictionService = predictionService;
            _logger = logger;
        }

        // GET: Equipment Prediction Dashboard
        public async Task<IActionResult> Index()
        {
            var viewModel = new EquipmentPredictionDashboardViewModel();

            try
            {
                // Check API health
                viewModel.ApiHealthy = await _predictionService.IsApiHealthyAsync();
                viewModel.ModelInfo = await _predictionService.GetModelInfoAsync();

                // Get equipment list with basic data first
                var equipmentData = await _context.Equipment
                    .Include(e => e.EquipmentType)
                    .Include(e => e.EquipmentModel)
                    .Include(e => e.Building)
                    .Include(e => e.Room)
                    .Where(e => e.Status == EquipmentStatus.Active)
                    .ToListAsync();

                // Transform to view models with calculated values
                viewModel.Equipment = equipmentData.Select(e => new EquipmentPredictionItemViewModel
                {
                    EquipmentId = e.EquipmentId,
                    Name = e.EquipmentModel?.ModelName ?? "Unknown",
                    Type = e.EquipmentType?.EquipmentTypeName ?? "Unknown",
                    Location = $"{e.Building?.BuildingName} - {e.Room?.RoomName}",
                    InstallationDate = e.InstallationDate,
                    AgeMonths = e.InstallationDate.HasValue ? 
                        (int)((DateTime.Now - e.InstallationDate.Value).TotalDays / 30.44) : 0,
                    WeeklyUsageHours = e.AverageWeeklyUsageHours ?? 40.0,
                    // Calculate operational parameters using static methods
                    OperatingTemperature = EstimateOperatingTemperature(e.EquipmentType?.EquipmentTypeName, e.InstallationDate),
                    VibrationLevel = EstimateVibrationLevel(e.EquipmentType?.EquipmentTypeName, e.InstallationDate),
                    PowerConsumption = EstimatePowerConsumption(e.EquipmentType?.EquipmentTypeName),
                    Status = e.Status.ToString(),
                    LastPrediction = null // Will be populated when prediction is made
                }).ToList();

                viewModel.Statistics = new EquipmentPredictionStatistics
                {
                    TotalEquipment = viewModel.Equipment.Count,
                    PendingPredictions = viewModel.Equipment.Count,
                    ApiConnected = viewModel.ApiHealthy,
                    ModelAccuracy = viewModel.ModelInfo?.Accuracy ?? 0.0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading equipment prediction dashboard");
                ViewBag.Error = "Unable to load equipment data";
            }

            return View(viewModel);
        }

        // POST: Predict single equipment
        [HttpPost]
        public async Task<IActionResult> PredictSingle(int equipmentId)
        {
            try
            {
                var equipment = await _context.Equipment
                    .Include(e => e.EquipmentType)
                    .Include(e => e.EquipmentModel)
                    .Include(e => e.Building)
                    .Include(e => e.Room)
                    .FirstOrDefaultAsync(e => e.EquipmentId == equipmentId);

                if (equipment == null)
                {
                    return Json(new { success = false, message = "Equipment not found" });
                }

                var predictionData = CreatePredictionData(equipment);
                var result = await _predictionService.PredictEquipmentFailureAsync(predictionData);

                _logger.LogInformation("Predicted failure for equipment {EquipmentId}: {RiskLevel} ({Probability:P})", 
                    equipmentId, result.RiskLevel, result.FailureProbability);

                return Json(new
                {
                    success = result.Success,
                    equipment = new
                    {
                        id = equipment.EquipmentId,
                        name = equipment.EquipmentModel?.ModelName,
                        type = equipment.EquipmentType?.EquipmentTypeName,
                        location = $"{equipment.Building?.BuildingName} - {equipment.Room?.RoomName}"
                    },
                    prediction = new
                    {
                        riskLevel = result.RiskLevel,
                        failureProbability = result.FailureProbability,
                        confidenceScore = result.ConfidenceScore,
                        modelVersion = result.ModelVersion,
                        predictionTimestamp = result.PredictionTimestamp,
                        errorMessage = result.ErrorMessage
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error predicting failure for equipment {EquipmentId}", equipmentId);
                return Json(new { success = false, message = "Prediction failed: " + ex.Message });
            }
        }

        // POST: Predict batch equipment
        [HttpPost]
        public async Task<IActionResult> PredictBatch()
        {
            try
            {
                var equipment = await _context.Equipment
                    .Include(e => e.EquipmentType)
                    .Include(e => e.EquipmentModel)
                    .Include(e => e.Building)
                    .Include(e => e.Room)
                    .Where(e => e.Status == EquipmentStatus.Active)
                    .Take(10) // Limit to first 10 for demo
                    .ToListAsync();

                var predictionDataList = equipment.Select(CreatePredictionData).ToList();
                var result = await _predictionService.PredictBatchEquipmentFailureAsync(predictionDataList);

                _logger.LogInformation("Batch predicted {Count} equipment items", result.ProcessedCount);

                return Json(new
                {
                    success = result.Success,
                    processedCount = result.ProcessedCount,
                    predictions = result.Predictions.Select(p => new
                    {
                        equipmentId = p.EquipmentId,
                        riskLevel = p.RiskLevel,
                        failureProbability = p.FailureProbability,
                        confidenceScore = p.ConfidenceScore,
                        success = p.Success,
                        errorMessage = p.ErrorMessage
                    }),
                    errorMessage = result.ErrorMessage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in batch prediction");
                return Json(new { success = false, message = "Batch prediction failed: " + ex.Message });
            }
        }

        // GET: Equipment Risk Analysis
        public async Task<IActionResult> RiskAnalysis()
        {
            var viewModel = new EquipmentRiskAnalysisViewModel();

            try
            {
                // Get all active equipment
                var equipment = await _context.Equipment
                    .Include(e => e.EquipmentType)
                    .Include(e => e.EquipmentModel)
                    .Include(e => e.Building)
                    .Include(e => e.Room)
                    .Where(e => e.Status == EquipmentStatus.Active)
                    .ToListAsync();

                // Create prediction data for all equipment
                var predictionDataList = equipment.Select(CreatePredictionData).ToList();

                // Get batch predictions
                var batchResult = await _predictionService.PredictBatchEquipmentFailureAsync(predictionDataList);

                if (batchResult.Success)
                {
                    // Combine equipment data with predictions
                    viewModel.RiskAssessments = equipment.Zip(batchResult.Predictions, (eq, pred) => new EquipmentRiskAssessment
                    {
                        EquipmentId = eq.EquipmentId,
                        Name = eq.EquipmentModel?.ModelName ?? "Unknown",
                        Type = eq.EquipmentType?.EquipmentTypeName ?? "Unknown",
                        Location = $"{eq.Building?.BuildingName} - {eq.Room?.RoomName}",
                        AgeMonths = eq.InstallationDate.HasValue ? 
                            (int)((DateTime.Now - eq.InstallationDate.Value).TotalDays / 30.44) : 0,
                        RiskLevel = pred.RiskLevel,
                        FailureProbability = pred.FailureProbability,
                        ConfidenceScore = pred.ConfidenceScore,
                        PredictionDate = pred.PredictionTimestamp,
                        RecommendedAction = GetRecommendedAction(pred.RiskLevel, pred.FailureProbability)
                    }).OrderByDescending(r => r.FailureProbability).ToList();

                    // Calculate summary statistics
                    viewModel.Summary = new RiskAnalysisSummary
                    {
                        TotalEquipment = viewModel.RiskAssessments.Count,
                        HighRiskCount = viewModel.RiskAssessments.Count(r => r.RiskLevel == "High" || r.RiskLevel == "Critical"),
                        MediumRiskCount = viewModel.RiskAssessments.Count(r => r.RiskLevel == "Medium"),
                        LowRiskCount = viewModel.RiskAssessments.Count(r => r.RiskLevel == "Low"),
                        AverageFailureProbability = viewModel.RiskAssessments.Average(r => r.FailureProbability),
                        RecommendedImmediateActions = viewModel.RiskAssessments.Count(r => r.RiskLevel == "Critical")
                    };
                }
                else
                {
                    ViewBag.Error = "Unable to get risk predictions: " + batchResult.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in risk analysis");
                ViewBag.Error = "Error performing risk analysis: " + ex.Message;
            }

            return View(viewModel);
        }

        #region Helper Methods

        private EquipmentPredictionData CreatePredictionData(Equipment equipment)
        {
            var ageMonths = equipment.InstallationDate.HasValue ? 
                (int)((DateTime.Now - equipment.InstallationDate.Value).TotalDays / 30.44) : 12;

            return new EquipmentPredictionData
            {
                EquipmentId = equipment.EquipmentId.ToString(),
                AgeMonths = ageMonths,
                OperatingTemperature = EstimateOperatingTemperature(equipment.EquipmentType?.EquipmentTypeName, equipment.InstallationDate),
                VibrationLevel = EstimateVibrationLevel(equipment.EquipmentType?.EquipmentTypeName, equipment.InstallationDate),
                PowerConsumption = EstimatePowerConsumption(equipment.EquipmentType?.EquipmentTypeName)
            };
        }

        private static double EstimateOperatingTemperature(string? equipmentType, DateTime? installationDate)
        {
            var baseTemp = equipmentType?.ToLower() switch
            {
                var t when t != null && (t.Contains("computer") || t.Contains("server")) => 45.0,
                var t when t != null && t.Contains("projector") => 65.0,
                var t when t != null && t.Contains("printer") => 40.0,
                var t when t != null && (t.Contains("air conditioning") || t.Contains("hvac")) => 25.0,
                var t when t != null && (t.Contains("motor") || t.Contains("pump")) => 70.0,
                var t when t != null && t.Contains("microscope") => 22.0,
                _ => 50.0 // Default
            };

            // Add age-related temperature increase
            if (installationDate.HasValue)
            {
                var ageYears = (DateTime.Now - installationDate.Value).TotalDays / 365.25;
                baseTemp += ageYears * 2.0; // 2°C increase per year of age
            }

            return Math.Min(baseTemp, 95.0); // Cap at 95°C
        }

        private static double EstimateVibrationLevel(string? equipmentType, DateTime? installationDate)
        {
            var baseVibration = equipmentType?.ToLower() switch
            {
                var t when t != null && (t.Contains("motor") || t.Contains("pump") || t.Contains("compressor")) => 4.0,
                var t when t != null && t.Contains("printer") => 2.5,
                var t when t != null && t.Contains("projector") => 1.5,
                var t when t != null && t.Contains("computer") => 0.5,
                var t when t != null && t.Contains("microscope") => 0.2,
                _ => 2.0 // Default
            };

            // Add age-related vibration increase
            if (installationDate.HasValue)
            {
                var ageYears = (DateTime.Now - installationDate.Value).TotalDays / 365.25;
                baseVibration += ageYears * 0.3; // 0.3 units increase per year
            }

            return Math.Min(baseVibration, 10.0); // Cap at 10
        }

        private static double EstimatePowerConsumption(string? equipmentType)
        {
            return equipmentType?.ToLower() switch
            {
                var t when t != null && t.Contains("server") => 2500.0,
                var t when t != null && t.Contains("computer") => 300.0,
                var t when t != null && t.Contains("projector") => 400.0,
                var t when t != null && t.Contains("printer") => 150.0,
                var t when t != null && (t.Contains("air conditioning") || t.Contains("hvac")) => 3000.0,
                var t when t != null && t.Contains("motor") => 1800.0,
                var t when t != null && t.Contains("microscope") => 100.0,
                var t when t != null && t.Contains("pump") => 2200.0,
                _ => 1000.0 // Default
            };
        }

        private string GetRecommendedAction(string riskLevel, double failureProbability)
        {
            return riskLevel switch
            {
                "Critical" => "Immediate maintenance required - Schedule emergency inspection",
                "High" => failureProbability > 0.7 ? "Priority maintenance - Schedule within 1 week" : "Schedule maintenance within 2 weeks",
                "Medium" => "Routine maintenance - Schedule within 1 month",
                "Low" => "Continue normal maintenance schedule",
                _ => "Monitor equipment condition"
            };
        }

        #endregion
    }
}
