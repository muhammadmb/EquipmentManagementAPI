using Application.BulkOperations;
using Application.ResourceParameters;
using Domain.Entities;
using Shared.Results;

namespace Application.Interface.Repositories
{
    public interface IRentalContractRepository
    {
        Task<PagedList<RentalContract>?> GetRentalContracts(
        RentalContractResourceParameters parameters);

        Task<RentalContract> GetRentalContractById(Guid id, string? fields);

        Task<IEnumerable<RentalContract>> GetRentalContractsByCustomerId(Guid customerId, string? fields);
        Task<IEnumerable<RentalContract>> GetRentalContractsByEquipmentId(Guid equipmentId, string? fields);
        Task<IEnumerable<RentalContract>> GetRentalContractsByIds(IEnumerable<Guid> ids, string? fields);

        Task<IEnumerable<RentalContract>> GetActiveContracts(string? fields);
        Task<IEnumerable<RentalContract>> GetExpiredContracts(int daysUntilExpiration, string? fields);
        Task<RentalContract?> GetDeletedRentalContractById(Guid id);
        Task<IEnumerable<RentalContract>> GetDeletedRentalContracts();

        Task<bool> RentalContractExists(Guid id);
        Task<bool> CustomerHasContracts(Guid customerId);
        Task<bool> EquipmentHasContracts(Guid equipmentId);
        Task<bool> HasOverlappingContracts(Guid equipmentId, DateTimeOffset startDate, DateTimeOffset endDate, Guid? excludeContractId = null);

        Task<IEnumerable<RentalContract>> GetActiveContractsEndingBefore(DateTimeOffset date);

        Task CreateRentalContract(RentalContract rentalContract);
        Task UpdateRentalContract(RentalContract rentalContract);
        Task SoftDeleteRentalContract(Guid id);
        Task<bool> RestoreRentalContract(Guid id);

        Task<BulkOperationResult> CreateRentalContracts(IEnumerable<RentalContract> rentalContracts);
        Task<BulkOperationResult> DeleteRentalContracts(IEnumerable<Guid> rentalContractsIds);
        Task<BulkOperationResult> RestoreRentalContracts(IEnumerable<Guid> rentalContractsIds);

        Task<bool> SaveChangesAsync();
    }
}
