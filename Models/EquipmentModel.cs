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
        public int EquipmentTypeId { get; set; }

        [Required]
        public string ModelName { get; set; } = string.Empty;

        // Navigation property
        public virtual EquipmentType? EquipmentType { get; set; }

        public virtual ICollection<Equipment> Equipments { get; set; } = new List<Equipment>();
    }
}