using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FEENALOoFINALE.Data;
using FEENALOoFINALE.Models; 
using FEENALOoFINALE.Services;
using Microsoft.AspNetCore.Authorization;

namespace FEENALOoFINALE.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAdvancedAnalyticsService _analyticsService;

        public DashboardController(ApplicationDbContext context, IAdvancedAnalyticsService analyticsService)
        {
            _context = context;
            _analyticsService = analyticsService;
        }

        public async Task<IActionResult> Index()
        {
            var enhancedViewModel = await _analyticsService.GetEnhancedDashboardDataAsync();
            return View(enhancedViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetLiveMetrics()
        {
            var metrics = new
            {
                EquipmentHealth = await _analyticsService.CalculateSystemHealthScoreAsync(),
                MaintenanceEfficiency = await _analyticsService.CalculateMaintenanceEfficiencyAsync(),
                CostEfficiency = await _analyticsService.CalculateCostEfficiencyAsync(),
                UtilizationRate = await _analyticsService.CalculateEquipmentUtilizationAsync()
            };
            return Json(metrics);
        }

        [HttpGet]
        public async Task<IActionResult> GetTrendData(int days = 30)
        {
            var trendData = await _analyticsService.GetMaintenanceTrendDataAsync(days);
            return Json(trendData);
        }

        [HttpGet]
        public async Task<IActionResult> GetCostAnalysis()
        {
            var costData = await _analyticsService.GetCostAnalysisDataAsync();
            return Json(costData);
        }
    }
}