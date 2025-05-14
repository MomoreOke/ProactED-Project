using System.ComponentModel.DataAnnotations;

namespace FEENALOoFINALE.Models
{
    public class FailurePrediction
    {
        [Key]
        public int PredictionId { get; set; }
        public int EquipmentId { get; set; }
        public DateTime PredictionDate { get; set; }
        public string PredictedFailureType { get; set; }
        public decimal ConfidenceScore { get; set; }
        public DateTime PredictedFailureWindowStart { get; set; }
        public DateTime PredictedFailureWindowEnd { get; set; }
        public PredictionStatus Status { get; set; }

        // Navigation property
        public Equipment Equipment { get; set; }
    }

    public enum PredictionStatus
    {
        Pending,
        Confirmed,
        FalsePositive
    }
}
