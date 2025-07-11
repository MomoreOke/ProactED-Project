using Microsoft.AspNetCore.Mvc;
using FEENALOoFINALE.Models;
using FEENALOoFINALE.Models.ViewModels;
using System.Linq;
using FEENALOoFINALE.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using FEENALOoFINALE.Services;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;

namespace FEENALOoFINALE.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAdvancedAnalyticsService _analyticsService;
        private readonly IExportService _exportService;
        private readonly UserManager<User> _userManager;

        public ReportController(ApplicationDbContext context, IAdvancedAnalyticsService analyticsService, IExportService exportService, UserManager<User> userManager)
        {
            _context = context;
            _analyticsService = analyticsService;
            _exportService = exportService;
            _userManager = userManager;
        }

        // Enhanced Reports Dashboard
        public async Task<IActionResult> Enhanced(ReportFilters? filters = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserId = currentUser?.Id ?? "";

            var viewModel = new EnhancedReportDashboardViewModel
            {
                UserId = currentUserId
            };

            // Apply filters
            filters ??= new ReportFilters();
            viewModel.Filters = filters;

            // Build statistics
            viewModel.Statistics = await BuildReportStatistics(filters);

            // Build chart data
            viewModel.ChartData = await BuildReportChartData(filters);

            // Build recent reports
            viewModel.RecentReports = BuildRecentReports(filters);

            // Build scheduled reports
            viewModel.ScheduledReports = BuildScheduledReports(currentUserId);

            // Build quick report options
            viewModel.QuickReports = BuildQuickReportOptions();

            // Build export options
            viewModel.ExportOptions = BuildExportOptions();

            return View(viewModel);
        }

        // Report Builder
        public IActionResult Builder(int? templateId = null)
        {
            var viewModel = new ReportBuilderViewModel
            {
                PageTitle = "Report Builder",
                PageDescription = "Create custom reports with advanced filtering and analytics"
            };

            // Load template if specified
            if (templateId.HasValue)
            {
                LoadReportTemplate(viewModel, templateId.Value);
            }

            // Build available fields
            viewModel.AvailableFields = BuildAvailableFields();

            return View(viewModel);
        }

        // Generate Report
        [HttpPost]
        public async Task<IActionResult> Generate([FromBody] ReportBuilderViewModel model)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                
                // Generate the report
                var reportData = GenerateReportData(model);
                
                // Save report metadata
                var reportId = SaveReportMetadata(model, currentUser?.Id ?? "", reportData);
                
                return Json(new { 
                    success = true, 
                    reportId = reportId,
                    message = "Report generated successfully" 
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = $"Error generating report: {ex.Message}" 
                });
            }
        }

        // Export Report
        public async Task<IActionResult> Export(int reportId, string format = "pdf")
        {
            try
            {
                var reportData = GetReportData(reportId);
                if (reportData == null)
                {
                    return NotFound("Report not found");
                }

                var exportResult = await _exportService.ExportReportAsync(reportData, format);
                
                return File(exportResult.Data, exportResult.ContentType, exportResult.FileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error exporting report: {ex.Message}";
                return RedirectToAction("Enhanced");
            }
        }

        // Schedule Report
        [HttpPost]
        public async Task<IActionResult> Schedule([FromBody] ScheduledReportViewModel model)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                
                // Save scheduled report
                await SaveScheduledReport(model, currentUser?.Id ?? "");
                
                return Json(new { 
                    success = true, 
                    message = "Report scheduled successfully" 
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = $"Error scheduling report: {ex.Message}" 
                });
            }
        }

        // Legacy methods maintained for backward compatibility
        public IActionResult Index()
        {
            return RedirectToAction("Enhanced");
        }

        // Inventory Report
        public async Task<IActionResult> Inventory()
        {
            var items = await _context.InventoryItems
                .Include(i => i.InventoryStocks)
                .ToListAsync();
            return View(items);
        }

        // Alerts Report
        public async Task<IActionResult> Alerts()
        {
            var alerts = await _context.Alerts
                .Include(a => a.Equipment)
                    .ThenInclude(e => e!.EquipmentModel)
                .Include(a => a.AssignedTo)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
            return View(alerts);
        }

        // Maintenance Logs Report
        public async Task<IActionResult> MaintenanceLogs()
        {
            var logs = await _context.MaintenanceLogs
                .Include(m => m.Equipment)
                    .ThenInclude(e => e!.EquipmentModel)
                .OrderByDescending(m => m.LogDate)
                .ToListAsync();
            return View(logs);
        }

        // Helper Methods
        private async Task<ReportStatistics> BuildReportStatistics(ReportFilters filters)
        {
            var totalReports = await _context.MaintenanceLogs.CountAsync();
            var reportsThisMonth = await _context.MaintenanceLogs
                .Where(m => m.LogDate >= DateTime.Now.AddDays(-30))
                .CountAsync();

            return new ReportStatistics
            {
                TotalReports = totalReports,
                ReportsThisMonth = reportsThisMonth,
                ScheduledReports = 0, // Would need a ScheduledReports table
                AutomatedReports = 0,
                AverageReportGenerationTime = 2.5m,
                TotalReportDownloads = 1250,
                ReportsShared = 85,
                LastReportGenerated = DateTime.Now.AddHours(-2)
            };
        }

        private async Task<ReportChartData> BuildReportChartData(ReportFilters filters)
        {
            var chartData = new ReportChartData();

            // Equipment Uptime Chart
            var equipmentUptime = await _context.Equipment
                .GroupBy(e => e.Status)
                .Select(g => new ChartDataPoint
                {
                    Label = g.Key.ToString(),
                    Value = g.Count(),
                    Color = g.Key == EquipmentStatus.Active ? "#28a745" :
                           g.Key == EquipmentStatus.Inactive ? "#dc3545" : "#6c757d"
                })
                .ToListAsync();

            chartData.EquipmentUptimeChart = equipmentUptime;

            // Maintenance Cost Trends (last 12 months)
            var maintenanceCosts = new List<ChartDataPoint>();
            for (int i = 11; i >= 0; i--)
            {
                var date = DateTime.Now.AddMonths(-i);
                var cost = await _context.MaintenanceLogs
                    .Where(m => m.LogDate.Year == date.Year && m.LogDate.Month == date.Month)
                    .SumAsync(m => m.Cost);

                maintenanceCosts.Add(new ChartDataPoint
                {
                    Label = date.ToString("MMM yyyy"),
                    Value = cost,
                    Date = date,
                    Color = "#007bff"
                });
            }
            chartData.MaintenanceCostTrends = maintenanceCosts;

            // Alert Frequency Chart
            var alertFrequency = new List<ChartDataPoint>();
            var alertGroups = await _context.Alerts
                .GroupBy(a => a.Priority)
                .Select(g => new { Priority = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var group in alertGroups)
            {
                string color = group.Priority switch
                {
                    AlertPriority.High => "#dc3545",
                    AlertPriority.Medium => "#ffc107",
                    AlertPriority.Low => "#28a745",
                    _ => "#6c757d"
                };

                alertFrequency.Add(new ChartDataPoint
                {
                    Label = group.Priority.ToString(),
                    Value = group.Count,
                    Color = color
                });
            }

            chartData.AlertFrequencyChart = alertFrequency;

            return chartData;
        }

        private List<ReportItemViewModel> BuildRecentReports(ReportFilters filters)
        {
            // For demo purposes, returning mock data
            // In a real implementation, this would query a Reports table
            var reports = new List<ReportItemViewModel>
            {
                new ReportItemViewModel
                {
                    Id = 1,
                    Name = "Monthly Equipment Performance",
                    Description = "Comprehensive equipment performance analysis",
                    Type = ReportType.EquipmentPerformance,
                    GeneratedDate = DateTime.Now.AddHours(-2),
                    GeneratedBy = "System Admin",
                    Status = ReportStatus.Completed,
                    FileSizeBytes = 2048576,
                    FileFormat = "PDF",
                    CanDelete = true,
                    CanShare = true,
                    DownloadCount = 15
                },
                new ReportItemViewModel
                {
                    Id = 2,
                    Name = "Maintenance Cost Analysis",
                    Description = "Detailed breakdown of maintenance expenses",
                    Type = ReportType.MaintenanceCosts,
                    GeneratedDate = DateTime.Now.AddDays(-1),
                    GeneratedBy = "John Doe",
                    Status = ReportStatus.Completed,
                    FileSizeBytes = 1536000,
                    FileFormat = "Excel",
                    CanDelete = true,
                    CanShare = true,
                    DownloadCount = 8
                }
            };

            return reports;
        }

        private List<ScheduledReportViewModel> BuildScheduledReports(string userId)
        {
            // Mock data for scheduled reports
            var scheduledReports = new List<ScheduledReportViewModel>
            {
                new ScheduledReportViewModel
                {
                    Id = 1,
                    Name = "Weekly Equipment Status",
                    Description = "Weekly summary of all equipment status",
                    Type = ReportType.EquipmentPerformance,
                    Frequency = ReportFrequency.Weekly,
                    NextRunDate = DateTime.Now.AddDays(2),
                    LastRunDate = DateTime.Now.AddDays(-5),
                    IsActive = true,
                    CreatedBy = "System Admin",
                    Recipients = new List<string> { "admin@company.com", "maintenance@company.com" },
                    CanEdit = true,
                    CanDelete = true
                }
            };

            return scheduledReports;
        }

        private List<QuickReportOption> BuildQuickReportOptions()
        {
            return new List<QuickReportOption>
            {
                new QuickReportOption
                {
                    Id = "equipment-performance",
                    Name = "Equipment Performance",
                    Description = "Quick overview of equipment performance metrics",
                    Icon = "bi-speedometer2",
                    Color = "primary",
                    Type = ReportType.EquipmentPerformance,
                    Controller = "Report",
                    Action = "GenerateQuick",
                    EstimatedDuration = 30
                },
                new QuickReportOption
                {
                    Id = "maintenance-costs",
                    Name = "Maintenance Costs",
                    Description = "Current month maintenance cost summary",
                    Icon = "bi-currency-dollar",
                    Color = "success",
                    Type = ReportType.MaintenanceCosts,
                    Controller = "Report",
                    Action = "GenerateQuick",
                    EstimatedDuration = 45
                },
                new QuickReportOption
                {
                    Id = "alert-summary",
                    Name = "Alert Summary",
                    Description = "Active alerts and resolution status",
                    Icon = "bi-exclamation-triangle",
                    Color = "warning",
                    Type = ReportType.AlertSummary,
                    Controller = "Report",
                    Action = "GenerateQuick",
                    EstimatedDuration = 20
                },
                new QuickReportOption
                {
                    Id = "inventory-levels",
                    Name = "Inventory Levels",
                    Description = "Current inventory stock levels and alerts",
                    Icon = "bi-boxes",
                    Color = "info",
                    Type = ReportType.InventoryLevels,
                    Controller = "Report",
                    Action = "GenerateQuick",
                    EstimatedDuration = 25
                }
            };
        }

        private List<ExportOption> BuildExportOptions()
        {
            return new List<ExportOption>
            {
                new ExportOption
                {
                    Format = "pdf",
                    DisplayName = "PDF Document",
                    Icon = "bi-file-pdf",
                    MimeType = "application/pdf",
                    Description = "High-quality PDF format suitable for printing and sharing",
                    RequiresConfiguration = false,
                    SupportedReportTypes = new List<string> { "All" }
                },
                new ExportOption
                {
                    Format = "excel",
                    DisplayName = "Excel Spreadsheet",
                    Icon = "bi-file-excel",
                    MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    Description = "Excel format for data analysis and manipulation",
                    RequiresConfiguration = false,
                    SupportedReportTypes = new List<string> { "All" }
                },
                new ExportOption
                {
                    Format = "csv",
                    DisplayName = "CSV File",
                    Icon = "bi-file-text",
                    MimeType = "text/csv",
                    Description = "Comma-separated values for data import/export",
                    RequiresConfiguration = false,
                    SupportedReportTypes = new List<string> { "EquipmentPerformance", "MaintenanceCosts", "InventoryLevels" }
                },
                new ExportOption
                {
                    Format = "json",
                    DisplayName = "JSON Data",
                    Icon = "bi-code-square",
                    MimeType = "application/json",
                    Description = "JSON format for API integration and web applications",
                    RequiresConfiguration = false,
                    SupportedReportTypes = new List<string> { "All" }
                }
            };
        }

        private void LoadReportTemplate(ReportBuilderViewModel viewModel, int templateId)
        {
            // Mock implementation - in reality, this would load from database
            viewModel.ReportName = $"Template Report {templateId}";
            viewModel.Description = "Report generated from template";
            viewModel.Type = ReportType.EquipmentPerformance;
        }

        private List<ReportField> BuildAvailableFields()
        {
            return new List<ReportField>
            {
                new ReportField { FieldName = "EquipmentName", DisplayName = "Equipment Name", DataType = "string", IsRequired = true },
                new ReportField { FieldName = "EquipmentType", DisplayName = "Equipment Type", DataType = "string", IsRequired = false },
                new ReportField { FieldName = "Location", DisplayName = "Location", DataType = "string", IsRequired = false },
                new ReportField { FieldName = "Status", DisplayName = "Status", DataType = "enum", IsRequired = false },
                new ReportField { FieldName = "LastMaintenanceDate", DisplayName = "Last Maintenance", DataType = "datetime", IsRequired = false },
                new ReportField { FieldName = "TotalCost", DisplayName = "Total Cost", DataType = "decimal", IsRequired = false, AggregationType = ReportAggregationType.Sum },
                new ReportField { FieldName = "Uptime", DisplayName = "Uptime %", DataType = "decimal", IsRequired = false, AggregationType = ReportAggregationType.Average }
            };
        }

        private object GenerateReportData(ReportBuilderViewModel model)
        {
            // Mock implementation - would generate actual report data based on model configuration
            return new { Message = "Report data generated", Timestamp = DateTime.Now };
        }

        private int SaveReportMetadata(ReportBuilderViewModel model, string userId, object reportData)
        {
            // Mock implementation - would save to database
            return new Random().Next(1000, 9999);
        }

        private object GetReportData(int reportId)
        {
            // Mock implementation - would retrieve from database
            return new { ReportId = reportId, Data = "Sample report data" };
        }

        private async Task SaveScheduledReport(ScheduledReportViewModel model, string userId)
        {
            // Mock implementation - would save to database
            await Task.CompletedTask;
        }
    }
}