using Microsoft.EntityFrameworkCore;
using FEENALOoFINALE.Data;
using FEENALOoFINALE.Models;

namespace FEENALOoFINALE.Services
{
    public interface IAdvancedAnalyticsService
    {
        Task<AdvancedAnalyticsViewModel> GetAdvancedAnalyticsAsync(DashboardFilterViewModel? filters = null);
        Task<List<EquipmentPerformanceMetrics>> GetEquipmentPerformanceMetricsAsync(DashboardFilterViewModel? filters = null);
        Task<List<PredictiveAnalyticsData>> GetPredictiveAnalyticsAsync(DashboardFilterViewModel? filters = null);
        Task<List<KPIProgressIndicator>> GetKPIProgressIndicatorsAsync(DashboardFilterViewModel? filters = null);
        Task<EquipmentHeatmapData> GetEquipmentHeatmapDataAsync(DashboardFilterViewModel? filters = null);
        Task<List<MaintenanceTrendData>> GetMaintenanceTrendAnalysisAsync(DashboardFilterViewModel? filters = null, int days = 30);
        Task<List<CostAnalysisData>> GetCostAnalysisDataAsync(DashboardFilterViewModel? filters = null, int months = 6);
        Task<Dictionary<string, object>> GetRealtimeMetricsAsync();
    }

    public class AdvancedAnalyticsService : IAdvancedAnalyticsService
    {
        private readonly ApplicationDbContext _context;

        public AdvancedAnalyticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdvancedAnalyticsViewModel> GetAdvancedAnalyticsAsync(DashboardFilterViewModel? filters = null)
        {
            var analytics = new AdvancedAnalyticsViewModel
            {
                EquipmentPerformance = await GetEquipmentPerformanceMetricsAsync(filters),
                PredictiveInsights = await GetPredictiveAnalyticsAsync(filters),
                KPIProgress = await GetKPIProgressIndicatorsAsync(filters),
                HeatmapData = await GetEquipmentHeatmapDataAsync(filters),
                MaintenanceTrends = await GetMaintenanceTrendAnalysisAsync(filters),
                CostAnalysis = await GetCostAnalysisDataAsync(filters),
                RealTimeMetrics = await GetRealtimeMetricsAsync(),
                LastUpdated = DateTime.Now
            };

            return analytics;
        }

        public async Task<List<EquipmentPerformanceMetrics>> GetEquipmentPerformanceMetricsAsync(DashboardFilterViewModel? filters = null)
        {
            var equipmentQuery = _context.Equipment
                .Include(e => e.EquipmentModel)
                    .ThenInclude(em => em!.EquipmentType)
                .Include(e => e.Building)
                .Include(e => e.Room)
                .AsQueryable();

            // Apply filters if provided
            if (filters != null)
            {
                if (filters.BuildingIds?.Any() == true)
                    equipmentQuery = equipmentQuery.Where(e => filters.BuildingIds.Contains(e.BuildingId));
                
                if (filters.EquipmentTypeIds?.Any() == true)
                    equipmentQuery = equipmentQuery.Where(e => e.EquipmentModel != null && 
                        filters.EquipmentTypeIds.Contains(e.EquipmentModel.EquipmentTypeId));
            }

            var equipment = await equipmentQuery.ToListAsync();
            var performanceMetrics = new List<EquipmentPerformanceMetrics>();

            foreach (var eq in equipment)
            {
                var maintenanceLogs = await _context.MaintenanceLogs
                    .Where(ml => ml.EquipmentId == eq.EquipmentId)
                    .OrderBy(ml => ml.LogDate)
                    .ToListAsync();

                var failurePrediction = await _context.FailurePredictions
                    .Where(fp => fp.EquipmentId == eq.EquipmentId)
                    .OrderByDescending(fp => fp.CreatedDate)
                    .FirstOrDefaultAsync();

                var metrics = await CalculateEquipmentMetrics(eq, maintenanceLogs, failurePrediction);
                performanceMetrics.Add(metrics);
            }

            return performanceMetrics.OrderByDescending(m => m.HealthScore).ToList();
        }

        public async Task<List<PredictiveAnalyticsData>> GetPredictiveAnalyticsAsync(DashboardFilterViewModel? filters = null)
        {
            var predictions = await _context.FailurePredictions
                .Include(fp => fp.Equipment)
                    .ThenInclude(e => e!.EquipmentModel)
                .Where(fp => fp.PredictedFailureDate >= DateTime.Now)
                .OrderBy(fp => fp.PredictedFailureDate)
                .ToListAsync();

            var analyticsData = new List<PredictiveAnalyticsData>();

            foreach (var prediction in predictions)
            {
                var data = new PredictiveAnalyticsData
                {
                    EquipmentId = prediction.EquipmentId,
                    EquipmentName = prediction.Equipment?.EquipmentModel?.ModelName ?? "Unknown",
                    RiskLevel = prediction.Status,
                    PredictedFailureDate = prediction.PredictedFailureDate,
                    ConfidenceScore = prediction.ConfidenceLevel / 100.0, // Convert to 0-1 scale
                    PredictionReason = prediction.AnalysisNotes ?? "System analysis",
                    DaysUntilPredictedFailure = (int)(prediction.PredictedFailureDate - DateTime.Now).TotalDays,
                    FinancialImpact = 5000.0, // Default estimated cost
                    RecommendedActions = GenerateRecommendedActions(prediction),
                    FactorContributions = GetFactorContributions(prediction.EquipmentId)
                };

                analyticsData.Add(data);
            }

            return analyticsData;
        }

        public async Task<List<KPIProgressIndicator>> GetKPIProgressIndicatorsAsync(DashboardFilterViewModel? filters = null)
        {
            var kpis = new List<KPIProgressIndicator>();

            // Overall Equipment Effectiveness (OEE)
            var totalEquipment = await _context.Equipment.CountAsync();
            var activeEquipment = await _context.Equipment.CountAsync(e => e.Status == EquipmentStatus.Active);
            var currentOEE = totalEquipment > 0 ? (double)activeEquipment / totalEquipment * 100 : 0;

            kpis.Add(new KPIProgressIndicator
            {
                KPIName = "Overall Equipment Effectiveness",
                Category = "Equipment",
                CurrentValue = Math.Round(currentOEE, 1),
                TargetValue = 85.0,
                PreviousValue = 82.3, // This would come from historical data
                Unit = "%",
                Direction = currentOEE > 82.3 ? "up" : "down",
                PercentageChange = Math.Round(((currentOEE - 82.3) / 82.3) * 100, 1),
                ProgressPercentage = Math.Round((currentOEE / 85.0) * 100, 1),
                Status = currentOEE >= 85.0 ? "on-track" : currentOEE >= 75.0 ? "warning" : "critical",
                Icon = "bi-speedometer2",
                Color = currentOEE >= 85.0 ? "#10b981" : currentOEE >= 75.0 ? "#f59e0b" : "#ef4444",
                LastUpdated = DateTime.Now
            });

            // Mean Time Between Failures (MTBF)
            var mtbf = await CalculateMTBF();
            kpis.Add(new KPIProgressIndicator
            {
                KPIName = "Mean Time Between Failures",
                Category = "Maintenance",
                CurrentValue = Math.Round(mtbf, 1),
                TargetValue = 30.0,
                PreviousValue = 25.2,
                Unit = "days",
                Direction = mtbf > 25.2 ? "up" : "down",
                PercentageChange = Math.Round(((mtbf - 25.2) / 25.2) * 100, 1),
                ProgressPercentage = Math.Round((mtbf / 30.0) * 100, 1),
                Status = mtbf >= 30.0 ? "on-track" : mtbf >= 20.0 ? "warning" : "critical",
                Icon = "bi-clock-history",
                Color = mtbf >= 30.0 ? "#10b981" : mtbf >= 20.0 ? "#f59e0b" : "#ef4444",
                LastUpdated = DateTime.Now
            });

            // Maintenance Cost Efficiency
            var totalCost = await _context.MaintenanceLogs.SumAsync(ml => ml.Cost);
            var costPerEquipment = totalEquipment > 0 ? (double)(totalCost / totalEquipment) : 0;
            kpis.Add(new KPIProgressIndicator
            {
                KPIName = "Maintenance Cost per Equipment",
                Category = "Financial",
                CurrentValue = Math.Round(costPerEquipment, 2),
                TargetValue = 500.0,
                PreviousValue = 520.5,
                Unit = "$",
                Direction = costPerEquipment < 520.5 ? "down" : "up",
                PercentageChange = Math.Round(((costPerEquipment - 520.5) / 520.5) * 100, 1),
                ProgressPercentage = Math.Round((1 - (costPerEquipment / 1000.0)) * 100, 1), // Inverse for cost
                Status = costPerEquipment <= 500.0 ? "on-track" : costPerEquipment <= 750.0 ? "warning" : "critical",
                Icon = "bi-currency-dollar",
                Color = costPerEquipment <= 500.0 ? "#10b981" : costPerEquipment <= 750.0 ? "#f59e0b" : "#ef4444",
                LastUpdated = DateTime.Now
            });

            // Preventive Maintenance Ratio
            var totalMaintenance = await _context.MaintenanceLogs.CountAsync();
            var preventiveMaintenance = await _context.MaintenanceLogs
                .CountAsync(ml => ml.MaintenanceType == MaintenanceType.Preventive);
            var preventiveRatio = totalMaintenance > 0 ? (double)preventiveMaintenance / totalMaintenance * 100 : 0;

            kpis.Add(new KPIProgressIndicator
            {
                KPIName = "Preventive Maintenance Ratio",
                Category = "Operational",
                CurrentValue = Math.Round(preventiveRatio, 1),
                TargetValue = 80.0,
                PreviousValue = 65.8,
                Unit = "%",
                Direction = preventiveRatio > 65.8 ? "up" : "down",
                PercentageChange = Math.Round(((preventiveRatio - 65.8) / 65.8) * 100, 1),
                ProgressPercentage = Math.Round((preventiveRatio / 80.0) * 100, 1),
                Status = preventiveRatio >= 80.0 ? "on-track" : preventiveRatio >= 60.0 ? "warning" : "critical",
                Icon = "bi-tools",
                Color = preventiveRatio >= 80.0 ? "#10b981" : preventiveRatio >= 60.0 ? "#f59e0b" : "#ef4444",
                LastUpdated = DateTime.Now
            });

            return kpis;
        }

        public async Task<EquipmentHeatmapData> GetEquipmentHeatmapDataAsync(DashboardFilterViewModel? filters = null)
        {
            var buildings = await _context.Buildings
                .Include(b => b.Rooms)
                    .ThenInclude(r => r.Equipments)
                        .ThenInclude(e => e.EquipmentModel)
                .ToListAsync();

            var heatmapData = new EquipmentHeatmapData();
            var totalEquipment = 0;
            var totalHealthScore = 0.0;

            foreach (var building in buildings)
            {
                var buildingData = new BuildingHeatmapData
                {
                    BuildingId = building.BuildingId,
                    BuildingName = building.BuildingName,
                    StatusCounts = new Dictionary<string, int>()
                };

                foreach (var room in building.Rooms ?? new List<Room>())
                {
                    var roomData = new RoomHeatmapData
                    {
                        RoomId = room.RoomId,
                        RoomName = room.RoomName
                    };

                    foreach (var equipment in room.Equipments ?? new List<Equipment>())
                    {
                        var healthScore = await CalculateEquipmentHealthScore(equipment.EquipmentId);
                        var riskLevel = await GetEquipmentRiskLevel(equipment.EquipmentId);

                        var equipmentItem = new EquipmentHeatmapItem
                        {
                            EquipmentId = equipment.EquipmentId,
                            Name = equipment.EquipmentModel?.ModelName ?? "Unknown",
                            Status = equipment.Status,
                            HealthScore = healthScore,
                            RiskLevel = riskLevel,
                            StatusColor = GetStatusColor(equipment.Status),
                            ToolTip = $"{equipment.EquipmentModel?.ModelName} - Health: {healthScore:F1}%"
                        };

                        roomData.Equipment.Add(equipmentItem);
                        totalEquipment++;
                        totalHealthScore += healthScore;

                        // Update building status counts
                        var statusKey = equipment.Status.ToString();
                        buildingData.StatusCounts[statusKey] = buildingData.StatusCounts.GetValueOrDefault(statusKey, 0) + 1;
                    }

                    roomData.RoomHealthScore = roomData.Equipment.Any() ? 
                        roomData.Equipment.Average(e => e.HealthScore) : 100.0;
                    roomData.DominantStatus = GetDominantStatus(roomData.Equipment);

                    buildingData.Rooms.Add(roomData);
                }

                buildingData.TotalEquipment = buildingData.Rooms.Sum(r => r.Equipment.Count);
                buildingData.BuildingHealthScore = buildingData.Rooms.Any() ? 
                    buildingData.Rooms.Average(r => r.RoomHealthScore) : 100.0;

                heatmapData.Buildings.Add(buildingData);
            }

            heatmapData.TotalEquipment = totalEquipment;
            heatmapData.OverallHealthScore = totalEquipment > 0 ? totalHealthScore / totalEquipment : 100.0;
            heatmapData.StatusCounts = await GetOverallStatusCounts();
            heatmapData.StatusColors = GetStatusColorMapping();

            return heatmapData;
        }

        public async Task<List<MaintenanceTrendData>> GetMaintenanceTrendAnalysisAsync(DashboardFilterViewModel? filters = null, int days = 30)
        {
            var startDate = DateTime.Now.AddDays(-days).Date;
            var trends = new List<MaintenanceTrendData>();

            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                var dayStart = date.Date;
                var dayEnd = dayStart.AddDays(1);

                var dayLogs = await _context.MaintenanceLogs
                    .Where(ml => ml.LogDate >= dayStart && ml.LogDate < dayEnd)
                    .ToListAsync();

                var trendData = new MaintenanceTrendData
                {
                    Date = date,
                    PreventiveMaintenanceCount = dayLogs.Count(ml => ml.MaintenanceType == MaintenanceType.Preventive),
                    CorrectiveMaintenanceCount = dayLogs.Count(ml => ml.MaintenanceType == MaintenanceType.Corrective),
                    InspectionMaintenanceCount = dayLogs.Count(ml => ml.MaintenanceType == MaintenanceType.Inspection),
                    TotalCost = dayLogs.Sum(ml => ml.Cost),
                    AverageDowntime = dayLogs.Where(ml => ml.DowntimeDuration.HasValue)
                        .Select(ml => ml.DowntimeDuration!.Value.TotalHours)
                        .DefaultIfEmpty(0)
                        .Average(),
                    EquipmentAffected = dayLogs.Select(ml => ml.EquipmentId).Distinct().Count(),
                    EfficiencyScore = CalculateDailyEfficiencyScore(dayLogs)
                };

                trends.Add(trendData);
            }

            return trends;
        }

        public async Task<List<CostAnalysisData>> GetCostAnalysisDataAsync(DashboardFilterViewModel? filters = null, int months = 6)
        {
            var costAnalysis = new List<CostAnalysisData>();

            // Cost by Equipment Type
            var equipmentTypeCosts = await _context.MaintenanceLogs
                .Include(ml => ml.Equipment)
                    .ThenInclude(e => e!.EquipmentModel)
                        .ThenInclude(em => em!.EquipmentType)
                .GroupBy(ml => ml.Equipment!.EquipmentModel!.EquipmentType!.EquipmentTypeName)
                .Select(g => new CostAnalysisData
                {
                    Category = "Equipment Type",
                    CategoryValue = g.Key,
                    TotalCost = g.Sum(ml => ml.Cost),
                    AverageCost = g.Average(ml => ml.Cost),
                    MaintenanceCount = g.Count(),
                    Trend = "stable" // This would be calculated from historical data
                })
                .ToListAsync();

            var totalCost = equipmentTypeCosts.Sum(c => c.TotalCost);
            foreach (var cost in equipmentTypeCosts)
            {
                cost.CostPercentage = totalCost > 0 ? (double)(cost.TotalCost / totalCost) * 100 : 0;
                cost.ProjectedCost = cost.TotalCost * 1.1m; // 10% increase projection
            }

            costAnalysis.AddRange(equipmentTypeCosts);

            return costAnalysis;
        }

        public async Task<Dictionary<string, object>> GetRealtimeMetricsAsync()
        {
            var metrics = new Dictionary<string, object>
            {
                ["TotalEquipment"] = await _context.Equipment.CountAsync(),
                ["ActiveEquipment"] = await _context.Equipment.CountAsync(e => e.Status == EquipmentStatus.Active),
                ["CriticalAlerts"] = await _context.Alerts.CountAsync(a => a.Priority == AlertPriority.High && a.Status == AlertStatus.Open),
                ["PendingMaintenance"] = await _context.MaintenanceTasks.CountAsync(m => m.Status != MaintenanceStatus.Completed),
                ["LowStockItems"] = await _context.InventoryItems.CountAsync(i => i.InventoryStocks != null && i.InventoryStocks.Sum(s => s.Quantity) <= i.MinStockLevel),
                ["LastUpdated"] = DateTime.Now
            };

            return metrics;
        }

        #region Private Helper Methods

        private async Task<EquipmentPerformanceMetrics> CalculateEquipmentMetrics(
            Equipment equipment, 
            List<MaintenanceLog> maintenanceLogs, 
            FailurePrediction? prediction)
        {
            var failureCount = maintenanceLogs.Count(ml => ml.MaintenanceType == MaintenanceType.Corrective);
            var totalCost = maintenanceLogs.Sum(ml => ml.Cost);
            var avgDailyCost = maintenanceLogs.Any() ? totalCost / Math.Max(1, (DateTime.Now - maintenanceLogs.Min(ml => ml.LogDate)).Days) : 0;
            
            var mtbf = CalculateMTBFForEquipment(maintenanceLogs);
            var mttr = maintenanceLogs.Where(ml => ml.DowntimeDuration.HasValue)
                .Select(ml => ml.DowntimeDuration!.Value.TotalHours)
                .DefaultIfEmpty(0)
                .Average();

            var healthScore = await CalculateEquipmentHealthScore(equipment.EquipmentId);
            var uptimePercentage = CalculateUptimePercentage(equipment, maintenanceLogs);

            return new EquipmentPerformanceMetrics
            {
                EquipmentId = equipment.EquipmentId,
                EquipmentName = equipment.EquipmentModel?.ModelName ?? "Unknown",
                EquipmentType = equipment.EquipmentModel?.EquipmentType?.EquipmentTypeName ?? "Unknown",
                Building = equipment.Building?.BuildingName ?? "Unknown",
                Room = equipment.Room?.RoomName ?? "Unknown",
                Status = equipment.Status,
                UptimePercentage = uptimePercentage,
                EfficiencyScore = CalculateEfficiencyScore(equipment, maintenanceLogs),
                MaintenanceCostPerMonth = (double)(totalCost / Math.Max(1, maintenanceLogs.Count)),
                FailureCount = failureCount,
                MeanTimeBetweenFailures = mtbf,
                MeanTimeToRepair = mttr,
                PredictedRisk = prediction?.Status ?? PredictionStatus.Low,
                NextMaintenanceDue = await GetNextMaintenanceDue(equipment.EquipmentId),
                HealthScore = healthScore,
                TotalMaintenanceCost = totalCost,
                AverageDailyCost = avgDailyCost,
                EstimatedReplacementCost = 50000, // This would come from equipment specifications
                TotalMaintenanceHours = (int)maintenanceLogs.Where(ml => ml.DowntimeDuration.HasValue).Sum(ml => ml.DowntimeDuration!.Value.TotalHours),
                LastMaintenanceDate = maintenanceLogs.Any() ? maintenanceLogs.Max(ml => ml.LogDate) : DateTime.MinValue,
                DaysSinceLastMaintenance = maintenanceLogs.Any() ? 
                    (int)(DateTime.Now - maintenanceLogs.Max(ml => ml.LogDate)).TotalDays : 0
            };
        }

        private double CalculateMTBFForEquipment(List<MaintenanceLog> maintenanceLogs)
        {
            var failures = maintenanceLogs
                .Where(ml => ml.MaintenanceType == MaintenanceType.Corrective)
                .OrderBy(ml => ml.LogDate)
                .ToList();

            if (failures.Count < 2) return 365.0; // Default if insufficient data

            var intervals = new List<double>();
            for (int i = 1; i < failures.Count; i++)
            {
                var interval = (failures[i].LogDate - failures[i - 1].LogDate).TotalDays;
                intervals.Add(interval);
            }

            return intervals.Average();
        }

        private async Task<double> CalculateMTBF()
        {
            var failures = await _context.MaintenanceLogs
                .Where(ml => ml.MaintenanceType == MaintenanceType.Corrective)
                .OrderBy(ml => ml.LogDate)
                .ToListAsync();

            if (failures.Count < 2) return 30.0;

            var intervals = new List<double>();
            for (int i = 1; i < failures.Count; i++)
            {
                var interval = (failures[i].LogDate - failures[i - 1].LogDate).TotalDays;
                intervals.Add(interval);
            }

            return intervals.Average();
        }

        private async Task<double> CalculateEquipmentHealthScore(int equipmentId)
        {
            var equipment = await _context.Equipment.FindAsync(equipmentId);
            if (equipment == null) return 0;

            // Simple health score calculation based on status and recent maintenance
            var baseScore = equipment.Status switch
            {
                EquipmentStatus.Active => 90.0,
                EquipmentStatus.Inactive => 30.0,
                EquipmentStatus.Retired => 10.0,
                _ => 50.0
            };

            // Adjust based on recent maintenance
            var recentMaintenance = await _context.MaintenanceLogs
                .Where(ml => ml.EquipmentId == equipmentId && ml.LogDate >= DateTime.Now.AddDays(-30))
                .CountAsync();

            if (recentMaintenance > 3) baseScore -= 20; // Frequent maintenance reduces health
            else if (recentMaintenance == 0) baseScore -= 10; // No maintenance might indicate neglect

            return Math.Max(0, Math.Min(100, baseScore));
        }

        private async Task<PredictionStatus> GetEquipmentRiskLevel(int equipmentId)
        {
            var prediction = await _context.FailurePredictions
                .Where(fp => fp.EquipmentId == equipmentId)
                .OrderByDescending(fp => fp.CreatedDate)
                .FirstOrDefaultAsync();

            return prediction?.Status ?? PredictionStatus.Low;
        }

        private string GetStatusColor(EquipmentStatus status)
        {
            return status switch
            {
                EquipmentStatus.Active => "#10b981",
                EquipmentStatus.Inactive => "#ef4444",
                EquipmentStatus.Retired => "#6b7280",
                _ => "#3b82f6"
            };
        }

        private Dictionary<string, string> GetStatusColorMapping()
        {
            return new Dictionary<string, string>
            {
                ["Active"] = "#10b981",
                ["Inactive"] = "#ef4444",
                ["Retired"] = "#6b7280"
            };
        }

        private async Task<Dictionary<string, int>> GetOverallStatusCounts()
        {
            return await _context.Equipment
                .GroupBy(e => e.Status)
                .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count());
        }

        private string GetDominantStatus(List<EquipmentHeatmapItem> equipment)
        {
            if (!equipment.Any()) return "Unknown";

            return equipment
                .GroupBy(e => e.Status)
                .OrderByDescending(g => g.Count())
                .First()
                .Key
                .ToString();
        }

        private double CalculateUptimePercentage(Equipment equipment, List<MaintenanceLog> maintenanceLogs)
        {
            if (equipment.Status == EquipmentStatus.Retired) return 0;

            var totalDowntime = maintenanceLogs
                .Where(ml => ml.DowntimeDuration.HasValue)
                .Sum(ml => ml.DowntimeDuration!.Value.TotalHours);

            var totalTime = (DateTime.Now - DateTime.Now.AddDays(-30)).TotalHours; // Last 30 days
            
            return Math.Max(0, Math.Min(100, ((totalTime - totalDowntime) / totalTime) * 100));
        }

        private double CalculateEfficiencyScore(Equipment equipment, List<MaintenanceLog> maintenanceLogs)
        {
            var preventiveMaintenance = maintenanceLogs.Count(ml => ml.MaintenanceType == MaintenanceType.Preventive);
            var correctiveMaintenance = maintenanceLogs.Count(ml => ml.MaintenanceType == MaintenanceType.Corrective);
            var total = preventiveMaintenance + correctiveMaintenance;

            if (total == 0) return 100; // No maintenance data, assume good

            var preventiveRatio = (double)preventiveMaintenance / total;
            return Math.Min(100, preventiveRatio * 100 + 20); // Bonus for preventive maintenance
        }

        private async Task<DateTime?> GetNextMaintenanceDue(int equipmentId)
        {
            var lastMaintenance = await _context.MaintenanceLogs
                .Where(ml => ml.EquipmentId == equipmentId)
                .OrderByDescending(ml => ml.LogDate)
                .FirstOrDefaultAsync();

            if (lastMaintenance == null) return DateTime.Now.AddDays(30); // Default schedule

            // Simple calculation - next maintenance in 90 days after last
            return lastMaintenance.LogDate.AddDays(90);
        }

        private List<string> GenerateRecommendedActions(FailurePrediction prediction)
        {
            var actions = new List<string>();

            switch (prediction.Status)
            {
                case PredictionStatus.High:
                    actions.Add("Schedule immediate inspection");
                    actions.Add("Prepare replacement parts");
                    actions.Add("Plan maintenance downtime");
                    break;
                case PredictionStatus.Medium:
                    actions.Add("Schedule preventive maintenance");
                    actions.Add("Monitor performance closely");
                    break;
                case PredictionStatus.Low:
                    actions.Add("Continue routine monitoring");
                    break;
            }

            return actions;
        }

        private Dictionary<string, double> GetFactorContributions(int equipmentId)
        {
            // This would analyze various factors contributing to failure prediction
            return new Dictionary<string, double>
            {
                ["Age"] = 0.3,
                ["Usage"] = 0.25,
                ["Maintenance History"] = 0.2,
                ["Environmental Factors"] = 0.15,
                ["Performance Degradation"] = 0.1
            };
        }

        private double CalculateDailyEfficiencyScore(List<MaintenanceLog> dayLogs)
        {
            if (!dayLogs.Any()) return 100;

            var preventive = dayLogs.Count(ml => ml.MaintenanceType == MaintenanceType.Preventive);
            var corrective = dayLogs.Count(ml => ml.MaintenanceType == MaintenanceType.Corrective);
            var total = dayLogs.Count;

            var preventiveRatio = (double)preventive / total;
            var correctivePenalty = (double)corrective / total * 30; // Corrective maintenance reduces efficiency

            return Math.Max(0, Math.Min(100, (preventiveRatio * 100) - correctivePenalty));
        }

        private async Task<Dictionary<string, object>> GetRealTimeMetricsAsync()
        {
            return new Dictionary<string, object>
            {
                ["equipmentOnline"] = await _context.Equipment.CountAsync(e => e.Status == EquipmentStatus.Active),
                ["criticalAlerts"] = await _context.Alerts.CountAsync(a => a.Priority == AlertPriority.High && a.Status == AlertStatus.Open),
                ["maintenanceInProgress"] = await _context.MaintenanceTasks.CountAsync(m => m.Status == MaintenanceStatus.InProgress),
                ["systemHealth"] = 92.5,
                ["lastUpdate"] = DateTime.Now
            };
        }

        private async Task<List<object>> GetRecentAlertUpdates()
        {
            var recentAlerts = await _context.Alerts
                .Include(a => a.Equipment)
                .Where(a => a.CreatedDate >= DateTime.Now.AddMinutes(-5))
                .OrderByDescending(a => a.CreatedDate)
                .Take(5)
                .Select(a => new
                {
                    id = a.AlertId,
                    title = a.Title,
                    priority = a.Priority.ToString(),
                    equipmentId = a.EquipmentId,
                    timestamp = a.CreatedDate
                })
                .ToListAsync();

            return recentAlerts.Cast<object>().ToList();
        }

        private List<object> GetRecentEquipmentUpdates()
        {
            // This would track recent equipment status changes
            // For now, return empty list
            return new List<object>();
        }

        private async Task<Dictionary<string, object>> GetChartUpdates()
        {
            return new Dictionary<string, object>
            {
                ["equipmentStatus"] = await _context.Equipment
                    .GroupBy(e => e.Status)
                    .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count()),
                ["alertPriority"] = await _context.Alerts
                    .Where(a => a.Status == AlertStatus.Open)
                    .GroupBy(a => a.Priority)
                    .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count())
            };
        }

        #endregion
    }
}
