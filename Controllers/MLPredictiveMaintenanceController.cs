using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using FEENALOoFINALE.Data;
using FEENALOoFINALE.Models;
using FEENALOoFINALE.Services;
using FEENALOoFINALE.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using System.IO;

namespace FEENALOoFINALE.Controllers
{
    [Authorize]
    public class MLPredictiveMaintenanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEquipmentPredictionService _predictionService;
        private readonly IHubContext<MaintenanceHub> _hubContext;
        private readonly ILogger<MLPredictiveMaintenanceController> _logger;

        public MLPredictiveMaintenanceController(
            ApplicationDbContext context,
            IEquipmentPredictionService predictionService,
            IHubContext<MaintenanceHub> hubContext,
            ILogger<MLPredictiveMaintenanceController> logger)
        {
            _context = context;
            _predictionService = predictionService;
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// Main ML-powered predictive maintenance dashboard - Redirects to Streamlit Dashboard
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                // Check if Streamlit dashboard is running, if not start it
                await EnsureStreamlitDashboardIsRunning();
                
                // Redirect to the Streamlit dashboard
                var streamlitUrl = "http://localhost:8501";
                return View("RedirectToStreamlit", streamlitUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error redirecting to Streamlit ML dashboard");
                TempData["ErrorMessage"] = "Unable to load ML dashboard. Please try again later.";
                return RedirectToAction("Index", "Dashboard");
            }
        }

        /// <summary>
        /// Get prediction for a single equipment item
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> PredictEquipment(int id)
        {
            try
            {
                var equipment = await _context.Equipment
                    .Include(e => e.EquipmentType)
                    .Include(e => e.EquipmentModel)
                    .Include(e => e.Building)
                    .Include(e => e.Room)
                    .FirstOrDefaultAsync(e => e.EquipmentId == id);

                if (equipment == null)
                {
                    return NotFound();
                }

                var predictionData = EquipmentPredictionData.FromEquipment(equipment);
                var prediction = await _predictionService.PredictEquipmentFailureAsync(predictionData);

                // Store prediction in database
                if (prediction.Success)
                {
                    await StorePredictionInDatabase(prediction, id);
                }

                var viewModel = new EquipmentWithPredictionViewModel
                {
                    Equipment = equipment,
                    Prediction = prediction
                };

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = prediction.Success, data = viewModel, error = prediction.ErrorMessage });
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prediction for equipment {EquipmentId}", id);
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, error = "Unable to get prediction. Please try again." });
                }
                
                TempData["ErrorMessage"] = "Unable to get equipment prediction.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Run batch predictions for all active equipment
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RunBatchPredictions()
        {
            try
            {
                var activeEquipment = await _context.Equipment
                    .Include(e => e.EquipmentType)
                    .Include(e => e.EquipmentModel)
                    .Where(e => e.Status == EquipmentStatus.Active)
                    .ToListAsync();

                if (!activeEquipment.Any())
                {
                    return Json(new { success = false, message = "No active equipment found for prediction." });
                }

                var predictionDataList = activeEquipment
                    .Select(EquipmentPredictionData.FromEquipment)
                    .ToList();

                var batchResult = await _predictionService.PredictBatchEquipmentFailureAsync(predictionDataList);

                if (batchResult.Success)
                {
                    // Store all predictions in database
                    var storedCount = 0;
                    foreach (var prediction in batchResult.Predictions)
                    {
                        if (int.TryParse(prediction.EquipmentId, out var equipmentId))
                        {
                            await StorePredictionInDatabase(prediction, equipmentId);
                            storedCount++;
                        }
                    }

                    // Notify connected clients about the update
                    await _hubContext.Clients.Group("Dashboard").SendAsync("BatchPredictionsCompleted", new
                    {
                        ProcessedCount = batchResult.ProcessedCount,
                        StoredCount = storedCount,
                        Timestamp = DateTime.Now
                    });

                    _logger.LogInformation("Batch predictions completed: {ProcessedCount} processed, {StoredCount} stored", 
                        batchResult.ProcessedCount, storedCount);

                    return Json(new 
                    { 
                        success = true, 
                        message = $"Successfully processed {batchResult.ProcessedCount} equipment predictions.",
                        processedCount = batchResult.ProcessedCount,
                        storedCount = storedCount
                    });
                }
                else
                {
                    return Json(new { success = false, message = batchResult.ErrorMessage ?? "Batch prediction failed." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during batch predictions");
                return Json(new { success = false, message = "Error occurred during batch predictions." });
            }
        }

        /// <summary>
        /// Get equipment with high failure risk
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetHighRiskEquipment()
        {
            try
            {
                var highRiskPredictions = await _context.FailurePredictions
                    .Include(fp => fp.Equipment)
                        .ThenInclude(e => e!.EquipmentType)
                    .Include(fp => fp.Equipment)
                        .ThenInclude(e => e!.EquipmentModel)
                    .Include(fp => fp.Equipment)
                        .ThenInclude(e => e!.Building)
                    .Include(fp => fp.Equipment)
                        .ThenInclude(e => e!.Room)
                    .Where(fp => fp.Status == PredictionStatus.High && 
                                fp.PredictedFailureDate >= DateTime.Now)
                    .OrderBy(fp => fp.PredictedFailureDate)
                    .Take(10)
                    .ToListAsync();

                var viewModels = highRiskPredictions.Select(fp => new EquipmentWithPredictionViewModel
                {
                    Equipment = fp.Equipment!,
                    Prediction = new PredictionResult
                    {
                        Success = true,
                        EquipmentId = fp.Equipment!.EquipmentId.ToString(),
                        FailureProbability = fp.ConfidenceLevel / 100.0,
                        RiskLevel = fp.Status.ToString(),
                        ConfidenceScore = fp.ConfidenceLevel / 100.0,
                        PredictionTimestamp = fp.CreatedDate,
                        ModelVersion = "ML Model"
                    }
                }).ToList();

                return Json(new { success = true, data = viewModels });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting high risk equipment");
                return Json(new { success = false, error = "Unable to get high risk equipment data." });
            }
        }

        /// <summary>
        /// Check ML API health status
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckApiHealth()
        {
            try
            {
                var isHealthy = await _predictionService.IsApiHealthyAsync();
                var modelInfo = isHealthy ? await _predictionService.GetModelInfoAsync() : null;

                return Json(new 
                { 
                    success = true, 
                    healthy = isHealthy,
                    modelInfo = modelInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking API health");
                return Json(new { success = false, healthy = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Get prediction statistics for dashboard
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPredictionStatistics()
        {
            try
            {
                var stats = await CalculatePredictionStatisticsAsync();
                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prediction statistics");
                return Json(new { success = false, error = "Unable to get prediction statistics." });
            }
        }

        /// <summary>
        /// Generate maintenance recommendations based on ML predictions
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GenerateMLRecommendations()
        {
            try
            {
                var recommendations = await GenerateMaintenanceRecommendationsAsync();
                
                return Json(new 
                { 
                    success = true, 
                    recommendations = recommendations,
                    count = recommendations.Count 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating ML recommendations");
                return Json(new { success = false, error = "Unable to generate recommendations." });
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Ensure Streamlit dashboard is running
        /// </summary>
        private async Task EnsureStreamlitDashboardIsRunning()
        {
            try
            {
                // Check if Streamlit is already running on port 8501
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(2);
                
                try
                {
                    var response = await httpClient.GetAsync("http://localhost:8501");
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Streamlit dashboard is already running");
                        return; // Already running
                    }
                }
                catch (HttpRequestException)
                {
                    // Not running, need to start it
                }

                // Start Streamlit dashboard
                var streamlitPath = Path.Combine(
                    Directory.GetCurrentDirectory(), 
                    "..", 
                    "Predictive Model", 
                    "dashboard.py"
                );

                if (System.IO.File.Exists(streamlitPath))
                {
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c cd \"{Path.GetDirectoryName(streamlitPath)}\" && streamlit run dashboard.py --server.port 8501 --server.headless true",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    var process = Process.Start(processStartInfo);
                    
                    if (process != null)
                    {
                        _logger.LogInformation("Started Streamlit dashboard process");
                        
                        // Give it a moment to start up
                        await Task.Delay(3000);
                    }
                }
                else
                {
                    _logger.LogWarning("Streamlit dashboard file not found at: {Path}", streamlitPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring Streamlit dashboard is running");
            }
        }

        /// <summary>
        /// Get dashboard data with ML predictions
        /// </summary>
        private async Task<MLPredictionDashboardViewModel> GetMLDashboardDataAsync()
        {
            var recentPredictions = await _context.FailurePredictions
                .Include(fp => fp.Equipment)
                    .ThenInclude(e => e!.EquipmentType)
                .Include(fp => fp.Equipment)
                    .ThenInclude(e => e!.EquipmentModel)
                .Where(fp => fp.CreatedDate >= DateTime.Now.AddDays(-7))
                .OrderByDescending(fp => fp.CreatedDate)
                .Take(20)
                .ToListAsync();

            var totalAnalyzed = recentPredictions.Count;
            var highRisk = recentPredictions.Count(p => p.Status == PredictionStatus.High);
            var mediumRisk = recentPredictions.Count(p => p.Status == PredictionStatus.Medium);
            var lowRisk = recentPredictions.Count(p => p.Status == PredictionStatus.Low);

            var apiHealthy = await _predictionService.IsApiHealthyAsync();
            var modelInfo = apiHealthy ? await _predictionService.GetModelInfoAsync() : null;

            var highRiskEquipment = recentPredictions
                .Where(p => p.Status == PredictionStatus.High)
                .Take(5)
                .Select(fp => new EquipmentWithPredictionViewModel
                {
                    Equipment = fp.Equipment!,
                    Prediction = new PredictionResult
                    {
                        Success = true,
                        EquipmentId = fp.Equipment!.EquipmentId.ToString(),
                        FailureProbability = fp.ConfidenceLevel / 100.0,
                        RiskLevel = fp.Status.ToString(),
                        ConfidenceScore = fp.ConfidenceLevel / 100.0,
                        PredictionTimestamp = fp.CreatedDate,
                        ModelVersion = "ML Model"
                    }
                }).ToList();

            return new MLPredictionDashboardViewModel
            {
                TotalEquipmentAnalyzed = totalAnalyzed,
                HighRiskEquipment = highRisk,
                MediumRiskEquipment = mediumRisk,
                LowRiskEquipment = lowRisk,
                AverageFailureProbability = totalAnalyzed > 0 ? recentPredictions.Average(p => p.ConfidenceLevel) / 100.0 : 0,
                ModelAccuracy = modelInfo?.Accuracy ?? 0.0,
                ModelVersion = modelInfo?.ModelVersion ?? "Unknown",
                LastPredictionUpdate = recentPredictions.FirstOrDefault()?.CreatedDate ?? DateTime.MinValue,
                ApiHealthy = apiHealthy,
                HighRiskEquipmentList = highRiskEquipment,
                RecentPredictions = recentPredictions.Take(10).Select(fp => new EquipmentWithPredictionViewModel
                {
                    Equipment = fp.Equipment!,
                    Prediction = new PredictionResult
                    {
                        Success = true,
                        EquipmentId = fp.Equipment!.EquipmentId.ToString(),
                        FailureProbability = fp.ConfidenceLevel / 100.0,
                        RiskLevel = fp.Status.ToString(),
                        ConfidenceScore = fp.ConfidenceLevel / 100.0,
                        PredictionTimestamp = fp.CreatedDate,
                        ModelVersion = "ML Model"
                    }
                }).ToList(),
                RiskLevelDistribution = new Dictionary<string, int>
                {
                    ["High"] = highRisk,
                    ["Medium"] = mediumRisk,
                    ["Low"] = lowRisk
                }
            };
        }

        /// <summary>
        /// Store ML prediction result in database
        /// </summary>
        private async Task StorePredictionInDatabase(PredictionResult prediction, int equipmentId)
        {
            try
            {
                // Remove old predictions for this equipment (keep only latest)
                var oldPredictions = await _context.FailurePredictions
                    .Where(fp => fp.EquipmentId == equipmentId)
                    .ToListAsync();

                if (oldPredictions.Any())
                {
                    _context.FailurePredictions.RemoveRange(oldPredictions);
                }

                // Add new prediction
                var failurePrediction = prediction.ToFailurePrediction(equipmentId);
                _context.FailurePredictions.Add(failurePrediction);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Stored ML prediction for equipment {EquipmentId}: Risk={RiskLevel}, Probability={Probability:P2}", 
                    equipmentId, prediction.RiskLevel, prediction.FailureProbability);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing prediction for equipment {EquipmentId}", equipmentId);
                // Don't throw - prediction storage failure shouldn't break the flow
            }
        }

        /// <summary>
        /// Calculate prediction statistics
        /// </summary>
        private async Task<object> CalculatePredictionStatisticsAsync()
        {
            var last30Days = DateTime.Now.AddDays(-30);
            
            var predictions = await _context.FailurePredictions
                .Where(fp => fp.CreatedDate >= last30Days)
                .ToListAsync();

            var accuracyRate = predictions.Count > 0 ? 
                predictions.Count(p => p.Status == PredictionStatus.High) * 100.0 / predictions.Count : 0;

            return new
            {
                TotalPredictions = predictions.Count,
                AccuracyRate = Math.Round(accuracyRate, 1),
                HighRiskCount = predictions.Count(p => p.Status == PredictionStatus.High),
                MediumRiskCount = predictions.Count(p => p.Status == PredictionStatus.Medium),
                LowRiskCount = predictions.Count(p => p.Status == PredictionStatus.Low),
                AverageConfidence = predictions.Count > 0 ? 
                    Math.Round(predictions.Average(p => p.ConfidenceLevel), 1) : 0,
                PredictionsByDay = predictions
                    .GroupBy(p => p.CreatedDate.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new { Date = g.Key, Count = g.Count() })
                    .ToList()
            };
        }

        /// <summary>
        /// Generate maintenance recommendations based on ML predictions
        /// </summary>
        private async Task<List<object>> GenerateMaintenanceRecommendationsAsync()
        {
            var highRiskPredictions = await _context.FailurePredictions
                .Include(fp => fp.Equipment)
                    .ThenInclude(e => e!.EquipmentType)
                .Include(fp => fp.Equipment)
                    .ThenInclude(e => e!.EquipmentModel)
                .Where(fp => fp.Status == PredictionStatus.High && 
                            fp.PredictedFailureDate >= DateTime.Now &&
                            fp.PredictedFailureDate <= DateTime.Now.AddDays(30))
                .OrderBy(fp => fp.PredictedFailureDate)
                .ToListAsync();

            return highRiskPredictions.Select(fp => new
            {
                EquipmentId = fp.EquipmentId,
                EquipmentName = fp.Equipment?.EquipmentModel?.ModelName ?? "Unknown",
                EquipmentType = fp.Equipment?.EquipmentType?.EquipmentTypeName ?? "Unknown",
                RiskLevel = fp.Status.ToString(),
                PredictedFailureDate = fp.PredictedFailureDate,
                DaysUntilFailure = (fp.PredictedFailureDate - DateTime.Now).Days,
                Priority = fp.PredictedFailureDate <= DateTime.Now.AddDays(7) ? "Immediate" : "High",
                RecommendedAction = fp.PredictedFailureDate <= DateTime.Now.AddDays(7) 
                    ? "Schedule immediate preventive maintenance"
                    : "Schedule maintenance within 2 weeks",
                EstimatedCost = CalculateEstimatedMaintenanceCost(fp.Equipment?.EquipmentType?.EquipmentTypeName),
                ConfidenceLevel = fp.ConfidenceLevel
            }).Cast<object>().ToList();
        }

        /// <summary>
        /// Calculate estimated maintenance cost based on equipment type
        /// </summary>
        private decimal CalculateEstimatedMaintenanceCost(string? equipmentType)
        {
            return (equipmentType?.ToLower()) switch
            {
                var type when type?.Contains("projector") == true => 150m,
                var type when type?.Contains("computer") == true => 100m,
                var type when type?.Contains("air condition") == true => 300m,
                var type when type?.Contains("printer") == true => 80m,
                _ => 120m
            };
        }

        #endregion
    }
}
