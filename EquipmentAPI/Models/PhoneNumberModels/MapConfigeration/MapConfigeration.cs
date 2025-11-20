using EquipmentAPI.Entities;
using EquipmentAPI.Models.PhoneNumberModels.Write;
using Mapster;

namespace EquipmentAPI.Models.PhoneNumberModels.MapConfigeration
{
    public class MapConfigeration
    {
        public static void Configure()
        {
            TypeAdapterConfig<SupplierPhoneNumberUpdateDto, SupplierPhoneNumber>.NewConfig()
            .Map(dest => dest.Number, src => src.Number);
        }
    }
}
