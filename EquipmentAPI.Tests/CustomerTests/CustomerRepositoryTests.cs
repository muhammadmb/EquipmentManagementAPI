using EquipmentAPI.Contexts;
using EquipmentAPI.Entities;
using EquipmentAPI.Helper;
using EquipmentAPI.Repositories.Customer_Repository;
using EquipmentAPI.ResourceParameters;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;

namespace EquipmentAPI.Tests.CustomerTests
{
    public class CustomerRepositoryTests
    {
        private readonly ApplicationDbContext _context;
        private readonly CustomerRepository _customerRepository;
        private readonly IMemoryCache _cache;

        public CustomerRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"{Guid.NewGuid().ToString()}_CustomerTestsDb")
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            _context = new ApplicationDbContext(options);
            _cache = new MemoryCache(new MemoryCacheOptions());
            _customerRepository = new CustomerRepository(_context, _cache);

            SeedDatabase();
        }

        private Customer _customerAhmed;
        private Customer _customerHassan;
        private Customer _customerAli;

        private void SeedDatabase()
        {
            _customerAhmed = new Customer()
            {
                Id = Guid.NewGuid(),
                Name = "Ahmed",
                Email = "ahmed@example.com",
                Country = "Egypt",
                City = "Alexandria",
                AddedDate = DateTime.UtcNow,
                PhoneNumbers = new List<CustomerPhoneNumber>()
                {
                    new CustomerPhoneNumber()
                    {
                        Id = Guid.NewGuid(),
                        Number = "+20987456321",
                        RowVersion = new byte[] { 1, 2, 3, 4 }
                    }
                },
                RowVersion = new byte[] { 0, 2, 3, 4 },
            };

            _customerHassan = new Customer()
            {
                Id = Guid.NewGuid(),
                Name = "Hassan",
                Email = "hassan@example.com",
                Country = "Egypt",
                City = "Cairo",
                AddedDate = DateTime.UtcNow,
                PhoneNumbers = new List<CustomerPhoneNumber>()
                {
                    new CustomerPhoneNumber()
                    {
                        Id = Guid.NewGuid(),
                        Number = "+201234567890",
                        RowVersion = new byte[] { 1, 7, 7, 4 }
                    }
                },
                RowVersion = new byte[] { 1, 2, 6, 5 },
            };

            _customerAli = new Customer()
            {
                Id = Guid.NewGuid(),
                Name = "Ali",
                Email = "Aly@example.com",
                Country = "USA",
                City = "New York",
                AddedDate = DateTime.UtcNow,
                DeletedDate = DateTime.UtcNow,
                PhoneNumbers = new List<CustomerPhoneNumber>()
                {
                    new CustomerPhoneNumber()
                    {
                        Id = Guid.NewGuid(),
                        Number = "+201277767890",
                        RowVersion = new byte[] { 1, 7, 7, 5 },
                        DeletedDate = DateTime.UtcNow
                    }
                },
                RowVersion = new byte[] { 1, 2, 5, 5 },
            };

            _context.Customers.AddRange(_customerAhmed, _customerHassan, _customerAli);
            _context.SaveChanges();
        }

        // ============================================================================
        // GET CUSTOMERS
        // ============================================================================

        [Fact]
        public async Task GetAllCustomers_ReturnsPagedList()
        {
            // Act
            var parameters = new CustomerResourceParameters
            {
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var customers = await _customerRepository.GetCustomers(parameters);

            // Assert
            customers.Should().BeOfType<PagedList<Customer>>();
            Assert.NotNull(customers);
            Assert.Equal(2, customers.Count());
        }

        [Fact]
        public async Task GetCustomers_FiltersByCountry_WhenCountryProvided()
        {
            // Act
            var parameters = new CustomerResourceParameters
            {
                FilterByCountry = "Egypt"
            };

            // Act
            var customers = await _customerRepository.GetCustomers(parameters);

            // Assert
            customers.Should().BeOfType<PagedList<Customer>>();
            customers.All(s => s.Country == "Egypt").Should().BeTrue();
            customers.Select(s => s.Name).Should().BeEquivalentTo("Hassan", "Ahmed");
            Assert.NotNull(customers);
            Assert.Equal(2, customers.Count());
        }

        [Fact]
        public async Task GetCustomers_FiltersByCity_WhenCityProvided()
        {
            // Arrange
            var parameters = new CustomerResourceParameters
            {
                FilterByCity = "Alexandria"
            };

            // Act
            var customers = await _customerRepository.GetCustomers(parameters);

            // Assert
            customers.Should().BeOfType<PagedList<Customer>>();
            customers.All(s => s.City == "Alexandria").Should().BeTrue();
            customers.Select(s => s.Name).Should().BeEquivalentTo("Ahmed");
            Assert.NotNull(customers);
            Assert.Equal(1, customers.Count());
        }

        [Fact]
        public async Task GetCustomers_SearchQuery_whenSearchProvided()
        {
            // Arrange
            var parameters = new CustomerResourceParameters
            {
                SearchQuery = "has"
            };

            // Act
            var customers = await _customerRepository.GetCustomers(parameters);

            // Assert
            customers.Should().BeOfType<PagedList<Customer>>();
            customers.All(s => s.Name.Contains("hass", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
            customers.Select(s => s.Name).Should().BeEquivalentTo("Hassan");
            Assert.NotNull(customers);
            Assert.Equal(1, customers.Count());
        }

        [Fact]
        public async Task GetCustomers_AppliesFieldSelection_WhenFieldsProvided()
        {
            // Arrange
            var parameters = new CustomerResourceParameters
            {
                Fields = "phoneNumbers"
            };

            // Act
            var customers = await _customerRepository.GetCustomers(parameters);

            // Assert
            customers.Should().BeOfType<PagedList<Customer>>();
            foreach (var customer in customers)
            {
                customer.PhoneNumbers.Count().Should().NotBe(0);
            }
        }

        [Fact]
        public async Task GetCustomersFromCache_ReturnsCachedCustomers_WhenCacheIsPopulated()
        {
            // Arrange
            var parameters = new CustomerResourceParameters
            {
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var firstResult = await _customerRepository.GetCustomers(parameters);
            _context.ChangeTracker.Clear();
            _context.Customers.RemoveRange(_context.Customers);
            _context.ChangeTracker.Clear();
            await _context.SaveChangesAsync();
            var cachedResult = await _customerRepository.GetCustomers(parameters);

            // Assert
            cachedResult.Should().NotBeNull();
            cachedResult.Should().BeSameAs(firstResult);
            cachedResult.Count.Should().Be(2);
            cachedResult.Select(c => c.Name)
                        .Should().BeEquivalentTo("Ahmed", "Hassan");
        }

        [Fact]
        public async Task GetCustomersByIds_ReturnsMatchingCustomers_WithValidIds()
        {
            // Arrange
            var ids = new List<Guid> { _customerAhmed.Id, _customerHassan.Id };

            // Act
            var customers = await _customerRepository.GetCustomersByIds(ids, string.Empty);

            // Assert
            customers.Should().NotBeNull();
            customers.Count().Should().Be(2);
            customers.Select(c => c.Name).Should().BeEquivalentTo("Ahmed", "Hassan");
        }

        [Fact]
        public async Task GetCustomersByIds_ReturnsEmptyList_WithEmptyIds()
        {
            // Arrange
            var ids = new List<Guid> { Guid.Empty, Guid.Empty };

            // Act
            var customers = await _customerRepository.GetCustomersByIds(ids, string.Empty);

            // Assert
            customers.Count().Equals(0);
        }

        [Fact]
        public async Task GetCustomerById_ReturnsCustomer_WhenCustomerExists()
        {
            // Arrange
            var id = _customerHassan.Id;

            // Act
            var customer = await _customerRepository.GetCustomerById(id, string.Empty);

            // Assert
            customer.Should().NotBeNull();
            customer.Id.Should().Be(_customerHassan.Id);
            customer.Name.Should().Be("Hassan");
        }

        [Fact]
        public async Task GetCustomerById_ReturnsNull_WhenCustomerDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var customer = await _customerRepository.GetCustomerById(id, null);

            // Assert
            customer.Should().BeNull();
        }

        [Fact]
        public async Task GetCustomerByIdFromCache_ReturnsCachedCustomers_WhenCacheIsPopulated()
        {
            // Arrange
            var id = _customerHassan.Id;

            // Act
            var customer = await _customerRepository.GetCustomerById(id, null);
            _context.ChangeTracker.Clear();
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
            var cachedCustomer = await _customerRepository.GetCustomerById(id, null);

            // Assert
            cachedCustomer.Should().NotBeNull();
        }

        [Fact]
        public async Task GetCustomerById_AppliesFieldSelection_WhenFieldsProvided()
        {
            // Arrange
            var id = _customerAhmed.Id;
            var fields = "phoneNumbers";

            // Act
            var customer = await _customerRepository.GetCustomerById(id, fields);

            // Assert
            customer.Should().NotBeNull();
            customer.Id.Should().Be(_customerAhmed.Id);
            customer.PhoneNumbers.Count().Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetCustomerByEmail_ReturnsCustomer_WhenCustomerExists()
        {
            // Arrange
            var email = _customerHassan.Email;

            // Act
            var customer = await _customerRepository.GetCustomerByEmail(email, null);

            // Assert
            customer.Should().NotBeNull();
            customer.Id.Should().Be(_customerHassan.Id);
            customer.Name.Should().Be("Hassan");
        }

        [Fact]
        public async Task GetCustomerByEmail_ReturnsNull_WhenCustomerDoesNotExist()
        {
            // Arrange
            var email = "nonexist@example.com";

            // Act
            var customer = await _customerRepository.GetCustomerByEmail(email, null);

            // Assert
            customer.Should().BeNull();
        }

        [Fact]
        public async Task GetCustomersNumber_ReturnCount_WhenCustomersExist()
        {
            // Act
            var count = await _customerRepository.GetCustomersCount();

            // Assert
            count.Should().Be(2);
        }

        [Fact]
        public async Task GetDeletedCustomers_ReturnDeletedCustomers()
        {
            // Arrange
            var id = _customerAli.Id;

            // Act
            var deletedCustomers = await _customerRepository.GetDeletedCustomers();

            // Assert
            deletedCustomers.Count().Should().Be(1);
            deletedCustomers.First().Id.Should().Be(_customerAli.Id);
            deletedCustomers.First().DeletedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDeletedPhoneNumbers_ReturnDeletedPhoneNumbers()
        {
            // Arrange
            var customerId = _customerAli.Id;
            var id = _customerAli.PhoneNumbers.First().Id;

            // Act
            var deletedPhoneNumbers = await _customerRepository.GetDeletedPhoneNumbers(customerId);

            // Assert
            deletedPhoneNumbers.Count().Should().Be(1);
            deletedPhoneNumbers.First().Id.Should().Be(id);
            deletedPhoneNumbers.First().DeletedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task GetCustomersNumber_ReturnCount_WhenFieldsProvided()
        {
            // Arrange
            var parameters = new CustomerResourceParameters
            {
                FilterByCity = "Alexandria"
            };

            // Act
            var count = await _customerRepository.GetCustomersCount(parameters);

            // Assert
            count.Should().Be(1);
        }

        [Fact]
        public async Task CustomerExists_ReturnsTrue_WhenCustomerExists()
        {
            // Arrange
            var id = _customerAhmed.Id;
            // Act
            var exists = await _customerRepository.CustomerExists(id);
            // Assert
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task CustomerExists_ReturnsFalse_WhenCustomerDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            // Act
            var exists = await _customerRepository.CustomerExists(id);
            // Assert
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task CustomerEmailExists_ReturnsTrue_WhenEmailExists()
        {
            // Arrange
            var email = _customerHassan.Email;
            // Act
            var exists = await _customerRepository.EmailExists(email);
            // Assert
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task CustomerEmailExists_ReturnsFalse_WhenEmailDoesNotExist()
        {
            // Arrange
            var email = "A@a.com";

            // Act
            var exists = await _customerRepository.EmailExists(email);

            // Assert
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task PhoneNumberExists_ReturnsTrue_WhenPhoneNumberExists()
        {
            // Arrange
            var id = _customerAhmed.PhoneNumbers.First().Id;
            // Act
            var exists = await _customerRepository.PhoneNumberExists(id);
            // Assert
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task PhoneNumberExists_ReturnsFalse_WhenPhoneNumberDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            // Act
            var exists = await _customerRepository.PhoneNumberExists(id);
            // Assert
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task CreateCustomer_AddsCustomerToContext()
        {
            // Arrange
            var newCustomer = new Customer()
            {
                Id = Guid.NewGuid(),
                Name = "Test Customer",
                Email = "test@example.com",
                Country = "Test Country",
                City = "Test City",
                AddedDate = DateTime.UtcNow,
                RowVersion = []
            };

            // Act
            _customerRepository.CreateCustomer(newCustomer);
            await _customerRepository.SaveChangesAsync();

            // Assert
            _customerRepository.CustomerExists(newCustomer.Id).Result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateCustomer_UpdatesExistingCustomer()
        {
            // Arrange
            _customerAhmed.Name = "Updated Name";
            // Act
            await _customerRepository.UpdateCustomer(_customerAhmed);
            await _customerRepository.SaveChangesAsync();
            // Assert
            var updatedCustomer = await _customerRepository.GetCustomerById(_customerAhmed.Id, null);
            updatedCustomer.Name.Should().Be("Updated Name");
        }

        [Fact]
        public async Task UpdateCustomer_ThrowsConcurrencyException_WhenRowVersionMismatch()
        {
            // Arrange
            _customerAhmed.RowVersion = new byte[] { 9, 9, 9, 9 }; // Simulate a different RowVersion
            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
            {
                await _customerRepository.UpdateCustomer(_customerAhmed);
            });
        }

        [Fact]
        public async Task GetPhoneNumberById_ReturnsPhoneNumber_WhenExists()
        {
            // Arrange
            var phoneNumberId = _customerHassan.PhoneNumbers.First().Id;

            // Act
            var phoneNumber = await _customerRepository.GetPhoneNumber(phoneNumberId);

            // Assert
            phoneNumber.Should().NotBeNull();
            phoneNumber.Id.Should().Be(phoneNumberId);
        }

        [Fact]
        public async Task GetPhoneNumberById_ReturnsNull_WhenDoesNotExist()
        {
            // Arrange
            var phoneNumberId = Guid.NewGuid();
            // Act
            var phoneNumber = await _customerRepository.GetPhoneNumber(phoneNumberId);
            // Assert
            phoneNumber.Should().BeNull();
        }

        [Fact]
        public async Task UpdatePhoneNumber_UpdatesExistingPhoneNumber()
        {
            // Arrange
            var customerId = _customerAhmed.Id;
            var phoneNumberId = _customerAhmed.PhoneNumbers.First().Id;
            var phoneNumber = _customerAhmed.PhoneNumbers.First();
            phoneNumber.Number = "+20999999999";
            // Act
            await _customerRepository.UpdatePhoneNumber(customerId, phoneNumberId, phoneNumber);
            await _customerRepository.SaveChangesAsync();
            // Assert
            var updatedPhoneNumber = await _customerRepository.GetPhoneNumber(phoneNumber.Id);
            updatedPhoneNumber.Number.Should().Be("+20999999999");
        }

        [Fact]
        public async Task UpdatePhoneNumber_ThrowsConcurrencyException_WhenRowVersionMismatch()
        {
            // Arrange
            var customerId = _customerAhmed.Id;
            var phoneNumberId = _customerAhmed.PhoneNumbers.First().Id;
            var phoneNumber = _customerAhmed.PhoneNumbers.First();
            phoneNumber.RowVersion = new byte[] { 9, 9, 9, 9 }; // Simulate a different RowVersion
            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
            {
                await _customerRepository.UpdatePhoneNumber(customerId, phoneNumberId, phoneNumber);
            });
        }
        [Fact]
        public async Task CreatePhoneNumber_AddsPhoneNumberToCustomer()
        {
            // Arrange
            var customerId = _customerHassan.Id;
            var newPhoneNumber = new CustomerPhoneNumber()
            {
                Id = Guid.NewGuid(),
                Number = "+20111222333",
                RowVersion = new byte[] { 1, 1, 1, 1 }
            };
            // Act
            _customerRepository.CreateCustomerPhoneNumber(customerId, newPhoneNumber);
            await _customerRepository.SaveChangesAsync();
            // Assert
            var customer = await _customerRepository.GetCustomerById(customerId, "phoneNumbers");
            customer.PhoneNumbers.Should().ContainSingle(pn => pn.Id == newPhoneNumber.Id);
        }

        [Fact]
        public async Task CreatePhoneNumber_ThrowsException_WhenCustomerDoesNotExist()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var newPhoneNumber = new CustomerPhoneNumber()
            {
                Id = Guid.NewGuid(),
                Number = "+20111222333",
                RowVersion = new byte[] { 1, 1, 1, 1 }
            };
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                _customerRepository.CreateCustomerPhoneNumber(customerId, newPhoneNumber);
                await _customerRepository.SaveChangesAsync();
            });
        }

        [Fact]
        public async Task DeleteCustomer_RemovesCustomer()
        {
            // Arrange
            var customerId = _customerHassan.Id;

            // Act
            _context.ChangeTracker.Clear();
            await _customerRepository.DeleteCustomer(customerId);
            await _customerRepository.SaveChangesAsync();

            // Assert
            var customer = await _customerRepository.CustomerExists(customerId);
            customer.Should().BeFalse();
        }

        [Fact]
        public async Task ResoreCustomer_RestoreDeletedCustomer()
        {
            // Arrange
            var customerId = _customerAli.Id;
            // Act
            _context.ChangeTracker.Clear();
            await _customerRepository.RestoreCustomer(customerId);
            await _customerRepository.SaveChangesAsync();
            // Assert
            var customer = await _customerRepository.GetCustomerById(customerId, null);
            customer.Should().NotBeNull();
            customer.DeletedDate.Should().BeNull();
        }

        [Fact]
        public async Task ResoreCustomer_ReturnFalseWhenDeletedDateEqualNull()
        {
            // Arrange
            var customerId = _customerAhmed.Id;
            // Act
            _context.ChangeTracker.Clear();
            var result = await _customerRepository.RestoreCustomer(customerId);
            await _customerRepository.SaveChangesAsync();
            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeletePhoneNumber_RemovesPhoneNumber()
        {
            // Arrange
            var customerId = _customerAhmed.Id;
            var phoneNumberId = _customerAhmed.PhoneNumbers.First().Id;

            // Act
            _context.ChangeTracker.Clear();
            await _customerRepository.DeletePhoneNumber(customerId, phoneNumberId);
            await _customerRepository.SaveChangesAsync();

            // Assert
            var phoneNumber = await _customerRepository.PhoneNumberExists(phoneNumberId);
            phoneNumber.Should().BeFalse();
        }

        [Fact]
        public async Task RestorePhoneNumber_RestoreDeletedPhoneNumber()
        {
            // Arrange
            var customerId = _customerAli.Id;
            var phoneNumberId = _customerAli.PhoneNumbers.First().Id;
            _context.ChangeTracker.Clear();

            // Act
            await _customerRepository.RestorePhoneNumber(phoneNumberId);
            await _customerRepository.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            // Assert
            await _customerRepository.RestorePhoneNumber(phoneNumberId);
            var phoneNumberExists = await _customerRepository.PhoneNumberExists(phoneNumberId);
            phoneNumberExists.Should().BeTrue();
        }

        [Fact]
        public async Task RestorePhoneNumber_ReturnFalseWhenDeletedDateEqualNull()
        {
            // Arrange
            var phoneNumberId = _customerAhmed.PhoneNumbers.First().Id;
            // Act
            _context.ChangeTracker.Clear();
            var result = await _customerRepository.RestorePhoneNumber(phoneNumberId);
            await _customerRepository.SaveChangesAsync();
            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CreateCustomers_AddCustomersInBulk()
        {
            // Arrange
            var newCustomers = new List<Customer>()
            {
                new Customer()
                {
                    Id = Guid.NewGuid(),
                    Name = "Bulk Customer 1",
                    Email = "new@example.com",
                    RowVersion = []
                },
                new Customer()
                {
                    Id = Guid.NewGuid(),
                    Name = "Bulk Customer 2",
                    Email = "newe@example.com",
                    RowVersion = []
                }
            };

            // Act
            await _customerRepository.CreateCustomers(newCustomers);
            await _customerRepository.SaveChangesAsync();

            // Assert
            _customerRepository.CustomerExists(newCustomers[0].Id).Result.Should().BeTrue();
            _customerRepository.CustomerExists(newCustomers[1].Id).Result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteCustomers_RemoveCustomersInBulk()
        {
            // Assert            
            var customers = new List<Customer>() { _customerAhmed, _customerHassan };
            var ids = customers.Select(x => x.Id).ToList();
            _context.ChangeTracker.Clear();
            // Act
           var result = await _customerRepository.DeleteCustomers(ids);
            await _customerRepository.SaveChangesAsync();

            // Assert
            var customerAhmedExists = await _customerRepository.CustomerExists(_customerAhmed.Id);
            var customerHassanExists = await _customerRepository.CustomerExists(_customerHassan.Id);

            customerAhmedExists.Should().BeFalse();
            customerHassanExists.Should().BeFalse();
        }

        [Fact]
        public async Task RestoreCustomers_RestoreCustomerInBulk()
        {
            // Arrange
            var customers = new List<Customer>() { _customerAli};
            var ids = customers.Select(x => x.Id).ToList();
            _context.ChangeTracker.Clear();

            // Act
            var result = _customerRepository.RestoreCustomers(ids);
            await _customerRepository.SaveChangesAsync();

            // Assert
            var customerAliExists = await _customerRepository.CustomerExists(_customerAli.Id);
            customerAliExists.Should().BeTrue();
        }

        [Fact]
        public async Task SaveChangesAsync_ReturnsTrue_WhenChangesSaved()
        {
            // Arrange
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                AddedDate = DateTime.Now,
                RowVersion = [1, 1, 1, 2]
            };

            _context.Customers.Add(customer);

            // Act
            var result = await _customerRepository.SaveChangesAsync();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task SaveChangesAsync_ReturnsFalse_WhenNoChangesSaved()
        {
            // Arrange
            _context.ChangeTracker.Clear();

            // Act
            var result = await _customerRepository.SaveChangesAsync();

            // Assert
            Assert.False(result);
        }
    }
}
