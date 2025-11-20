using EquipmentAPI.Entities;
using EquipmentAPI.Helper;
using EquipmentAPI.Models.PhoneNumberModels.Write;
using EquipmentAPI.ResourceParameters;

namespace EquipmentAPI.Repositories.SupplierRepository
{
    public interface ISupplierRepository
    {
        Task<PagedList<Supplier>> GetSuppliers(SupplierResourceParameters parameters);

        Task<Supplier> GetSupplier(Guid supplierId, string fields);

        Task<bool> Exists(Guid supplierId);

        Task<bool> PhoneNumberExist(Guid id);

        void Create(Supplier supplier);
        
        Task Update (Supplier supplier);

        Task UpdatePhoneNumber(Guid supplierId, List<SupplierPhoneNumberUpdateDto> phonenumbers);

        Task Delete(Guid supplierId);
        
        Task DeletePhoneNumber(Guid supplierId, Guid phoneId);

        Task<bool> RestoreSupplier(Guid id);

        Task<bool> SaveChangesAsync();

        Task<bool> RestorePhoneNumber(Guid phoneId);
    }
}
