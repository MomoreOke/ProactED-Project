using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace FEENALOoFINALE.Models
{
    public class User
    {
        [Key]
        public required string UserId { get; set; }
        
        [Required]
        public required string Username { get; set; }
        
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        
        [Required]
        public string? FullName { get; set; }
        
        [Required]
        public string? Department { get; set; }
        
        [Required]
        [Phone]
        public string? ContactNumber { get; set; }
        
        [Required]
        public UserRole Role { get; set; }
        
        public DateTime LastLogin { get; set; }

        // Navigation property
        public required ICollection<Alert> AssignedAlerts { get; set; }
        
        // Add this navigation property
        public ICollection<MaintenanceTask> MaintenanceTasks { get; set; } = new List<MaintenanceTask>();
    }

    public enum UserRole
    {
        Administrator,
        Technician,
        Operator,
        Supervisor
    }
}
