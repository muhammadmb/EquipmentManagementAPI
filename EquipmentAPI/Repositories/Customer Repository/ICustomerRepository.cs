using EquipmentAPI.Entities;
using EquipmentAPI.Helper;
using EquipmentAPI.Helper.BulkOperations;
using EquipmentAPI.ResourceParameters;

namespace EquipmentAPI.Repositories.Customer_Repository
{
    public interface ICustomerRepository
    {
        Task<PagedList<Customer>> GetCustomers(CustomerResourceParameters parameters);

        Task<Customer> GetCustomerById(Guid id, string fields);

        Task<IEnumerable<Customer>> GetCustomersByIds(IEnumerable<Guid> ids, string? fields);

        Task<Customer?> GetCustomerByEmail(string email, string fields);

        Task<int> GetCustomersCount(CustomerResourceParameters? parameters = null);

        Task<IEnumerable<Customer>> GetDeletedCustomers();

        Task<IEnumerable<CustomerPhoneNumber>> GetDeletedPhoneNumbers(Guid customerId);

        Task<bool> CustomerExists(Guid id);

        Task<bool> EmailExists(string email);

        Task<bool> PhoneNumberExists(Guid id);

        void CreateCustomer(Customer customer);

        Task<BulkOperationResult> CreateCustomers(IEnumerable<Customer> customers);

        Task UpdateCustomer(Customer customer);

        Task<CustomerPhoneNumber?> GetPhoneNumber(Guid id);

        Task UpdatePhoneNumber(Guid customerId, Guid phoneNumberId, CustomerPhoneNumber phonenumber);

        void CreateCustomerPhoneNumber(Guid customerId, CustomerPhoneNumber phoneNumber);

        Task DeleteCustomer(Guid id);

        Task<BulkOperationResult> DeleteCustomers(IEnumerable<Guid> customerIds);

        Task<BulkOperationResult> RestoreCustomers(IEnumerable<Guid> customerIds);

        Task<bool> RestoreCustomer(Guid id);

        Task DeletePhoneNumber(Guid customerId, Guid phoneId);

        Task<bool> RestorePhoneNumber(Guid phoneId);

        Task<bool> SaveChangesAsync();
    }
}
