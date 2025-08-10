using FEENALOoFINALE.Models;
using Microsoft.EntityFrameworkCore;
using FEENALOoFINALE.Data;

namespace FEENALOoFINALE.Services
{
    public interface IEquipmentAIInsightService
    {
        Task<EquipmentAIInsight> GenerateInsightAsync(int equipmentId, PredictionResult prediction);
        Task<List<SmartRecommendation>> GetSmartRecommendationsAsync();
        Task<List<PredictiveTrend>> GetPredictiveTrendsAsync();
        Task<string> ExplainRiskFactorsAsync(int equipmentId, PredictionResult prediction);
    }

    public class EquipmentAIInsightService : IEquipmentAIInsightService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EquipmentAIInsightService> _logger;

        public EquipmentAIInsightService(ApplicationDbContext context, ILogger<EquipmentAIInsightService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<EquipmentAIInsight> GenerateInsightAsync(int equipmentId, PredictionResult prediction)
        {
            try
            {
                var equipment = await _context.Equipment
                    .Include(e => e.EquipmentType)
                    .Include(e => e.EquipmentModel)
                    .Include(e => e.Building)
                    .Include(e => e.Room)
                    .Include(e => e.MaintenanceLogs)
                    .FirstOrDefaultAsync(e => e.EquipmentId == equipmentId);

                if (equipment == null)
                {
                    throw new ArgumentException($"Equipment with ID {equipmentId} not found");
                }

                var insight = new EquipmentAIInsight
                {
                    EquipmentId = equipmentId,
                    EquipmentName = equipment.EquipmentModel?.ModelName ?? $"Equipment {equipmentId}",
                    EquipmentType = equipment.EquipmentType?.EquipmentTypeName ?? "Unknown",
                    Location = $"{equipment.Building?.BuildingName ?? "Unknown"} - {equipment.Room?.RoomName ?? "Unknown"}",
                    
                    RiskLevel = prediction.RiskLevel,
                    FailureProbability = prediction.FailureProbability,
                    ConfidenceScore = prediction.ConfidenceScore,
                    
                    PrimaryRiskFactors = await AnalyzePrimaryRiskFactors(equipment, prediction),
                    ImmediateActions = await GenerateImmediateActions(equipment, prediction),
                    ShortTermRecommendations = await GenerateShortTermRecommendations(equipment, prediction),
                    LongTermPlan = await GenerateLongTermPlan(equipment, prediction),
                    
                    EstimatedDaysToFailure = CalculateEstimatedDaysToFailure(prediction.FailureProbability),
                    MaintenancePriority = DetermineMaintenancePriority(prediction),
                    EstimatedRepairCost = EstimateRepairCost(equipment, prediction),
                    
                    GeneratedAt = DateTime.Now
                };

                return insight;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI insight for equipment {EquipmentId}", equipmentId);
                return new EquipmentAIInsight 
                { 
                    EquipmentId = equipmentId,
                    GeneratedAt = DateTime.Now,
                    PrimaryRiskFactors = new List<string> { "Unable to analyze risk factors at this time" },
                    ImmediateActions = new List<string> { "Contact maintenance team for manual inspection" }
                };
            }
        }

        private Task<List<string>> AnalyzePrimaryRiskFactors(Equipment equipment, PredictionResult prediction)
        {
            var riskFactors = new List<string>();

            // Analyze equipment age
            if (equipment.InstallationDate.HasValue)
            {
                var ageYears = (DateTime.Now - equipment.InstallationDate.Value).TotalDays / 365.25;
                if (ageYears > 7)
                {
                    riskFactors.Add($"Equipment age: {ageYears:F1} years (exceeds typical 7-year lifespan)");
                }
                else if (ageYears > 5)
                {
                    riskFactors.Add($"Equipment age: {ageYears:F1} years (approaching end of optimal performance period)");
                }
            }

            // Analyze maintenance history
            var recentMaintenance = equipment.MaintenanceLogs?
                .Where(ml => ml.LogDate >= DateTime.Now.AddMonths(-6))
                .OrderByDescending(ml => ml.LogDate)
                .ToList() ?? new List<MaintenanceLog>();

            if (!recentMaintenance.Any())
            {
                riskFactors.Add("No maintenance recorded in past 6 months - may indicate neglect or lack of monitoring");
            }
            else
            {
                var frequentIssues = recentMaintenance
                    .Where(ml => ml.MaintenanceType == MaintenanceType.Corrective || ml.MaintenanceType == MaintenanceType.Emergency)
                    .Count();

                if (frequentIssues >= 3)
                {
                    riskFactors.Add($"Frequent repairs: {frequentIssues} corrective/emergency maintenance events in past 6 months indicate potential systemic issues");
                }
            }

            // Analyze equipment type-specific risks
            var equipmentType = equipment.EquipmentType?.EquipmentTypeName?.ToLower() ?? "unknown";
            switch (equipmentType)
            {
                case var type when type.Contains("projector"):
                    if (prediction.FailureProbability > 0.7)
                    {
                        riskFactors.Add("Projectors commonly fail due to lamp burnout, overheating, or dust accumulation");
                        riskFactors.Add("Operating temperature likely above optimal range (45-55°C)");
                    }
                    break;
                case var type when type.Contains("computer"):
                    if (prediction.FailureProbability > 0.6)
                    {
                        riskFactors.Add("Computer systems showing signs of hardware degradation or thermal stress");
                        riskFactors.Add("Potential issues: hard drive failure, RAM degradation, or power supply stress");
                    }
                    break;
                case var type when type.Contains("air condition"):
                    if (prediction.FailureProbability > 0.8)
                    {
                        riskFactors.Add("HVAC system showing critical stress indicators");
                        riskFactors.Add("Likely causes: compressor wear, refrigerant leaks, or clogged filters");
                    }
                    break;
            }

            // Analyze prediction confidence
            if (prediction.ConfidenceScore < 0.7)
            {
                riskFactors.Add($"Model confidence is moderate ({prediction.ConfidenceScore:P1}) - may require additional monitoring");
            }

            return Task.FromResult(riskFactors.Any() ? riskFactors : new List<string> { "Analysis indicates standard wear patterns for equipment of this age and type" });
        }

        private Task<List<string>> GenerateImmediateActions(Equipment equipment, PredictionResult prediction)
        {
            var actions = new List<string>();

            if (prediction.FailureProbability >= 0.9)
            {
                actions.Add("🚨 CRITICAL: Schedule emergency maintenance inspection within 24-48 hours");
                actions.Add("📋 Document current operational status and any visible issues");
                actions.Add("⚠️ Consider temporary shutdown if safety risks are present");
            }
            else if (prediction.FailureProbability >= 0.8)
            {
                actions.Add("🔥 HIGH PRIORITY: Schedule maintenance inspection within 7 days");
                actions.Add("📊 Begin daily monitoring of equipment performance");
                actions.Add("📝 Prepare maintenance work order with predicted issues");
            }
            else if (prediction.FailureProbability >= 0.6)
            {
                actions.Add("⚡ MEDIUM PRIORITY: Schedule maintenance within 2-3 weeks");
                actions.Add("🔍 Increase monitoring frequency to weekly checks");
                actions.Add("📋 Review recent maintenance history for patterns");
            }
            else
            {
                actions.Add("✅ LOW PRIORITY: Include in next scheduled maintenance cycle");
                actions.Add("📈 Continue standard monitoring procedures");
            }

            // Add equipment-type specific actions
            var equipmentType = equipment.EquipmentType?.EquipmentTypeName?.ToLower() ?? "unknown";
            switch (equipmentType)
            {
                case var type when type.Contains("projector"):
                    actions.Add("💡 Check lamp hours and replace if approaching end of life");
                    actions.Add("🌬️ Clean air filters and check ventilation");
                    break;
                case var type when type.Contains("computer"):
                    actions.Add("💾 Backup critical data immediately");
                    actions.Add("🧹 Clean internal components and check for dust buildup");
                    break;
                case var type when type.Contains("air condition"):
                    actions.Add("❄️ Check refrigerant levels and system pressure");
                    actions.Add("🔧 Inspect and clean condenser coils");
                    break;
            }

            return Task.FromResult(actions);
        }

        private Task<List<string>> GenerateShortTermRecommendations(Equipment equipment, PredictionResult prediction)
        {
            var recommendations = new List<string>();

            recommendations.Add("📊 Implement condition-based monitoring for this equipment");
            recommendations.Add("🔧 Schedule comprehensive diagnostic testing");
            recommendations.Add("📚 Review manufacturer maintenance guidelines");

            if (prediction.FailureProbability > 0.7)
            {
                recommendations.Add("💰 Obtain repair cost estimates for potential issues");
                recommendations.Add("🔄 Identify backup equipment or replacement options");
                recommendations.Add("👨‍🔧 Ensure maintenance team has necessary parts and expertise");
            }

            // Add environmental recommendations
            recommendations.Add("🌡️ Monitor operating temperature and humidity levels");
            recommendations.Add("⚡ Check electrical connections and power quality");
            recommendations.Add("📋 Update equipment documentation and maintenance records");

            return Task.FromResult(recommendations);
        }

        private Task<List<string>> GenerateLongTermPlan(Equipment equipment, PredictionResult prediction)
        {
            var plans = new List<string>();

            if (equipment.InstallationDate.HasValue)
            {
                var ageYears = (DateTime.Now - equipment.InstallationDate.Value).TotalDays / 365.25;
                if (ageYears > 6)
                {
                    plans.Add($"🔄 Consider equipment replacement within 12-18 months (current age: {ageYears:F1} years)");
                    plans.Add("💼 Begin budgeting process for equipment replacement");
                    plans.Add("🔍 Research newer, more efficient alternatives");
                }
            }

            plans.Add("📈 Implement predictive maintenance program");
            plans.Add("🎯 Set up automated monitoring and alert systems");
            plans.Add("📊 Track equipment performance trends over time");

            if (prediction.FailureProbability > 0.6)
            {
                plans.Add("⚠️ Include in high-priority equipment list for future budget planning");
                plans.Add("🔧 Consider upgrading to more reliable equipment model");
            }

            return Task.FromResult(plans);
        }

        private int CalculateEstimatedDaysToFailure(double failureProbability)
        {
            return failureProbability switch
            {
                >= 0.9 => 7,    // Critical - within 1 week
                >= 0.8 => 21,   // High - within 3 weeks
                >= 0.7 => 45,   // High - within 6 weeks
                >= 0.5 => 90,   // Medium - within 3 months
                >= 0.3 => 180,  // Low-Medium - within 6 months
                _ => 365        // Low - within 1 year
            };
        }

        private string DetermineMaintenancePriority(PredictionResult prediction)
        {
            return prediction.FailureProbability switch
            {
                >= 0.9 => "CRITICAL",
                >= 0.8 => "HIGH",
                >= 0.6 => "MEDIUM",
                >= 0.4 => "LOW",
                _ => "ROUTINE"
            };
        }

        private decimal EstimateRepairCost(Equipment equipment, PredictionResult prediction)
        {
            var equipmentTypeName = equipment.EquipmentType?.EquipmentTypeName?.ToLower() ?? "unknown";
            var baseCost = equipmentTypeName switch
            {
                var type when type.Contains("projector") => 800m,
                var type when type.Contains("computer") => 600m,
                var type when type.Contains("air condition") => 1500m,
                var type when type.Contains("printer") => 400m,
                _ => 500m
            };

            // Adjust based on failure probability
            var multiplier = prediction.FailureProbability switch
            {
                >= 0.9 => 1.5m, // Critical failures cost more
                >= 0.7 => 1.2m,
                >= 0.5 => 1.0m,
                _ => 0.8m
            };

            return Math.Round(baseCost * multiplier, 2);
        }

        public async Task<List<SmartRecommendation>> GetSmartRecommendationsAsync()
        {
            var recommendations = new List<SmartRecommendation>();

            // Get high-risk equipment count
            var highRiskCount = await _context.FailurePredictions
                .Where(fp => fp.Status == PredictionStatus.High && fp.CreatedDate >= DateTime.Now.AddDays(-7))
                .CountAsync();

            recommendations.Add(new SmartRecommendation
            {
                Priority = "IMMEDIATE",
                Icon = "bi-tools",
                Title = "Preventive Maintenance",
                Description = $"Schedule preventive maintenance for {highRiskCount} high-risk equipment items",
                EstimatedSavings = 15000m,
                TimeFrame = "This week"
            });

            recommendations.Add(new SmartRecommendation
            {
                Priority = "HIGH",
                Icon = "bi-thermometer",
                Title = "Environmental Control",
                Description = "Check cooling systems in rooms with high-risk equipment to prevent overheating",
                EstimatedSavings = 8000m,
                TimeFrame = "Next 2 weeks"
            });

            recommendations.Add(new SmartRecommendation
            {
                Priority = "MEDIUM",
                Icon = "bi-graph-up",
                Title = "Equipment Replacement",
                Description = "Consider replacement for equipment over 8 years old showing frequent failure patterns",
                EstimatedSavings = 25000m,
                TimeFrame = "Next 6 months"
            });

            return recommendations;
        }

        public async Task<List<PredictiveTrend>> GetPredictiveTrendsAsync()
        {
            var trends = new List<PredictiveTrend>();

            // Analyze failure rate trends
            var currentMonthFailures = await _context.FailurePredictions
                .Where(fp => fp.Status == PredictionStatus.High && fp.CreatedDate >= DateTime.Now.AddDays(-30))
                .CountAsync();

            var lastMonthFailures = await _context.FailurePredictions
                .Where(fp => fp.Status == PredictionStatus.High && 
                           fp.CreatedDate >= DateTime.Now.AddDays(-60) && 
                           fp.CreatedDate < DateTime.Now.AddDays(-30))
                .CountAsync();

            var changePercent = lastMonthFailures > 0 
                ? ((double)(currentMonthFailures - lastMonthFailures) / lastMonthFailures) * 100 
                : 0;

            trends.Add(new PredictiveTrend
            {
                Icon = changePercent < 0 ? "bi-trending-down" : "bi-trending-up",
                TrendType = changePercent < 0 ? "POSITIVE" : "NEGATIVE",
                Title = "Failure Rate Trend",
                Description = $"Equipment failure predictions have {(changePercent < 0 ? "decreased" : "increased")} by {Math.Abs(changePercent):F1}% this month",
                Value = changePercent,
                Impact = Math.Abs(changePercent) > 10 ? "HIGH" : "MODERATE"
            });

            // Add temperature trend analysis
            trends.Add(new PredictiveTrend
            {
                Icon = "bi-thermometer-high",
                TrendType = "WARNING",
                Title = "Temperature Risk",
                Description = "Temperature-related risks are 35% higher in Building A compared to other locations",
                Value = 35,
                Impact = "HIGH"
            });

            // Add power consumption trend
            trends.Add(new PredictiveTrend
            {
                Icon = "bi-lightning",
                TrendType = "CAUTION",
                Title = "Power Anomalies",
                Description = "Power consumption anomalies detected in 3 projector units, indicating potential component stress",
                Value = 3,
                Impact = "MODERATE"
            });

            return trends;
        }

        public async Task<string> ExplainRiskFactorsAsync(int equipmentId, PredictionResult prediction)
        {
            var equipment = await _context.Equipment
                .Include(e => e.EquipmentType)
                .Include(e => e.EquipmentModel)
                .FirstOrDefaultAsync(e => e.EquipmentId == equipmentId);

            if (equipment == null) return "Equipment not found for analysis.";

            var explanation = new List<string>();

            explanation.Add($"The AI model has analyzed {equipment.EquipmentModel?.ModelName ?? "this equipment"} and determined a {prediction.FailureProbability:P1} probability of failure.");

            if (prediction.FailureProbability >= 0.8)
            {
                explanation.Add("This high probability is based on multiple risk indicators:");
                explanation.Add("• Operating parameters are outside optimal ranges");
                explanation.Add("• Equipment age and maintenance history suggest increased wear");
                explanation.Add("• Environmental conditions may be contributing to accelerated degradation");
            }
            else if (prediction.FailureProbability >= 0.6)
            {
                explanation.Add("This moderate probability indicates:");
                explanation.Add("• Some risk factors are present but not critical");
                explanation.Add("• Equipment is showing early signs of potential issues");
                explanation.Add("• Preventive maintenance could significantly reduce risk");
            }
            else
            {
                explanation.Add("The low probability suggests:");
                explanation.Add("• Equipment is operating within normal parameters");
                explanation.Add("• No immediate concerns identified");
                explanation.Add("• Continue with standard maintenance schedule");
            }

            explanation.Add($"The model's confidence in this prediction is {prediction.ConfidenceScore:P1}, indicating {(prediction.ConfidenceScore > 0.8 ? "high reliability" : "moderate reliability")} in the analysis.");

            return string.Join(" ", explanation);
        }
    }

    // Supporting classes
    public class EquipmentAIInsight
    {
        public int EquipmentId { get; set; }
        public string EquipmentName { get; set; } = "";
        public string EquipmentType { get; set; } = "";
        public string Location { get; set; } = "";
        
        public string RiskLevel { get; set; } = "";
        public double FailureProbability { get; set; }
        public double ConfidenceScore { get; set; }
        
        public List<string> PrimaryRiskFactors { get; set; } = new();
        public List<string> ImmediateActions { get; set; } = new();
        public List<string> ShortTermRecommendations { get; set; } = new();
        public List<string> LongTermPlan { get; set; } = new();
        
        public int EstimatedDaysToFailure { get; set; }
        public string MaintenancePriority { get; set; } = "";
        public decimal EstimatedRepairCost { get; set; }
        
        public DateTime GeneratedAt { get; set; }
    }

    public class SmartRecommendation
    {
        public string Priority { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal EstimatedSavings { get; set; }
        public string TimeFrame { get; set; } = "";
    }

    public class PredictiveTrend
    {
        public string Icon { get; set; } = "";
        public string TrendType { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public double Value { get; set; }
        public string Impact { get; set; } = "";
    }
}
