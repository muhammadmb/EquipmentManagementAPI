using EquipmentAPI.Enums;

namespace EquipmentAPI.Models.TechnicalInfo.Read
{
    public class TechnicalInformationForEquipmentDto
    {
        public Guid Id { get; set; }

        public Guid EquipmentId { get; set; }

        public EngineType EngineType { get; set; }

        public int? EnginePower { get; set; }

        public PowerUnit EnginePowerUnit { get; set; }

        public int? FuelCapacity { get; set; }

        public FuelCapacityUnit FuelCapacityUnit { get; set; }

        public int? Weight { get; set; }

        public WeightUnit WeightUnit { get; set; }

        public string? Dimensions { get; set; }

        public DimensionUnit DimensionUnit { get; set; }

        public int? MaxSpeed { get; set; }

        public SpeedUnit SpeedUnit { get; set; }

        public DateTimeOffset AddedDate { get; set; }

        public DateTimeOffset? DeletedDate { get; set; }

        public DateTimeOffset? UpdateDate { get; set; }
    }
}
