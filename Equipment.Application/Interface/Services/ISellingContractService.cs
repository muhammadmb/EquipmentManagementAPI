using Application.BulkOperations;
using Application.Models.RentalContractModels.Write;
using Application.Models.SellingContract.Read;
using Application.Models.SellingContract.Write;
using Application.ResourceParameters;
using Domain.Entities;
using Microsoft.AspNetCore.JsonPatch;
using Shared.Results;

namespace Application.Interface.Services
{
    public interface ISellingContractService
    {
        Task<PagedList<SellingContractDto>?> GetSellingContracts(SellingContractResourceParameters parameters);
        Task<SellingContractDto?> GetSellingContractById(Guid id, string? fields = null);
        Task<SellingContractDto?> GetDeletedSellingContractById(Guid id, string? fields = null);
        Task<SellingContractDto> GetSellingContractByYear(int year);

        Task<IEnumerable<SellingContractDto>?> GetSellingContractsByCustomerId(Guid customerId, string? fields = null);
        Task<IEnumerable<SellingContractDto>?> GetSellingContractsByEquipmentId(Guid equipmentId, string? fields = null);
        Task<IEnumerable<SellingContractDto>?> GetSellingContractsByIds(IEnumerable<Guid> ids, string? fields = null);
        Task<IEnumerable<SellingContractDto>?> GetDeletedSellingContracts(SellingContractResourceParameters parameters);

        Task<bool> SellingContractExists(Guid id);
        Task<bool> CustomerHasContracts(Guid customerId);

        Task<SellingContractDto> CreateSellingContract(SellingContractCreateDto sellingContract);
        Task UpdateSellingContract(Guid id, SellingContractUpdateDto sellingContract);
        Task Patch(Guid id, JsonPatchDocument<SellingContractUpdateDto> patchDocument);
        Task SoftDeleteSellingContract(Guid id);
        Task RestoreSellingContract(Guid id);

        Task<BulkOperationResult> CreateSellingContracts(IEnumerable<SellingContractCreateDto> sellingContracts);
        Task<BulkOperationResult> SoftDeleteSellingContracts(IEnumerable<Guid> sellingContractsIds);
        Task<BulkOperationResult> RestoreSellingContracts(IEnumerable<Guid> sellingContractsIds);
    }
}
