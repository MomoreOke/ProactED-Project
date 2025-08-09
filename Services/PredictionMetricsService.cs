using System.Collections.Concurrent;

namespace FEENALOoFINALE.Services
{
    /// <summary>
    /// Service for tracking ML prediction metrics and performance
    /// </summary>
    public class PredictionMetricsService
    {
        private readonly ConcurrentQueue<PredictionMetric> _metrics = new();
        private readonly object _lock = new object();
        private readonly ILogger<PredictionMetricsService> _logger;

        public PredictionMetricsService(ILogger<PredictionMetricsService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Record a prediction metric
        /// </summary>
        public void RecordPrediction(int equipmentId, bool success, TimeSpan responseTime, string? errorMessage = null)
        {
            var metric = new PredictionMetric
            {
                EquipmentId = equipmentId,
                Timestamp = DateTime.UtcNow,
                Success = success,
                ResponseTimeMs = (int)responseTime.TotalMilliseconds,
                ErrorMessage = errorMessage
            };

            _metrics.Enqueue(metric);

            // Keep only last 1000 metrics to prevent memory issues
            lock (_lock)
            {
                while (_metrics.Count > 1000)
                {
                    _metrics.TryDequeue(out _);
                }
            }

            if (!success)
            {
                _logger.LogWarning("Prediction failed for equipment {EquipmentId}: {ErrorMessage}", 
                    equipmentId, errorMessage);
            }
        }

        /// <summary>
        /// Get current metrics summary
        /// </summary>
        public MetricsSummary GetMetricsSummary()
        {
            var metrics = _metrics.ToArray();
            if (!metrics.Any())
            {
                return new MetricsSummary();
            }

            var last24Hours = DateTime.UtcNow.AddHours(-24);
            var recent = metrics.Where(m => m.Timestamp >= last24Hours).ToArray();

            return new MetricsSummary
            {
                TotalPredictions = recent.Length,
                SuccessfulPredictions = recent.Count(m => m.Success),
                FailedPredictions = recent.Count(m => !m.Success),
                SuccessRate = recent.Length > 0 ? (double)recent.Count(m => m.Success) / recent.Length * 100 : 0,
                AverageResponseTimeMs = recent.Where(m => m.Success).Any() ? 
                    (int)recent.Where(m => m.Success).Average(m => m.ResponseTimeMs) : 0,
                LastUpdated = DateTime.UtcNow,
                PeriodHours = 24
            };
        }

        /// <summary>
        /// Get metrics for a specific equipment
        /// </summary>
        public List<PredictionMetric> GetEquipmentMetrics(int equipmentId, int limitCount = 10)
        {
            return _metrics.Where(m => m.EquipmentId == equipmentId)
                          .OrderByDescending(m => m.Timestamp)
                          .Take(limitCount)
                          .ToList();
        }

        /// <summary>
        /// Clear old metrics (older than specified days)
        /// </summary>
        public void ClearOldMetrics(int daysToKeep = 7)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            var originalCount = _metrics.Count;

            lock (_lock)
            {
                var newQueue = new ConcurrentQueue<PredictionMetric>();
                foreach (var metric in _metrics.Where(m => m.Timestamp >= cutoffDate))
                {
                    newQueue.Enqueue(metric);
                }

                _metrics.Clear();
                foreach (var metric in newQueue)
                {
                    _metrics.Enqueue(metric);
                }
            }

            var clearedCount = originalCount - _metrics.Count;
            if (clearedCount > 0)
            {
                _logger.LogInformation("Cleared {ClearedCount} old prediction metrics", clearedCount);
            }
        }
    }

    public class PredictionMetric
    {
        public int EquipmentId { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Success { get; set; }
        public int ResponseTimeMs { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class MetricsSummary
    {
        public int TotalPredictions { get; set; }
        public int SuccessfulPredictions { get; set; }
        public int FailedPredictions { get; set; }
        public double SuccessRate { get; set; }
        public int AverageResponseTimeMs { get; set; }
        public DateTime LastUpdated { get; set; }
        public int PeriodHours { get; set; }
    }
}
