using Application.Interface.Services;

namespace API.GraphQL.RentalContract.Queries
{
    [ExtendObjectType(Name = "Query")]
    public class RentalContractChecks
    {
        public async Task<bool> RentalContractExists(
            Guid id,
            [Service] IRentalContractService service)
        {
            return await service.RentalContractExists(id);
        }

        public async Task<bool> CustomerHasContracts(
            Guid customerId,
            [Service] IRentalContractService service)
        {
            return await service.CustomerHasContracts(customerId);
        }

        public async Task<bool> EquipmentHasContracts(
           Guid equipmentId,
           [Service] IRentalContractService service)
        {
            return await service.EquipmentHasContracts(equipmentId);
        }

        public async Task<bool> HasOverlappingContracts(
           Guid equipmentId,
           DateTimeOffset startDate,
           DateTimeOffset endDate,
           Guid? excludeContractId,
           [Service] IRentalContractService service)
        {
            return await service
                .HasOverlappingContracts(
                    equipmentId,
                    startDate,
                    endDate,
                    excludeContractId);
        }
    }
}
