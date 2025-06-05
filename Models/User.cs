using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace FEENALOoFINALE.Models
{
    public class User : IdentityUser
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        public string WorkerId { get; set; } = string.Empty;
        
        // Computed property for convenience
        public string FullName => $"{FirstName} {LastName}";

        [Phone]
        public string? ContactNumber { get; set; }
        
        public DateTime LastLogin { get; set; }
    
        // Navigation properties
        public virtual ICollection<Alert> AssignedAlerts { get; set; } = new List<Alert>();
        public virtual ICollection<MaintenanceTask> MaintenanceTasks { get; set; } = new List<MaintenanceTask>();
    }
}
