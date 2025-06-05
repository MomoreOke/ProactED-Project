using System.ComponentModel.DataAnnotations;

namespace FEENALOoFINALE.Models
{
    public class Room
    {
        public int RoomId { get; set; }
        public required string RoomName { get; set; }
        public int BuildingId { get; set; }

        // Navigation can be nullable for seeding; the relationship is enforced via BuildingId and Fluent API
        public Building? Building { get; set; }
        
        public ICollection<Equipment>? Equipments { get; set; }
    }
}