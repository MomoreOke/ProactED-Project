using FEENALOoFINALE.Models;
using FEENALOoFINALE.Models.ViewModels;

namespace FEENALOoFINALE.Services
{
    public interface IExportService
    {
        Task<byte[]> ExportDashboardDataToExcel();
        Task<byte[]> ExportDashboardDataToPdf();
        Task<byte[]> ExportCustomReportToExcel(CustomReportTemplate template, DashboardFilterViewModel? filters = null);
        Task<byte[]> ExportCustomReportToPdf(CustomReportTemplate template, DashboardFilterViewModel? filters = null);
        byte[] ExportAnalyticsDataToExcel(AdvancedAnalyticsViewModel analytics);
        byte[] ExportAnalyticsDataToPdf(AdvancedAnalyticsViewModel analytics);
        
        // Enhanced report export functionality
        Task<ExportResult> ExportReportAsync(object reportData, string format);
    }
}
