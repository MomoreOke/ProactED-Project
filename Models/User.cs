using System.ComponentModel.DataAnnotations;

namespace FEENALOoFINALE.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public UserRole Role { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime LastLogin { get; set; }

        // Navigation property
        public ICollection<Alert> AssignedAlerts { get; set; }
    }

    public enum UserRole
    {
        Admin,
        Technician,
        Viewer
    }
}
