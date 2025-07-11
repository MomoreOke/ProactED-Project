namespace FEENALOoFINALE.Services
{
    public class PerformanceReport
    {
        public int TotalOperations { get; set; }
        public Dictionary<string, long> OperationStats { get; set; } = new();
        public List<SlowOperation> SlowOperations { get; set; } = new();
        public double AverageResponseTime { get; set; }
        public long TotalMemoryUsage { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime GeneratedAt { get; set; }
        public List<string> ActiveTimers { get; set; } = new();
        public Dictionary<string, object> SystemMetrics { get; set; } = new();
    }

    public class SlowOperation
    {
        public string OperationName { get; set; } = "";
        public long ExecutionTime { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
