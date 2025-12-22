using Application.Models.PhoneNumberModels.Write;
using Domain.Entities;
using Mapster;

namespace Application.Models.PhoneNumberModels.MapConfigeration
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
