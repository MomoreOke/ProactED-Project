using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FEENALOoFINALE.Services;

namespace FEENALOoFINALE.Controllers.Api
{
    /// <summary>
    /// API controller for monitoring ML prediction metrics
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MetricsController : ControllerBase
    {
        private readonly PredictionMetricsService _metricsService;
        private readonly ILogger<MetricsController> _logger;

        public MetricsController(PredictionMetricsService metricsService, ILogger<MetricsController> logger)
        {
            _metricsService = metricsService;
            _logger = logger;
        }

        /// <summary>
        /// Get overall prediction metrics summary
        /// </summary>
        [HttpGet("summary")]
        public ActionResult<MetricsSummary> GetMetricsSummary()
        {
            try
            {
                var summary = _metricsService.GetMetricsSummary();
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get metrics summary");
                return StatusCode(500, new { error = "Failed to retrieve metrics" });
            }
        }

        /// <summary>
        /// Get prediction metrics for specific equipment
        /// </summary>
        [HttpGet("equipment/{equipmentId}")]
        public ActionResult<List<PredictionMetric>> GetEquipmentMetrics(int equipmentId, int limit = 10)
        {
            try
            {
                if (equipmentId <= 0)
                {
                    return BadRequest(new { error = "Invalid equipment ID" });
                }

                var metrics = _metricsService.GetEquipmentMetrics(equipmentId, limit);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get equipment metrics for {EquipmentId}", equipmentId);
                return StatusCode(500, new { error = "Failed to retrieve equipment metrics" });
            }
        }

        /// <summary>
        /// Clear old metrics (admin only)
        /// </summary>
        [HttpPost("clear-old")]
        public IActionResult ClearOldMetrics(int daysToKeep = 7)
        {
            try
            {
                if (daysToKeep < 1 || daysToKeep > 365)
                {
                    return BadRequest(new { error = "Days to keep must be between 1 and 365" });
                }

                _metricsService.ClearOldMetrics(daysToKeep);
                
                return Ok(new { 
                    message = $"Cleared metrics older than {daysToKeep} days",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear old metrics");
                return StatusCode(500, new { error = "Failed to clear old metrics" });
            }
        }

        /// <summary>
        /// Health check for metrics service
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult HealthCheck()
        {
            return Ok(new { 
                status = "healthy", 
                service = "metrics",
                timestamp = DateTime.UtcNow 
            });
        }
    }
}
