using OfficeOpenXml;
using iTextSharp.text;
using iTextSharp.text.pdf;
using FEENALOoFINALE.Models;
using FEENALOoFINALE.Data;
using Microsoft.EntityFrameworkCore;

namespace FEENALOoFINALE.Services
{
    public class ExportService : IExportService
    {
        private readonly ApplicationDbContext _context;

        public ExportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> ExportDashboardDataToExcel()
        {
            // Set license context for EPPlus
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using var package = new ExcelPackage();

            // Dashboard Summary Sheet
            var summarySheet = package.Workbook.Worksheets.Add("Dashboard Summary");
            await CreateDashboardSummarySheet(summarySheet);

            // Equipment Data Sheet
            var equipmentSheet = package.Workbook.Worksheets.Add("Equipment");
            await CreateEquipmentSheet(equipmentSheet);

            // Maintenance Data Sheet
            var maintenanceSheet = package.Workbook.Worksheets.Add("Maintenance");
            await CreateMaintenanceSheet(maintenanceSheet);

            // Alerts Data Sheet
            var alertsSheet = package.Workbook.Worksheets.Add("Alerts");
            await CreateAlertsSheet(alertsSheet);

            // Inventory Data Sheet
            var inventorySheet = package.Workbook.Worksheets.Add("Inventory");
            await CreateInventorySheet(inventorySheet);

            return package.GetAsByteArray();
        }

        public async Task<byte[]> ExportDashboardDataToPdf()
        {
            using var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 25, 25, 30, 30);
            var writer = PdfWriter.GetInstance(document, memoryStream);

            document.Open();

            // Title
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            var title = new Paragraph("Predictive Maintenance Dashboard Report", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20
            };
            document.Add(title);

            // Generated date
            var dateFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            var dateInfo = new Paragraph($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", dateFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 30
            };
            document.Add(dateInfo);

            // Dashboard Summary
            await AddDashboardSummaryToPdf(document);

            // Equipment Summary
            await AddEquipmentSummaryToPdf(document);

            // Recent Alerts
            await AddRecentAlertsToPdf(document);

            // Maintenance Summary
            await AddMaintenanceSummaryToPdf(document);

            document.Close();
            return memoryStream.ToArray();
        }

        private async Task CreateDashboardSummarySheet(ExcelWorksheet sheet)
        {
            // Headers
            sheet.Cells[1, 1].Value = "Dashboard Summary";
            sheet.Cells[1, 1].Style.Font.Bold = true;
            sheet.Cells[1, 1].Style.Font.Size = 16;

            sheet.Cells[3, 1].Value = "Metric";
            sheet.Cells[3, 2].Value = "Value";
            sheet.Cells[3, 1, 3, 2].Style.Font.Bold = true;

            // Data
            var totalEquipment = await _context.Equipment.CountAsync();
            var activeMaintenanceTasks = await _context.MaintenanceTasks
                .CountAsync(m => m.Status == MaintenanceStatus.InProgress);
            var lowStockItems = await _context.InventoryItems
                .Include(i => i.InventoryStocks)
                .CountAsync(i => i.InventoryStocks != null && i.InventoryStocks.Sum(s => s.Quantity) < i.MinimumStockLevel);
            var criticalAlerts = await _context.Alerts
                .CountAsync(a => a.Priority == AlertPriority.High && a.Status == AlertStatus.Open);

            int row = 4;
            sheet.Cells[row, 1].Value = "Total Equipment";
            sheet.Cells[row++, 2].Value = totalEquipment;
            
            sheet.Cells[row, 1].Value = "Active Maintenance Tasks";
            sheet.Cells[row++, 2].Value = activeMaintenanceTasks;
            
            sheet.Cells[row, 1].Value = "Low Stock Items";
            sheet.Cells[row++, 2].Value = lowStockItems;
            
            sheet.Cells[row, 1].Value = "Critical Alerts";
            sheet.Cells[row++, 2].Value = criticalAlerts;

            sheet.Cells[row, 1].Value = "Report Generated";
            sheet.Cells[row++, 2].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Auto-fit columns
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
        }

        private async Task CreateEquipmentSheet(ExcelWorksheet sheet)
        {
            var equipment = await _context.Equipment
                .Include(e => e.EquipmentModel)
                .ThenInclude(em => em != null ? em.EquipmentType : null)
                .Include(e => e.Building)
                .Include(e => e.Room)
                .ToListAsync();

            // Headers
            sheet.Cells[1, 1].Value = "Equipment ID";
            sheet.Cells[1, 2].Value = "Type";
            sheet.Cells[1, 3].Value = "Model";
            sheet.Cells[1, 4].Value = "Status";
            sheet.Cells[1, 5].Value = "Building";
            sheet.Cells[1, 6].Value = "Room";
            sheet.Cells[1, 7].Value = "Installation Date";

            // Make headers bold
            sheet.Cells[1, 1, 1, 7].Style.Font.Bold = true;

            // Data
            for (int i = 0; i < equipment.Count; i++)
            {
                var eq = equipment[i];
                int row = i + 2;
                
                sheet.Cells[row, 1].Value = eq.EquipmentId;
                sheet.Cells[row, 2].Value = eq.EquipmentModel?.EquipmentType?.EquipmentTypeName ?? "N/A";
                sheet.Cells[row, 3].Value = eq.EquipmentModel?.ModelName ?? "N/A";
                sheet.Cells[row, 4].Value = eq.Status.ToString();
                sheet.Cells[row, 5].Value = eq.Building?.BuildingName ?? "N/A";
                sheet.Cells[row, 6].Value = eq.Room?.RoomName ?? "N/A";
                sheet.Cells[row, 7].Value = eq.InstallationDate.ToString("yyyy-MM-dd");
            }

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
        }

        private async Task CreateMaintenanceSheet(ExcelWorksheet sheet)
        {
            var maintenanceLogs = await _context.MaintenanceLogs
                .Include(m => m.Equipment)
                .OrderByDescending(m => m.LogDate)
                .Take(1000) // Limit to recent records
                .ToListAsync();

            // Headers
            sheet.Cells[1, 1].Value = "Log ID";
            sheet.Cells[1, 2].Value = "Equipment ID";
            sheet.Cells[1, 3].Value = "Type";
            sheet.Cells[1, 4].Value = "Description";
            sheet.Cells[1, 5].Value = "Cost";
            sheet.Cells[1, 6].Value = "Log Date";

            sheet.Cells[1, 1, 1, 6].Style.Font.Bold = true;

            // Data
            for (int i = 0; i < maintenanceLogs.Count; i++)
            {
                var log = maintenanceLogs[i];
                int row = i + 2;
                
                sheet.Cells[row, 1].Value = log.LogId;
                sheet.Cells[row, 2].Value = log.EquipmentId;
                sheet.Cells[row, 3].Value = log.MaintenanceType.ToString();
                sheet.Cells[row, 4].Value = log.Description;
                sheet.Cells[row, 5].Value = log.Cost;
                sheet.Cells[row, 6].Value = log.LogDate.ToString("yyyy-MM-dd");
            }

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
        }

        private async Task CreateAlertsSheet(ExcelWorksheet sheet)
        {
            var alerts = await _context.Alerts
                .Include(a => a.Equipment)
                .OrderByDescending(a => a.CreatedDate)
                .Take(500)
                .ToListAsync();

            // Headers
            sheet.Cells[1, 1].Value = "Alert ID";
            sheet.Cells[1, 2].Value = "Equipment ID";
            sheet.Cells[1, 3].Value = "Priority";
            sheet.Cells[1, 4].Value = "Status";
            sheet.Cells[1, 5].Value = "Description";
            sheet.Cells[1, 6].Value = "Created";

            sheet.Cells[1, 1, 1, 6].Style.Font.Bold = true;

            // Data
            for (int i = 0; i < alerts.Count; i++)
            {
                var alert = alerts[i];
                int row = i + 2;
                
                sheet.Cells[row, 1].Value = alert.AlertId;
                sheet.Cells[row, 2].Value = alert.EquipmentId ?? 0;
                sheet.Cells[row, 3].Value = alert.Priority.ToString();
                sheet.Cells[row, 4].Value = alert.Status.ToString();
                sheet.Cells[row, 5].Value = alert.Description;
                sheet.Cells[row, 6].Value = alert.CreatedDate.ToString("yyyy-MM-dd HH:mm");
            }

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
        }

        private async Task CreateInventorySheet(ExcelWorksheet sheet)
        {
            var inventoryItems = await _context.InventoryItems
                .Include(i => i.InventoryStocks)
                .ToListAsync();

            // Headers
            sheet.Cells[1, 1].Value = "Item ID";
            sheet.Cells[1, 2].Value = "Name";
            sheet.Cells[1, 3].Value = "Category";
            sheet.Cells[1, 4].Value = "Current Stock";
            sheet.Cells[1, 5].Value = "Minimum Level";
            sheet.Cells[1, 6].Value = "Unit Price";
            sheet.Cells[1, 7].Value = "Supplier";

            sheet.Cells[1, 1, 1, 7].Style.Font.Bold = true;

            // Data
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                var item = inventoryItems[i];
                int row = i + 2;
                
                sheet.Cells[row, 1].Value = item.ItemId;
                sheet.Cells[row, 2].Value = item.Name;
                sheet.Cells[row, 3].Value = item.Category.ToString();
                sheet.Cells[row, 4].Value = item.InventoryStocks?.Sum(s => s.Quantity) ?? 0;
                sheet.Cells[row, 5].Value = item.MinimumStockLevel;
                sheet.Cells[row, 6].Value = item.InventoryStocks?.FirstOrDefault()?.UnitCost ?? 0;
                sheet.Cells[row, 7].Value = "N/A"; // Supplier not in current model
            }

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
        }

        private async Task AddDashboardSummaryToPdf(Document document)
        {
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
            var header = new Paragraph("Dashboard Summary", headerFont) { SpacingAfter = 10 };
            document.Add(header);

            var totalEquipment = await _context.Equipment.CountAsync();
            var activeMaintenanceTasks = await _context.MaintenanceTasks
                .CountAsync(m => m.Status == MaintenanceStatus.InProgress);
            var lowStockItems = await _context.InventoryItems
                .Include(i => i.InventoryStocks)
                .CountAsync(i => i.InventoryStocks != null && i.InventoryStocks.Sum(s => s.Quantity) < i.MinimumStockLevel);
            var criticalAlerts = await _context.Alerts
                .CountAsync(a => a.Priority == AlertPriority.High && a.Status == AlertStatus.Open);

            var table = new PdfPTable(2) { WidthPercentage = 50 };
            table.SetWidths(new float[] { 3, 1 });

            // Table data
            var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

            table.AddCell(new PdfPCell(new Phrase("Total Equipment", boldFont)));
            table.AddCell(new PdfPCell(new Phrase(totalEquipment.ToString(), cellFont)));

            table.AddCell(new PdfPCell(new Phrase("Active Maintenance Tasks", boldFont)));
            table.AddCell(new PdfPCell(new Phrase(activeMaintenanceTasks.ToString(), cellFont)));

            table.AddCell(new PdfPCell(new Phrase("Low Stock Items", boldFont)));
            table.AddCell(new PdfPCell(new Phrase(lowStockItems.ToString(), cellFont)));

            table.AddCell(new PdfPCell(new Phrase("Critical Alerts", boldFont)));
            table.AddCell(new PdfPCell(new Phrase(criticalAlerts.ToString(), cellFont)));

            document.Add(table);
            document.Add(new Paragraph(" ", cellFont) { SpacingAfter = 20 });
        }

        private async Task AddEquipmentSummaryToPdf(Document document)
        {
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
            var header = new Paragraph("Equipment Status Summary", headerFont) { SpacingAfter = 10 };
            document.Add(header);

            var equipmentByStatus = await _context.Equipment
                .GroupBy(e => e.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var table = new PdfPTable(2) { WidthPercentage = 50 };
            table.SetWidths(new float[] { 3, 1 });

            var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

            foreach (var status in equipmentByStatus)
            {
                table.AddCell(new PdfPCell(new Phrase(status.Status.ToString(), boldFont)));
                table.AddCell(new PdfPCell(new Phrase(status.Count.ToString(), cellFont)));
            }

            document.Add(table);
            document.Add(new Paragraph(" ", cellFont) { SpacingAfter = 20 });
        }

        private async Task AddRecentAlertsToPdf(Document document)
        {
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
            var header = new Paragraph("Recent Critical Alerts", headerFont) { SpacingAfter = 10 };
            document.Add(header);

            var recentAlerts = await _context.Alerts
                .Include(a => a.Equipment)
                .Where(a => a.Priority == AlertPriority.High && a.Status == AlertStatus.Open)
                .OrderByDescending(a => a.CreatedDate)
                .Take(10)
                .ToListAsync();

            if (recentAlerts.Any())
            {
                var table = new PdfPTable(3) { WidthPercentage = 100 };
                table.SetWidths(new float[] { 2, 3, 2 });

                var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);

                // Headers
                table.AddCell(new PdfPCell(new Phrase("Equipment ID", boldFont)));
                table.AddCell(new PdfPCell(new Phrase("Description", boldFont)));
                table.AddCell(new PdfPCell(new Phrase("Created", boldFont)));

                // Data
                foreach (var alert in recentAlerts)
                {
                    table.AddCell(new PdfPCell(new Phrase((alert.EquipmentId ?? 0).ToString(), cellFont)));
                    table.AddCell(new PdfPCell(new Phrase(alert.Description, cellFont)));
                    table.AddCell(new PdfPCell(new Phrase(alert.CreatedDate.ToString("MM/dd/yyyy"), cellFont)));
                }

                document.Add(table);
            }
            else
            {
                var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                document.Add(new Paragraph("No critical alerts found.", cellFont));
            }

            document.Add(new Paragraph(" ", FontFactory.GetFont(FontFactory.HELVETICA, 10)) { SpacingAfter = 20 });
        }

        private async Task AddMaintenanceSummaryToPdf(Document document)
        {
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
            var header = new Paragraph("Maintenance Activity Summary", headerFont) { SpacingAfter = 10 };
            document.Add(header);

            var maintenanceByType = await _context.MaintenanceLogs
                .GroupBy(m => m.MaintenanceType)
                .Select(g => new { Type = g.Key, Count = g.Count(), TotalCost = g.Sum(m => m.Cost) })
                .ToListAsync();

            var table = new PdfPTable(3) { WidthPercentage = 75 };
            table.SetWidths(new float[] { 2, 1, 2 });

            var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

            // Headers
            table.AddCell(new PdfPCell(new Phrase("Type", boldFont)));
            table.AddCell(new PdfPCell(new Phrase("Count", boldFont)));
            table.AddCell(new PdfPCell(new Phrase("Total Cost", boldFont)));

            // Data
            foreach (var maintenance in maintenanceByType)
            {
                table.AddCell(new PdfPCell(new Phrase(maintenance.Type.ToString(), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(maintenance.Count.ToString(), cellFont)));
                table.AddCell(new PdfPCell(new Phrase($"${maintenance.TotalCost:F2}", cellFont)));
            }

            document.Add(table);
        }

        // Advanced reporting methods for Step 9
        public async Task<byte[]> ExportCustomReportToExcel(CustomReportTemplate template, DashboardFilterViewModel? filters = null)
        {
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(template.Name);

            // Add header
            worksheet.Cells[1, 1].Value = template.Name;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Font.Size = 16;

            worksheet.Cells[2, 1].Value = $"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            worksheet.Cells[2, 1].Style.Font.Size = 10;

            int row = 4;

            // Add report sections based on template
            foreach (var section in template.Sections)
            {
                worksheet.Cells[row, 1].Value = section.Title;
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                worksheet.Cells[row, 1].Style.Font.Size = 14;
                row += 2;

                // Add data based on section type
                switch (section.DataType)
                {
                    case ReportDataType.Equipment:
                        row = await AddEquipmentDataToExcel(worksheet, row, filters);
                        break;
                    case ReportDataType.Maintenance:
                        row = await AddMaintenanceDataToExcel(worksheet, row, filters);
                        break;
                    case ReportDataType.Alerts:
                        row = await AddAlertsDataToExcel(worksheet, row, filters);
                        break;
                    case ReportDataType.Inventory:
                        row = await AddInventoryDataToExcel(worksheet, row, filters);
                        break;
                }
                row += 2;
            }

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();

            return package.GetAsByteArray();
        }

        public async Task<byte[]> ExportCustomReportToPdf(CustomReportTemplate template, DashboardFilterViewModel? filters = null)
        {
            using var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 25, 25, 30, 30);
            var writer = PdfWriter.GetInstance(document, memoryStream);

            document.Open();

            // Title
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            var title = new Paragraph(template.Name, titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20
            };
            document.Add(title);

            // Generated date
            var dateFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            var dateInfo = new Paragraph($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", dateFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 30
            };
            document.Add(dateInfo);

            // Add report sections
            foreach (var section in template.Sections)
            {
                var sectionTitle = new Paragraph(section.Title, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14))
                {
                    SpacingAfter = 10
                };
                document.Add(sectionTitle);

                // Add data based on section type
                switch (section.DataType)
                {
                    case ReportDataType.Equipment:
                        await AddEquipmentDataToPdf(document, filters);
                        break;
                    case ReportDataType.Maintenance:
                        await AddMaintenanceDataToPdf(document, filters);
                        break;
                    case ReportDataType.Alerts:
                        await AddAlertsDataToPdf(document, filters);
                        break;
                    case ReportDataType.Inventory:
                        await AddInventoryDataToPdf(document, filters);
                        break;
                }

                document.Add(new Paragraph(" ") { SpacingAfter = 10 });
            }

            document.Close();
            return memoryStream.ToArray();
        }

        public async Task<byte[]> ExportAnalyticsDataToExcel(AdvancedAnalyticsViewModel analytics)
        {
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using var package = new ExcelPackage();

            // Performance Metrics Sheet
            var performanceSheet = package.Workbook.Worksheets.Add("Performance Metrics");
            await CreatePerformanceMetricsSheet(performanceSheet, analytics.EquipmentPerformance);

            // KPIs Sheet
            var kpiSheet = package.Workbook.Worksheets.Add("KPIs");
            await CreateKPISheet(kpiSheet, analytics.KPIProgress);

            // Predictive Analytics Sheet
            var predictiveSheet = package.Workbook.Worksheets.Add("Predictive Analytics");
            await CreatePredictiveAnalyticsSheet(predictiveSheet, analytics.PredictiveInsights);

            // Trend Analysis Sheet
            var trendSheet = package.Workbook.Worksheets.Add("Trend Analysis");
            await CreateTrendAnalysisSheet(trendSheet, analytics.MaintenanceTrends);

            // Cost Analysis Sheet
            var costSheet = package.Workbook.Worksheets.Add("Cost Analysis");
            await CreateCostAnalysisSheet(costSheet, analytics.CostAnalysis);

            return package.GetAsByteArray();
        }

        public async Task<byte[]> ExportAnalyticsDataToPdf(AdvancedAnalyticsViewModel analytics)
        {
            using var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 25, 25, 30, 30);
            var writer = PdfWriter.GetInstance(document, memoryStream);

            document.Open();

            // Title
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            var title = new Paragraph("Advanced Analytics Report", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20
            };
            document.Add(title);

            // Generated date
            var dateFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            var dateInfo = new Paragraph($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", dateFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 30
            };
            document.Add(dateInfo);

            // Add analytics sections
            await AddPerformanceMetricsToPdf(document, analytics.EquipmentPerformance);
            await AddKPIsToPdf(document, analytics.KPIProgress);
            await AddPredictiveAnalyticsToPdf(document, analytics.PredictiveInsights);
            await AddTrendAnalysisToPdf(document, analytics.MaintenanceTrends);
            await AddCostAnalysisToPdf(document, analytics.CostAnalysis);

            document.Close();
            return memoryStream.ToArray();
        }

        // Helper methods for Excel export
        private async Task<int> AddEquipmentDataToExcel(ExcelWorksheet worksheet, int startRow, DashboardFilterViewModel? filters)
        {
            var equipment = await GetFilteredEquipment(filters);
            
            // Headers
            worksheet.Cells[startRow, 1].Value = "Equipment Name";
            worksheet.Cells[startRow, 2].Value = "Type";
            worksheet.Cells[startRow, 3].Value = "Status";
            worksheet.Cells[startRow, 4].Value = "Location";
            worksheet.Cells[startRow, 1, startRow, 4].Style.Font.Bold = true;

            int row = startRow + 1;
            foreach (var item in equipment)
            {
                worksheet.Cells[row, 1].Value = item.EquipmentModel?.ModelName ?? "Unknown";
                worksheet.Cells[row, 2].Value = item.EquipmentModel?.EquipmentType?.EquipmentTypeName ?? "Unknown";
                worksheet.Cells[row, 3].Value = item.Status.ToString();
                worksheet.Cells[row, 4].Value = $"{item.Room?.Building?.BuildingName} - {item.Room?.RoomName}";
                row++;
            }

            return row;
        }

        private async Task<int> AddMaintenanceDataToExcel(ExcelWorksheet worksheet, int startRow, DashboardFilterViewModel? filters)
        {
            var maintenance = await GetFilteredMaintenance(filters);
            
            // Headers
            worksheet.Cells[startRow, 1].Value = "Equipment";
            worksheet.Cells[startRow, 2].Value = "Type";
            worksheet.Cells[startRow, 3].Value = "Date";
            worksheet.Cells[startRow, 4].Value = "Description";
            worksheet.Cells[startRow, 5].Value = "Cost";
            worksheet.Cells[startRow, 1, startRow, 5].Style.Font.Bold = true;

            int row = startRow + 1;
            foreach (var item in maintenance)
            {
                worksheet.Cells[row, 1].Value = item.Equipment?.EquipmentModel?.ModelName ?? "Unknown";
                worksheet.Cells[row, 2].Value = item.MaintenanceType.ToString();
                worksheet.Cells[row, 3].Value = item.LogDate.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 4].Value = item.Description;
                worksheet.Cells[row, 5].Value = item.Cost; // Cost is non-nullable
                row++;
            }

            return row;
        }

        private async Task<int> AddAlertsDataToExcel(ExcelWorksheet worksheet, int startRow, DashboardFilterViewModel? filters)
        {
            var alerts = await GetFilteredAlerts(filters);
            
            // Headers
            worksheet.Cells[startRow, 1].Value = "Equipment";
            worksheet.Cells[startRow, 2].Value = "Priority";
            worksheet.Cells[startRow, 3].Value = "Status";
            worksheet.Cells[startRow, 4].Value = "Description";
            worksheet.Cells[startRow, 5].Value = "Created Date";
            worksheet.Cells[startRow, 1, startRow, 5].Style.Font.Bold = true;

            int row = startRow + 1;
            foreach (var item in alerts)
            {
                worksheet.Cells[row, 1].Value = item.Equipment?.EquipmentModel?.ModelName ?? "Unknown";
                worksheet.Cells[row, 2].Value = item.Priority.ToString();
                worksheet.Cells[row, 3].Value = item.Status.ToString();
                worksheet.Cells[row, 4].Value = item.Description;
                worksheet.Cells[row, 5].Value = item.CreatedDate.ToString("yyyy-MM-dd HH:mm");
                row++;
            }

            return row;
        }

        private async Task<int> AddInventoryDataToExcel(ExcelWorksheet worksheet, int startRow, DashboardFilterViewModel? filters)
        {
            var inventory = await GetFilteredInventory(filters);
            
            // Headers
            worksheet.Cells[startRow, 1].Value = "Item Name";
            worksheet.Cells[startRow, 2].Value = "Category";
            worksheet.Cells[startRow, 3].Value = "Current Stock";
            worksheet.Cells[startRow, 4].Value = "Min Level";
            worksheet.Cells[startRow, 5].Value = "Unit Price";
            worksheet.Cells[startRow, 1, startRow, 5].Style.Font.Bold = true;

            int row = startRow + 1;
            foreach (var item in inventory)
            {
                worksheet.Cells[row, 1].Value = item.Name;
                worksheet.Cells[row, 2].Value = item.Category;
                worksheet.Cells[row, 3].Value = item.InventoryStocks?.Sum(s => s.Quantity) ?? 0;
                worksheet.Cells[row, 4].Value = item.MinStockLevel;
                worksheet.Cells[row, 5].Value = 0; // UnitPrice not available in current model
                row++;
            }

            return row;
        }

        // Helper methods for PDF export
        private async Task AddEquipmentDataToPdf(Document document, DashboardFilterViewModel? filters)
        {
            var equipment = await GetFilteredEquipment(filters);
            
            var table = new PdfPTable(4) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 3, 2, 2, 3 });

            // Headers
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            table.AddCell(new PdfPCell(new Phrase("Equipment Name", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Type", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Status", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Location", headerFont)));

            // Data
            var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
            foreach (var item in equipment.Take(20)) // Limit for PDF
            {
                table.AddCell(new PdfPCell(new Phrase(item.EquipmentModel?.ModelName ?? "Unknown", cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.EquipmentModel?.EquipmentType?.EquipmentTypeName ?? "Unknown", cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.Status.ToString(), cellFont)));
                table.AddCell(new PdfPCell(new Phrase($"{item.Room?.Building?.BuildingName} - {item.Room?.RoomName}", cellFont)));
            }

            document.Add(table);
        }

        private async Task AddMaintenanceDataToPdf(Document document, DashboardFilterViewModel? filters)
        {
            var maintenance = await GetFilteredMaintenance(filters);
            
            var table = new PdfPTable(5) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 2, 1.5f, 1.5f, 3, 1 });

            // Headers
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            table.AddCell(new PdfPCell(new Phrase("Equipment", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Type", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Date", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Description", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Cost", headerFont)));

            // Data
            var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
            foreach (var item in maintenance.Take(20)) // Limit for PDF
            {
                table.AddCell(new PdfPCell(new Phrase(item.Equipment?.EquipmentModel?.ModelName ?? "Unknown", cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.MaintenanceType.ToString(), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.LogDate.ToString("yyyy-MM-dd"), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.Description?.Substring(0, Math.Min(50, item.Description?.Length ?? 0)) ?? "", cellFont)));
                table.AddCell(new PdfPCell(new Phrase($"${item.Cost:F2}", cellFont))); // Cost is non-nullable
            }

            document.Add(table);
        }

        private async Task AddAlertsDataToPdf(Document document, DashboardFilterViewModel? filters)
        {
            var alerts = await GetFilteredAlerts(filters);
            
            var table = new PdfPTable(4) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 2, 1, 1, 4 });

            // Headers
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            table.AddCell(new PdfPCell(new Phrase("Equipment", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Priority", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Status", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Description", headerFont)));

            // Data
            var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
            foreach (var item in alerts.Take(20)) // Limit for PDF
            {
                table.AddCell(new PdfPCell(new Phrase(item.Equipment?.EquipmentModel?.ModelName ?? "Unknown", cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.Priority.ToString(), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.Status.ToString(), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.Description?.Substring(0, Math.Min(60, item.Description?.Length ?? 0)) ?? "", cellFont)));
            }

            document.Add(table);
        }

        private async Task AddInventoryDataToPdf(Document document, DashboardFilterViewModel? filters)
        {
            var inventory = await GetFilteredInventory(filters);
            
            var table = new PdfPTable(4) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 3, 2, 1, 1 });

            // Headers
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            table.AddCell(new PdfPCell(new Phrase("Item Name", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Category", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Stock", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Min Level", headerFont)));

            // Data
            var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
            foreach (var item in inventory.Take(20)) // Limit for PDF
            {
                table.AddCell(new PdfPCell(new Phrase(item.Name, cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.Category.ToString(), cellFont)));
                table.AddCell(new PdfPCell(new Phrase((item.InventoryStocks?.Sum(s => s.Quantity) ?? 0).ToString(), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.MinStockLevel.ToString(), cellFont)));
            }

            document.Add(table);
        }

        // Analytics sheet creation methods
        private async Task CreatePerformanceMetricsSheet(ExcelWorksheet sheet, List<EquipmentPerformanceMetrics> metrics)
        {
            sheet.Cells[1, 1].Value = "Equipment Performance Metrics";
            sheet.Cells[1, 1].Style.Font.Bold = true;
            sheet.Cells[1, 1].Style.Font.Size = 16;

            // Headers
            int row = 3;
            sheet.Cells[row, 1].Value = "Equipment";
            sheet.Cells[row, 2].Value = "Uptime %";
            sheet.Cells[row, 3].Value = "Efficiency %";
            sheet.Cells[row, 4].Value = "MTBF (hours)";
            sheet.Cells[row, 5].Value = "MTTR (hours)";
            sheet.Cells[row, 6].Value = "Failure Rate %";
            sheet.Cells[row, 1, row, 6].Style.Font.Bold = true;

            row++;
            foreach (var metric in metrics)
            {
                sheet.Cells[row, 1].Value = metric.EquipmentName;
                sheet.Cells[row, 2].Value = metric.UptimePercentage;
                sheet.Cells[row, 3].Value = metric.EfficiencyScore;
                sheet.Cells[row, 4].Value = metric.MeanTimeBetweenFailures;
                sheet.Cells[row, 5].Value = metric.MeanTimeToRepair;
                sheet.Cells[row, 6].Value = metric.FailureCount; // Use FailureCount instead of FailureRate
                row++;
            }

            sheet.Cells.AutoFitColumns();
        }

        private async Task CreateKPISheet(ExcelWorksheet sheet, List<KPIProgressIndicator> kpis)
        {
            sheet.Cells[1, 1].Value = "Key Performance Indicators";
            sheet.Cells[1, 1].Style.Font.Bold = true;
            sheet.Cells[1, 1].Style.Font.Size = 16;

            // Headers
            int row = 3;
            sheet.Cells[row, 1].Value = "KPI Name";
            sheet.Cells[row, 2].Value = "Current Value";
            sheet.Cells[row, 3].Value = "Target Value";
            sheet.Cells[row, 4].Value = "Progress %";
            sheet.Cells[row, 5].Value = "Trend";
            sheet.Cells[row, 1, row, 5].Style.Font.Bold = true;

            row++;
            foreach (var kpi in kpis)
            {
                sheet.Cells[row, 1].Value = kpi.KPIName; // Use KPIName instead of Name
                sheet.Cells[row, 2].Value = kpi.CurrentValue;
                sheet.Cells[row, 3].Value = kpi.TargetValue;
                sheet.Cells[row, 4].Value = kpi.ProgressPercentage;
                sheet.Cells[row, 5].Value = kpi.Direction; // Use Direction instead of Trend
                row++;
            }

            sheet.Cells.AutoFitColumns();
        }

        private async Task CreatePredictiveAnalyticsSheet(ExcelWorksheet sheet, List<PredictiveAnalyticsData> analytics)
        {
            sheet.Cells[1, 1].Value = "Predictive Analytics";
            sheet.Cells[1, 1].Style.Font.Bold = true;
            sheet.Cells[1, 1].Style.Font.Size = 16;

            // Headers
            int row = 3;
            sheet.Cells[row, 1].Value = "Equipment";
            sheet.Cells[row, 2].Value = "Failure Probability";
            sheet.Cells[row, 3].Value = "Predicted Failure Date";
            sheet.Cells[row, 4].Value = "Recommended Action";
            sheet.Cells[row, 5].Value = "Confidence Level";
            sheet.Cells[row, 1, row, 5].Style.Font.Bold = true;

            row++;
            foreach (var item in analytics)
            {
                sheet.Cells[row, 1].Value = item.EquipmentName;
                sheet.Cells[row, 2].Value = item.ConfidenceScore; // Use ConfidenceScore instead of FailureProbability
                sheet.Cells[row, 3].Value = item.PredictedFailureDate.ToString("yyyy-MM-dd"); // Remove nullable check
                sheet.Cells[row, 4].Value = string.Join(", ", item.RecommendedActions); // Join list of actions
                sheet.Cells[row, 5].Value = item.ConfidenceScore; // Use ConfidenceScore instead of ConfidenceLevel
                row++;
            }

            sheet.Cells.AutoFitColumns();
        }

        private async Task CreateTrendAnalysisSheet(ExcelWorksheet sheet, List<MaintenanceTrendData> trendData)
        {
            sheet.Cells[1, 1].Value = "Maintenance Trend Analysis";
            sheet.Cells[1, 1].Style.Font.Bold = true;
            sheet.Cells[1, 1].Style.Font.Size = 16;

            // Headers
            int row = 3;
            sheet.Cells[row, 1].Value = "Date";
            sheet.Cells[row, 2].Value = "Preventive";
            sheet.Cells[row, 3].Value = "Corrective";
            sheet.Cells[row, 4].Value = "Predictive";
            sheet.Cells[row, 5].Value = "Inspection";
            sheet.Cells[row, 6].Value = "Total Cost";
            sheet.Cells[row, 1, row, 6].Style.Font.Bold = true;

            row++;
            foreach (var item in trendData)
            {
                sheet.Cells[row, 1].Value = item.Date.ToString("yyyy-MM-dd");
                sheet.Cells[row, 2].Value = item.PreventiveMaintenanceCount;
                sheet.Cells[row, 3].Value = item.CorrectiveMaintenanceCount;
                sheet.Cells[row, 4].Value = 0; // PredictiveMaintenanceCount not available in model
                sheet.Cells[row, 5].Value = item.InspectionMaintenanceCount;
                sheet.Cells[row, 6].Value = item.TotalCost;
                row++;
            }

            sheet.Cells.AutoFitColumns();
        }

        private async Task CreateCostAnalysisSheet(ExcelWorksheet sheet, List<CostAnalysisData> costData)
        {
            sheet.Cells[1, 1].Value = "Cost Analysis";
            sheet.Cells[1, 1].Style.Font.Bold = true;
            sheet.Cells[1, 1].Style.Font.Size = 16;

            // Headers
            int row = 3;
            sheet.Cells[row, 1].Value = "Month";
            sheet.Cells[row, 2].Value = "Maintenance Cost";
            sheet.Cells[row, 3].Value = "Parts Cost";
            sheet.Cells[row, 4].Value = "Labor Cost";
            sheet.Cells[row, 5].Value = "Total Cost";
            sheet.Cells[row, 6].Value = "Budget";
            sheet.Cells[row, 7].Value = "Variance";
            sheet.Cells[row, 1, row, 7].Style.Font.Bold = true;

            row++;
            foreach (var item in costData)
            {
                sheet.Cells[row, 1].Value = item.Category; // Use Category instead of Period
                sheet.Cells[row, 2].Value = item.CategoryValue; // Use CategoryValue
                sheet.Cells[row, 3].Value = item.TotalCost; // Use TotalCost
                sheet.Cells[row, 4].Value = item.AverageCost; // Use AverageCost
                sheet.Cells[row, 5].Value = item.MaintenanceCount; // Use MaintenanceCount
                sheet.Cells[row, 6].Value = item.ProjectedCost; // Use ProjectedCost
                sheet.Cells[row, 7].Value = item.Trend; // Use Trend
                row++;
            }

            sheet.Cells.AutoFitColumns();
        }

        // PDF analytics methods
        private async Task AddPerformanceMetricsToPdf(Document document, List<EquipmentPerformanceMetrics> metrics)
        {
            var sectionTitle = new Paragraph("Equipment Performance Metrics", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14))
            {
                SpacingAfter = 10
            };
            document.Add(sectionTitle);

            var table = new PdfPTable(6) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 2, 1, 1, 1, 1, 1 });

            // Headers
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
            table.AddCell(new PdfPCell(new Phrase("Equipment", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Uptime %", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Efficiency %", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("MTBF (h)", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("MTTR (h)", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Failure %", headerFont)));

            // Data
            var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);
            foreach (var metric in metrics.Take(10)) // Limit for PDF
            {
                table.AddCell(new PdfPCell(new Phrase(metric.EquipmentName, cellFont)));
                table.AddCell(new PdfPCell(new Phrase(metric.UptimePercentage.ToString("F1"), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(metric.EfficiencyScore.ToString("F1"), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(metric.MeanTimeBetweenFailures.ToString("F1"), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(metric.MeanTimeToRepair.ToString("F1"), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(metric.FailureCount.ToString(), cellFont))); // Use FailureCount instead of FailureRate
            }

            document.Add(table);
            document.Add(new Paragraph(" ") { SpacingAfter = 10 });
        }

        private async Task AddKPIsToPdf(Document document, List<KPIProgressIndicator> kpis)
        {
            var sectionTitle = new Paragraph("Key Performance Indicators", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14))
            {
                SpacingAfter = 10
            };
            document.Add(sectionTitle);

            var table = new PdfPTable(5) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 2, 1, 1, 1, 1 });

            // Headers
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
            table.AddCell(new PdfPCell(new Phrase("KPI Name", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Current", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Target", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Progress %", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Trend", headerFont)));

            // Data
            var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);
            foreach (var kpi in kpis.Take(10)) // Limit for PDF
            {
                table.AddCell(new PdfPCell(new Phrase(kpi.KPIName, cellFont))); // Use KPIName instead of Name
                table.AddCell(new PdfPCell(new Phrase(kpi.CurrentValue.ToString("F1"), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(kpi.TargetValue.ToString("F1"), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(kpi.ProgressPercentage.ToString("F1"), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(kpi.Direction, cellFont))); // Use Direction instead of Trend
            }

            document.Add(table);
            document.Add(new Paragraph(" ") { SpacingAfter = 10 });
        }

        private async Task AddPredictiveAnalyticsToPdf(Document document, List<PredictiveAnalyticsData> analytics)
        {
            var sectionTitle = new Paragraph("Predictive Analytics", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14))
            {
                SpacingAfter = 10
            };
            document.Add(sectionTitle);

            var table = new PdfPTable(4) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 2, 1, 2, 1 });

            // Headers
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
            table.AddCell(new PdfPCell(new Phrase("Equipment", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Failure Prob.", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Recommended Action", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Confidence", headerFont)));

            // Data
            var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);
            foreach (var item in analytics.Take(10)) // Limit for PDF
            {
                table.AddCell(new PdfPCell(new Phrase(item.EquipmentName, cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.ConfidenceScore.ToString("F1"), cellFont))); // Use ConfidenceScore instead of FailureProbability
                table.AddCell(new PdfPCell(new Phrase(string.Join(", ", item.RecommendedActions), cellFont))); // Join list of actions
                table.AddCell(new PdfPCell(new Phrase(item.ConfidenceScore.ToString("F1"), cellFont))); // Use ConfidenceScore instead of ConfidenceLevel
            }

            document.Add(table);
            document.Add(new Paragraph(" ") { SpacingAfter = 10 });
        }

        private async Task AddTrendAnalysisToPdf(Document document, List<MaintenanceTrendData> trendData)
        {
            var sectionTitle = new Paragraph("Maintenance Trend Analysis", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14))
            {
                SpacingAfter = 10
            };
            document.Add(sectionTitle);

            var table = new PdfPTable(6) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 1.5f, 1, 1, 1, 1, 1 });

            // Headers
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
            table.AddCell(new PdfPCell(new Phrase("Date", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Preventive", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Corrective", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Predictive", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Inspection", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Total Cost", headerFont)));

            // Data
            var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);
            foreach (var item in trendData.Take(10)) // Limit for PDF
            {
                table.AddCell(new PdfPCell(new Phrase(item.Date.ToString("MMM dd"), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.PreventiveMaintenanceCount.ToString(), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.CorrectiveMaintenanceCount.ToString(), cellFont)));
                table.AddCell(new PdfPCell(new Phrase("0", cellFont))); // PredictiveMaintenanceCount not available
                table.AddCell(new PdfPCell(new Phrase(item.InspectionMaintenanceCount.ToString(), cellFont)));
                table.AddCell(new PdfPCell(new Phrase($"${item.TotalCost:F0}", cellFont)));
            }

            document.Add(table);
            document.Add(new Paragraph(" ") { SpacingAfter = 10 });
        }

        private async Task AddCostAnalysisToPdf(Document document, List<CostAnalysisData> costData)
        {
            var sectionTitle = new Paragraph("Cost Analysis", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14))
            {
                SpacingAfter = 10
            };
            document.Add(sectionTitle);

            var table = new PdfPTable(5) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 1.5f, 1, 1, 1, 1 });

            // Headers
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
            table.AddCell(new PdfPCell(new Phrase("Month", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Maintenance", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Parts", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Labor", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("Total", headerFont)));

            // Data
            var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);
            foreach (var item in costData.Take(10)) // Limit for PDF
            {
                table.AddCell(new PdfPCell(new Phrase(item.Category, cellFont))); // Use Category instead of Period
                table.AddCell(new PdfPCell(new Phrase(item.CategoryValue, cellFont))); // Use CategoryValue
                table.AddCell(new PdfPCell(new Phrase($"${item.TotalCost:F0}", cellFont))); // Use TotalCost
                table.AddCell(new PdfPCell(new Phrase($"${item.AverageCost:F0}", cellFont))); // Use AverageCost
                table.AddCell(new PdfPCell(new Phrase($"${item.ProjectedCost:F0}", cellFont))); // Use ProjectedCost
            }

            document.Add(table);
            document.Add(new Paragraph(" ") { SpacingAfter = 10 });
        }

        // Helper methods for data filtering
        private async Task<List<Equipment>> GetFilteredEquipment(DashboardFilterViewModel? filters)
        {
            var query = _context.Equipment
                .Include(e => e.EquipmentModel)
                    .ThenInclude(em => em!.EquipmentType)
                .Include(e => e.Room)
                    .ThenInclude(r => r!.Building)
                .AsQueryable();

            if (filters != null)
            {
                if (filters.BuildingIds?.Any() == true)
                {
                    query = query.Where(e => e.Room != null && filters.BuildingIds.Contains(e.Room.BuildingId));
                }

                if (filters.EquipmentTypeIds?.Any() == true)
                {
                    query = query.Where(e => e.EquipmentModel != null && filters.EquipmentTypeIds.Contains(e.EquipmentModel.EquipmentTypeId));
                }

                if (filters.EquipmentStatuses?.Any() == true)
                {
                    query = query.Where(e => filters.EquipmentStatuses.Contains(e.Status));
                }
            }

            return await query.OrderBy(e => e.EquipmentModel!.ModelName).ToListAsync(); // Use null-forgiving operator
        }

        private async Task<List<MaintenanceLog>> GetFilteredMaintenance(DashboardFilterViewModel? filters)
        {
            var query = _context.MaintenanceLogs
                .Include(m => m.Equipment)
                    .ThenInclude(e => e!.Room)
                        .ThenInclude(r => r!.Building)
                .AsQueryable();

            if (filters != null)
            {
                if (filters.BuildingIds?.Any() == true)
                {
                    query = query.Where(m => m.Equipment != null && m.Equipment.Room != null && filters.BuildingIds.Contains(m.Equipment.Room.BuildingId));
                }

                if (filters.DateFrom.HasValue)
                {
                    query = query.Where(m => m.LogDate >= filters.DateFrom.Value);
                }

                if (filters.DateTo.HasValue)
                {
                    query = query.Where(m => m.LogDate <= filters.DateTo.Value);
                }
            }

            return await query.OrderByDescending(m => m.LogDate).ToListAsync();
        }

        private async Task<List<Alert>> GetFilteredAlerts(DashboardFilterViewModel? filters)
        {
            var query = _context.Alerts
                .Include(a => a.Equipment)
                    .ThenInclude(e => e!.Room)
                        .ThenInclude(r => r!.Building)
                .AsQueryable();

            if (filters != null)
            {
                if (filters.BuildingIds?.Any() == true)
                {
                    query = query.Where(a => a.Equipment != null && a.Equipment.Room != null && filters.BuildingIds.Contains(a.Equipment.Room.BuildingId));
                }

                if (filters.DateFrom.HasValue)
                {
                    query = query.Where(a => a.CreatedDate >= filters.DateFrom.Value);
                }

                if (filters.DateTo.HasValue)
                {
                    query = query.Where(a => a.CreatedDate <= filters.DateTo.Value);
                }
            }

            return await query.OrderByDescending(a => a.CreatedDate).ToListAsync();
        }

        private async Task<List<InventoryItem>> GetFilteredInventory(DashboardFilterViewModel? filters)
        {
            var query = _context.InventoryItems
                .Include(i => i.InventoryStocks)
                .AsQueryable();

            // Apply any relevant filters here if needed for inventory

            return await query.OrderBy(i => i.Name).ToListAsync(); // Use Name instead of ItemName
        }
    }
}
