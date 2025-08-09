using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using FEENALOoFINALE.Data;
using FEENALOoFINALE.Models;
using FEENALOoFINALE.Hubs;

namespace FEENALOoFINALE.Services
{
    public class PredictiveAnalyticsService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<PredictiveAnalyticsService> _logger;
        private readonly IHubContext<MaintenanceHub> _hubContext;
        private readonly TimeSpan _analysisPeriod = TimeSpan.FromMinutes(30); // Run every 30 minutes

        public PredictiveAnalyticsService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<PredictiveAnalyticsService> logger,
            IHubContext<MaintenanceHub> hubContext)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Predictive Analytics Service started");

            try
            {
                // Wait 30 seconds before first analysis to reduce startup load
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await PerformPredictiveAnalysis();
                        await Task.Delay(_analysisPeriod, stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected when cancellation is requested
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred during predictive analysis");
                        try
                        {
                            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait 5 minutes before retry
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested during startup delay
                _logger.LogInformation("Predictive Analytics Service cancelled during startup");
            }
            
            _logger.LogInformation("Predictive Analytics Service stopped");
        }

        private async Task PerformPredictiveAnalysis()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            _logger.LogInformation("Starting predictive analysis cycle");

            // Get all active equipment
            var equipment = await dbContext.Equipment
                .Include(e => e.MaintenanceLogs)
                .Where(e => e.Status == EquipmentStatus.Active)
                .ToListAsync();

            var newPredictions = new List<FailurePrediction>();

            foreach (var item in equipment)
            {
                try
                {
                    var prediction = await AnalyzeEquipmentFailureRisk(item, dbContext);
                    if (prediction != null)
                    {
                        newPredictions.Add(prediction);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error analyzing equipment {EquipmentId}", item.EquipmentId);
                }
            }

            // Save new predictions
            if (newPredictions.Any())
            {
                dbContext.FailurePredictions.AddRange(newPredictions);
                await dbContext.SaveChangesAsync();

                // Notify clients about new predictions
                await _hubContext.Clients.All.SendAsync("PredictionsUpdated", newPredictions.Count);

                _logger.LogInformation("Generated {Count} new failure predictions", newPredictions.Count);
            }
        }

        private async Task<FailurePrediction?> AnalyzeEquipmentFailureRisk(Equipment equipment, ApplicationDbContext dbContext)
        {
            try
            {
                // Use ML prediction service for analysis
                var predictionService = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IEquipmentPredictionService>();
                
                // Check if we already have a recent prediction for this equipment
                var existingPrediction = await dbContext.FailurePredictions
                    .Where(fp => fp.EquipmentId == equipment.EquipmentId && 
                                fp.CreatedDate >= DateTime.Now.AddDays(-7))
                    .FirstOrDefaultAsync();

                if (existingPrediction == null)
                {
                    // Convert Equipment to EquipmentPredictionData using the static method
                    var predictionData = EquipmentPredictionData.FromEquipment(equipment);
                    var mlPrediction = await predictionService.PredictEquipmentFailureAsync(predictionData);
                    
                    if (mlPrediction != null && mlPrediction.Success && mlPrediction.FailureProbability > 0)
                    {
                        var riskLevel = mlPrediction.RiskLevel.ToLower() switch
                        {
                            "critical" => PredictionStatus.High,
                            "high" => PredictionStatus.High,
                            "medium" => PredictionStatus.Medium,
                            _ => PredictionStatus.Low
                        };

                        // Calculate predicted failure date based on probability
                        var daysToFailure = mlPrediction.FailureProbability switch
                        {
                            >= 0.9 => 7,   // Critical - 1 week
                            >= 0.8 => 14,  // High - 2 weeks  
                            >= 0.7 => 30,  // High - 1 month
                            >= 0.5 => 60,  // Medium - 2 months
                            _ => 90         // Low - 3 months
                        };

                        return new FailurePrediction
                        {
                            EquipmentId = equipment.EquipmentId,
                            CreatedDate = DateTime.Now,
                            PredictedFailureDate = DateTime.Now.AddDays(daysToFailure),
                            ConfidenceLevel = (int)(mlPrediction.ConfidenceScore * 100),
                            Status = riskLevel,
                            AnalysisNotes = $"ML prediction indicates {mlPrediction.RiskLevel} risk with {mlPrediction.FailureProbability:P1} failure probability. Confidence: {mlPrediction.ConfidenceScore:P1}",
                            ContributingFactors = $"Advanced ML analysis using {mlPrediction.ModelVersion}"
                        };
                    }
                    else
                    {
                        // Fallback to rule-based analysis if ML prediction fails
                        return FallbackRuleBasedAnalysis(equipment, dbContext);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ML prediction failed for equipment {EquipmentId}, falling back to rule-based analysis", equipment.EquipmentId);
                return FallbackRuleBasedAnalysis(equipment, dbContext);
            }

            return null;
        }

        private FailurePrediction? FallbackRuleBasedAnalysis(Equipment equipment, ApplicationDbContext dbContext)
        {
            // Simple predictive algorithm based on maintenance history and equipment age
            var recentMaintenanceLogs = equipment.MaintenanceLogs?
                .Where(ml => ml.LogDate >= DateTime.Now.AddDays(-90))
                .OrderByDescending(ml => ml.LogDate)
                .ToList() ?? new List<MaintenanceLog>();

            // Calculate failure probability based on various factors
            double failureProbability = 0.0;
            string riskFactors = "";

            // Factor 1: Age of equipment
            var equipmentAge = equipment.InstallationDate.HasValue 
                ? (DateTime.Now - equipment.InstallationDate.Value).TotalDays 
                : 0;
            if (equipmentAge > 365 * 5)
            {
                failureProbability += 0.2;
                riskFactors += "Equipment age over 5 years; ";
            }
            else if (equipmentAge > 365 * 3)
            {
                failureProbability += 0.1;
                riskFactors += "Equipment age over 3 years; ";
            }

            // Factor 2: Frequency of recent maintenance
            var maintenanceFrequency = recentMaintenanceLogs.Count;
            if (maintenanceFrequency > 5)
            {
                failureProbability += 0.3;
                riskFactors += "High maintenance frequency; ";
            }
            else if (maintenanceFrequency > 3)
            {
                failureProbability += 0.15;
                riskFactors += "Moderate maintenance frequency; ";
            }

            // Factor 3: Time since last maintenance
            var lastMaintenance = recentMaintenanceLogs.FirstOrDefault();
            if (lastMaintenance != null)
            {
                var daysSinceLastMaintenance = (DateTime.Now - lastMaintenance.LogDate).TotalDays;
                if (daysSinceLastMaintenance > 180)
                {
                    failureProbability += 0.25;
                    riskFactors += "No recent maintenance; ";
                }
                else if (daysSinceLastMaintenance > 90)
                {
                    failureProbability += 0.1;
                    riskFactors += "Limited recent maintenance; ";
                }
            }
            else
            {
                failureProbability += 0.2;
                riskFactors += "No maintenance history; ";
            }

            // Only create prediction if probability is significant
            if (failureProbability >= 0.3)
            {
                var riskLevel = failureProbability switch
                {
                    >= 0.7 => PredictionStatus.High,
                    >= 0.5 => PredictionStatus.High,
                    >= 0.3 => PredictionStatus.Medium,
                    _ => PredictionStatus.Low
                };

                return new FailurePrediction
                {
                    EquipmentId = equipment.EquipmentId,
                    CreatedDate = DateTime.Now,
                    PredictedFailureDate = DateTime.Now.AddDays(30 / failureProbability),
                    ConfidenceLevel = (int)Math.Min(failureProbability * 100, 95),
                    Status = riskLevel,
                    AnalysisNotes = $"Rule-based analysis indicates {riskLevel.ToString().ToLower()} risk of failure. Risk factors: {riskFactors.TrimEnd(' ', ';')}",
                    ContributingFactors = "Rule-based fallback analysis"
                };
            }

            return null;
        }

        // Add this method to allow retraining/updating the model with new maintenance logs
        public async Task UpdateModelWithMaintenanceLogAsync(MaintenanceLog log)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Gather all relevant logs and usage data for the equipment
            var logs = await dbContext.MaintenanceLogs
                .Where(l => l.EquipmentId == log.EquipmentId)
                .ToListAsync();

            var usage = await dbContext.EquipmentUsageHistories
                .Where(u => u.EquipmentId == log.EquipmentId)
                .ToListAsync();

            // Call your ML retraining/update logic here
            // e.g., MLModelTrainer.UpdateModel(logs, usage);

            await Task.CompletedTask;
        }
    }
}
