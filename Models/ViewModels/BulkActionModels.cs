using System.ComponentModel.DataAnnotations;

namespace FEENALOoFINALE.Models.ViewModels
{
    public class BulkActionRequest
    {
        [Required]
        public required string Action { get; set; }
        
        [Required]
        public required List<int> SelectedIds { get; set; }
        
        public Dictionary<string, object>? Parameters { get; set; }
    }

    public class BulkActionResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int ProcessedCount { get; set; }
        public int FailedCount { get; set; }
        public List<string>? Errors { get; set; }
        public Dictionary<string, object>? Data { get; set; }
    }

    public class BulkUpdateRequest
    {
        [Required]
        public required List<int> EquipmentIds { get; set; }
        
        public string? Status { get; set; }
        public int? BuildingId { get; set; }
        public int? EquipmentTypeId { get; set; }
        public string? Location { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        public string? Notes { get; set; }
    }

    public class BulkDeleteRequest
    {
        [Required]
        public required List<int> EquipmentIds { get; set; }
        
        public bool ConfirmPermanentDelete { get; set; } = false;
        public string? Reason { get; set; }
    }
}
