using FEENALOoFINALE.Data;
using FEENALOoFINALE.Models;
using FEENALOoFINALE.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FEENALOoFINALE.Services
{
    /// <summary>
    /// Enhanced Equipment Tracking Service - Automatically integrates new equipment with ML prediction system
    /// </summary>
    public class EnhancedEquipmentTrackingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEquipmentPredictionService _predictionService;
        private readonly ILogger<EnhancedEquipmentTrackingService> _logger;
        public EnhancedEquipmentTrackingService(
            ApplicationDbContext context,
            IEquipmentPredictionService predictionService,
            ILogger<EnhancedEquipmentTrackingService> logger)
        {
            _context = context;
            _predictionService = predictionService;
            _logger = logger;
        }

        /// <summary>
        /// Automatically track new equipment in ML system
        /// </summary>
        public async Task<bool> AutoRegisterNewEquipmentAsync(Equipment equipment)
        {
            try
            {
                _logger.LogInformation($"🔧 Auto-registering new equipment {equipment.EquipmentId} in ML system");

                // Immediate ML analysis for new equipment
                var predictionData = EquipmentPredictionData.FromEquipment(equipment);
                var prediction = await _predictionService.PredictEquipmentFailureAsync(predictionData);

                if (prediction != null && prediction.Success)
                {
                    _logger.LogInformation($"📊 ML prediction for new equipment {equipment.EquipmentId}: {prediction.FailureProbability:P2} failure probability ({prediction.RiskLevel})");

                    // Create immediate alert for high-risk new equipment (≥60% failure probability)
                    if (prediction.FailureProbability >= 0.6)
                    {
                        var alert = new Alert
                        {
                            Title = $"🚨 New High-Risk Equipment Detected",
                            Description = $"Newly added equipment '{equipment.EquipmentModel?.ModelName ?? equipment.EquipmentType?.EquipmentTypeName ?? "Unknown"}' " +
                                        $"has {prediction.FailureProbability:P1} failure probability. Immediate inspection recommended.",
                            Priority = prediction.FailureProbability >= 0.8 ? AlertPriority.High : AlertPriority.Medium,
                            EquipmentId = equipment.EquipmentId,
                            Status = AlertStatus.Open,
                            CreatedDate = DateTime.Now
                        };

                        _context.Alerts.Add(alert);
                        await _context.SaveChangesAsync();

                        _logger.LogWarning($"⚠️ High-risk equipment alert created for {equipment.EquipmentId}");
                    }

                    // Store the prediction in the database for historical tracking
                    var failurePrediction = prediction.ToFailurePrediction(equipment.EquipmentId);
                    _context.FailurePredictions.Add(failurePrediction);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"✅ New equipment {equipment.EquipmentId} successfully registered in ML system");
                    return true;
                }
                else
                {
                    _logger.LogWarning($"⚠️ ML prediction failed for new equipment {equipment.EquipmentId}: {prediction?.ErrorMessage}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error auto-registering equipment {equipment.EquipmentId} in ML system");
                // Don't fail equipment creation due to ML registration errors
                return false;
            }
        }

        /// <summary>
        /// Batch register multiple equipment items in ML system
        /// </summary>
        public async Task<int> BatchRegisterEquipmentAsync(IEnumerable<Equipment> equipmentList)
        {
            var successCount = 0;
            var equipmentArray = equipmentList.ToArray();

            _logger.LogInformation($"🔧 Batch registering {equipmentArray.Length} equipment items in ML system");

            foreach (var equipment in equipmentArray)
            {
                var success = await AutoRegisterNewEquipmentAsync(equipment);
                if (success) successCount++;
            }

            _logger.LogInformation($"✅ Batch registration complete: {successCount}/{equipmentArray.Length} equipment items successfully registered");
            return successCount;
        }

        /// <summary>
        /// Check if equipment needs ML re-evaluation (after maintenance, age milestones, etc.)
        /// </summary>
        public async Task<bool> ReEvaluateEquipmentAsync(Equipment equipment, string reason = "Manual re-evaluation")
        {
            try
            {
                _logger.LogInformation($"🔄 Re-evaluating equipment {equipment.EquipmentId} in ML system. Reason: {reason}");

                var predictionData = EquipmentPredictionData.FromEquipment(equipment);
                var prediction = await _predictionService.PredictEquipmentFailureAsync(predictionData);

                if (prediction != null && prediction.Success)
                {
                    // Store the new prediction
                    var failurePrediction = prediction.ToFailurePrediction(equipment.EquipmentId);
                    failurePrediction.AnalysisNotes += $" - Re-evaluation: {reason}";
                    
                    _context.FailurePredictions.Add(failurePrediction);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"📊 Re-evaluation complete for equipment {equipment.EquipmentId}: {prediction.FailureProbability:P2} ({prediction.RiskLevel})");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error re-evaluating equipment {equipment.EquipmentId}");
                return false;
            }
        }

        /// <summary>
        /// Get ML tracking status for equipment
        /// </summary>
        public async Task<EquipmentMLTrackingStatus> GetTrackingStatusAsync(int equipmentId)
        {
            try
            {
                var equipment = await _context.Equipment
                    .Include(e => e.EquipmentType)
                    .Include(e => e.EquipmentModel)
                    .FirstOrDefaultAsync(e => e.EquipmentId == equipmentId);

                if (equipment == null)
                {
                    return new EquipmentMLTrackingStatus
                    {
                        EquipmentId = equipmentId,
                        IsTracked = false,
                        ErrorMessage = "Equipment not found"
                    };
                }

                var latestPrediction = await _context.FailurePredictions
                    .Where(fp => fp.EquipmentId == equipmentId)
                    .OrderByDescending(fp => fp.CreatedDate)
                    .FirstOrDefaultAsync();

                var alertCount = await _context.Alerts
                    .CountAsync(a => a.EquipmentId == equipmentId && a.Status == AlertStatus.Open);

                return new EquipmentMLTrackingStatus
                {
                    EquipmentId = equipmentId,
                    IsTracked = latestPrediction != null,
                    LastPredictionDate = latestPrediction?.CreatedDate,
                    CurrentRiskLevel = latestPrediction?.Status.ToString() ?? "Unknown",
                    OpenAlertsCount = alertCount,
                    EquipmentName = equipment.EquipmentModel?.ModelName ?? equipment.EquipmentType?.EquipmentTypeName ?? "Unknown"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting tracking status for equipment {equipmentId}");
                return new EquipmentMLTrackingStatus
                {
                    EquipmentId = equipmentId,
                    IsTracked = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }

    /// <summary>
    /// Equipment ML tracking status information
    /// </summary>
    public class EquipmentMLTrackingStatus
    {
        public int EquipmentId { get; set; }
        public bool IsTracked { get; set; }
        public DateTime? LastPredictionDate { get; set; }
        public string CurrentRiskLevel { get; set; } = "";
        public int OpenAlertsCount { get; set; }
        public string EquipmentName { get; set; } = "";
        public string? ErrorMessage { get; set; }
    }
}
