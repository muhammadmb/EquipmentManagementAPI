using EquipmentAPI.Entities;
using EquipmentAPI.Models.EquipmentModels.Read;
using EquipmentAPI.Models.MaintenanceRecordModels.Read;
using EquipmentAPI.Models.SupplierModels.Read;
using EquipmentAPI.Models.TechnicalInfo.Read;
using Mapster;

namespace EquipmentAPI.Models.EquipmentModels.MapConfigeration
{
    public class MapConfigeration
    {
        public static void Configure()
        {
            TypeAdapterConfig<Equipment, EquipmentDto>.NewConfig()
                .Map(dest => dest.TechnicalInformation, src => src.TechnicalInformation.Adapt<TechnicalInformationForEquipmentDto>())
                .Map(dest => dest.MaintenanceRecords, src => src.MaintenanceRecords.Adapt<MaintenanceRecordForEquipmentDto>())
                .Map(dest => dest.Supplier, src => src.Supplier.Adapt<SupplierForEquipmentDto>());
        }
    }
}