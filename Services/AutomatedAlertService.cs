using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using FEENALOoFINALE.Data;
using FEENALOoFINALE.Models;
using FEENALOoFINALE.Hubs;

namespace FEENALOoFINALE.Services
{
    public class AutomatedAlertService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<AutomatedAlertService> _logger;
        private readonly IHubContext<MaintenanceHub> _hubContext;
        private readonly TimeSpan _checkPeriod = TimeSpan.FromMinutes(10); // Check every 10 minutes

        public AutomatedAlertService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<AutomatedAlertService> logger,
            IHubContext<MaintenanceHub> hubContext)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Automated Alert Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndGenerateAlerts();
                    await Task.Delay(_checkPeriod, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during alert generation");
                    await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken); // Wait 2 minutes before retry
                }
            }
        }

        private async Task CheckAndGenerateAlerts()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var newAlerts = new List<Alert>();

            // Check for overdue maintenance tasks
            await CheckOverdueMaintenanceTasks(dbContext, newAlerts);

            // Check for equipment status changes
            await CheckEquipmentStatusIssues(dbContext, newAlerts);

            // Check for low inventory levels
            await CheckLowInventoryLevels(dbContext, newAlerts);

            // Check for high-risk failure predictions
            await CheckHighRiskPredictions(dbContext, newAlerts);

            // Check for equipment without recent maintenance
            await CheckEquipmentMaintenanceOverdue(dbContext, newAlerts);

            // Save new alerts
            if (newAlerts.Any())
            {
                dbContext.Alerts.AddRange(newAlerts);
                await dbContext.SaveChangesAsync();

                // Notify clients about new alerts
                await _hubContext.Clients.All.SendAsync("NewAlerts", newAlerts.Count);

                // Send critical alerts immediately
                var criticalAlerts = newAlerts.Where(a => a.Priority == AlertPriority.High).ToList();
                if (criticalAlerts.Any())
                {
                    await _hubContext.Clients.All.SendAsync("CriticalAlerts", criticalAlerts);
                }

                _logger.LogInformation("Generated {Count} new alerts ({Critical} critical)", 
                    newAlerts.Count, criticalAlerts.Count);
            }
        }

        private async Task CheckOverdueMaintenanceTasks(ApplicationDbContext dbContext, List<Alert> newAlerts)
        {
            var overdueTasks = await dbContext.MaintenanceTasks
                .Include(mt => mt.Equipment)
                .Where(mt => mt.Status == MaintenanceStatus.Pending && mt.ScheduledDate < DateTime.Now.AddDays(-1))
                .ToListAsync();

            foreach (var task in overdueTasks)
            {
                // Check if we already have a recent alert for this task
                var existingAlert = await dbContext.Alerts
                    .Where(a => a.Description.Contains($"Task {task.TaskId}") && 
                               a.CreatedDate >= DateTime.Now.AddDays(-1))
                    .FirstOrDefaultAsync();

                if (existingAlert == null)
                {
                    var daysOverdue = (DateTime.Now - task.ScheduledDate).TotalDays;
                    var priority = daysOverdue > 7 ? AlertPriority.High : daysOverdue > 3 ? AlertPriority.High : AlertPriority.Medium;

                    newAlerts.Add(new Alert
                    {
                        EquipmentId = task.EquipmentId,
                        Priority = priority,
                        Description = $"Maintenance task {task.TaskId} is {Math.Floor(daysOverdue)} days overdue for equipment {task.Equipment?.EquipmentModel?.ModelName ?? "Unknown"}",
                        CreatedDate = DateTime.Now,
                        Status = AlertStatus.Open
                    });
                }
            }
        }

        private async Task CheckEquipmentStatusIssues(ApplicationDbContext dbContext, List<Alert> newAlerts)
        {
            var problematicEquipment = await dbContext.Equipment
                .Where(e => e.Status == EquipmentStatus.Inactive)
                .ToListAsync();

            foreach (var equipment in problematicEquipment)
            {
                // Check if we already have a recent alert for this equipment status
                var existingAlert = await dbContext.Alerts
                    .Where(a => a.EquipmentId == equipment.EquipmentId && 
                               a.Description.Contains("status") && 
                               a.CreatedDate >= DateTime.Now.AddHours(-4))
                    .FirstOrDefaultAsync();

                if (existingAlert == null)
                {
                    var priority = equipment.Status == EquipmentStatus.Inactive ? AlertPriority.High : AlertPriority.Medium;
                    var statusMessage = equipment.Status == EquipmentStatus.Inactive ? "inactive" : "under maintenance";

                    newAlerts.Add(new Alert
                    {
                        EquipmentId = equipment.EquipmentId,
                        Priority = priority,
                        Description = $"Equipment {equipment.EquipmentModel?.ModelName ?? "Unknown"} is currently {statusMessage}",
                        CreatedDate = DateTime.Now,
                        Status = AlertStatus.Open
                    });
                }
            }
        }

        private async Task CheckLowInventoryLevels(ApplicationDbContext dbContext, List<Alert> newAlerts)
        {
            var inventoryItems = await dbContext.InventoryItems
                .Include(ii => ii.InventoryStocks)
                .ToListAsync();

            foreach (var item in inventoryItems)
            {
                var currentStock = item.InventoryStocks?.Sum(stock => stock.Quantity) ?? 0;
                
                if (currentStock <= item.MinimumStockLevel)
                {
                    // Check if we already have a recent alert for this inventory item
                    var existingAlert = await dbContext.Alerts
                        .Where(a => a.Description.Contains($"inventory item {item.Name}") && 
                                   a.CreatedDate >= DateTime.Now.AddDays(-1))
                        .FirstOrDefaultAsync();

                    if (existingAlert == null)
                    {
                        var priority = currentStock == 0 ? AlertPriority.High : AlertPriority.High;
                        var stockStatus = currentStock == 0 ? "out of stock" : "low stock";

                        newAlerts.Add(new Alert
                        {
                            InventoryItemId = item.ItemId,
                            Priority = priority,
                            Description = $"Inventory item {item.Name} is {stockStatus} (Current: {currentStock}, Minimum: {item.MinimumStockLevel})",
                            CreatedDate = DateTime.Now,
                            Status = AlertStatus.Open
                        });
                    }
                }
            }
        }

        private async Task CheckHighRiskPredictions(ApplicationDbContext dbContext, List<Alert> newAlerts)
        {
            var highRiskPredictions = await dbContext.FailurePredictions
                .Include(fp => fp.Equipment)
                .Where(fp => (fp.Status == PredictionStatus.High) && 
                            fp.CreatedDate >= DateTime.Now.AddDays(-1))
                .ToListAsync();

            foreach (var prediction in highRiskPredictions)
            {
                // Check if we already have a recent alert for this prediction
                var existingAlert = await dbContext.Alerts
                    .Where(a => a.EquipmentId == prediction.EquipmentId && 
                               a.Description.Contains("failure prediction") && 
                               a.CreatedDate >= DateTime.Now.AddDays(-1))
                    .FirstOrDefaultAsync();

                if (existingAlert == null)
                {
                    var priority = prediction.Status == PredictionStatus.High ? AlertPriority.High : AlertPriority.High;

                    newAlerts.Add(new Alert
                    {
                        EquipmentId = prediction.EquipmentId,
                        Priority = priority,
                        Description = $"High failure prediction for equipment {prediction.Equipment?.EquipmentModel?.ModelName ?? "Unknown"}: {prediction.Status} risk level with {prediction.ConfidenceLevel}% confidence",
                        CreatedDate = DateTime.Now,
                        Status = AlertStatus.Open
                    });
                }
            }
        }

        private async Task CheckEquipmentMaintenanceOverdue(ApplicationDbContext dbContext, List<Alert> newAlerts)
        {
            var equipment = await dbContext.Equipment
                .Include(e => e.MaintenanceLogs)
                .Where(e => e.Status == EquipmentStatus.Active)
                .ToListAsync();

            foreach (var item in equipment)
            {
                var lastMaintenance = item.MaintenanceLogs
                    .OrderByDescending(ml => ml.LogDate)
                    .FirstOrDefault();

                if (lastMaintenance == null || (DateTime.Now - lastMaintenance.LogDate).TotalDays > 365)
                {
                    // Check if we already have a recent alert for this equipment maintenance
                    var existingAlert = await dbContext.Alerts
                        .Where(a => a.EquipmentId == item.EquipmentId && 
                                   a.Description.Contains("maintenance overdue") && 
                                   a.CreatedDate >= DateTime.Now.AddDays(-7))
                        .FirstOrDefaultAsync();

                    if (existingAlert == null)
                    {
                        var daysSinceLastMaintenance = lastMaintenance != null 
                            ? (DateTime.Now - lastMaintenance.LogDate).TotalDays 
                            : 9999;

                        var priority = daysSinceLastMaintenance > 730 ? AlertPriority.High : AlertPriority.Medium; // 2 years = high priority

                        var message = lastMaintenance != null 
                            ? $"Equipment {item.EquipmentModel?.ModelName ?? "Unknown"} maintenance overdue: {Math.Floor(daysSinceLastMaintenance)} days since last maintenance"
                            : $"Equipment {item.EquipmentModel?.ModelName ?? "Unknown"} has no maintenance history and may require inspection";

                        newAlerts.Add(new Alert
                        {
                            EquipmentId = item.EquipmentId,
                            Priority = priority,
                            Description = message,
                            CreatedDate = DateTime.Now,
                            Status = AlertStatus.Open
                        });
                    }
                }
            }
        }
    }
}
