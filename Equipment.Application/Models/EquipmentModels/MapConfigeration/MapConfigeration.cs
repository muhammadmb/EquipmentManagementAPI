using Application.Models.EquipmentModels.Read;
using Application.Models.MaintenanceRecordModels.Read;
using Application.Models.SupplierModels.Read;
using Application.Models.TechnicalInformationModels.Read;
using Domain.Entities;
using Mapster;

namespace Application.Models.EquipmentModels.MapConfigeration
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