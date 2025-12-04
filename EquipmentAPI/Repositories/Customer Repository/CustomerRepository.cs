using EquipmentAPI.Contexts;
using EquipmentAPI.Entities;
using EquipmentAPI.Helper;
using EquipmentAPI.Helper.BulkOperations;
using EquipmentAPI.Models.PhoneNumberModels.Write;
using EquipmentAPI.ResourceParameters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace EquipmentAPI.Repositories.Customer_Repository
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public CustomerRepository(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context ??
                throw new ArgumentNullException(nameof(context));
            _cache = cache ??
                throw new ArgumentNullException(nameof(cache));
        }

        public async Task<PagedList<Customer>> GetCustomers(CustomerResourceParameters parameters)
        {
            ArgumentNullException.ThrowIfNull(parameters);

            string cacheKey = $"Customer_{JsonSerializer.Serialize(parameters)}";
            if (_cache.TryGetValue(cacheKey, out PagedList<Customer>? cachedResult))
                return cachedResult!;

            IQueryable<Customer> collection = _context.Customers.AsQueryable();

            if (!string.IsNullOrEmpty(parameters.Fields))
            {
                collection = collection.ApplyInclude(parameters.Fields);
            }

            if (!string.IsNullOrEmpty(parameters.FilterByCountry))
            {
                collection = collection.Where(c => c.Country.ToLower() == parameters.FilterByCountry);
            }

            if (!string.IsNullOrEmpty(parameters.FilterByCity))
            {
                collection = collection.Where(c => c.City.ToLower() == parameters.FilterByCity);
            }

            if (!string.IsNullOrEmpty(parameters.SearchQuery))
            {
                collection = collection.Where(c
                    => EF.Functions.Like(c.Name, parameters.SearchQuery) ||
                    EF.Functions.Like(c.Email, parameters.SearchQuery));
            }

            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                collection = collection.OrderByProperty(parameters.SortBy, parameters.SortDescending);
            }

            var result = await PagedList<Customer>.Create(
                collection,
                parameters.PageNumber,
                parameters.PageSize);

            _cache.Set(cacheKey, result, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1),
                SlidingExpiration = TimeSpan.FromMinutes(1)
            });

            return result;
        }

        public async Task<Customer> GetCustomerById(Guid id, string fields)
        {
            ArgumentNullException.ThrowIfNull(id);

            var cacheKey = $"customer_{id}_{JsonSerializer.Serialize(fields)}";

            if (_cache.TryGetValue<Customer>(cacheKey, out var cachedCustomer))
            {
                return cachedCustomer!;
            }

            IQueryable<Customer> customer = _context.Customers.AsQueryable();

            if (!string.IsNullOrEmpty(fields))
            {
                customer = customer.ApplyInclude(fields);
            }

            var result = await customer.FirstOrDefaultAsync(c => c.Id == id);

            _cache.Set(cacheKey, result, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1),
                SlidingExpiration = TimeSpan.FromMinutes(1)
            });

            return result;
        }

        public async Task<IEnumerable<Customer>> GetCustomersByIds(IEnumerable<Guid> ids, string? fields)
        {
            if (ids == null || !ids.Any()) return Enumerable.Empty<Customer>();

            IQueryable<Customer> collection = _context.Customers.AsQueryable();

            collection = collection.Where(c => ids.Contains(c.Id));

            if (!string.IsNullOrEmpty(fields))
            {
                collection = collection.ApplyInclude(fields);
            }

            return await collection.ToListAsync();
        }

        public async Task<Customer?> GetCustomerByEmail(string email, string fields)
        {
            ArgumentNullException.ThrowIfNull(email);

            var cacheKey = $"customer_{email}_{JsonSerializer.Serialize(fields)}";
            if (_cache.TryGetValue<Customer>(cacheKey, out var cachedCustomer))
            {
                return cachedCustomer;
            }

            IQueryable<Customer> customer = _context.Customers.AsQueryable();

            if (!string.IsNullOrEmpty(fields))
            {
                customer = customer.ApplyInclude(fields);
            }

            var result = await customer.FirstOrDefaultAsync(c => c.Email.ToLower() == email);

            _cache.Set(cacheKey, result, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1),
                SlidingExpiration = TimeSpan.FromMinutes(1)
            });

            return result;
        }

        public async Task<int> GetCustomersCount(CustomerResourceParameters? parameters = null)
        {
            IQueryable<Customer> collection = _context.Customers.AsQueryable();

            if (parameters != null)
            {
                if (!string.IsNullOrEmpty(parameters.FilterByCountry))
                {
                    collection = collection.Where(c => c.Country.ToLower() == parameters.FilterByCountry);
                }

                if (!string.IsNullOrEmpty(parameters.FilterByCity))
                {
                    collection = collection.Where(c => c.City.ToLower() == parameters.FilterByCity);
                }
            }
            return await collection.CountAsync();
        }

        public async Task<IEnumerable<Customer>> GetDeletedCustomers()
        {
            return await _context.Customers
                .IgnoreQueryFilters()
                .Where(c => c.DeletedDate != null)
                .Include(c => c.PhoneNumbers)
                .ToListAsync();
        }

        public async Task<IEnumerable<CustomerPhoneNumber>> GetDeletedPhoneNumbers(Guid customerId)
        {
            return await _context.CustomerPhoneNumbers
                .IgnoreQueryFilters()
                .Where(pn => pn.CustomerId == customerId && pn.DeletedDate != null)
                .ToListAsync();
        }

        public async Task<bool> CustomerExists(Guid id)
        {
            return await _context.Customers.AnyAsync(c => c.Id == id);
        }
        
        public async Task<bool> EmailExists(string email)
        {
            return await _context.Customers
                .AnyAsync(c => c.Email == email);
        }

        public async Task<bool> PhoneNumberExists(Guid id)
        {
            return await _context.CustomerPhoneNumbers.AnyAsync(c => c.Id == id);
        }

        public void CreateCustomer(Customer customer)
        {
            _context.Customers.Add(customer);
        }

        public async Task UpdateCustomer(Customer customer)
        {
            var currentCustomer = await _context
                .Customers
                .Include(c => c.PhoneNumbers)
                .AsTracking()
                .FirstOrDefaultAsync(c => c.Id == customer.Id);

            if (currentCustomer == null) return;

            if (!currentCustomer.RowVersion.SequenceEqual(customer.RowVersion))
            {
                throw new DbUpdateConcurrencyException("The entity has been modified by another user.");
            }

            var distinctNumbers = customer.PhoneNumbers
                .GroupBy(pn => pn.Number)
                .Select(n => n.First())
                .ToList();

            foreach (var phone in currentCustomer.PhoneNumbers)
            {
                if (!distinctNumbers.Any(pn => pn.Id == phone.Id))
                {
                    phone.DeletedDate = DateTime.Now;
                }
            }
            foreach (var phone in customer.PhoneNumbers)
            {
                if (phone.Id != Guid.Empty)
                {
                    var existing = currentCustomer.PhoneNumbers.FirstOrDefault(p => p.Id == phone.Id);
                    if (existing != null)
                    {
                        existing.Number = phone.Number;
                        existing.UpdateDate = DateTime.Now;
                    }
                    else
                    {
                        currentCustomer.PhoneNumbers.Add(new CustomerPhoneNumber
                        {
                            Number = phone.Number,
                            CustomerId = currentCustomer.Id,
                            AddedDate = DateTime.Now,
                            RowVersion = [1, 1, 1, 1]
                        });
                    }
                }
                else
                {
                    currentCustomer.PhoneNumbers.Add(new CustomerPhoneNumber
                    {
                        Number = phone.Number,
                        CustomerId = currentCustomer.Id,
                        AddedDate = DateTime.Now,
                        RowVersion = [1, 1, 1, 1]
                    });
                }
            }

            currentCustomer.Name = customer.Name;
            currentCustomer.Email = customer.Email;
            currentCustomer.City = customer.City;
            currentCustomer.Country = customer.Country;
            currentCustomer.UpdateDate = DateTime.Now;
        }

        public async Task UpdatePhoneNumber(Guid customerId, List<CustomerPhoneNumberUpdateDto> phonenumbers)
        {
            var customer = await _context.Customers
                .Include(c => c.PhoneNumbers)
                .IgnoreQueryFilters()
                .AsTracking()
                .FirstOrDefaultAsync(c => c.Id == customerId);

            var distinctNumbers = phonenumbers
                .GroupBy(c => c.Number)
                .Select(n => n.First())
                .ToList();

            foreach (var dNumber in distinctNumbers)
            {
                var existing = customer.PhoneNumbers
                    .FirstOrDefault(pn => pn.Id == dNumber.Id);

                if (existing != null)
                {
                    if (!existing.RowVersion.SequenceEqual(dNumber.RowVersion))
                        throw new DbUpdateConcurrencyException("The entity has been modified by another user.");
                    if (existing.Number != dNumber.Number)

                    {
                        existing.UpdateDate = DateTime.Now;
                        existing.Number = dNumber.Number;
                        existing.DeletedDate = null;
                    }
                }
                else
                {
                    customer.PhoneNumbers.Add(new CustomerPhoneNumber
                    {
                        CustomerId = customerId,
                        Number = dNumber.Number,
                        AddedDate = DateTime.Now,
                        RowVersion = [1, 1, 1, 1]
                    });
                }
            }
        }

        public async Task DeleteCustomer(Guid id)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
            customer.DeletedDate = DateTime.Now;
            _context.Customers.Update(customer);
        }

        public async Task<bool> RestoreCustomer(Guid id)
        {
            var customer = await _context.Customers
                .IgnoreQueryFilters()
                .Include(c => c.PhoneNumbers)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null || customer.DeletedDate == null) return false;
            customer.DeletedDate = null;

            foreach (var phoneNumber in customer.PhoneNumbers)
                phoneNumber.DeletedDate = null;

            _context.Customers.Update(customer);
            return true;
        }

        public async Task DeletePhoneNumber(Guid customerId, Guid phoneId)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == customerId);
            if (customer == null) return;

            var customerPhoneNumber = await _context
                .CustomerPhoneNumbers
                .FirstOrDefaultAsync(c => c.Id == phoneId);

            if (customerPhoneNumber == null) return;

            customerPhoneNumber.DeletedDate = DateTime.Now;
            _context.CustomerPhoneNumbers.Update(customerPhoneNumber);
        }

        public async Task<bool> RestorePhoneNumber(Guid phoneId)
        {
            var customerPhoneNumber = await _context.CustomerPhoneNumbers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == phoneId);

            if (customerPhoneNumber == null || customerPhoneNumber.DeletedDate != null) return false;

            customerPhoneNumber.DeletedDate = null;
            _context.CustomerPhoneNumbers.Update(customerPhoneNumber);
            return true;
        }

        public async Task<BulkOperationResult> CreateCustomers(IEnumerable<Customer> customers)
        {
            if (customers == null || !customers.Any())
                throw new ArgumentException("Customers collection cannot be empty", nameof(customers));
            var result = new BulkOperationResult();
            var customerList = customers.ToList();
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var customer in customerList)
                {
                    try
                    {
                        _context.Customers.Add(customer);
                        result.SuccessCount++;
                        result.SuccessIds.Add(customer.Id);
                    }
                    catch (Exception ex)
                    {
                        result.FailureCount++;
                        result.Errors.Add(new BulkOperationError
                        {
                            EntityId = customer.Id,
                            ErrorMessage = ex.Message
                        });
                    }
                }
                await SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }

            return result;
        }

        public async Task<BulkOperationResult> DeleteCustomers(IEnumerable<Guid> customerIds)
        {
            if (customerIds == null || !customerIds.Any())
                throw new ArgumentException("Customers ids cannot be empty", nameof(customerIds));

            var result = new BulkOperationResult();
            var idsList = customerIds.ToList();

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var id in customerIds)
                {
                    try
                    {
                        await DeleteCustomer(id);
                        result.SuccessCount++;
                        result.SuccessIds.Add(id);

                    }
                    catch (Exception ex)
                    {
                        result.FailureCount++;
                        result.Errors.Add(new BulkOperationError
                        {
                            EntityId = id,
                            ErrorMessage = ex.Message
                        });
                    }
                }
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
            return result;
        }

        public async Task<BulkOperationResult> RestoreCustomers(IEnumerable<Guid> customerIds)
        {
            if (customerIds == null || !customerIds.Any())
                throw new ArgumentException("Customers ids cannot be empty", nameof(customerIds));

            var result = new BulkOperationResult();
            var idsList = customerIds.ToList();

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var id in customerIds)
                {
                    try
                    {
                        await RestoreCustomer(id);
                        result.SuccessCount++;
                        result.SuccessIds.Add(id);

                    }
                    catch (Exception ex)
                    {
                        result.FailureCount++;
                        result.Errors.Add(new BulkOperationError
                        {
                            EntityId = id,
                            ErrorMessage = ex.Message
                        });
                    }
                }
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
            return result;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() > 0);
        }
    }
}
