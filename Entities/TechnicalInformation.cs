using EquipmentAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace EquipmentAPI.Entities
{
    public class TechnicalInformation
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid EquipmentId { get; set; }

        public EngineType EngineType { get; set; }
        
        public decimal? EnginePower { get; set; }

        public PowerUnit EnginePowerUnit { get; set; }
       
        public decimal? FuelCapacity { get; set; }

        public FuelCapacityUnit FuelCapacityUnit { get; set; }
        
        public decimal? Weight { get; set; }
        
        public WeightUnit WeightUnit { get; set; }

        [StringLength(50)]
        public string? Dimensions { get; set; }
        
        public DimensionUnit DimensionUnit { get; set; }

        public decimal? MaxSpeed { get; set; }
        
        public SpeedUnit SpeedUnit { get; set; }

        public DateTimeOffset AddedDate { get; set; } = DateTimeOffset.Now;

        public DateTimeOffset? DeletedDate { get; set; }

        public DateTimeOffset? UpdateDate { get; set; }
    }
}