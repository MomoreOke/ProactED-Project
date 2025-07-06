using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FEENALOoFINALE.Models
{
    public class FailurePrediction
    {
        [Key]
        public int PredictionId { get; set; }

        [Display(Name = "Equipment")]
        public int EquipmentId { get; set; }

        public DateTime PredictedFailureDate { get; set; }

        [Range(0, 100)]
        public int ConfidenceLevel { get; set; }

        public PredictionStatus Status { get; set; }

        public string? AnalysisNotes { get; set; }

        public string? ContributingFactors { get; set; }

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
