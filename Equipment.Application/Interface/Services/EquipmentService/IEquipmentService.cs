using Application.BulkOperations;
using Application.Models.EquipmentModels.Read;
using Application.Models.EquipmentModels.Write;
using Application.ResourceParameters;
using Domain.Enums;
using Microsoft.AspNetCore.JsonPatch;
using Shared.Results;

namespace Application.Interface.Services.EquipmentService
{
    public interface IEquipmentService
    {
        #region Read
        Task<PagedList<EquipmentDto>> GetEquipmentsAsync(EquipmentResourceParameters parameters);

        Task<EquipmentDto?> GetEquipmentByIdAsync(Guid equipmentId, string? fields = null);

        Task<EquipmentDto?> GetSoftDeletedEquipmentByIdAsync(Guid equipmentId, string? fields = null);

        Task<PagedList<EquipmentDto>> GetEquipmentsByStatusAsync(EquipmentStatus status, EquipmentResourceParameters parameters);

        Task<PagedList<EquipmentDto>> GetEquipmentsBySupplierAsync(Guid supplierId, EquipmentResourceParameters parameters);

        Task<PagedList<EquipmentDto>> GetEquipmentsByBrandAsync(EquipmentBrand brand, EquipmentResourceParameters parameters);

        Task<PagedList<EquipmentDto>> GetEquipmentsByTypeAsync(EquipmentType type, EquipmentResourceParameters parameters);

        Task<PagedList<EquipmentDto>> GetSoftDeletedEquipmentsAsync(EquipmentResourceParameters parameters);

        #endregion

        #region Create

        Task<EquipmentDto> CreateEquipmentAsync(EquipmentCreateDto dto);

        #endregion

        #region Update

        Task UpdateEquipmentAsync(Guid equipmentId, EquipmentUpdateDto dto);
        Task Patch(Guid equipmentId, JsonPatchDocument<EquipmentUpdateDto> patchDocumnet);
        #endregion

        #region Status Management
        Task MarkEquipmentAsAvailableAsync(Guid equipmentId);
        Task MarkEquipmentAsSoldAsync(Guid equipmentId);
        Task MarkEquipmentAsUnderMaintenanceAsync(Guid equipmentId);
        #endregion

        #region Soft Delete / Restore
        Task SoftDeleteEquipmentAsync(Guid equipmentId);

        Task RestoreEquipmentAsync(Guid equipmentId);
        #endregion

        #region Bulk operations
        Task<BulkOperationResult> CreateEquipmentCollectionAsync(IEnumerable<EquipmentCreateDto> equipment);
        Task<BulkOperationResult> SoftDeleteEquipmentCollectionAsync(IEnumerable<Guid> equipmentIds);
        Task<BulkOperationResult> RestoreEquipmentCollectionAsync(IEnumerable<Guid> equipmentIds);
        #endregion

        #region Validation / Helpers
        Task<bool> IsEquipmentExistsAsync(Guid equipmentId);
        Task<bool> IsEquipmentAvailableAsync(Guid equipmentId);
        #endregion

    }
}
