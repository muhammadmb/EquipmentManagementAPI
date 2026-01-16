using API.GraphQL.SellingContract.Inputs;
using Application.Interface.Services;
using Application.Models.SellingContract.Read;
using Application.ResourceParameters;
using Mapster;
namespace API.GraphQL.SellingContract.Queries
{
    [ExtendObjectType(Name = "Query")]
    public class SellingContractQueries
    {
        public async Task<IEnumerable<SellingContractDto>> GetSellingContracts(
               SellingContractFilterInput filterInput,
               [Service] ISellingContractService service)
        {
            var parameters = filterInput.Adapt<SellingContractResourceParameters>();
            var sellingContracts = await service.GetSellingContracts(parameters);
            return sellingContracts;
        }
        public async Task<SellingContractDto> GetSellingContractById(
            Guid id,
            [Service] ISellingContractService service)
        {
            var sellingContract = await service.GetSellingContractById(id);
            return sellingContract;
        }
        public async Task<IEnumerable<SellingContractDto>> GetDeletedSellingContracts(
            SellingContractFilterInput filterInput,
            [Service] ISellingContractService service)
        {
            var parameters = filterInput.Adapt<SellingContractResourceParameters>();
            var sellingContracts = await service.GetDeletedSellingContracts(parameters);
            return sellingContracts;
        }
        public async Task<SellingContractDto> GetDeletedSellingContractById(
            Guid id,
            [Service] ISellingContractService service)
        {
            var sellingContract = await service.GetDeletedSellingContractById(id);
            return sellingContract;
        }
        public async Task<IEnumerable<SellingContractDto>> GetSellingContractsByYear(int year,
            [Service] ISellingContractService service)
        {
            if (year < 0 || year > DateTime.Now.Year)
            {
                throw new ArgumentOutOfRangeException(nameof(year), "Year must be a valid positive integer and cannot be in the future.");
            }
            var sellingContract = await service.GetSellingContractsByYear(year);
            return sellingContract;
        }
        public async Task<IEnumerable<SellingContractDto>> GetSellingContractsByIds(
            IEnumerable<Guid> ids,
            [Service] ISellingContractService service)
        {
            var sellingContracts = await service.GetSellingContractsByIds(ids);
            return sellingContracts;
        }
        public async Task<IEnumerable<SellingContractDto>> GetSellingContractsByCustomerId(
            Guid customerId,
            [Service] ISellingContractService service)
        {
            var sellingContracts = await service.GetSellingContractsByCustomerId(customerId);
            return sellingContracts;
        }
        public async Task<IEnumerable<SellingContractDto>> GetSellingContractsByEquipmentId(
            Guid equipmentId,
            [Service] ISellingContractService service)
        {
            var sellingContracts = await service.GetSellingContractsByEquipmentId(equipmentId);
            return sellingContracts;
        }
    }
}