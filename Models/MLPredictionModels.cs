using System.ComponentModel.DataAnnotations;

namespace FEENALOoFINALE.Models
{
    /// <summary>
    /// Data structure for sending equipment data to the ML prediction API
    /// </summary>
    public class EquipmentPredictionData
    {
        [Required]
        public string EquipmentId { get; set; } = "";
        
        [Required]
        [Range(0, 240)] // Max 20 years in months
        public int AgeMonths { get; set; }
        
        [Required]
        [Range(-50, 200)] // Temperature range in Celsius
        public double OperatingTemperature { get; set; }
        
        [Required]
        [Range(0, 50)] // Vibration level range
        public double VibrationLevel { get; set; }
        
        [Required]
        [Range(0, 10000)] // Power consumption in watts
        public double PowerConsumption { get; set; }

        /// <summary>
        /// Convert Equipment entity to prediction data format
        /// </summary>
        public static EquipmentPredictionData FromEquipment(Equipment equipment)
        {
            var ageMonths = equipment.InstallationDate.HasValue 
                ? (int)(DateTime.Now - equipment.InstallationDate.Value).TotalDays / 30
                : 12; // Default to 1 year if no installation date

            return new EquipmentPredictionData
            {
                EquipmentId = equipment.EquipmentId.ToString(),
                AgeMonths = Math.Max(0, ageMonths),
                OperatingTemperature = GenerateRealisticTemperature(equipment),
                VibrationLevel = GenerateRealisticVibration(equipment),
                PowerConsumption = GenerateRealisticPowerConsumption(equipment)
            };
        }

        /// <summary>
        /// Generate realistic temperature data based on equipment type
        /// </summary>
        private static double GenerateRealisticTemperature(Equipment equipment)
        {
            var equipmentType = equipment.EquipmentType?.EquipmentTypeName?.ToLower() ?? "";
            var baseTemp = equipmentType switch
            {
                var type when type.Contains("projector") => 65.0, // Projectors run hot
                var type when type.Contains("computer") => 45.0,   // Computers moderate heat
                var type when type.Contains("air condition") => 35.0, // ACs should run cool
                var type when type.Contains("printer") => 40.0,    // Printers moderate
                _ => 50.0 // Default
            };

            // Add some realistic variation (+/- 15 degrees)
            var random = new Random();
            return Math.Round(baseTemp + (random.NextDouble() - 0.5) * 30, 1);
        }

        /// <summary>
        /// Generate realistic vibration data based on equipment type
        /// </summary>
        private static double GenerateRealisticVibration(Equipment equipment)
        {
            var equipmentType = equipment.EquipmentType?.EquipmentTypeName?.ToLower() ?? "";
            var baseVibration = equipmentType switch
            {
                var type when type.Contains("projector") => 1.0,  // Low vibration
                var type when type.Contains("computer") => 0.5,   // Very low vibration
                var type when type.Contains("air condition") => 3.0, // Higher vibration due to compressor
                var type when type.Contains("printer") => 2.0,    // Moderate vibration from moving parts
                _ => 1.5 // Default
            };

            // Add equipment age factor - older equipment vibrates more
            var ageMonths = equipment.InstallationDate.HasValue 
                ? (DateTime.Now - equipment.InstallationDate.Value).TotalDays / 30
                : 12;
            
            var ageFactor = 1.0 + (ageMonths / 100.0); // Increase vibration by 1% per month of age
            
            var random = new Random();
            return Math.Round(baseVibration * ageFactor + (random.NextDouble() - 0.5) * 2, 2);
        }

        /// <summary>
        /// Generate realistic power consumption based on equipment type
        /// </summary>
        private static double GenerateRealisticPowerConsumption(Equipment equipment)
        {
            var equipmentType = equipment.EquipmentType?.EquipmentTypeName?.ToLower() ?? "";
            var basePower = equipmentType switch
            {
                var type when type.Contains("projector") => 250.0,    // 250W typical for projectors
                var type when type.Contains("computer") => 150.0,     // 150W for desktop computers
                var type when type.Contains("air condition") => 1500.0, // 1.5kW for AC units
                var type when type.Contains("printer") => 200.0,      // 200W for laser printers
                _ => 300.0 // Default
            };

            // Add usage-based variation
            var weeklyUsage = equipment.AverageWeeklyUsageHours ?? 40.0;
            var usageFactor = weeklyUsage > 40 ? 1.2 : 1.0; // Higher power consumption for heavily used equipment
            
            var random = new Random();
            return Math.Round(basePower * usageFactor + (random.NextDouble() - 0.5) * 100, 1);
        }
    }

    /// <summary>
    /// Result from ML prediction API
    /// </summary>
    public class PredictionResult
    {
        public bool Success { get; set; }
        public string EquipmentId { get; set; } = "";
        public double FailureProbability { get; set; }
        public string RiskLevel { get; set; } = "";
        public double ConfidenceScore { get; set; }
        public DateTime PredictionTimestamp { get; set; }
        public string ModelVersion { get; set; } = "";
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Convert to FailurePrediction entity for database storage
        /// </summary>
        public FailurePrediction ToFailurePrediction(int equipmentId)
        {
            var status = RiskLevel.ToLower() switch
            {
                "critical" or "high" => PredictionStatus.High,
                "medium" => PredictionStatus.Medium,
                "low" => PredictionStatus.Low,
                _ => PredictionStatus.Low
            };

            return new FailurePrediction
            {
                EquipmentId = equipmentId,
                PredictedFailureDate = CalculateFailureDate(),
                ConfidenceLevel = (int)(ConfidenceScore * 100),
                Status = status,
                AnalysisNotes = $"ML Prediction - Model: {ModelVersion}, Risk: {RiskLevel}, Probability: {FailureProbability:P2}",
                CreatedDate = PredictionTimestamp
            };
        }

        /// <summary>
        /// Calculate estimated failure date based on probability
        /// </summary>
        private DateTime CalculateFailureDate()
        {
            // Convert failure probability to days until failure
            var daysUntilFailure = FailureProbability switch
            {
                >= 0.8 => 7,    // Critical: 1 week
                >= 0.6 => 30,   // High: 1 month
                >= 0.4 => 90,   // Medium: 3 months
                >= 0.2 => 180,  // Low-Medium: 6 months
                _ => 365        // Low: 1 year
            };

            return DateTime.Now.AddDays(daysUntilFailure);
        }
    }

    /// <summary>
    /// Result from batch prediction API
    /// </summary>
    public class BatchPredictionResult
    {
        public bool Success { get; set; }
        public int ProcessedCount { get; set; }
        public List<PredictionResult> Predictions { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Information about the ML model
    /// </summary>
    public class ModelInfo
    {
        public bool Success { get; set; }
        public string ModelVersion { get; set; } = "";
        public DateTime TrainingDate { get; set; }
        public double Accuracy { get; set; }
        public List<string> Features { get; set; } = new();
    }

    /// <summary>
    /// Enhanced equipment view model with ML predictions
    /// </summary>
    public class EquipmentWithPredictionViewModel
    {
        public Equipment Equipment { get; set; } = new();
        public PredictionResult? Prediction { get; set; }
        public bool PredictionAvailable => Prediction?.Success == true;
        public string RiskLevelDisplay => Prediction?.RiskLevel ?? "Unknown";
        public string RiskLevelClass => RiskLevel switch
        {
            "Critical" or "High" => "danger",
            "Medium" => "warning", 
            "Low" => "success",
            _ => "secondary"
        };
        public string FailureProbabilityDisplay => Prediction?.FailureProbability.ToString("P1") ?? "N/A";
        public string ConfidenceDisplay => Prediction?.ConfidenceScore.ToString("P1") ?? "N/A";
        
        private string RiskLevel => Prediction?.RiskLevel ?? "Unknown";
    }

    /// <summary>
    /// Dashboard view model with ML prediction insights
    /// </summary>
    public class MLPredictionDashboardViewModel
    {
        public int TotalEquipmentAnalyzed { get; set; }
        public int HighRiskEquipment { get; set; }
        public int MediumRiskEquipment { get; set; }
        public int LowRiskEquipment { get; set; }
        public double AverageFailureProbability { get; set; }
        public double ModelAccuracy { get; set; }
        public string ModelVersion { get; set; } = "";
        public DateTime LastPredictionUpdate { get; set; }
        public bool ApiHealthy { get; set; }
        public List<EquipmentWithPredictionViewModel> HighRiskEquipmentList { get; set; } = new();
        public List<EquipmentWithPredictionViewModel> RecentPredictions { get; set; } = new();
        
        // Chart data for ML predictions
        public Dictionary<string, int> RiskLevelDistribution { get; set; } = new();
        public List<Models.ViewModels.ChartDataPoint> FailureProbabilityTrend { get; set; } = new();
        public List<Models.ViewModels.ChartDataPoint> ConfidenceLevelByType { get; set; } = new();
    }
}
