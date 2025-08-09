using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using FEENALOoFINALE.Services;
using FEENALOoFINALE.Data;
using Microsoft.EntityFrameworkCore;

namespace FEENALOoFINALE.Controllers
{
    /// <summary>
    /// 🔍 Model Interpretability Controller
    /// 
    /// Handles model interpretability and explainability features,
    /// integrated with Equipment Management and the Predictive Model folder's 5000+ dataset.
    /// 
    /// INTEGRATION POINTS:
    /// - Equipment Management: Brain icons in Equipment Index view
    /// - Predictive Model: Connects to model_interpretability.py in Predictive Model folder
    /// - Dataset: Works with 5000+ equipment records for comprehensive analysis
    /// </summary>
    public class ModelInterpretabilityController : Controller
    {
        private readonly ILogger<ModelInterpretabilityController> _logger;
        private readonly IEquipmentPredictionService _predictionService;
        private readonly ApplicationDbContext _context;
        private const string InterpretabilityUrl = "http://localhost:8503";
        private const string PredictiveModelPath = @"C:\Users\NABILA\Desktop\ProactED-Project\Predictive Model";

        public ModelInterpretabilityController(
            ILogger<ModelInterpretabilityController> logger,
            IEquipmentPredictionService predictionService,
            ApplicationDbContext context)
        {
            _logger = logger;
            _predictionService = predictionService;
            _context = context;
        }

        /// <summary>
        /// Main interpretability dashboard - now redirects to Equipment Management
        /// </summary>
        public IActionResult Index()
        {
            // Redirect to Equipment Management with Model Interpretability flag
            TempData["InfoMessage"] = "Model Interpretability is now integrated with Equipment Management. Look for the 🧠 brain icons next to each equipment.";
            return RedirectToAction("Index", "Equipment", new { modelInterpretability = true });
        }

        /// <summary>
        /// Feature importance analysis
        /// </summary>
        public IActionResult FeatureImportance()
        {
            ViewBag.InterpretabilityUrl = $"{InterpretabilityUrl}?analysis=importance";
            ViewBag.Title = "Feature Importance Analysis";
            ViewBag.Description = "Global analysis of which features matter most for predictions";
            
            return View("Index");
        }

        /// <summary>
        /// Individual prediction explanations
        /// </summary>
        public IActionResult PredictionExplanation()
        {
            ViewBag.InterpretabilityUrl = $"{InterpretabilityUrl}?analysis=explanation";
            ViewBag.Title = "Prediction Explanations";
            ViewBag.Description = "Detailed explanations for specific equipment predictions";
            
            return View("Index");
        }

        /// <summary>
        /// User guide for interpretability
        /// </summary>
        public IActionResult Guide()
        {
            ViewBag.InterpretabilityUrl = $"{InterpretabilityUrl}?analysis=guide";
            ViewBag.Title = "Interpretability Guide";
            ViewBag.Description = "Learn how to understand and use AI explanations";
            
            return View("Index");
        }

        /// <summary>
        /// Equipment-specific model interpretability analysis
        /// Now integrates directly with Equipment Management and Predictive Model dataset
        /// </summary>
        /// <param name="equipmentId">The ID of the equipment to analyze</param>
        public IActionResult EquipmentAnalysis(int equipmentId)
        {
            ViewBag.InterpretabilityUrl = $"{InterpretabilityUrl}?analysis=equipment&equipmentId={equipmentId}";
            ViewBag.Title = $"AI Model Analysis - Equipment #{equipmentId}";
            ViewBag.Description = "AI explanation using 5000+ equipment dataset from Predictive Model folder";
            ViewBag.EquipmentId = equipmentId;
            ViewBag.PredictiveModelPath = PredictiveModelPath;
            
            // Check if interpretability service is running
            ViewBag.ServiceStatus = CheckInterpretabilityService();
            
            // Additional context for the integration
            ViewBag.IntegrationInfo = new
            {
                DatasetLocation = Path.Combine(PredictiveModelPath, "knust_classroom_equipment_dataset.csv"),
                ModelLocation = Path.Combine(PredictiveModelPath, "model_interpretability.py"),
                ServiceUrl = InterpretabilityUrl,
                TotalRecords = "5000+"
            };
            
            return View("EquipmentAnalysis");
        }

        /// <summary>
        /// API endpoint to check if interpretability service is running
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckService()
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                
                var response = await client.GetAsync($"{InterpretabilityUrl}/health");
                var isRunning = response.IsSuccessStatusCode;
                
                return Json(new { 
                    isRunning = isRunning,
                    status = isRunning ? "running" : "stopped",
                    message = isRunning ? "Interpretability service is running" : "Interpretability service is not available"
                });
            }
            catch
            {
                return Json(new { 
                    isRunning = false,
                    status = "error",
                    message = "Unable to connect to interpretability service"
                });
            }
        }

        /// <summary>
        /// Start the interpretability service from the Predictive Model folder
        /// </summary>
        [HttpPost]
        public IActionResult StartService()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C cd \"{PredictiveModelPath}\" && streamlit run model_interpretability.py --server.port 8503",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process.Start(startInfo);
                
                _logger.LogInformation("Started Model Interpretability service from {Path}", PredictiveModelPath);
                
                return Json(new { 
                    success = true, 
                    message = "Model Interpretability service is starting from Predictive Model folder...",
                    path = PredictiveModelPath,
                    dataset = "5000+ equipment records"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start interpretability service from {Path}", PredictiveModelPath);
                return Json(new { 
                    success = false, 
                    message = $"Failed to start interpretability service. Check that the Predictive Model folder exists at: {PredictiveModelPath}" 
                });
            }
        }

        /// <summary>
        /// Check if interpretability service is running (synchronous)
        /// </summary>
        private bool CheckInterpretabilityService()
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(2);
                
                var response = client.GetAsync(InterpretabilityUrl).Result;
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Enhanced Equipment Prediction with Maintenance Scheduling
        /// This demonstrates the new intelligent prediction system that provides:
        /// - Potential failure causes analysis
        /// - Automatic maintenance scheduling 5-7 days before predicted failure
        /// - Equipment features display (name, location, etc.)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EnhancedPrediction(int equipmentId)
        {
            try
            {
                // Get the enhanced prediction with maintenance scheduling
                var result = await _predictionService.PredictWithMaintenanceSchedulingAsync(equipmentId);

                if (!result.Success)
                {
                    TempData["ErrorMessage"] = result.ErrorMessage;
                    return RedirectToAction("Index", "Equipment");
                }

                ViewBag.Title = "Enhanced AI Prediction with Maintenance Scheduling";
                ViewBag.Description = "Complete equipment analysis with failure cause identification and automatic maintenance planning";
                
                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in enhanced prediction for equipment {EquipmentId}", equipmentId);
                TempData["ErrorMessage"] = "An error occurred while generating the enhanced prediction.";
                return RedirectToAction("Index", "Equipment");
            }
        }

        /// <summary>
        /// API endpoint for enhanced prediction (for AJAX calls)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetEnhancedPrediction(int equipmentId)
        {
            try
            {
                var result = await _predictionService.PredictWithMaintenanceSchedulingAsync(equipmentId);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting enhanced prediction for equipment {EquipmentId}", equipmentId);
                return Json(new { success = false, errorMessage = ex.Message });
            }
        }

        /// <summary>
        /// Batch Enhanced Prediction - analyze multiple equipment at once
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> BatchEnhancedPrediction([FromBody] List<int> equipmentIds)
        {
            try
            {
                if (equipmentIds?.Any() != true)
                {
                    return Json(new { success = false, errorMessage = "No equipment IDs provided" });
                }

                var result = await _predictionService.PredictBatchWithMaintenanceSchedulingAsync(equipmentIds);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in batch enhanced prediction for {Count} equipment items", equipmentIds?.Count ?? 0);
                return Json(new { success = false, errorMessage = ex.Message });
            }
        }

        /// <summary>
        /// Dashboard view for all high-risk equipment with maintenance recommendations
        /// </summary>
        public async Task<IActionResult> MaintenanceDashboard()
        {
            try
            {
                // Get all active equipment
                var activeEquipment = await _context.Equipment
                    .Where(e => e.Status == Models.EquipmentStatus.Active)
                    .Select(e => e.EquipmentId)
                    .ToListAsync();

                // Get batch predictions
                var batchResults = await _predictionService.PredictBatchWithMaintenanceSchedulingAsync(activeEquipment);

                // Filter high-risk equipment
                var highRiskEquipment = batchResults.EnhancedPredictions
                    .Where(p => p.Success && (p.RiskLevel == "Critical" || p.RiskLevel == "High"))
                    .OrderByDescending(p => p.FailureProbability)
                    .ToList();

                // Get scheduled maintenance tasks
                var scheduledTasks = await _context.MaintenanceTasks
                    .Include(mt => mt.Equipment)
                    .ThenInclude(e => e.EquipmentType)
                    .Include(mt => mt.Equipment)
                    .ThenInclude(e => e.EquipmentModel)
                    .Where(mt => mt.Status == Models.MaintenanceStatus.Pending || 
                                mt.Status == Models.MaintenanceStatus.InProgress)
                    .OrderBy(mt => mt.ScheduledDate)
                    .Take(10)
                    .ToListAsync();

                ViewBag.Title = "Intelligent Maintenance Dashboard";
                ViewBag.Description = "AI-driven maintenance planning with predictive failure analysis";
                ViewBag.TotalEquipment = activeEquipment.Count;
                ViewBag.HighRiskCount = highRiskEquipment.Count;
                ViewBag.BatchResults = batchResults;
                ViewBag.ScheduledTasks = scheduledTasks;

                return View(highRiskEquipment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading maintenance dashboard");
                TempData["ErrorMessage"] = "Error loading maintenance dashboard.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Get maintenance recommendation for specific equipment
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMaintenanceRecommendation(int equipmentId)
        {
            try
            {
                // First get a basic prediction
                var equipment = await _context.Equipment.FindAsync(equipmentId);
                if (equipment == null)
                {
                    return Json(new { success = false, errorMessage = "Equipment not found" });
                }

                var predictionData = Models.EquipmentPredictionData.FromEquipment(equipment);
                var prediction = await _predictionService.PredictEquipmentFailureAsync(predictionData);

                if (!prediction.Success)
                {
                    return Json(new { success = false, errorMessage = prediction.ErrorMessage });
                }

                // Get maintenance recommendation
                var recommendation = await _predictionService.GetMaintenanceRecommendationAsync(equipmentId, prediction);

                return Json(new
                {
                    success = true,
                    equipmentId = equipmentId,
                    prediction = new
                    {
                        failureProbability = prediction.FailureProbability,
                        riskLevel = prediction.RiskLevel,
                        confidenceScore = prediction.ConfidenceScore
                    },
                    recommendation = new
                    {
                        recommendedDate = recommendation.RecommendedDate.ToString("yyyy-MM-dd"),
                        maintenanceType = recommendation.MaintenanceType,
                        priority = recommendation.Priority.ToString(),
                        description = recommendation.Description,
                        daysUntilRecommended = recommendation.DaysUntilRecommended,
                        estimatedFailureDate = recommendation.EstimatedFailureDate.ToString("yyyy-MM-dd"),
                        urgencyDisplay = recommendation.UrgencyDisplay,
                        priorityClass = recommendation.PriorityClass,
                        maintenanceBufferInfo = recommendation.MaintenanceBufferInfo
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting maintenance recommendation for equipment {EquipmentId}", equipmentId);
                return Json(new { success = false, errorMessage = ex.Message });
            }
        }

        /// <summary>
        /// Demo page showing the complete intelligent prediction workflow
        /// </summary>
        public async Task<IActionResult> IntelligentPredictionDemo()
        {
            try
            {
                // Get a few sample equipment items for demonstration
                var sampleEquipment = await _context.Equipment
                    .Include(e => e.EquipmentType)
                    .Include(e => e.EquipmentModel)
                    .Include(e => e.Building)
                    .Include(e => e.Room)
                    .Where(e => e.Status == Models.EquipmentStatus.Active)
                    .Take(5)
                    .ToListAsync();

                ViewBag.Title = "Intelligent Prediction System Demo";
                ViewBag.Description = "Demonstration of AI-powered equipment failure prediction with automatic maintenance scheduling";
                ViewBag.Features = new List<string>
                {
                    "Equipment feature analysis (name, location, specifications)",
                    "Potential failure cause identification", 
                    "Risk level assessment with probability scoring",
                    "Automatic maintenance scheduling 5-7 days before predicted failure",
                    "Integration with existing maintenance workflow",
                    "Real-time SignalR notifications for critical equipment"
                };

                return View(sampleEquipment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading intelligent prediction demo");
                TempData["ErrorMessage"] = "Error loading demo page.";
                return RedirectToAction("Index");
            }
        }
    }
}
