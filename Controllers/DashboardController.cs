using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FEENALOoFINALE.Data;
using FEENALOoFINALE.Models; // Ensures MaintenanceTask and MaintenanceStatus enum are in scope
using FEENALOoFINALE.Hubs; // Add SignalR Hub
using FEENALOoFINALE.Services; // Add Services namespace
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR; // Add SignalR services
using Microsoft.AspNetCore.Identity;
using System.Text.Json;

namespace FEENALOoFINALE.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<MaintenanceHub>? _hubContext;
        private readonly UserManager<User> _userManager;
        private readonly IAdvancedAnalyticsService _analyticsService;

        public DashboardController(ApplicationDbContext context, IHubContext<MaintenanceHub>? hubContext, UserManager<User> userManager, IAdvancedAnalyticsService analyticsService)
        {
            _context = context;
            _hubContext = hubContext;
            _userManager = userManager;
            _analyticsService = analyticsService;
        }

        public async Task<IActionResult> Index(DashboardFilterViewModel? filters = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserId = currentUser?.Id ?? "";

            // Initialize filters if null or apply defaults
            filters ??= new DashboardFilterViewModel();

            // Load saved views for the current user
            var savedViews = await _context.SavedDashboardViews
                .Where(v => v.UserId == currentUserId || v.IsPublic)
                .OrderBy(v => v.Name)
                .ToListAsync();

            // Get filter options
            var filterOptions = new DashboardFilterOptions
            {
                Buildings = await _context.Buildings.OrderBy(b => b.BuildingName).ToListAsync(),
                EquipmentTypes = await _context.EquipmentTypes.OrderBy(et => et.EquipmentTypeName).ToListAsync(),
                Users = await _userManager.Users.OrderBy(u => u.UserName).ToListAsync(),
                SavedViews = savedViews
            };

            // Apply filters to get dashboard data
            var dashboardData = await GetFilteredDashboardData(filters ?? new DashboardFilterViewModel());

            // Use EnhancedDashboardViewModel to include filter data
            var viewModel = new EnhancedDashboardViewModel
            {
                // Basic dashboard data
                TotalEquipment = dashboardData.TotalEquipment,
                ActiveMaintenanceTasks = dashboardData.ActiveMaintenanceTasks,
                LowStockItems = dashboardData.LowStockItems,
                CriticalAlerts = dashboardData.CriticalAlerts,
                OverdueMaintenances = dashboardData.OverdueMaintenances,
                EquipmentNeedingAttention = dashboardData.EquipmentNeedingAttention,
                RecentAlerts = dashboardData.RecentAlerts,
                UpcomingMaintenances = dashboardData.UpcomingMaintenances,
                CriticalEquipment = dashboardData.CriticalEquipment,
                SuggestedActions = dashboardData.SuggestedActions,
                EquipmentStatus = dashboardData.EquipmentStatus,

                // Filtering data
                Filters = filters ?? new DashboardFilterViewModel(),
                FilterOptions = filterOptions,
                SavedViews = savedViews,
                CurrentViewName = filters?.SavedViewName ?? (filters?.HasActiveFilters() == true ? "Custom Filter" : "All Data"),
                TotalRecordsBeforeFilter = dashboardData.TotalRecordsBeforeFilter,
                TotalRecordsAfterFilter = dashboardData.TotalRecordsAfterFilter,
                FilteredAnalytics = dashboardData.FilteredAnalytics
            };

            return View(viewModel);
        }

        private List<QuickAction> GenerateBasicSuggestedActions(int criticalAlerts, int overdueMaintenances, int equipmentNeedingAttention)
        {
            var suggestedActions = new List<QuickAction>();

            if (criticalAlerts > 0)
            {
                suggestedActions.Add(new QuickAction
                {
                    Title = "Review Critical Alerts",
                    Description = "Address high-priority equipment alerts",
                    Icon = "bi-exclamation-triangle",
                    Controller = "Alert",
                    Action = "Index",
                    Priority = "critical",
                    BadgeText = criticalAlerts.ToString()
                });
            }

            if (overdueMaintenances > 0)
            {
                suggestedActions.Add(new QuickAction
                {
                    Title = "Overdue Maintenance",
                    Description = "Schedule or complete overdue maintenance tasks",
                    Icon = "bi-calendar-x",
                    Controller = "MaintenanceLog",
                    Action = "Index",
                    Priority = "urgent",
                    BadgeText = overdueMaintenances.ToString()
                });
            }

            if (equipmentNeedingAttention > 0)
            {
                suggestedActions.Add(new QuickAction
                {
                    Title = "Equipment Issues",
                    Description = "Review equipment requiring immediate attention",
                    Icon = "bi-gear",
                    Controller = "Equipment",
                    Action = "Index",
                    Priority = "urgent",
                    BadgeText = equipmentNeedingAttention.ToString()
                });
            }

            suggestedActions.Add(new QuickAction
            {
                Title = "Add Maintenance Log",
                Description = "Record new maintenance activity",
                Icon = "bi-plus-circle",
                Controller = "MaintenanceLog",
                Action = "Create",
                Priority = "normal"
            });

            suggestedActions.Add(new QuickAction
            {
                Title = "Manage Assets",
                Description = "View and manage equipment and inventory",
                Icon = "bi-boxes",
                Controller = "Asset",
                Action = "Index",
                Priority = "normal"
            });

            return suggestedActions;
        }

        // API endpoint for real-time dashboard data
        [HttpGet]
        public async Task<IActionResult> GetDashboardData()
        {
            var data = new
            {
                TotalEquipment = await _context.Equipment.CountAsync(),
                ActiveMaintenanceTasks = await _context.MaintenanceTasks
                    .Where(t => t.Status != MaintenanceStatus.Completed)
                    .CountAsync(),
                LowStockItems = await _context.InventoryItems
                    .Where(i => i.InventoryStocks != null && i.InventoryStocks.Sum(s => s.Quantity) <= i.MinStockLevel)
                    .CountAsync(),
                CriticalAlerts = await _context.Alerts
                    .Where(a => a.Priority == AlertPriority.High && a.Status == AlertStatus.Open)
                    .CountAsync(),
                OverdueMaintenances = await _context.MaintenanceTasks
                    .Where(m => m.ScheduledDate < DateTime.Now && m.Status != MaintenanceStatus.Completed)
                    .CountAsync(),
                EquipmentNeedingAttention = await _context.Equipment
                    .Where(e => e.Status == EquipmentStatus.Inactive || e.Status == EquipmentStatus.Retired)
                    .CountAsync(),
                LastUpdated = DateTime.Now
            };

            return Json(data);
        }

        // Method to broadcast dashboard updates
        public async Task BroadcastDashboardUpdate()
        {
            try
            {
                var dashboardData = await GetDashboardDataInternal();
                if (_hubContext != null)
                {
                    await _hubContext.Clients.Group("Dashboard").SendAsync("DashboardUpdate", dashboardData);
                }
            }
            catch (Exception ex)
            {
                // Log error but don't throw - SignalR updates are nice-to-have
                Console.WriteLine($"Error broadcasting dashboard update: {ex.Message}");
            }
        }

        private async Task<object> GetDashboardDataInternal()
        {
            return new
            {
                TotalEquipment = await _context.Equipment.CountAsync(),
                ActiveMaintenanceTasks = await _context.MaintenanceTasks
                    .Where(t => t.Status != MaintenanceStatus.Completed)
                    .CountAsync(),
                LowStockItems = await _context.InventoryItems
                    .Where(i => i.InventoryStocks != null && i.InventoryStocks.Sum(s => s.Quantity) <= i.MinStockLevel)
                    .CountAsync(),
                CriticalAlerts = await _context.Alerts
                    .Where(a => a.Priority == AlertPriority.High && a.Status == AlertStatus.Open)
                    .CountAsync(),
                OverdueMaintenances = await _context.MaintenanceTasks
                    .Where(m => m.ScheduledDate < DateTime.Now && m.Status != MaintenanceStatus.Completed)
                    .CountAsync(),
                EquipmentNeedingAttention = await _context.Equipment
                    .Where(e => e.Status == EquipmentStatus.Inactive || e.Status == EquipmentStatus.Retired)
                    .CountAsync(),
                LastUpdated = DateTime.Now
            };
        }

        // Chart data endpoints
        [HttpGet]
        public async Task<IActionResult> GetEquipmentStatusChart()
        {
            var equipmentStatus = await _context.Equipment
                .GroupBy(e => e.Status)
                .Select(g => new { 
                    Status = g.Key.ToString(), 
                    Count = g.Count(),
                    Color = GetStatusColor(g.Key)
                })
                .ToListAsync();

            return Json(new
            {
                labels = equipmentStatus.Select(x => x.Status).ToArray(),
                datasets = new[]
                {
                    new
                    {
                        data = equipmentStatus.Select(x => x.Count).ToArray(),
                        backgroundColor = equipmentStatus.Select(x => x.Color).ToArray(),
                        borderColor = "#ffffff",
                        borderWidth = 2
                    }
                }
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetMaintenanceTrendsChart()
        {
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-6 + i))
                .ToList();

            var maintenanceData = new List<object>();
            
            foreach (var date in last7Days)
            {
                var dayStart = date.Date;
                var dayEnd = dayStart.AddDays(1);
                
                var maintenanceCount = await _context.MaintenanceLogs
                    .Where(m => m.LogDate >= dayStart && m.LogDate < dayEnd)
                    .CountAsync();
                    
                maintenanceData.Add(new
                {
                    date = date.ToString("MMM dd"),
                    count = maintenanceCount
                });
            }

            return Json(new
            {
                labels = maintenanceData.Select(x => ((dynamic)x).date).ToArray(),
                datasets = new[]
                {
                    new
                    {
                        label = "Maintenance Activities",
                        data = maintenanceData.Select(x => ((dynamic)x).count).ToArray(),
                        borderColor = "#4f46e5",
                        backgroundColor = "rgba(79, 70, 229, 0.1)",
                        tension = 0.4,
                        fill = true
                    }
                }
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAlertPriorityChart()
        {
            var alertData = await _context.Alerts
                .Where(a => a.Status == AlertStatus.Open)
                .GroupBy(a => a.Priority)
                .Select(g => new {
                    Priority = g.Key.ToString(),
                    Count = g.Count(),
                    Color = GetPriorityColor(g.Key)
                })
                .ToListAsync();

            return Json(new
            {
                labels = alertData.Select(x => x.Priority).ToArray(),
                datasets = new[]
                {
                    new
                    {
                        label = "Open Alerts",
                        data = alertData.Select(x => x.Count).ToArray(),
                        backgroundColor = alertData.Select(x => x.Color).ToArray(),
                        borderColor = "#ffffff",
                        borderWidth = 1
                    }
                }
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetInventoryStatusChart()
        {
            var totalItems = await _context.InventoryItems.CountAsync();
            var lowStockItems = await _context.InventoryItems
                .Where(i => i.InventoryStocks != null && 
                           i.InventoryStocks.Sum(s => s.Quantity) <= i.MinStockLevel)
                .CountAsync();
            var normalStockItems = totalItems - lowStockItems;

            return Json(new
            {
                labels = new[] { "Normal Stock", "Low Stock" },
                datasets = new[]
                {
                    new
                    {
                        data = new[] { normalStockItems, lowStockItems },
                        backgroundColor = new[] { "#10b981", "#f59e0b" },
                        borderColor = "#ffffff",
                        borderWidth = 2
                    }
                }
            });
        }

        // Advanced Analytics Endpoints

        [HttpGet]
        public async Task<IActionResult> GetMaintenanceCostAnalysis()
        {
            try
            {
                var monthlyData = await _context.MaintenanceLogs
                    .GroupBy(ml => new { ml.LogDate.Year, ml.LogDate.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        TotalCost = g.Sum(ml => ml.Cost),
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month)
                    .Take(12)
                    .ToListAsync();

                var labels = monthlyData.Select(x => $"{x.Year}-{x.Month:D2}").ToArray();
                var costs = monthlyData.Select(x => x.TotalCost).ToArray();
                var counts = monthlyData.Select(x => x.Count).ToArray();

                return Json(new
                {
                    labels = labels,
                    datasets = new object[]
                    {
                        new
                        {
                            label = "Total Cost",
                            data = costs,
                            backgroundColor = "rgba(102, 126, 234, 0.7)",
                            borderColor = "rgba(102, 126, 234, 1)",
                            type = "line",
                            yAxisID = "y"
                        },
                        new
                        {
                            label = "Maintenance Count",
                            data = counts,
                            backgroundColor = "rgba(255, 107, 107, 0.7)",
                            borderColor = "rgba(255, 107, 107, 1)",
                            type = "bar",
                            yAxisID = "y1"
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetEquipmentLifecycleChart()
        {
            try
            {
                var equipmentData = await _context.Equipment
                    .Include(e => e.EquipmentType)
                    .GroupBy(e => e.EquipmentType!.EquipmentTypeName)
                    .Select(g => new
                    {
                        Type = g.Key,
                        Active = g.Count(e => e.Status == EquipmentStatus.Active),
                        Inactive = g.Count(e => e.Status == EquipmentStatus.Inactive),
                        Retired = g.Count(e => e.Status == EquipmentStatus.Retired)
                    })
                    .ToListAsync();

                var labels = equipmentData.Select(x => x.Type).ToArray();
                var activeData = equipmentData.Select(x => x.Active).ToArray();
                var inactiveData = equipmentData.Select(x => x.Inactive).ToArray();
                var retiredData = equipmentData.Select(x => x.Retired).ToArray();

                return Json(new
                {
                    labels = labels,
                    datasets = new[]
                    {
                        new
                        {
                            label = "Active",
                            data = activeData,
                            backgroundColor = "rgba(86, 171, 47, 0.8)"
                        },
                        new
                        {
                            label = "Inactive",
                            data = inactiveData,
                            backgroundColor = "rgba(255, 193, 7, 0.8)"
                        },
                        new
                        {
                            label = "Retired",
                            data = retiredData,
                            backgroundColor = "rgba(220, 53, 69, 0.8)"
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFailurePredictionTrends()
        {
            try
            {
                var predictionData = await _context.FailurePredictions
                    .Include(fp => fp.Equipment)
                        .ThenInclude(e => e.EquipmentType)
                    .Where(fp => fp.PredictedFailureDate >= DateTime.Now.AddDays(-30))
                    .GroupBy(fp => fp.PredictedFailureDate.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        HighRisk = g.Count(fp => fp.Status == PredictionStatus.High),
                        MediumRisk = g.Count(fp => fp.Status == PredictionStatus.Medium),
                        LowRisk = g.Count(fp => fp.Status == PredictionStatus.Low)
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync();

                var labels = predictionData.Select(x => x.Date.ToString("MM-dd")).ToArray();
                var highRisk = predictionData.Select(x => x.HighRisk).ToArray();
                var mediumRisk = predictionData.Select(x => x.MediumRisk).ToArray();
                var lowRisk = predictionData.Select(x => x.LowRisk).ToArray();

                return Json(new
                {
                    labels = labels,
                    datasets = new[]
                    {
                        new
                        {
                            label = "High Risk",
                            data = highRisk,
                            borderColor = "rgba(255, 107, 107, 1)",
                            backgroundColor = "rgba(255, 107, 107, 0.2)",
                            fill = true
                        },
                        new
                        {
                            label = "Medium Risk",
                            data = mediumRisk,
                            borderColor = "rgba(255, 193, 7, 1)",
                            backgroundColor = "rgba(255, 193, 7, 0.2)",
                            fill = true
                        },
                        new
                        {
                            label = "Low Risk",
                            data = lowRisk,
                            borderColor = "rgba(86, 171, 47, 1)",
                            backgroundColor = "rgba(86, 171, 47, 0.2)",
                            fill = true
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetKPIMetrics()
        {
            try
            {
                var totalEquipment = await _context.Equipment.CountAsync();
                var activeEquipment = await _context.Equipment.CountAsync(e => e.Status == EquipmentStatus.Active);
                
                // Overall Equipment Effectiveness (OEE) - simplified calculation
                var oee = totalEquipment > 0 ? (double)activeEquipment / totalEquipment * 100 : 0;

                // Mean Time Between Failures (MTBF) - calculated in days
                var maintenanceLogs = await _context.MaintenanceLogs
                    .Where(ml => ml.MaintenanceType == MaintenanceType.Corrective)
                    .OrderBy(ml => ml.LogDate)
                    .ToListAsync();

                double mtbf = 0;
                if (maintenanceLogs.Count > 1)
                {
                    var intervals = new List<double>();
                    for (int i = 1; i < maintenanceLogs.Count; i++)
                    {
                        var interval = (maintenanceLogs[i].LogDate - maintenanceLogs[i - 1].LogDate).TotalDays;
                        intervals.Add(interval);
                    }
                    mtbf = intervals.Average();
                }

                // Mean Time To Repair (MTTR) - based on downtime
                var mttrList = await _context.MaintenanceLogs
                    .Where(ml => ml.DowntimeDuration.HasValue)
                    .Select(ml => ml.DowntimeDuration!.Value.TotalHours)
                    .ToListAsync();
                var mttr = mttrList.Any() ? mttrList.Average() : 0;

                // Maintenance Cost per Equipment
                var totalMaintenanceCost = await _context.MaintenanceLogs.SumAsync(ml => ml.Cost);
                var costPerEquipment = totalEquipment > 0 ? totalMaintenanceCost / totalEquipment : 0;

                // Inventory Turnover Rate (simplified)
                var totalInventoryValue = await _context.InventoryItems.SumAsync(i => i.MinimumStockLevel * 10); // Estimated value
                var inventoryTurnover = totalInventoryValue > 0 ? (double)totalMaintenanceCost / totalInventoryValue : 0;

                return Json(new
                {
                    oee = Math.Round(oee, 1),
                    mtbf = Math.Round(mtbf, 1),
                    mttr = Math.Round(mttr, 1),
                    costPerEquipment = Math.Round(costPerEquipment, 2),
                    inventoryTurnover = Math.Round(inventoryTurnover, 2),
                    targets = new
                    {
                        oee = 85.0,
                        mtbf = 30.0,
                        mttr = 4.0,
                        costPerEquipment = 500.0,
                        inventoryTurnover = 4.0
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMaintenanceEfficiencyChart()
        {
            try
            {
                var efficiencyData = await _context.MaintenanceLogs
                    .Include(ml => ml.Equipment)
                        .ThenInclude(e => e.EquipmentType)
                    .GroupBy(ml => ml.Equipment!.EquipmentType!.EquipmentTypeName)
                    .Select(g => new
                    {
                        EquipmentType = g.Key,
                        TotalMaintenance = g.Count(),
                        PreventiveMaintenance = g.Count(ml => ml.MaintenanceType == MaintenanceType.Preventive),
                        CorrectiveMaintenance = g.Count(ml => ml.MaintenanceType == MaintenanceType.Corrective),
                        AverageDowntime = g.Where(ml => ml.DowntimeDuration.HasValue)
                                          .Select(ml => ml.DowntimeDuration!.Value.TotalHours)
                                          .DefaultIfEmpty(0)
                                          .Average(),
                        TotalCost = g.Sum(ml => ml.Cost)
                    })
                    .ToListAsync();

                var labels = efficiencyData.Select(x => x.EquipmentType).ToArray();
                var preventiveRatio = efficiencyData.Select(x => 
                    x.TotalMaintenance > 0 ? (double)x.PreventiveMaintenance / x.TotalMaintenance * 100 : 0
                ).ToArray();
                var avgDowntime = efficiencyData.Select(x => x.AverageDowntime).ToArray();
                var costs = efficiencyData.Select(x => x.TotalCost).ToArray();

                return Json(new
                {
                    labels = labels,
                    datasets = new object[]
                    {
                        new
                        {
                            label = "Preventive Maintenance %",
                            data = preventiveRatio,
                            backgroundColor = "rgba(86, 171, 47, 0.7)",
                            yAxisID = "y"
                        },
                        new
                        {
                            label = "Average Downtime (hours)",
                            data = avgDowntime,
                            backgroundColor = "rgba(255, 193, 7, 0.7)",
                            type = "line",
                            yAxisID = "y1"
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // Export functionality will be implemented later
        // TODO: Add export endpoints after service configuration is complete

        // Export endpoints
        public async Task<IActionResult> ExportExcel()
        {
            try
            {
                var exportService = new ExportService(_context);
                var excelData = await exportService.ExportDashboardDataToExcel();

                // Trigger real-time update
                await BroadcastDashboardUpdate();

                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    $"Dashboard_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting to Excel: {ex.Message}");
                return BadRequest("Failed to export dashboard data to Excel");
            }
        }

        public async Task<IActionResult> ExportPdf()
        {
            try
            {
                var exportService = new ExportService(_context);
                var pdfData = await exportService.ExportDashboardDataToPdf();

                // Trigger real-time update
                await BroadcastDashboardUpdate();

                return File(pdfData, "application/pdf", 
                    $"Dashboard_Export_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting to PDF: {ex.Message}");
                return BadRequest("Failed to export dashboard data to PDF");
            }
        }

        private string GetStatusColor(EquipmentStatus status)
        {
            return status switch
            {
                EquipmentStatus.Active => "#10b981",      // Green
                EquipmentStatus.Inactive => "#ef4444",    // Red
                EquipmentStatus.Retired => "#6b7280",     // Gray
                _ => "#3b82f6"                            // Blue (default)
            };
        }

        private string GetPriorityColor(AlertPriority priority)
        {
            return priority switch
            {
                AlertPriority.Low => "#10b981",     // Green
                AlertPriority.Medium => "#f59e0b",  // Yellow
                AlertPriority.High => "#ef4444",   // Red
                _ => "#3b82f6"                      // Blue (default)
            };
        }

        // Helper method to get filtered dashboard data
        private async Task<DashboardViewModel> GetFilteredDashboardData(DashboardFilterViewModel? filters)
        {
            filters ??= new DashboardFilterViewModel();
            // Base queries
            var equipmentQuery = _context.Equipment.AsQueryable();
            var alertQuery = _context.Alerts.AsQueryable();
            var maintenanceTaskQuery = _context.MaintenanceTasks.AsQueryable();
            var inventoryQuery = _context.InventoryItems.AsQueryable();

            // Count totals before filtering
            var totalEquipmentBefore = await equipmentQuery.CountAsync();
            var totalAlertsBefore = await alertQuery.CountAsync();
            var totalMaintenanceBefore = await maintenanceTaskQuery.CountAsync();

            // Apply date filters
            if (filters.DateFrom.HasValue)
            {
                alertQuery = alertQuery.Where(a => a.CreatedDate >= filters.DateFrom.Value);
                maintenanceTaskQuery = maintenanceTaskQuery.Where(m => m.ScheduledDate >= filters.DateFrom.Value);
            }

            if (filters.DateTo.HasValue)
            {
                var dateTo = filters.DateTo.Value.Date.AddDays(1);
                alertQuery = alertQuery.Where(a => a.CreatedDate < dateTo);
                maintenanceTaskQuery = maintenanceTaskQuery.Where(m => m.ScheduledDate < dateTo);
            }

            // Apply equipment status filters
            if (filters.EquipmentStatuses?.Any() == true)
            {
                equipmentQuery = equipmentQuery.Where(e => filters.EquipmentStatuses.Contains(e.Status));
            }

            // Apply building filters
            if (filters.BuildingIds?.Any() == true)
            {
                equipmentQuery = equipmentQuery.Where(e => filters.BuildingIds.Contains(e.BuildingId));
                alertQuery = alertQuery.Where(a => a.Equipment != null && filters.BuildingIds.Contains(a.Equipment.BuildingId));
                maintenanceTaskQuery = maintenanceTaskQuery.Where(m => m.Equipment != null && filters.BuildingIds.Contains(m.Equipment.BuildingId));
            }

            // Apply equipment type filters
            if (filters.EquipmentTypeIds?.Any() == true)
            {
                equipmentQuery = equipmentQuery.Where(e => e.EquipmentModel != null && filters.EquipmentTypeIds.Contains(e.EquipmentModel.EquipmentTypeId));
                alertQuery = alertQuery.Where(a => a.Equipment != null && a.Equipment.EquipmentModel != null && filters.EquipmentTypeIds.Contains(a.Equipment.EquipmentModel.EquipmentTypeId));
                maintenanceTaskQuery = maintenanceTaskQuery.Where(m => m.Equipment != null && m.Equipment.EquipmentModel != null && filters.EquipmentTypeIds.Contains(m.Equipment.EquipmentModel.EquipmentTypeId));
            }

            // Apply alert priority filters
            if (filters.AlertPriorities?.Any() == true)
            {
                alertQuery = alertQuery.Where(a => filters.AlertPriorities.Contains(a.Priority));
            }

            // Apply maintenance status filters
            if (filters.MaintenanceStatuses?.Any() == true)
            {
                maintenanceTaskQuery = maintenanceTaskQuery.Where(m => filters.MaintenanceStatuses.Contains(m.Status));
            }

            // Apply user filters
            if (filters.UserIds?.Any() == true)
            {
                alertQuery = alertQuery.Where(a => a.AssignedToUserId != null && filters.UserIds.Contains(a.AssignedToUserId));
                maintenanceTaskQuery = maintenanceTaskQuery.Where(m => m.AssignedToUserId != null && filters.UserIds.Contains(m.AssignedToUserId));
            }

            // Apply search term
            if (!string.IsNullOrEmpty(filters.SearchTerm))
            {
                var searchTerm = filters.SearchTerm.ToLower();
                // Search in equipment model names since Equipment doesn't have Name property
                equipmentQuery = equipmentQuery.Where(e => 
                    (e.EquipmentModel != null && e.EquipmentModel.ModelName.ToLower().Contains(searchTerm)));
                
                alertQuery = alertQuery.Where(a => 
                    (a.Title != null && a.Title.ToLower().Contains(searchTerm)) ||
                    a.Description.ToLower().Contains(searchTerm));
                
                maintenanceTaskQuery = maintenanceTaskQuery.Where(m => 
                    m.Description.ToLower().Contains(searchTerm));
            }

            // Apply critical filter
            if (filters.ShowOnlyCritical)
            {
                equipmentQuery = equipmentQuery.Where(e => e.Status == EquipmentStatus.Inactive || e.Status == EquipmentStatus.Retired);
                alertQuery = alertQuery.Where(a => a.Priority == AlertPriority.High);
                maintenanceTaskQuery = maintenanceTaskQuery.Where(m => m.ScheduledDate < DateTime.Now && m.Status != MaintenanceStatus.Completed);
            }

            // Apply completed filter
            if (!filters.IncludeCompleted)
            {
                alertQuery = alertQuery.Where(a => a.Status != AlertStatus.Closed);
                maintenanceTaskQuery = maintenanceTaskQuery.Where(m => m.Status != MaintenanceStatus.Completed);
            }

            // Calculate metrics based on filtered data
            var criticalAlerts = await alertQuery
                .Where(a => a.Priority == AlertPriority.High && a.Status == AlertStatus.Open)
                .CountAsync();

            var overdueMaintenances = await maintenanceTaskQuery
                .Where(m => m.ScheduledDate < DateTime.Now && m.Status != MaintenanceStatus.Completed)
                .CountAsync();

            var equipmentNeedingAttention = await equipmentQuery
                .Where(e => e.Status == EquipmentStatus.Inactive || e.Status == EquipmentStatus.Retired)
                .CountAsync();

            // Enhanced workflow status calculations
            var activeAlerts = await alertQuery
                .Where(a => a.Status == AlertStatus.Open)
                .CountAsync();

            var pendingTasks = await maintenanceTaskQuery
                .Where(m => m.Status == MaintenanceStatus.Pending)
                .CountAsync();

            var inProgressTasks = await maintenanceTaskQuery
                .Where(m => m.Status == MaintenanceStatus.InProgress)
                .CountAsync();

            var resolvedToday = await _context.Alerts
                .Where(a => a.Status == AlertStatus.Resolved && a.CreatedDate.Date == DateTime.Today)
                .CountAsync();

            var autoGeneratedTasks = await maintenanceTaskQuery
                .Where(m => m.CreatedFromAlertId != null)
                .CountAsync();

            // Get data for display based on filtered queries
            var criticalEquipment = await equipmentQuery
                .Include(e => e.EquipmentModel)
                .Include(e => e.Building)
                .Include(e => e.Room)
                .Where(e => e.Status == EquipmentStatus.Inactive)
                .Take(3)
                .ToListAsync();

            var recentAlerts = await alertQuery
                .Include(a => a.Equipment)
                    .ThenInclude(e => e!.EquipmentModel)
                .Include(a => a.AssignedTo)
                .OrderByDescending(a => a.CreatedDate)
                .Take(5)
                .ToListAsync();

            var upcomingMaintenances = await maintenanceTaskQuery
                .Include(m => m.Equipment)
                    .ThenInclude(e => e!.EquipmentModel)
                .Include(m => m.AssignedTo)
                .Where(m => m.Status != MaintenanceStatus.Completed)
                .OrderBy(m => m.ScheduledDate)
                .Take(5)
                .ToListAsync();

            var equipmentStatus = await equipmentQuery
                .GroupBy(e => e.Status)
                .Select(g => new EquipmentStatusCount { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            // Count totals after filtering
            var totalEquipmentAfter = await equipmentQuery.CountAsync();
            var totalAlertsAfter = await alertQuery.CountAsync();
            var totalMaintenanceAfter = await maintenanceTaskQuery.CountAsync();

            // Build filtered analytics
            var filteredAnalytics = new Dictionary<string, object>
            {
                ["FilteredEquipment"] = totalEquipmentAfter,
                ["FilteredAlerts"] = totalAlertsAfter,
                ["FilteredMaintenance"] = totalMaintenanceAfter,
                ["FilterReductionPercent"] = totalEquipmentBefore > 0 ? Math.Round(((double)(totalEquipmentBefore - totalEquipmentAfter) / totalEquipmentBefore) * 100, 1) : 0
            };

            // Generate suggested actions
            var suggestedActions = GenerateBasicSuggestedActions(criticalAlerts, overdueMaintenances, equipmentNeedingAttention);

            return new DashboardViewModel
            {
                TotalEquipment = totalEquipmentAfter,
                ActiveMaintenanceTasks = totalMaintenanceAfter,
                LowStockItems = await inventoryQuery
                    .Where(i => i.InventoryStocks != null && i.InventoryStocks.Sum(s => s.Quantity) <= i.MinStockLevel)
                    .CountAsync(),
                CriticalAlerts = criticalAlerts,
                OverdueMaintenances = overdueMaintenances,
                EquipmentNeedingAttention = equipmentNeedingAttention,
                
                // Enhanced workflow status
                ActiveAlerts = activeAlerts,
                PendingTasks = pendingTasks,
                InProgressTasks = inProgressTasks,
                ResolvedToday = resolvedToday,
                AutoGeneratedTasks = autoGeneratedTasks,
                
                RecentAlerts = recentAlerts,
                UpcomingMaintenances = upcomingMaintenances,
                CriticalEquipment = criticalEquipment,
                SuggestedActions = suggestedActions,
                EquipmentStatus = equipmentStatus,
                TotalRecordsBeforeFilter = totalEquipmentBefore,
                TotalRecordsAfterFilter = totalEquipmentAfter,
                FilteredAnalytics = filteredAnalytics
            };
        }

        // API endpoint for real-time filtered dashboard data
        [HttpPost]
        public async Task<IActionResult> ApplyDashboardFilters([FromForm] DashboardFilterViewModel? filters = null)
        {
            try
            {
                filters ??= new DashboardFilterViewModel();
                
                var filteredData = await GetFilteredDashboardData(filters);
                
                return Json(new
                {
                    success = true,
                    data = new
                    {
                        totalEquipment = filteredData.TotalEquipment,
                        activeMaintenanceTasks = filteredData.ActiveMaintenanceTasks,
                        lowStockItems = filteredData.LowStockItems,
                        criticalAlerts = filteredData.CriticalAlerts,
                        overdueMaintenances = filteredData.OverdueMaintenances,
                        equipmentNeedingAttention = filteredData.EquipmentNeedingAttention,
                        totalRecordsBeforeFilter = filteredData.TotalRecordsBeforeFilter,
                        totalRecordsAfterFilter = filteredData.TotalRecordsAfterFilter,
                        filteredAnalytics = filteredData.FilteredAnalytics,
                        recentAlerts = filteredData.RecentAlerts?.Select(a => new
                        {
                            id = a.AlertId,
                            title = a.Title,
                            description = a.Description,
                            priority = a.Priority.ToString(),
                            status = a.Status.ToString(),
                            createdDate = a.CreatedDate,
                            equipmentName = a.Equipment?.EquipmentModel?.ModelName ?? "Unknown",
                            assignedTo = a.AssignedTo?.UserName ?? "Unassigned"
                        }),
                        upcomingMaintenances = filteredData.UpcomingMaintenances?.Select(m => new
                        {
                            id = m.TaskId,
                            description = m.Description,
                            scheduledDate = m.ScheduledDate,
                            status = m.Status.ToString(),
                            equipmentName = m.Equipment?.EquipmentModel?.ModelName ?? "Unknown",
                            assignedTo = m.AssignedTo?.UserName ?? "Unassigned"
                        }),
                        criticalEquipment = filteredData.CriticalEquipment?.Select(e => new
                        {
                            id = e.EquipmentId,
                            name = e.EquipmentModel?.ModelName ?? "Unknown",
                            status = e.Status.ToString(),
                            building = e.Building?.BuildingName ?? "Unknown",
                            room = e.Room?.RoomName ?? "Unknown"
                        }),
                        equipmentStatus = filteredData.EquipmentStatus
                    },
                    filters = filters,
                    filterSummary = new
                    {
                        hasActiveFilters = filters.HasActiveFilters(),
                        filterCount = CountActiveFilters(filters),
                        dateRange = filters.DateFrom.HasValue && filters.DateTo.HasValue 
                            ? $"{filters.DateFrom:yyyy-MM-dd} to {filters.DateTo:yyyy-MM-dd}"
                            : null,
                        searchTerm = filters.SearchTerm,
                        criticalOnly = filters.ShowOnlyCritical,
                        includeCompleted = filters.IncludeCompleted
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    details = ex.StackTrace
                });
            }
        }

        private int CountActiveFilters(DashboardFilterViewModel filters)
        {
            int count = 0;
            if (filters.DateFrom.HasValue || filters.DateTo.HasValue) count++;
            if (filters.EquipmentStatuses?.Any() == true) count++;
            if (filters.BuildingIds?.Any() == true) count++;
            if (filters.EquipmentTypeIds?.Any() == true) count++;
            if (filters.AlertPriorities?.Any() == true) count++;
            if (filters.MaintenanceStatuses?.Any() == true) count++;
            if (filters.UserIds?.Any() == true) count++;
            if (!string.IsNullOrEmpty(filters.SearchTerm)) count++;
            if (filters.ShowOnlyCritical) count++;
            if (!filters.IncludeCompleted) count++;
            return count;
        }

        // API endpoint for saving dashboard views
        [HttpPost]
        public async Task<IActionResult> SaveDashboardView([FromForm] string viewName, [FromForm] bool isPublic = false)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { success = false, error = "User not authenticated" });
                }

                // Get filter parameters from the form
                var filters = new DashboardFilterViewModel();
                if (Request.Form.ContainsKey("DateFrom") && DateTime.TryParse(Request.Form["DateFrom"], out var dateFrom))
                    filters.DateFrom = dateFrom;
                if (Request.Form.ContainsKey("DateTo") && DateTime.TryParse(Request.Form["DateTo"], out var dateTo))
                    filters.DateTo = dateTo;
                if (Request.Form.ContainsKey("SearchTerm"))
                    filters.SearchTerm = Request.Form["SearchTerm"];
                if (Request.Form.ContainsKey("ShowOnlyCritical"))
                    filters.ShowOnlyCritical = Request.Form["ShowOnlyCritical"] == "true";
                if (Request.Form.ContainsKey("IncludeCompleted"))
                    filters.IncludeCompleted = Request.Form["IncludeCompleted"] == "true";
                if (Request.Form.ContainsKey("EquipmentStatuses"))
                {
                    var statusStrings = Request.Form["EquipmentStatuses"].ToArray();
                    filters.EquipmentStatuses = statusStrings
                        .Where(s => !string.IsNullOrEmpty(s) && Enum.TryParse<EquipmentStatus>(s, out _))
                        .Select(s => Enum.Parse<EquipmentStatus>(s!))
                        .ToList();
                }

                // Check if view name already exists for this user
                var existingView = await _context.SavedDashboardViews
                    .FirstOrDefaultAsync(v => v.Name == viewName && v.UserId == currentUser.Id);

                if (existingView != null)
                {
                    // Update existing view
                    existingView.FilterData = JsonSerializer.Serialize(filters);
                    existingView.IsPublic = isPublic;
                    existingView.UpdatedAt = DateTime.Now;
                }
                else
                {
                    // Create new view
                    var savedView = new SavedDashboardView
                    {
                        Name = viewName,
                        UserId = currentUser.Id,
                        FilterData = JsonSerializer.Serialize(filters),
                        IsPublic = isPublic,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _context.SavedDashboardViews.Add(savedView);
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "View saved successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // API endpoint for getting saved views
        [HttpGet]
        public async Task<IActionResult> GetSavedViews()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new List<object>());
                }

                var savedViews = await _context.SavedDashboardViews
                    .Where(v => v.UserId == currentUser.Id || v.IsPublic)
                    .OrderBy(v => v.Name)
                    .Select(v => new
                    {
                        id = v.Id,
                        name = v.Name,
                        isPublic = v.IsPublic,
                        createdDate = v.CreatedAt,
                        isOwner = v.UserId == currentUser.Id
                    })
                    .ToListAsync();

                return Json(savedViews);
            }
            catch (Exception)
            {
                return Json(new List<object>());
            }
        }

        // API endpoint for loading a saved view
        [HttpGet]
        public async Task<IActionResult> LoadSavedView(int id)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { success = false, error = "User not authenticated" });
                }

                var savedView = await _context.SavedDashboardViews
                    .FirstOrDefaultAsync(v => v.Id == id && (v.UserId == currentUser.Id || v.IsPublic));

                if (savedView == null)
                {
                    return Json(new { success = false, error = "View not found or access denied" });
                }

                var filters = JsonSerializer.Deserialize<DashboardFilterViewModel>(savedView.FilterData ?? "{}");

                return Json(new { 
                    success = true, 
                    filters = filters,
                    viewName = savedView.Name 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // API endpoint for deleting a saved view
        [HttpDelete]
        public async Task<IActionResult> DeleteSavedView(int id)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { success = false, error = "User not authenticated" });
                }

                var savedView = await _context.SavedDashboardViews
                    .FirstOrDefaultAsync(v => v.Id == id && v.UserId == currentUser.Id);

                if (savedView == null)
                {
                    return Json(new { success = false, error = "View not found or access denied" });
                }

                _context.SavedDashboardViews.Remove(savedView);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "View deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // Advanced Analytics Endpoints for Step 9

        [HttpGet]
        public async Task<IActionResult> GetEquipmentPerformanceMetrics([FromQuery] DashboardFilterViewModel? filters = null)
        {
            try
            {
                filters ??= new DashboardFilterViewModel();
                var metrics = await _analyticsService.GetEquipmentPerformanceMetricsAsync(filters);
                return Json(new { success = true, data = metrics });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPredictiveAnalytics([FromQuery] DashboardFilterViewModel? filters = null)
        {
            try
            {
                filters ??= new DashboardFilterViewModel();
                var analytics = await _analyticsService.GetPredictiveAnalyticsAsync(filters);
                return Json(new { success = true, data = analytics });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetKPIProgressIndicators([FromQuery] DashboardFilterViewModel? filters = null)
        {
            try
            {
                filters ??= new DashboardFilterViewModel();
                var kpis = await _analyticsService.GetKPIProgressIndicatorsAsync(filters);
                return Json(new { success = true, data = kpis });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetEquipmentHeatmapData([FromQuery] DashboardFilterViewModel? filters = null)
        {
            try
            {
                filters ??= new DashboardFilterViewModel();
                var heatmapData = await _analyticsService.GetEquipmentHeatmapDataAsync(filters);
                return Json(new { success = true, data = heatmapData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMaintenanceTrendAnalysis([FromQuery] DashboardFilterViewModel? filters = null, [FromQuery] int days = 30)
        {
            try
            {
                filters ??= new DashboardFilterViewModel();
                var trendData = await _analyticsService.GetMaintenanceTrendAnalysisAsync(filters, days);
                return Json(new { success = true, data = trendData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCostAnalysisData([FromQuery] DashboardFilterViewModel? filters = null, [FromQuery] int months = 6)
        {
            try
            {
                filters ??= new DashboardFilterViewModel();
                var costData = await _analyticsService.GetCostAnalysisDataAsync(filters, months);
                return Json(new { success = true, data = costData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRealtimeMetrics()
        {
            try
            {
                var metrics = await _analyticsService.GetRealtimeMetricsAsync();
                return Json(new { success = true, data = metrics });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAdvancedAnalyticsSummary([FromQuery] DashboardFilterViewModel? filters = null)
        {
            try
            {
                filters ??= new DashboardFilterViewModel();
                
                // Get comprehensive analytics data
                var performanceMetrics = await _analyticsService.GetEquipmentPerformanceMetricsAsync(filters);
                var predictiveAnalytics = await _analyticsService.GetPredictiveAnalyticsAsync(filters);
                var kpis = await _analyticsService.GetKPIProgressIndicatorsAsync(filters);
                var heatmapData = await _analyticsService.GetEquipmentHeatmapDataAsync(filters);
                var trendData = await _analyticsService.GetMaintenanceTrendAnalysisAsync(filters, 30);
                var costData = await _analyticsService.GetCostAnalysisDataAsync(filters, 6);

                var summary = new
                {
                    PerformanceMetrics = performanceMetrics,
                    PredictiveAnalytics = predictiveAnalytics,
                    KPIs = kpis,
                    HeatmapData = heatmapData,
                    TrendData = trendData,
                    CostData = costData,
                    GeneratedAt = DateTime.Now
                };

                return Json(new { success = true, data = summary });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // Real-time analytics endpoint for SignalR updates
        [HttpPost]
        public async Task<IActionResult> TriggerAnalyticsUpdate()
        {
            try
            {
                var realtimeMetrics = await _analyticsService.GetRealtimeMetricsAsync();
                
                if (_hubContext != null)
                {
                    await _hubContext.Clients.Group("Dashboard").SendAsync("AnalyticsUpdate", realtimeMetrics);
                }

                return Json(new { success = true, message = "Analytics update triggered" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}