using Microsoft.AspNetCore.SignalR;
using FEENALOoFINALE.Data;
using FEENALOoFINALE.Models;
using FEENALOoFINALE.Hubs;
using Microsoft.EntityFrameworkCore;

namespace FEENALOoFINALE.Services
{
    public interface IRealtimeNotificationService
    {
        Task SendDashboardUpdateAsync(object dashboardData);
        Task SendAlertNotificationAsync(Alert alert);
        Task SendEquipmentStatusChangeAsync(int equipmentId, EquipmentStatus oldStatus, EquipmentStatus newStatus);
        Task SendMaintenanceUpdateAsync(MaintenanceLog maintenanceLog);
        Task SendInventoryLowStockAlertAsync(InventoryItem item, int currentStock);
        Task SendFailurePredictionAlertAsync(FailurePrediction prediction);
        Task SendKPIUpdateAsync(List<KPIProgressIndicator> kpis);
        Task SendSystemHealthUpdateAsync(object healthData);
        Task BroadcastToGroupAsync(string groupName, string eventType, object data);
    }

    public class RealtimeNotificationService : IRealtimeNotificationService
    {
        private readonly IHubContext<MaintenanceHub> _hubContext;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RealtimeNotificationService> _logger;

        public RealtimeNotificationService(
            IHubContext<MaintenanceHub> hubContext,
            ApplicationDbContext context,
            ILogger<RealtimeNotificationService> logger)
        {
            _hubContext = hubContext;
            _context = context;
            _logger = logger;
        }

        public async Task SendDashboardUpdateAsync(object dashboardData)
        {
            try
            {
                await _hubContext.Clients.Group("Dashboard").SendAsync("DashboardUpdate", new
                {
                    Type = "dashboard_update",
                    Data = dashboardData,
                    Timestamp = DateTime.Now
                });

                _logger.LogInformation("Dashboard update sent to all connected clients");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send dashboard update");
            }
        }

        public async Task SendAlertNotificationAsync(Alert alert)
        {
            try
            {
                var equipment = await _context.Equipment
                    .Include(e => e.EquipmentModel)
                    .ThenInclude(em => em!.EquipmentType)
                    .Include(e => e.Room)
                    .ThenInclude(r => r!.Building)
                    .FirstOrDefaultAsync(e => e.EquipmentId == alert.EquipmentId);

                var alertData = new
                {
                    AlertId = alert.AlertId,
                    Priority = alert.Priority.ToString(),
                    Status = alert.Status.ToString(),
                    Description = alert.Description,
                    CreatedDate = alert.CreatedDate,
                    Equipment = equipment?.EquipmentModel?.ModelName ?? "Unknown",
                    Location = equipment != null ? $"{equipment.Room?.Building?.BuildingName} - {equipment.Room?.RoomName}" : "Unknown",
                    Type = "alert",
                    Severity = GetAlertSeverityLevel(alert.Priority)
                };

                // Send to all users for high priority alerts
                if (alert.Priority == AlertPriority.High)
                {
                    await _hubContext.Clients.All.SendAsync("CriticalAlert", alertData);
                }
                else
                {
                    await _hubContext.Clients.Group("Alerts").SendAsync("NewAlert", alertData);
                }

                _logger.LogInformation($"Alert notification sent for Alert ID: {alert.AlertId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send alert notification for Alert ID: {alert.AlertId}");
            }
        }

        public async Task SendEquipmentStatusChangeAsync(int equipmentId, EquipmentStatus oldStatus, EquipmentStatus newStatus)
        {
            try
            {
                var equipment = await _context.Equipment
                    .Include(e => e.EquipmentModel)
                    .ThenInclude(em => em!.EquipmentType)
                    .Include(e => e.Room)
                    .ThenInclude(r => r!.Building)
                    .FirstOrDefaultAsync(e => e.EquipmentId == equipmentId);

                if (equipment == null) return;

                var statusChangeData = new
                {
                    EquipmentId = equipmentId,
                    EquipmentName = equipment.EquipmentModel?.ModelName ?? "Unknown",
                    Location = $"{equipment.Room?.Building?.BuildingName} - {equipment.Room?.RoomName}",
                    OldStatus = oldStatus.ToString(),
                    NewStatus = newStatus.ToString(),
                    Timestamp = DateTime.Now,
                    Type = "equipment_status_change",
                    StatusClass = GetStatusClass(newStatus)
                };

                await _hubContext.Clients.Group("Equipment").SendAsync("EquipmentStatusChange", statusChangeData);
                await _hubContext.Clients.Group("Dashboard").SendAsync("EquipmentUpdate", statusChangeData);

                _logger.LogInformation($"Equipment status change sent for Equipment ID: {equipmentId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send equipment status change for Equipment ID: {equipmentId}");
            }
        }

        public async Task SendMaintenanceUpdateAsync(MaintenanceLog maintenanceLog)
        {
            try
            {
                var equipment = await _context.Equipment
                    .Include(e => e.EquipmentModel)
                    .FirstOrDefaultAsync(e => e.EquipmentId == maintenanceLog.EquipmentId);

                var maintenanceData = new
                {
                    LogId = maintenanceLog.LogId,
                    EquipmentId = maintenanceLog.EquipmentId,
                    EquipmentName = equipment?.EquipmentModel?.ModelName ?? "Unknown",
                    MaintenanceType = maintenanceLog.MaintenanceType.ToString(),
                    Description = maintenanceLog.Description,
                    Cost = maintenanceLog.Cost,
                    LogDate = maintenanceLog.LogDate,
                    Type = "maintenance_update",
                    TypeClass = GetMaintenanceTypeClass(maintenanceLog.MaintenanceType)
                };

                await _hubContext.Clients.Group("Maintenance").SendAsync("MaintenanceUpdate", maintenanceData);
                await _hubContext.Clients.Group("Dashboard").SendAsync("MaintenanceLogUpdate", maintenanceData);

                _logger.LogInformation($"Maintenance update sent for Log ID: {maintenanceLog.LogId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send maintenance update for Log ID: {maintenanceLog.LogId}");
            }
        }

        public async Task SendInventoryLowStockAlertAsync(InventoryItem item, int currentStock)
        {
            try
            {
                var lowStockData = new
                {
                    ItemId = item.ItemId,
                    ItemName = item.Name,
                    Category = item.Category.ToString(),
                    CurrentStock = currentStock,
                    MinimumLevel = item.MinimumStockLevel,
                    ShortageAmount = item.MinimumStockLevel - currentStock,
                    Type = "low_stock_alert",
                    Severity = currentStock <= 0 ? "critical" : "warning",
                    Timestamp = DateTime.Now
                };

                await _hubContext.Clients.Group("Inventory").SendAsync("LowStockAlert", lowStockData);
                await _hubContext.Clients.Group("Dashboard").SendAsync("InventoryAlert", lowStockData);

                _logger.LogInformation($"Low stock alert sent for Item: {item.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send low stock alert for Item: {item.Name}");
            }
        }

        public async Task SendFailurePredictionAlertAsync(FailurePrediction prediction)
        {
            try
            {
                var equipment = await _context.Equipment
                    .Include(e => e.EquipmentModel)
                    .FirstOrDefaultAsync(e => e.EquipmentId == prediction.EquipmentId);

                var predictionData = new
                {
                    PredictionId = prediction.PredictionId,
                    EquipmentId = prediction.EquipmentId,
                    EquipmentName = equipment?.EquipmentModel?.ModelName ?? "Unknown",
                    PredictedDate = prediction.PredictedFailureDate,
                    Confidence = prediction.ConfidenceLevel,
                    DaysUntilFailure = (prediction.PredictedFailureDate - DateTime.Now).Days,
                    Type = "failure_prediction",
                    RiskLevel = GetRiskLevel(prediction.ConfidenceLevel, (prediction.PredictedFailureDate - DateTime.Now).Days),
                    Timestamp = DateTime.Now
                };

                // Send to predictive maintenance group and dashboard
                await _hubContext.Clients.Group("Predictive").SendAsync("FailurePrediction", predictionData);
                await _hubContext.Clients.Group("Dashboard").SendAsync("PredictiveAlert", predictionData);

                // If high risk, send to all users
                if (predictionData.RiskLevel == "high")
                {
                    await _hubContext.Clients.All.SendAsync("HighRiskPrediction", predictionData);
                }

                _logger.LogInformation($"Failure prediction alert sent for Equipment ID: {prediction.EquipmentId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send failure prediction alert for Equipment ID: {prediction.EquipmentId}");
            }
        }

        public async Task SendKPIUpdateAsync(List<KPIProgressIndicator> kpis)
        {
            try
            {
                var kpiData = new
                {
                    KPIs = kpis.Select(k => new
                    {
                        Name = k.KPIName,
                        CurrentValue = k.CurrentValue,
                        TargetValue = k.TargetValue,
                        Progress = k.ProgressPercentage,
                        Status = k.Status,
                        Direction = k.Direction,
                        Color = k.Color
                    }),
                    Type = "kpi_update",
                    Timestamp = DateTime.Now
                };

                await _hubContext.Clients.Group("Analytics").SendAsync("KPIUpdate", kpiData);
                await _hubContext.Clients.Group("Dashboard").SendAsync("KPIRefresh", kpiData);

                _logger.LogInformation($"KPI update sent for {kpis.Count} indicators");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send KPI update");
            }
        }

        public async Task SendSystemHealthUpdateAsync(object healthData)
        {
            try
            {
                var systemHealthData = new
                {
                    Health = healthData,
                    Type = "system_health",
                    Timestamp = DateTime.Now
                };

                await _hubContext.Clients.Group("System").SendAsync("SystemHealthUpdate", systemHealthData);

                _logger.LogInformation("System health update sent");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send system health update");
            }
        }

        public async Task BroadcastToGroupAsync(string groupName, string eventType, object data)
        {
            try
            {
                await _hubContext.Clients.Group(groupName).SendAsync(eventType, data);
                _logger.LogInformation($"Broadcast sent to group {groupName} with event type {eventType}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to broadcast to group {groupName}");
            }
        }

        // Helper methods
        private string GetAlertSeverityLevel(AlertPriority priority)
        {
            return priority switch
            {
                AlertPriority.High => "critical",
                AlertPriority.Medium => "warning",
                AlertPriority.Low => "info",
                _ => "info"
            };
        }

        private string GetStatusClass(EquipmentStatus status)
        {
            return status switch
            {
                EquipmentStatus.Active => "success",
                EquipmentStatus.Inactive => "warning",
                EquipmentStatus.Retired => "danger",
                _ => "secondary"
            };
        }

        private string GetMaintenanceTypeClass(MaintenanceType type)
        {
            return type switch
            {
                MaintenanceType.Preventive => "primary",
                MaintenanceType.Corrective => "warning",
                MaintenanceType.Emergency => "danger",
                MaintenanceType.Inspection => "info",
                _ => "secondary"
            };
        }

        private string GetRiskLevel(double confidence, int daysUntilFailure)
        {
            if (confidence > 0.8 && daysUntilFailure <= 7)
                return "high";
            else if (confidence > 0.6 && daysUntilFailure <= 30)
                return "medium";
            else
                return "low";
        }
    }
}
