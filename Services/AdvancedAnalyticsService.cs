using FEENALOoFINALE.Data;
using FEENALOoFINALE.Models;
using Microsoft.EntityFrameworkCore;

namespace FEENALOoFINALE.Services
{
    public interface IAdvancedAnalyticsService
    {
        Task<EnhancedDashboardViewModel> GetEnhancedDashboardDataAsync();
        Task<List<EquipmentPerformanceMetric>> GetEquipmentPerformanceMetricsAsync();
        Task<List<PredictiveMaintenanceInsight>> GetPredictiveMaintenanceInsightsAsync();
        Task<List<KPIIndicator>> GetKPIIndicatorsAsync();
        Task<List<MaintenanceTrendData>> GetMaintenanceTrendDataAsync(int days = 30);
        Task<List<CostAnalysisData>> GetCostAnalysisDataAsync();
        Task<double> CalculateSystemHealthScoreAsync();
        Task<double> CalculateMaintenanceEfficiencyAsync();
        Task<double> CalculateCostEfficiencyAsync();
        Task<double> CalculateEquipmentUtilizationAsync();
    }

    public class AdvancedAnalyticsService : IAdvancedAnalyticsService
    {
        private readonly ApplicationDbContext _context;

        public AdvancedAnalyticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EnhancedDashboardViewModel> GetEnhancedDashboardDataAsync()
        {
            var viewModel = new EnhancedDashboardViewModel();

            // Basic metrics
            viewModel.TotalEquipment = await _context.Equipment.CountAsync();
            viewModel.ActiveEquipment = await _context.Equipment.CountAsync(e => e.Status == EquipmentStatus.Active);
            viewModel.CriticalAlerts = await _context.Alerts.CountAsync(a => a.Priority == AlertPriority.High);
            viewModel.ActiveMaintenanceTasks = await _context.MaintenanceTasks.CountAsync(mt => mt.Status == MaintenanceStatus.InProgress);
            viewModel.CompletedMaintenanceTasks = await _context.MaintenanceTasks.CountAsync(mt => mt.Status == MaintenanceStatus.Completed);
            viewModel.PendingMaintenanceTasks = await _context.MaintenanceTasks.CountAsync(mt => mt.Status == MaintenanceStatus.Pending);
            viewModel.OverdueMaintenances = await _context.MaintenanceTasks.CountAsync(mt => mt.ScheduledDate < DateTime.Now && mt.Status != MaintenanceStatus.Completed);
            viewModel.EquipmentNeedingAttention = await _context.Equipment.CountAsync(e => e.Status == EquipmentStatus.Inactive);
            viewModel.TotalInventoryItems = await _context.InventoryItems.CountAsync();
            viewModel.LowStockItems = await _context.InventoryItems
                .Where(i => i.InventoryStocks != null && i.InventoryStocks.Sum(s => s.Quantity) <= i.MinStockLevel)
                .CountAsync();

            // Advanced data
            viewModel.RecentAlerts = await _context.Alerts
                .Include(a => a.Equipment)
                .OrderByDescending(a => a.CreatedDate)
                .Take(10)
                .ToListAsync();

            viewModel.UpcomingMaintenanceTasks = await _context.MaintenanceTasks
                .Include(mt => mt.Equipment)
                .Include(mt => mt.AssignedTo)
                .Where(mt => mt.Status == MaintenanceStatus.Pending)
                .OrderBy(mt => mt.ScheduledDate)
                .Take(10)
                .ToListAsync();

            viewModel.CriticalEquipment = await _context.Equipment
                .Include(e => e.EquipmentType)
                .Include(e => e.Building)
                .Where(e => e.Status == EquipmentStatus.Inactive) // Using Inactive as "critical" equivalent
                .Take(10)
                .ToListAsync();

            // Equipment status distribution
            var statusCounts = await _context.Equipment
                .GroupBy(e => e.Status)
                .Select(g => new EnhancedEquipmentStatusCount
                {
                    Status = g.Key,
                    Count = g.Count(),
                    DisplayName = g.Key.ToString(),
                    ColorClass = GetStatusColorClass(g.Key)
                })
                .ToListAsync();

            var totalEquipment = statusCounts.Sum(sc => sc.Count);
            foreach (var statusCount in statusCounts)
            {
                statusCount.Percentage = totalEquipment > 0 ? (double)statusCount.Count / totalEquipment * 100 : 0;
            }
            viewModel.EquipmentStatusCounts = statusCounts;

            // Performance metrics
            viewModel.OverallSystemHealth = await CalculateSystemHealthScoreAsync();
            viewModel.MaintenanceEfficiency = await CalculateMaintenanceEfficiencyAsync();
            viewModel.CostEfficiency = await CalculateCostEfficiencyAsync();
            viewModel.EquipmentUtilization = await CalculateEquipmentUtilizationAsync();

            // Analytics data
            viewModel.PerformanceMetrics = await GetEquipmentPerformanceMetricsAsync();
            viewModel.PredictiveInsights = await GetPredictiveMaintenanceInsightsAsync();
            viewModel.KPIIndicators = await GetKPIIndicatorsAsync();
            viewModel.MaintenanceTrends = await GetMaintenanceTrendDataAsync();
            viewModel.CostAnalysis = await GetCostAnalysisDataAsync();

            // Filter options
            viewModel.Buildings = await _context.Buildings.ToListAsync();
            viewModel.EquipmentTypes = await _context.EquipmentTypes.ToListAsync();

            return viewModel;
        }

        public async Task<List<EquipmentPerformanceMetric>> GetEquipmentPerformanceMetricsAsync()
        {
            var equipment = await _context.Equipment
                .Include(e => e.EquipmentType)
                .Include(e => e.EquipmentModel)
                .Include(e => e.MaintenanceLogs)
                .ToListAsync();

            return equipment.Select(e => new EquipmentPerformanceMetric
            {
                EquipmentName = e.EquipmentModel?.ModelName ?? "Unknown",
                EquipmentType = e.EquipmentType?.EquipmentTypeName ?? "Unknown",
                PerformanceScore = CalculatePerformanceScore(e),
                UtilizationRate = CalculateUtilizationRate(e),
                MaintenanceFrequency = e.MaintenanceLogs?.Count ?? 0,
                TotalMaintenanceCost = e.MaintenanceLogs?.Sum(ml => ml.Cost) ?? 0,
                AverageDowntime = CalculateAverageDowntime(e),
                LastMaintenanceDate = e.MaintenanceLogs?.OrderByDescending(ml => ml.MaintenanceDate).FirstOrDefault()?.MaintenanceDate ?? DateTime.MinValue,
                NextScheduledMaintenance = CalculateNextScheduledMaintenance(e),
                HealthStatus = CalculateHealthStatus(e)
            }).ToList();
        }

        public async Task<List<PredictiveMaintenanceInsight>> GetPredictiveMaintenanceInsightsAsync()
        {
            var equipment = await _context.Equipment
                .Include(e => e.EquipmentModel)
                .Include(e => e.MaintenanceLogs)
                .Where(e => e.Status == EquipmentStatus.Active)
                .ToListAsync();

            return equipment.Select(e => new PredictiveMaintenanceInsight
            {
                EquipmentName = e.EquipmentModel?.ModelName ?? "Unknown",
                PredictedIssue = GetPredictedIssue(e),
                ProbabilityPercentage = CalculateFailureProbability(e),
                PredictedFailureDate = CalculatePredictedFailureDate(e),
                RecommendedAction = GetRecommendedAction(e),
                RiskLevel = CalculateRiskLevel(e),
                EstimatedRepairCost = EstimateRepairCost(e),
                DaysUntilPredictedFailure = (int)(CalculatePredictedFailureDate(e) - DateTime.Now).TotalDays
            }).Where(p => p.ProbabilityPercentage > 30).ToList();
        }

        public async Task<List<KPIIndicator>> GetKPIIndicatorsAsync()
        {
            var kpis = new List<KPIIndicator>();

            var equipmentUptime = await CalculateEquipmentUptimeAsync();
            kpis.Add(new KPIIndicator
            {
                Name = "Equipment Uptime",
                CurrentValue = equipmentUptime,
                TargetValue = 95.0,
                PreviousValue = 92.5,
                Unit = "%",
                Status = equipmentUptime >= 95 ? "Good" : equipmentUptime >= 85 ? "Warning" : "Critical",
                TrendDirection = equipmentUptime > 92.5 ? "Up" : "Down",
                PercentageChange = ((equipmentUptime - 92.5) / 92.5) * 100,
                Description = "Percentage of time equipment is operational",
                Icon = "fas fa-chart-line"
            });

            var mttr = await CalculateMTTRAsync();
            kpis.Add(new KPIIndicator
            {
                Name = "Mean Time To Repair (MTTR)",
                CurrentValue = mttr,
                TargetValue = 4.0,
                PreviousValue = 5.2,
                Unit = "hours",
                Status = mttr <= 4 ? "Good" : mttr <= 6 ? "Warning" : "Critical",
                TrendDirection = mttr < 5.2 ? "Down" : "Up",
                PercentageChange = ((mttr - 5.2) / 5.2) * 100,
                Description = "Average time to repair equipment failures",
                Icon = "fas fa-wrench"
            });

            var mtbf = await CalculateMTBFAsync();
            kpis.Add(new KPIIndicator
            {
                Name = "Mean Time Between Failures (MTBF)",
                CurrentValue = mtbf,
                TargetValue = 720.0,
                PreviousValue = 680.0,
                Unit = "hours",
                Status = mtbf >= 720 ? "Good" : mtbf >= 600 ? "Warning" : "Critical",
                TrendDirection = mtbf > 680 ? "Up" : "Down",
                PercentageChange = ((mtbf - 680) / 680) * 100,
                Description = "Average time between equipment failures",
                Icon = "fas fa-clock"
            });

            return kpis;
        }

        public async Task<List<MaintenanceTrendData>> GetMaintenanceTrendDataAsync(int days = 30)
        {
            var startDate = DateTime.Now.AddDays(-days);
            var trends = new List<MaintenanceTrendData>();

            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                var dayStart = date.Date;
                var dayEnd = dayStart.AddDays(1);

                var completed = await _context.MaintenanceLogs
                    .Where(ml => ml.MaintenanceDate >= dayStart && ml.MaintenanceDate < dayEnd)
                    .CountAsync();

                var scheduled = await _context.MaintenanceTasks
                    .Where(mt => mt.ScheduledDate >= dayStart && mt.ScheduledDate < dayEnd)
                    .CountAsync();

                var overdue = await _context.MaintenanceTasks
                    .Where(mt => mt.ScheduledDate < dayStart && mt.Status != MaintenanceStatus.Completed)
                    .CountAsync();

                var totalCost = await _context.MaintenanceLogs
                    .Where(ml => ml.MaintenanceDate >= dayStart && ml.MaintenanceDate < dayEnd)
                    .SumAsync(ml => ml.Cost);

                var preventiveCost = await _context.MaintenanceLogs
                    .Where(ml => ml.MaintenanceDate >= dayStart && ml.MaintenanceDate < dayEnd && ml.MaintenanceType == MaintenanceType.Preventive)
                    .SumAsync(ml => ml.Cost);

                trends.Add(new MaintenanceTrendData
                {
                    Date = date,
                    CompletedCount = completed,
                    ScheduledCount = scheduled,
                    OverdueCount = overdue,
                    TotalCost = totalCost,
                    PreventiveCost = preventiveCost,
                    CorrectiveCost = totalCost - preventiveCost
                });
            }

            return trends;
        }

        public async Task<List<CostAnalysisData>> GetCostAnalysisDataAsync()
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var previousMonth = currentMonth == 1 ? 12 : currentMonth - 1;
            var previousYear = currentMonth == 1 ? currentYear - 1 : currentYear;

            var categories = new[] { "Preventive", "Corrective", "Emergency", "Parts", "Labor" };
            var costAnalysis = new List<CostAnalysisData>();

            foreach (var category in categories)
            {
                var currentCost = await GetCostByCategory(category, currentMonth, currentYear);
                var previousCost = await GetCostByCategory(category, previousMonth, previousYear);
                var budget = GetBudgetForCategory(category);

                costAnalysis.Add(new CostAnalysisData
                {
                    Category = category,
                    CurrentPeriodCost = currentCost,
                    PreviousPeriodCost = previousCost,
                    BudgetAllocated = budget,
                    VariancePercentage = budget > 0 ? ((double)(currentCost - budget) / (double)budget) * 100 : 0,
                    TrendDirection = currentCost > previousCost ? "Up" : currentCost < previousCost ? "Down" : "Stable"
                });
            }

            return costAnalysis;
        }

        public async Task<double> CalculateSystemHealthScoreAsync()
        {
            var totalEquipment = await _context.Equipment.CountAsync();
            if (totalEquipment == 0) return 100;

            var activeEquipment = await _context.Equipment.CountAsync(e => e.Status == EquipmentStatus.Active);
            var inactiveEquipment = await _context.Equipment.CountAsync(e => e.Status == EquipmentStatus.Inactive);
            var retiredEquipment = await _context.Equipment.CountAsync(e => e.Status == EquipmentStatus.Retired);

            var score = ((double)activeEquipment / totalEquipment * 100) - 
                       ((double)inactiveEquipment / totalEquipment * 30) - 
                       ((double)retiredEquipment / totalEquipment * 50);

            return Math.Max(0, Math.Min(100, score));
        }

        public async Task<double> CalculateMaintenanceEfficiencyAsync()
        {
            var totalTasks = await _context.MaintenanceTasks.CountAsync();
            if (totalTasks == 0) return 100;

            var completedOnTime = await _context.MaintenanceTasks
                .CountAsync(mt => mt.Status == MaintenanceStatus.Completed);

            return (double)completedOnTime / totalTasks * 100;
        }

        public async Task<double> CalculateCostEfficiencyAsync()
        {
            var currentMonthCost = await _context.MaintenanceLogs
                .Where(ml => ml.MaintenanceDate.Month == DateTime.Now.Month && ml.MaintenanceDate.Year == DateTime.Now.Year)
                .SumAsync(ml => ml.Cost);

            var budget = 50000; // Example budget
            if (budget == 0) return 100;

            var efficiency = (1 - ((double)currentMonthCost / budget)) * 100;
            return Math.Max(0, Math.Min(100, efficiency));
        }

        public async Task<double> CalculateEquipmentUtilizationAsync()
        {
            var totalEquipment = await _context.Equipment.CountAsync();
            if (totalEquipment == 0) return 0;

            var activeEquipment = await _context.Equipment.CountAsync(e => e.Status == EquipmentStatus.Active);
            return (double)activeEquipment / totalEquipment * 100;
        }

        // Helper methods
        private string GetStatusColorClass(EquipmentStatus status)
        {
            return status switch
            {
                EquipmentStatus.Active => "success",
                EquipmentStatus.Inactive => "warning",
                EquipmentStatus.Retired => "danger",
                _ => "info"
            };
        }

        private double CalculatePerformanceScore(Equipment equipment)
        {
            var maintenanceLogs = equipment.MaintenanceLogs?.ToList() ?? new List<MaintenanceLog>();
            if (!maintenanceLogs.Any()) return 95.0;

            var correctiveCount = maintenanceLogs.Count(ml => ml.MaintenanceType == MaintenanceType.Corrective);
            var totalCount = maintenanceLogs.Count;

            return Math.Max(0, 100 - (correctiveCount * 10) - (totalCount * 2));
        }

        private double CalculateUtilizationRate(Equipment equipment)
        {
            return equipment.Status == EquipmentStatus.Active ? 85.0 + (new Random().NextDouble() * 10) : 0;
        }

        private TimeSpan CalculateAverageDowntime(Equipment equipment)
        {
            return equipment.MaintenanceLogs?.Any() == true ? 
                TimeSpan.FromHours(2 + equipment.MaintenanceLogs.Count() * 0.5) : 
                TimeSpan.Zero;
        }

        private DateTime CalculateNextScheduledMaintenance(Equipment equipment)
        {
            var lastMaintenance = equipment.MaintenanceLogs?.OrderByDescending(ml => ml.MaintenanceDate).FirstOrDefault();
            return lastMaintenance?.MaintenanceDate.AddDays(90) ?? DateTime.Now.AddDays(30);
        }

        private string CalculateHealthStatus(Equipment equipment)
        {
            var score = CalculatePerformanceScore(equipment);
            return score >= 80 ? "Excellent" : score >= 60 ? "Good" : score >= 40 ? "Fair" : "Poor";
        }

        private string GetPredictedIssue(Equipment equipment)
        {
            var issues = new[] { "Bearing wear", "Oil change needed", "Filter replacement", "Calibration drift", "Belt tension" };
            return issues[new Random().Next(issues.Length)];
        }

        private double CalculateFailureProbability(Equipment equipment)
        {
            var daysSinceLastMaintenance = equipment.MaintenanceLogs?.Any() == true ?
                (DateTime.Now - equipment.MaintenanceLogs.OrderByDescending(ml => ml.MaintenanceDate).First().MaintenanceDate).TotalDays :
                365;

            return Math.Min(90, daysSinceLastMaintenance / 365 * 100);
        }

        private DateTime CalculatePredictedFailureDate(Equipment equipment)
        {
            var probability = CalculateFailureProbability(equipment);
            var daysUntilFailure = (100 - probability) * 3.65; // Rough calculation
            return DateTime.Now.AddDays(daysUntilFailure);
        }

        private string GetRecommendedAction(Equipment equipment)
        {
            var actions = new[] { "Schedule preventive maintenance", "Replace worn parts", "Inspect and lubricate", "Calibrate sensors", "Update software" };
            return actions[new Random().Next(actions.Length)];
        }

        private string CalculateRiskLevel(Equipment equipment)
        {
            var probability = CalculateFailureProbability(equipment);
            return probability >= 70 ? "High" : probability >= 40 ? "Medium" : "Low";
        }

        private decimal EstimateRepairCost(Equipment equipment)
        {
            var avgCost = equipment.MaintenanceLogs?.Any() == true ? equipment.MaintenanceLogs.Average(ml => ml.Cost) : 1000;
            return avgCost * (decimal)(1 + new Random().NextDouble());
        }

        private async Task<double> CalculateEquipmentUptimeAsync()
        {
            // Simplified calculation - in real scenario, you'd track actual uptime/downtime
            var totalEquipment = await _context.Equipment.CountAsync();
            var activeEquipment = await _context.Equipment.CountAsync(e => e.Status == EquipmentStatus.Active);
            return totalEquipment > 0 ? (double)activeEquipment / totalEquipment * 100 : 100;
        }

        private async Task<double> CalculateMTTRAsync()
        {
            // Mean Time To Repair - simplified calculation
            var recentRepairs = await _context.MaintenanceLogs
                .Where(ml => ml.MaintenanceType == MaintenanceType.Corrective && ml.MaintenanceDate >= DateTime.Now.AddDays(-30))
                .ToListAsync();

            return recentRepairs.Any() ? recentRepairs.Average(r => r.DowntimeDuration?.TotalHours ?? 4) : 4.0;
        }

        private async Task<double> CalculateMTBFAsync()
        {
            // Mean Time Between Failures - simplified calculation
            var failures = await _context.MaintenanceLogs
                .Where(ml => ml.MaintenanceType == MaintenanceType.Corrective)
                .CountAsync();

            var totalOperatingHours = 24 * 30 * await _context.Equipment.CountAsync(); // 30 days
            return failures > 0 ? totalOperatingHours / (double)failures : 720;
        }

        private async Task<decimal> GetCostByCategory(string category, int month, int year)
        {
            return category switch
            {
                "Preventive" => await _context.MaintenanceLogs
                    .Where(ml => ml.MaintenanceType == MaintenanceType.Preventive && ml.MaintenanceDate.Month == month && ml.MaintenanceDate.Year == year)
                    .SumAsync(ml => ml.Cost),
                "Corrective" => await _context.MaintenanceLogs
                    .Where(ml => ml.MaintenanceType == MaintenanceType.Corrective && ml.MaintenanceDate.Month == month && ml.MaintenanceDate.Year == year)
                    .SumAsync(ml => ml.Cost),
                _ => 0
            };
        }

        private decimal GetBudgetForCategory(string category)
        {
            return category switch
            {
                "Preventive" => 15000,
                "Corrective" => 10000,
                "Emergency" => 5000,
                "Parts" => 20000,
                "Labor" => 25000,
                _ => 10000
            };
        }
    }
}
