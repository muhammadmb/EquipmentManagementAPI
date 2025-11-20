using EquipmentAPI.Entities;
using EquipmentAPI.Models.PhoneNumberModels.Write;
using Mapster;

namespace EquipmentAPI.Models.SupplierModels.MapConfigeration
{
    public class MapConfigeration
    {
        public static void Configure()
        {
            TypeAdapterConfig<SupplierPhoneNumberUpdateDto, SupplierPhoneNumber>
            .NewConfig()
            .TwoWays();
        }
    }
}
