using System.ComponentModel.DataAnnotations;

namespace FEENALOoFINALE.Models
{
    public class Room
    {
        public int RoomId { get; set; }

        [Required]
        [StringLength(100)]
        public required string RoomName { get; set; }

        // Foreign key for Building
        public int BuildingId { get; set; }

        // Navigation property to Building
        public required Building Building { get; set; }

        // Navigation property for Equipments in this Room
        public ICollection<Equipment>? Equipments { get; set; }
    }
}