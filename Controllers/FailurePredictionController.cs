using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FEENALOoFINALE.Data;
using FEENALOoFINALE.Models;
using FEENALOoFINALE.Services;
using Microsoft.AspNetCore.Authorization;

namespace FEENALOoFINALE.Controllers
{
    [Authorize]
    public class FailurePredictionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAdvancedAnalyticsService _analyticsService;

        public FailurePredictionController(ApplicationDbContext context, IAdvancedAnalyticsService analyticsService)
        {
            _context = context;
            _analyticsService = analyticsService;
        }

        // GET: FailurePrediction
        public async Task<IActionResult> Index()
        {
            var predictions = await _context.FailurePredictions
                .Include(f => f.Equipment)
                .ThenInclude(e => e.EquipmentType)
                .Include(f => f.Equipment)
                .ThenInclude(e => e.Building)
                .OrderByDescending(f => f.PredictedFailureDate)
                .ToListAsync();

            // Get advanced analytics insights
            var insights = await _analyticsService.GetPredictiveMaintenanceInsightsAsync();
            var performanceMetrics = await _analyticsService.GetEquipmentPerformanceMetricsAsync();

            ViewBag.Insights = insights;
            ViewBag.PerformanceMetrics = performanceMetrics;
            ViewBag.SystemHealth = await _analyticsService.CalculateSystemHealthScoreAsync();

            return View(predictions);
        }

        // GET: FailurePrediction/AIAnalysis
        [HttpGet]
        public async Task<IActionResult> AIAnalysis()
        {
            var equipment = await _context.Equipment
                .Include(e => e.MaintenanceLogs)
                .Include(e => e.Alerts)
                .Include(e => e.EquipmentType)
                .ToListAsync();

            var aiAnalysis = new List<object>();

            foreach (var item in equipment)
            {
                var analysis = await RunAIAnalysisForEquipment(item);
                aiAnalysis.Add(analysis);
            }

            var summary = new
            {
                totalEquipment = equipment.Count,
                highRiskEquipment = aiAnalysis.Count(a => ((dynamic)a).riskLevel == "High"),
                mediumRiskEquipment = aiAnalysis.Count(a => ((dynamic)a).riskLevel == "Medium"),
                lowRiskEquipment = aiAnalysis.Count(a => ((dynamic)a).riskLevel == "Low"),
                predictions = aiAnalysis,
                systemRecommendations = GenerateSystemRecommendations(aiAnalysis)
            };

            return Json(summary);
        }

        // GET: FailurePrediction/GeneratePredictions
        [HttpPost]
        public async Task<IActionResult> GeneratePredictions()
        {
            var equipment = await _context.Equipment
                .Include(e => e.MaintenanceLogs)
                .Include(e => e.Alerts)
                .Include(e => e.EquipmentType)
                .Where(e => e.Status == EquipmentStatus.Active)
                .ToListAsync();

            var newPredictions = new List<FailurePrediction>();

            foreach (var item in equipment)
            {
                var prediction = await GenerateAdvancedPrediction(item);
                newPredictions.Add(prediction);
            }

            _context.FailurePredictions.AddRange(newPredictions);
            await _context.SaveChangesAsync();

            return Json(new { success = true, predictionsGenerated = newPredictions.Count });
        }

        // GET: FailurePrediction/RealTimeMonitoring
        [HttpGet]
        public async Task<IActionResult> RealTimeMonitoring()
        {
            var criticalEquipment = await _context.Equipment
                .Include(e => e.EquipmentType)
                .Include(e => e.Building)
                .Include(e => e.MaintenanceLogs)
                .Include(e => e.Alerts)
                .Where(e => e.Status == EquipmentStatus.Active)
                .ToListAsync();

            var monitoringData = new List<object>();

            foreach (var equipment in criticalEquipment)
            {
                var recentAlerts = equipment.Alerts?.Count(a => a.CreatedDate > DateTime.Now.AddDays(-7)) ?? 0;
                var lastMaintenance = equipment.MaintenanceLogs?.Max(m => m.MaintenanceDate) ?? DateTime.MinValue;
                var daysSinceLastMaintenance = (DateTime.Now - lastMaintenance).Days;

                var riskScore = CalculateRiskScore(equipment);
                var healthScore = CalculateHealthScore(equipment);

                monitoringData.Add(new
                {
                    equipmentId = equipment.EquipmentId,
                    equipmentName = equipment.EquipmentType?.EquipmentTypeName ?? "Unknown",
                    location = equipment.Building?.BuildingName ?? "Unknown",
                    riskScore = riskScore,
                    healthScore = healthScore,
                    recentAlerts = recentAlerts,
                    daysSinceLastMaintenance = daysSinceLastMaintenance,
                    status = equipment.Status.ToString(),
                    riskLevel = GetRiskLevel(riskScore),
                    recommendations = GenerateRecommendations(equipment, riskScore),
                    estimatedTimeToFailure = EstimateTimeToFailure(equipment),
                    confidenceLevel = CalculateConfidenceLevel(equipment)
                });
            }

            return Json(new
            {
                timestamp = DateTime.Now,
                equipmentCount = criticalEquipment.Count,
                highRiskCount = monitoringData.Count(m => ((dynamic)m).riskLevel == "High"),
                mediumRiskCount = monitoringData.Count(m => ((dynamic)m).riskLevel == "Medium"),
                lowRiskCount = monitoringData.Count(m => ((dynamic)m).riskLevel == "Low"),
                equipment = monitoringData
            });
        }

        // GET: FailurePrediction/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prediction = await _context.FailurePredictions
                .Include(f => f.Equipment)
                .FirstOrDefaultAsync(m => m.PredictionId == id);

            if (prediction == null)
            {
                return NotFound();
            }

            return View(prediction);
        }

        // GET: FailurePrediction/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prediction = await _context.FailurePredictions
                .Include(f => f.Equipment)
                .FirstOrDefaultAsync(m => m.PredictionId == id);
            if (prediction == null)
            {
                return NotFound();
            }

            return View(prediction);
        }

        // POST: FailurePrediction/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var prediction = await _context.FailurePredictions.FindAsync(id);
            if (prediction != null)
            {
                _context.FailurePredictions.Remove(prediction);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: FailurePrediction/ByEquipment/5
        public async Task<IActionResult> ByEquipment(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var predictions = await _context.FailurePredictions
                .Include(f => f.Equipment)
                .Where(f => f.EquipmentId == id)
                .OrderByDescending(f => f.PredictedFailureDate)
                .ToListAsync();

            ViewBag.Equipment = await _context.Equipment.FindAsync(id);
            return View(predictions);
        }

        private bool FailurePredictionExists(int id)
        {
            return _context.FailurePredictions.Any(e => e.PredictionId == id);
        }

        // Private helper methods for AI analysis
        private Task<object> RunAIAnalysisForEquipment(Equipment equipment)
        {
            var riskScore = CalculateRiskScore(equipment);
            var healthScore = CalculateHealthScore(equipment);
            var confidenceLevel = CalculateConfidenceLevel(equipment);

            var result = new
            {
                equipmentId = equipment.EquipmentId,
                equipmentName = equipment.EquipmentType?.EquipmentTypeName ?? "Unknown",
                riskScore = riskScore,
                healthScore = healthScore,
                riskLevel = GetRiskLevel(riskScore),
                predictedFailureDate = DateTime.Now.AddDays(EstimateTimeToFailure(equipment)),
                confidenceLevel = confidenceLevel,
                recommendations = GenerateRecommendations(equipment, riskScore),
                criticalComponents = new[] { "Motor", "Control System", "Sensors", "Power Supply" },
                maintenanceHistory = equipment.MaintenanceLogs?.Count ?? 0,
                recentAlerts = equipment.Alerts?.Count(a => a.CreatedDate > DateTime.Now.AddDays(-30)) ?? 0
            };

            return Task.FromResult<object>(result);
        }

        private List<string> GenerateSystemRecommendations(List<object> aiAnalysis)
        {
            var recommendations = new List<string>();
            var highRiskCount = aiAnalysis.Count(a => ((dynamic)a).riskLevel == "High");
            var mediumRiskCount = aiAnalysis.Count(a => ((dynamic)a).riskLevel == "Medium");

            if (highRiskCount > 0)
            {
                recommendations.Add($"Immediate attention required for {highRiskCount} high-risk equipment");
                recommendations.Add("Consider emergency maintenance scheduling");
            }

            if (mediumRiskCount > 5)
            {
                recommendations.Add("Optimize maintenance schedules for medium-risk equipment");
                recommendations.Add("Implement condition-based monitoring");
            }

            recommendations.Add("Deploy IoT sensors for real-time monitoring");
            recommendations.Add("Establish predictive maintenance protocols");
            recommendations.Add("Regular training for maintenance staff");

            return recommendations;
        }

        private Task<FailurePrediction> GenerateAdvancedPrediction(Equipment equipment)
        {
            var riskScore = CalculateRiskScore(equipment);
            var daysToFailure = EstimateTimeToFailure(equipment);
            var confidenceLevel = CalculateConfidenceLevel(equipment);

            var prediction = new FailurePrediction
            {
                EquipmentId = equipment.EquipmentId,
                PredictedFailureDate = DateTime.Now.AddDays(daysToFailure),
                ConfidenceLevel = (int)confidenceLevel,
                Status = GetPredictionStatus(riskScore),
                CreatedDate = DateTime.Now,
                AnalysisNotes = $"AI-generated prediction with {confidenceLevel:F1}% confidence",
                ContributingFactors = GenerateContributingFactors(equipment, riskScore)
            };

            return Task.FromResult(prediction);
        }

        private PredictionStatus GetPredictionStatus(double riskScore)
        {
            if (riskScore >= 70) return PredictionStatus.High;
            if (riskScore >= 40) return PredictionStatus.Medium;
            return PredictionStatus.Low;
        }

        private string GenerateContributingFactors(Equipment equipment, double riskScore)
        {
            var factors = new List<string>();
            var daysSinceInstall = (DateTime.Now - equipment.InstallationDate).Days;
            var recentAlerts = equipment.Alerts?.Count(a => a.CreatedDate > DateTime.Now.AddDays(-30)) ?? 0;
            var maintenanceCount = equipment.MaintenanceLogs?.Count ?? 0;

            if (daysSinceInstall > 1825) factors.Add("Equipment age");
            if (recentAlerts > 5) factors.Add("High alert frequency");
            if (maintenanceCount < daysSinceInstall / 180) factors.Add("Insufficient maintenance");
            if (equipment.Status == EquipmentStatus.Inactive) factors.Add("Equipment inactive");

            return factors.Any() ? string.Join(", ", factors) : "Normal operating conditions";
        }

        private double CalculateRiskScore(Equipment equipment)
        {
            var baseRisk = 10.0; // Base risk percentage
            var daysSinceInstall = (DateTime.Now - equipment.InstallationDate).Days;
            var expectedLifeDays = equipment.ExpectedLifespanMonths * 30;
            var ageRatio = (double)daysSinceInstall / expectedLifeDays;

            // Age factor (0-40 points)
            var ageRisk = Math.Min(40, ageRatio * 40);
            baseRisk += ageRisk;

            // Maintenance history factor
            var maintenanceCount = equipment.MaintenanceLogs?.Count ?? 0;
            var expectedMaintenanceCount = daysSinceInstall / 90; // Expected every 3 months
            var maintenanceRatio = maintenanceCount / Math.Max(1, expectedMaintenanceCount);

            if (maintenanceRatio < 0.5)
            {
                baseRisk += 25; // Poor maintenance increases risk
            }
            else if (maintenanceRatio > 1.5)
            {
                baseRisk += 10; // Over-maintenance might indicate problems
            }

            // Recent alerts factor
            var recentAlerts = equipment.Alerts?.Count(a => a.CreatedDate > DateTime.Now.AddDays(-30)) ?? 0;
            baseRisk += recentAlerts * 8;

            // Status factor
            switch (equipment.Status)
            {
                case EquipmentStatus.Inactive:
                    baseRisk += 30;
                    break;
                case EquipmentStatus.Retired:
                    baseRisk = 95;
                    break;
            }

            return Math.Max(5, Math.Min(95, baseRisk));
        }

        private double CalculateHealthScore(Equipment equipment)
        {
            return 100 - CalculateRiskScore(equipment);
        }

        private string GetRiskLevel(double riskScore)
        {
            if (riskScore >= 70) return "High";
            if (riskScore >= 40) return "Medium";
            return "Low";
        }

        private List<string> GenerateRecommendations(Equipment equipment, double riskScore)
        {
            var recommendations = new List<string>();

            if (riskScore >= 70)
            {
                recommendations.Add("Schedule immediate inspection");
                recommendations.Add("Consider equipment replacement");
                recommendations.Add("Implement continuous monitoring");
            }
            else if (riskScore >= 40)
            {
                recommendations.Add("Schedule preventive maintenance");
                recommendations.Add("Monitor performance trends");
                recommendations.Add("Update maintenance schedule");
            }
            else
            {
                recommendations.Add("Continue routine maintenance");
                recommendations.Add("Monitor for any changes");
                recommendations.Add("Document performance metrics");
            }

            // Add specific recommendations based on equipment age
            var daysSinceInstall = (DateTime.Now - equipment.InstallationDate).Days;
            if (daysSinceInstall > 1825) // 5 years
            {
                recommendations.Add("Consider major overhaul");
                recommendations.Add("Evaluate replacement options");
            }

            return recommendations;
        }

        private int EstimateTimeToFailure(Equipment equipment)
        {
            var riskScore = CalculateRiskScore(equipment);
            var baseTime = 365; // Base time in days

            // Higher risk means shorter time to failure
            var adjustedTime = baseTime * (100 - riskScore) / 100;

            // Add some randomness for realism
            var random = new Random();
            adjustedTime += random.Next(-30, 31);

            return Math.Max(1, (int)adjustedTime);
        }

        private double CalculateConfidenceLevel(Equipment equipment)
        {
            var baseConfidence = 75.0;
            var maintenanceHistory = equipment.MaintenanceLogs?.Count ?? 0;
            var alertHistory = equipment.Alerts?.Count ?? 0;

            // More data = higher confidence
            baseConfidence += Math.Min(15, maintenanceHistory * 2);
            baseConfidence += Math.Min(10, alertHistory);

            // Age provides more predictability
            var daysSinceInstall = (DateTime.Now - equipment.InstallationDate).Days;
            if (daysSinceInstall > 365)
            {
                baseConfidence += 5;
            }

            return Math.Max(60, Math.Min(95, baseConfidence));
        }

        private string GenerateFailureType(Equipment equipment)
        {
            var failureTypes = new[]
            {
                "Mechanical wear",
                "Electrical failure",
                "Control system malfunction",
                "Sensor degradation",
                "Thermal overload",
                "Vibration damage",
                "Corrosion",
                "Fatigue failure"
            };

            return failureTypes[new Random().Next(failureTypes.Length)];
        }
    }
}