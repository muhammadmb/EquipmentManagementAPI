using Application.Interface.Repositories;
using Application.Models.CustomersModels.Read;
using Application.Models.EquipmentModels.Read;
using Application.Models.RentalContractModels.Read;
using Mapster;

namespace API.GraphQL.RentalContract.Resolvers
{
    public class RentalContractResolvers
    {
        public async Task<CustomerDto?> GetCustomerAsync(
           [Parent] RentalContractDto contract,
           [Service] ICustomerRepository customerRepo)
        {
            if (contract.CustomerId == Guid.Empty)
                return null;

            return (await customerRepo.GetCustomerById(contract.CustomerId, null))
                .Adapt<CustomerDto>();
        }

        public async Task<EquipmentDto?> GetEquipmentAsync(
            [Parent] RentalContractDto contract,
            [Service] IEquipmentRepository equipmentRepo)
        {
            if (contract.EquipmentId == Guid.Empty)
                return null;

            return (await equipmentRepo.GetEquipmentById(contract.EquipmentId, null))
                .Adapt<EquipmentDto>();
        }
    }
}
