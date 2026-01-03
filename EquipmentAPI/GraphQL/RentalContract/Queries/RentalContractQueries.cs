using API.GraphQL.RentalContract.Inputs;
using Application.Interface.Services;
using Application.Models.RentalContractModels.Read;
using Application.ResourceParameters;
using HotChocolate.Resolvers;
using Mapster;

namespace API.GraphQL.RentalContract.Queries
{
    [ExtendObjectType(Name = "Query")]
    public class RentalContractQueries
    {
        public async Task<IEnumerable<RentalContractDto>> GetRentalContracts(
               RentalContractFilterInput filterInput,
               [Service] IRentalContractService service)
        {
            var parameters = filterInput.Adapt<RentalContractResourceParameters>();
            return (await service.GetRentalContracts(parameters))
                    .Adapt<IEnumerable<RentalContractDto>>();
        }

        public async Task<RentalContractDto> GetRentalContractById(
            Guid id,
            [Service] IRentalContractService service)
        {
            var rentalContract = await service.GetRentalContractById(id, null);
            return rentalContract.Adapt<RentalContractDto>();
        }

        public async Task<IEnumerable<RentalContractDto>> GetRentalContractsByIds(
            IEnumerable<Guid> ids,
            [Service] IRentalContractService service)
        {
            var rentalContract = await service.GetRentalContractsByIds(ids, null);
            return rentalContract.Adapt<IEnumerable<RentalContractDto>>();
        }

        public async Task<IEnumerable<RentalContractDto>> GetRentalContractsByCustomerId(
            Guid customerId,
            [Service] IRentalContractService service)
        {
            var rentalContract = await service.GetRentalContractsByCustomerId(customerId, null);
            return rentalContract.Adapt<IEnumerable<RentalContractDto>>();
        }

        public async Task<IEnumerable<RentalContractDto>> GetRentalContractsByEquipmentId(
            Guid equipmentId,
            [Service] IRentalContractService service)
        {
            var rentalContract = await service.GetRentalContractsByEquipmentId(equipmentId, null);
            return rentalContract.Adapt<IEnumerable<RentalContractDto>>();
        }

        public async Task<IEnumerable<RentalContractDto>> GetActiveContracts
            ([Service] IRentalContractService service)
        {
            var rentalContracts = await service.GetActiveContracts(null);
            return rentalContracts.Adapt<IEnumerable<RentalContractDto>>();
        }

        public async Task<IEnumerable<RentalContractDto>> GetExpiredContracts
            (int daysUntilExpiration,
            [Service] IRentalContractService service)
        {
            var rentalContracts = await service.GetExpiredContracts(daysUntilExpiration, null);
            return rentalContracts.Adapt<IEnumerable<RentalContractDto>>();
        }
    }
}
