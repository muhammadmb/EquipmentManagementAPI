using Application.Interface.Services;

namespace API.GraphQL.SellingContract.Queries
{
    [ExtendObjectType(Name = "Query")]
    public class SellingContractCheckQueries
    {
        public async Task<bool> SellingContractExists(Guid id, [Service] ISellingContractService service)
        {
            return await service.SellingContractExists(id);
        }
        public async Task<bool> CustomerHasContracts(Guid customerId, [Service] ISellingContractService service)
        {
            return await service.CustomerHasContracts(customerId);
        }
    }
}
