using Application.Models.PhoneNumberModels.Write;
using Domain.Entities;
using Mapster;

namespace Application.Models.SupplierModels.MapConfigeration
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
