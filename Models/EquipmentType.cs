using FEENALOoFINALE.Models;

namespace FEENALOoFINALE.Models
{
    public class EquipmentType
    {
        public int EquipmentTypeId { get; set; }
        public required string EquipmentTypeName { get; set; }
        public virtual ICollection<Equipment> Equipments { get; set; } = new List<Equipment>();
        public virtual ICollection<EquipmentModel> EquipmentModels { get; set; } =  new List<EquipmentModel>();
    }
} 