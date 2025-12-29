using Application.BulkOperations;
using Application.Models.RentalContractModels.Read;
using Application.Models.RentalContractModels.Write;
using Application.ResourceParameters;
using Microsoft.AspNetCore.JsonPatch;
using Shared.Results;

namespace Application.Interface.Services
{
    public interface IRentalContractService
    {
        Task<PagedList<RentalContractDto>?> GetRentalContracts(RentalContractResourceParameters parameters);
        Task<RentalContractDto?> GetRentalContractById(Guid id, string? fields);
        Task<IEnumerable<RentalContractDto>> GetRentalContractsByCustomerId(Guid customerId, string? fields);
        Task<IEnumerable<RentalContractDto>> GetRentalContractsByEquipmentId(Guid equipmentId, string? fields);
        Task<IEnumerable<RentalContractDto>> GetRentalContractsByIds(IEnumerable<Guid> ids, string? fields);
        Task<IEnumerable<RentalContractDto>> GetActiveContracts(string? fields);
        Task<IEnumerable<RentalContractDto>> GetExpiredContracts(int daysUntilExpiration, string? fields);

        Task<RentalContractDto> GetDeletedRentalContractById(Guid id);
        Task<IEnumerable<RentalContractDto>> GetDeletedRentalContracts();

        Task<bool> RentalContractExists(Guid id);
        Task<bool> CustomerHasContracts(Guid customerId);
        Task<bool> EquipmentHasContracts(Guid equipmentId);
        Task<bool> HasOverlappingContracts(Guid equipmentId, DateTimeOffset startDate, DateTimeOffset endDate, Guid? excludeContractId = null);

        Task ActiveRentalContract(Guid contractId);
        Task CancelRentalContract(Guid contractId);
        Task SuspendRentalContract(Guid contractId);
        Task ResumeRentalContract(Guid contractId);
        Task FinishRentalContract(Guid contractId);
        Task FinishExpiredContracts();

        Task<RentalContractDto> CreateRentalContract(RentalContractCreateDto rentalContract);
        Task<BulkOperationResult> CreateRentalContracts(IEnumerable<RentalContractCreateDto> rentalContracts);

        Task UpdateRentalContract(Guid id, RentalContractUpdateDto rentalContract);
        Task Patch(Guid id, JsonPatchDocument<RentalContractUpdateDto> patchDocument);

        Task SoftDeleteRentalContract(Guid id);
        Task<BulkOperationResult> DeleteRentalContracts(IEnumerable<Guid> ids);

        Task RestoreRentalContract(Guid id);
        Task<BulkOperationResult> RestoreRentalContracts(IEnumerable<Guid> ids);
    }
}
