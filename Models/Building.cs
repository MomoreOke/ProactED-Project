using System.ComponentModel.DataAnnotations;

namespace FEENALOoFINALE.Models
{
    public class Building
    {
        public int BuildingId { get; set; }

        [Required]
        [StringLength(100)]
        public required string BuildingName { get; set; }

        // Navigation property for related Rooms
        public ICollection<Room>? Rooms { get; set; }

        // Navigation property for Equipments in this Building
        public ICollection<Equipment>? Equipments { get; set; }
    }
}