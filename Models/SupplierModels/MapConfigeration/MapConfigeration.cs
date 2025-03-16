using EquipmentAPI.Entities;
using EquipmentAPI.Models.PhoneNumberModels.Read;
using EquipmentAPI.Models.SupplierModels.Read;
using Mapster;

namespace EquipmentAPI.Models.SupplierModels.MapConfigeration
{
    public class MapConfigeration
    {
        public static void Configure()
        {
            TypeAdapterConfig<Supplier, SupplierForEquipmentDto>.NewConfig()
                .Map(dest => dest.PhoneNumbers, src => src.PhoneNumbers.Adapt<SupplierPhoneNumberDto>());
        }
    }
}
