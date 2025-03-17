using EquipmentAPI.Attributes;
using EquipmentAPI.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquipmentAPI.Entities
{
    public class TechnicalInformation : BasicEntity
    {
        [Required]
        public Guid EquipmentId { get; set; }
        public Equipment? Equipment{ get; set; }

        public EngineType EngineType { get; set; }

        [Range(0, 1500)]
        public int? EnginePower { get; set; }

        public PowerUnit EnginePowerUnit { get; set; }

        [Range(0, 500)]
        public int? FuelCapacity { get; set; }

        public FuelCapacityUnit FuelCapacityUnit { get; set; }

        [Range(0, 35000)]
        public int? Weight { get; set; }
        
        public WeightUnit WeightUnit { get; set; }

        [StringLength(50)]
        [DimensionsFormatAttribute]
        public string? Dimensions { get; set; }

        public DimensionUnit DimensionUnit { get; set; }

        [Range(0, 500)]
        public int? MaxSpeed { get; set; }
        
        public SpeedUnit SpeedUnit { get; set; }
    }
}