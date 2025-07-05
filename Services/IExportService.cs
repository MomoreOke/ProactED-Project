using FEENALOoFINALE.Models;

namespace FEENALOoFINALE.Services
{
    public interface IExportService
    {
        Task<byte[]> ExportDashboardDataToExcel();
        Task<byte[]> ExportDashboardDataToPdf();
        Task<byte[]> ExportCustomReportToExcel(CustomReportTemplate template, DashboardFilterViewModel? filters = null);
        Task<byte[]> ExportCustomReportToPdf(CustomReportTemplate template, DashboardFilterViewModel? filters = null);
        Task<byte[]> ExportAnalyticsDataToExcel(AdvancedAnalyticsViewModel analytics);
        Task<byte[]> ExportAnalyticsDataToPdf(AdvancedAnalyticsViewModel analytics);
    }
}
