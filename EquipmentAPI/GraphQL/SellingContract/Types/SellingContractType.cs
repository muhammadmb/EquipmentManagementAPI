using API.GraphQL.SellingContract.Resolver;
using Application.Models.SellingContract.Read;

namespace API.GraphQL.SellingContract.Types
{
    public class SellingContractType : ObjectType<SellingContractDto>
    {
        protected override void Configure(IObjectTypeDescriptor<SellingContractDto> descriptor)
        {
            descriptor.Field(f => f.Id).Type<NonNullType<UuidType>>();
            descriptor.Field(f => f.EquipmentId).Type<NonNullType<UuidType>>();
            descriptor.Field(f => f.CustomerId).Type<NonNullType<UuidType>>();
            descriptor.Field(f => f.SalePrice).Type<NonNullType<DecimalType>>();

            descriptor.Field(f => f.Equipment)
                .ResolveWith<SellingContractResolver>(
                    r => r.GetEquipmentAsync(default!, default!));

            descriptor.Field(f => f.Customer).
                ResolveWith<SellingContractResolver>(
                    r => r.GetCustomerAsync(default!, default!));
        }
    }
}
