using System.ComponentModel.DataAnnotations;

namespace FEENALOoFINALE.Models
{
    public class SavedDashboardView
    {
        [Key]
        public int ViewId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public bool IsPublic { get; set; } = false;
        
        public bool IsDefault { get; set; } = false;
        
        [Required]
        public string FilterConfig { get; set; } = "{}"; // JSON string containing filter configuration
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime? LastModified { get; set; }
        
        // Navigation property
        public virtual User User { get; set; } = null!;
    }
}
