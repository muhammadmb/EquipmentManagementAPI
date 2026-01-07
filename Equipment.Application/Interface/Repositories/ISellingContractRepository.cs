using Application.BulkOperations;
using Application.ResourceParameters;
using Domain.Entities;
using Shared.Results;

namespace Application.Interface.Repositories
{
    public interface ISellingContractRepository
    {
        Task<PagedList<SellingContract>> GetSellingContracts(SellingContractResourceParameters parameters);
        Task<SellingContract> GetSellingContractById(Guid id, string? fields = null);
        Task<SellingContract> GetSoftDeletedSellingContractsById(Guid id, string? fields = null);
        Task<SellingContract> GetSellingContractByYear(int year);
        Task<SellingContract> GetSellingContractForUpdate(Guid id, string? fields = null);

        Task<IEnumerable<SellingContract>> GetSoftDeletedSellingContracts(SellingContractResourceParameters parameters);
        Task<IEnumerable<SellingContract>> GetSoftDeletedSellingContractsByIds(IEnumerable<Guid> ids, string? fields = null);
        Task<IEnumerable<SellingContract>> GetSellingContractsByCustomerId(Guid customerId, string? fields = null);
        Task<IEnumerable<SellingContract>> GetSellingContractsByEquipment(Guid equipmentId, string? fields = null);
        Task<IEnumerable<SellingContract>> GetSellingContractsByIds(IEnumerable<Guid> ids, string? fields = null);

        Task<bool> SellingContractExists(Guid id);
        Task<bool> CustomerHasContracts(Guid customerId);

        Task CreateSellingContract(SellingContract sellingContract);
        Task UpdateSellingContract(SellingContract sellingContract);
        Task SoftDeleteSellingContract(Guid id);
        Task RestoreSellingContract(Guid id);

        Task<BulkOperationResult> CreateSellingContracts(IEnumerable<SellingContract> sellingContracts);
        Task<BulkOperationResult> SoftDeleteSellingContracts(IEnumerable<Guid> sellingContractsIds);
        Task<BulkOperationResult> RestoreSellingContracts(IEnumerable<Guid> sellingContractsIds);

        Task<bool> SaveChangesAsync();
    }
}
