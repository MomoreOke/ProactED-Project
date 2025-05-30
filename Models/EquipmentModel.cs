using FEENALOoFINALE.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FEENALOoFINALE.Models
{
    public class EquipmentModel
    {
        public int EquipmentModelId { get; set; }

        [Required]
        [StringLength(100)]
        public required string ModelName { get; set; }

        // Foreign key for EquipmentType
        public int EquipmentTypeId { get; set; }

        // Navigation property to EquipmentType
        public required EquipmentType EquipmentType { get; set; }

        public virtual ICollection<Equipment> Equipments { get; set; } = new List<Equipment>();
    }
}