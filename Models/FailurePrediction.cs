using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FEENALOoFINALE.Models
{
    public class FailurePrediction
    {
        [Key]
        public int PredictionId { get; set; }

        [Required]
        public int EquipmentId { get; set; }

        [Required]
        public DateTime PredictedFailureDate { get; set; }

        [Required]
        [Range(0, 100)]
        public int ConfidenceLevel { get; set; }

        [Required]
        public PredictionStatus Status { get; set; }

        public string? AnalysisNotes { get; set; }

        public string? ContributingFactors { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        // Navigation property
        [ForeignKey("EquipmentId")]
        public Equipment Equipment { get; set; } = null!;
    }

    public enum PredictionStatus
    {
        Low,
        Medium,
        High
    }
}
