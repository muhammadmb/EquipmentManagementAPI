using EquipmentAPI.Entities;
using EquipmentAPI.Helper;
using EquipmentAPI.ResourceParameters;

namespace EquipmentAPI.Repositories.Equipment_Repository
{
    public interface IEquipmentRepository
    {
        Task<PagedList<Equipment>> GetEquipment(EquipmentResourceParameters equipmentResourceParameters);

        Task<Equipment> GetEquipmentById(Guid equipmentId, string fields);

        void Create(Equipment equipment);

        void Delete(Guid equipmentId);

        Task Update(Equipment equipment);

        Task<bool> SaveChangesAsync();
    }
}
