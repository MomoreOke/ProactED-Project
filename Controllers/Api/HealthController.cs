using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FEENALOoFINALE.Data;
using Microsoft.EntityFrameworkCore;

namespace FEENALOoFINALE.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HealthController> _logger;

        public HealthController(ApplicationDbContext context, ILogger<HealthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Health check endpoint to verify API is working
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Test database connectivity
                var canConnectToDatabase = await _context.Database.CanConnectAsync();
                
                var health = new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.Now,
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                    Database = canConnectToDatabase ? "Connected" : "Disconnected",
                    Version = "1.0.0",
                    ApiEndpoints = new
                    {
                        Health = "/api/health",
                        Swagger = "/api-docs",
                        SignalR = "/maintenanceHub"
                    }
                };

                return Ok(health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(500, new
                {
                    Status = "Unhealthy",
                    Timestamp = DateTime.Now,
                    Error = "Database connection failed"
                });
            }
        }

        /// <summary>
        /// Get API information and available endpoints
        /// </summary>
        [HttpGet("info")]
        [AllowAnonymous]
        public IActionResult GetApiInfo()
        {
            var apiInfo = new
            {
                Name = "Predictive Maintenance API",
                Version = "1.0.0",
                Description = "REST API for Predictive Maintenance Management System",
                Documentation = "/api-docs",
                Features = new[]
                {
                    "JWT Authentication",
                    "Swagger Documentation", 
                    "SignalR Real-time Updates",
                    "Background Services",
                    "Equipment Management",
                    "Maintenance Logging",
                    "Alert Management",
                    "Inventory Tracking"
                },
                Endpoints = new
                {
                    Health = "/api/health",
                    ApiInfo = "/api/health/info",
                    Statistics = "/api/health/stats",
                    Swagger = "/api-docs"
                },
                Infrastructure = new
                {
                    Framework = ".NET 8.0",
                    Database = "SQL Server",
                    Authentication = "JWT Bearer",
                    RealTime = "SignalR",
                    Documentation = "Swagger/OpenAPI"
                }
            };

            return Ok(apiInfo);
        }

        /// <summary>
        /// Get basic system statistics
        /// </summary>
        [HttpGet("stats")]
        [Authorize]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = new
                {
                    Timestamp = DateTime.Now,
                    Database = new
                    {
                        TotalEquipment = await _context.Equipment.CountAsync(),
                        TotalMaintenanceLogs = await _context.MaintenanceLogs.CountAsync(),
                        TotalAlerts = await _context.Alerts.CountAsync(),
                        TotalInventoryItems = await _context.InventoryItems.CountAsync(),
                        TotalUsers = await _context.Users.CountAsync()
                    },
                    System = new
                    {
                        MachineName = Environment.MachineName,
                        ProcessorCount = Environment.ProcessorCount,
                        WorkingSet = GC.GetTotalMemory(false),
                        TickCount = Environment.TickCount64
                    }
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve system statistics");
                return StatusCode(500, new { message = "Failed to retrieve system statistics" });
            }
        }
    }
}
