using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace FEENALOoFINALE.Models
{
    public class User : IdentityUser
    {
        [Required]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        public string Department { get; set; } = string.Empty;
        
        [Phone]
        public string? ContactNumber { get; set; }
        
        [Required]
        public UserRole Role { get; set; }
        
        public DateTime LastLogin { get; set; }
    
        // Add these navigation properties with initialization
        public virtual ICollection<Alert> AssignedAlerts { get; set; } = [];
        public virtual ICollection<MaintenanceTask> MaintenanceTasks { get; set; } = [];
    }

    public enum UserRole
    {
        Administrator,
        Technician,
        Operator,
        Supervisor
    }
}
