using API.GraphQL.RentalContract.Resolvers;
using Application.Models.RentalContractModels.Read;

namespace API.GraphQL.RentalContract.Types
{
    public class RentalContractType : ObjectType<RentalContractDto>
    {
        protected override void Configure(IObjectTypeDescriptor<RentalContractDto> descriptor)
        {
            descriptor.Field(f => f.Id).Type<NonNullType<UuidType>>();
            descriptor.Field(f => f.CustomerId);
            descriptor.Field(f => f.EquipmentId);
            descriptor.Field(f => f.StartDate);
            descriptor.Field(f => f.EndDate);
            descriptor.Field(f => f.RentalPrice);
            descriptor.Field(f => f.Status);

            descriptor.Field(f => f.Customer)
            .ResolveWith<RentalContractResolvers>(
                r => r.GetCustomerAsync(default!, default!));

            descriptor.Field(f => f.Equipment)
                .ResolveWith<RentalContractResolvers>(
                    r => r.GetEquipmentAsync(default!, default!));
        }
    }
}
