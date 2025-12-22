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

        Task<Equipment> GetEquipmentById(Guid equipmentId, string fields);

        Task<EquipmentStatus> GetEquipmentStatus(Guid customerId);

        Task SetEquipmentStatus(Guid equipmentId, EquipmentStatus status);

        Task<BulkOperationResult> SetEquipmentBulkStatus(IEnumerable<Guid> equipmentIdsList, EquipmentStatus status);

        Task<bool> EquipmentExists(Guid equipmentId)  ;

        void Create(Equipment equipment);

        void Delete(Guid equipmentId);

        Task Update(Equipment equipment);

        Task<bool> SaveChangesAsync();
    }
}
