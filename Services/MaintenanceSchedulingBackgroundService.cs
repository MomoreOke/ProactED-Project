namespace FEENALOoFINALE.Services
{
    public class MaintenanceSchedulingBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MaintenanceSchedulingBackgroundService> _logger;

        public MaintenanceSchedulingBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<MaintenanceSchedulingBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Maintenance Scheduling Background Service started");
            
            try
            {
                // Wait 1 minute before first run to reduce startup load
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        _logger.LogInformation("Maintenance scheduling background service running at: {time}", DateTimeOffset.Now);

                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var schedulingService = scope.ServiceProvider.GetRequiredService<MaintenanceSchedulingService>();
                            
                            var tasksCreated = await schedulingService.ScheduleRoutineMaintenanceAsync();
                            
                            if (tasksCreated > 0)
                            {
                                _logger.LogInformation("Created {count} new maintenance tasks", tasksCreated);
                            }
                        }

                        // Run every 6 hours
                        await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected when cancellation is requested
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred executing maintenance scheduling background service");
                        try
                        {
                            // Wait 1 hour before retry on error
                            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested during startup delay
                _logger.LogInformation("Maintenance Scheduling Service cancelled during startup");
            }
            
            _logger.LogInformation("Maintenance Scheduling Background Service stopped");
        }
    }
}
