using Application.BulkOperations;
using Application.ResourceParameters;
using Domain.Entities;
using Domain.Enums;
using Shared.Results;

namespace Application.Interface.Repositories
{
    public interface IEquipmentRepository
    {
        Task<PagedList<Equipment>> GetEquipment(EquipmentResourceParameters equipmentResourceParameters);
        Task<Equipment?> GetEquipmentById(Guid equipmentId, string? fields = null);
        Task<Equipment?> GetSoftDeletedEquipmentById(Guid equipmentId, string? fields = null);
        Task<Equipment?> GetEquipmentForUpdate(Guid equipmentId, string? fields = null);

        Task<PagedList<Equipment>> GetSoftDeletedEquipment(EquipmentResourceParameters parameters);
        Task<PagedList<Equipment>> GetSoftDeletedEquipmentByIds(IEnumerable<Guid> ids, EquipmentResourceParameters parameters);
        Task<PagedList<Equipment>> GetEquipmentByStatus(EquipmentStatus status, EquipmentResourceParameters parameters);
        Task<PagedList<Equipment>> GetEquipmentBySupplier(Guid supplierId, EquipmentResourceParameters parameters);
        Task<PagedList<Equipment>> GetEquipmentByBrand(EquipmentBrand brand, EquipmentResourceParameters parameters);
        Task<PagedList<Equipment>> GetEquipmentByType(EquipmentType type, EquipmentResourceParameters parameters);
        Task<EquipmentStatus?> GetEquipmentStatus(Guid customerId);

        Task<bool> EquipmentExists(Guid equipmentId);

        Task CreateEquipment(Equipment equipment);

        [Obsolete("Use CreateEquipment(Equipment equipment) instead.")]
        void Create(Equipment equipment);
        Task UpdateEquipment(Equipment equipment);

        [Obsolete("Use UpdateEquipment(Equipment equipment) instead.")]
        Task Update(Equipment equipment);
        Task SoftDeleteEquipment(Equipment equipment);

        [Obsolete("Use SoftDeleteEquipment(Equipment equipment) instead.")]
        public void Delete(Guid equipmentId);
        Task RestoreEquipment(Equipment equipment);
        Task SetEquipmentStatus(Equipment equipment, EquipmentStatus status);

        [Obsolete("Use SetEquipmentStatus(Equipment equipment, EquipmentStatus status) instead.")]
        Task SetEquipmentStatus(Guid equipmentId, EquipmentStatus status);

        Task<BulkOperationResult> CreateEquipmentBulk(IEnumerable<Equipment> equipmentList);
        Task<BulkOperationResult> SetEquipmentBulkStatus(IEnumerable<Guid> equipmentIdsList, EquipmentStatus status);
        Task<BulkOperationResult> SoftDeleteBulk(IEnumerable<Guid> equipmentIds);
        Task<BulkOperationResult> RestoreBulk(IEnumerable<Guid> equipmentIds);


        #region Analytics Support
        Task<int> GetTotalCount();
        Task<int> GetCountByStatus(EquipmentStatus status);

        Task<decimal> GetTotalPurchaseCost();
        Task<decimal> GetTotalExpenses();
        Task<decimal> GetTotalShippingCost();

        Task<decimal> GetAveragePrice();

        Task<IDictionary<EquipmentBrand, int>> GetCountByBrand();
        Task<IDictionary<EquipmentType, int>> GetCountByType();
        Task<IDictionary<Guid, int>> GetCountBySupplier();

        Task<IDictionary<EquipmentBrand, decimal>> GetTotalValueByBrand();
        Task<IDictionary<EquipmentType, decimal>> GetTotalValueByType();
        Task<IDictionary<Guid, decimal>> GetTotalValueBySupplier();

        Task<IDictionary<int, int>> GetCountByManufactureYear();
        Task<IDictionary<int, int>> GetPurchasedCountPerYear();

        #endregion

        Task<bool> SaveChangesAsync();
    }
}
