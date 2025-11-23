using EquipmentAPI.Contexts;
using EquipmentAPI.Entities;
using EquipmentAPI.Helper;
using EquipmentAPI.Models.PhoneNumberModels.Write;
using EquipmentAPI.Repositories.SupplierRepository;
using EquipmentAPI.ResourceParameters;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace EquipmentAPI.Tests.SupplierTests
{
    public class SupplierRepositoryTests
    {
        private readonly ApplicationDbContext _context;
        private readonly SupplierRepository _repository;

        public SupplierRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "supplierTestsDb")
                .Options;
            _context = new ApplicationDbContext(options);
            _repository = new SupplierRepository(_context);

            SeedDatabase();
        }

        private Supplier _supplierYahya;
        private Supplier _supplierAhmed;

        private void SeedDatabase()
        {
            if (_context.Suppliers.Any())
            {
                return;
            }

            _supplierYahya = new Supplier
            {
                Id = Guid.NewGuid(),
                Name = "Yahya",
                RowVersion = [1, 2, 3, 4],
                Country = "Egypt",
                City = "Alexandria",
                ContactPerson = "Hossam",
                Email = "info@yahya.com",
                PhoneNumbers =
                {
                    new SupplierPhoneNumber
                    {
                        Id = Guid.NewGuid(),
                        Number = "+44 44 25 25 852",
                        RowVersion = [7, 8, 8, 4]
                    }
                },
            };

            _supplierAhmed = new Supplier
            {
                Id = Guid.NewGuid(),
                Name = "Ahmed",
                RowVersion = [1, 2, 3, 7],
                Country = "Egypt",
                City = "Cairo",
                ContactPerson = "Mohamed",
                Email = "info@Ahmed.com",
                PhoneNumbers =
                [
                    new SupplierPhoneNumber
                    {
                        Id = Guid.NewGuid(),
                        Number = "+44 44 25 25 777",
                        RowVersion = [7, 6, 6, 4]
                    }
                ],
            };

            _context.Suppliers.AddRange(_supplierYahya, _supplierAhmed);
            _context.SaveChanges();
        }

        // ============================================================================
        // GET SUPPLIERS
        // ============================================================================
        [Fact]
        public async Task GetSuppliers_ReturnsPagedList()
        {
            // Arrange
            var parameters = new SupplierResourceParameters
            {
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _repository.GetSuppliers(parameters);

            // Assert
            result.Should().BeOfType<PagedList<Supplier>>();
            result.CurrentPage.Should().Be(1);
            result.Count.Should().Be(2);
            result.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task GetSuppliers_FiltersByCountry_WhenCountryProvided()
        {
            // Arrange
            var parameters = new SupplierResourceParameters
            {
                PageNumber = 1,
                PageSize = 10,
                FilterByCountry = "Egypt"
            };

            // Act
            var result = await _repository.GetSuppliers(parameters);

            // Assert
            result.Should().BeOfType<PagedList<Supplier>>();
            result.All(s => s.Country == "Egypt").Should().BeTrue();
            result.Count.Should().Be(2);
            result.Select(s => s.Name).Should().BeEquivalentTo("Yahya", "Ahmed");
        }

        [Fact]
        public async Task GetSuppliers_FiltersByCity_WhenCityProvided()
        {
            // Arrange
            var parameters = new SupplierResourceParameters
            {
                PageNumber = 1,
                PageSize = 10,
                FilterByCity = "Alexandria"
            };

            // Act
            var result = await _repository.GetSuppliers(parameters);

            // Assert
            result.Should().BeOfType<PagedList<Supplier>>();
            result.All(s => s.City == "Alexandria").Should().BeTrue();
            result.Count.Should().Be(1);
        }

        [Fact]
        public async Task GetSuppliers_FiltersBySearchQuery_WhenSearchProvided()
        {
            // Arrange
            var parameters = new SupplierResourceParameters
            {
                PageNumber = 1,
                PageSize = 10,
                SearchQuery = "ya"
            };

            // Act
            var result = await _repository.GetSuppliers(parameters);

            // Assert
            result.Should().BeOfType<PagedList<Supplier>>();
            result.All(s => s.Name.Contains("ya")).Should().BeTrue();
            result.Count.Should().Be(1);
        }

        [Fact]
        public async Task GetSuppliers_AppliesFieldSelection_WhenFieldsProvided()
        {
            // Arrange
            var parameters = new SupplierResourceParameters
            {
                PageNumber = 1,
                PageSize = 10,
                Fields = "phoneNumbers"
            };

            // Act
            var result = await _repository.GetSuppliers(parameters);

            // Assert
            result.Should().BeOfType<PagedList<Supplier>>();
            foreach (var supplier in result)
            {
                supplier.PhoneNumbers.Count.Should().NotBe(0);
            }
        }


        //// ============================================================================
        //// GET SINGLE SUPPLIER
        //// ============================================================================

        [Fact]
        public async Task GetSupplier_ReturnsSupplier_WhenExists()
        {
            // Arrange
            var id = _supplierYahya.Id;

            // Act
            var result = await _repository.GetSupplier(id, null);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Supplier>();
            result.Id.Should().Be(id);
        }

        [Fact]
        public async Task GetSupplier_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var result = await _repository.GetSupplier(id, null);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetSupplier_AppliesFieldSelection_WhenFieldsProvided()
        {
            // Arrange
            var parameters = new SupplierResourceParameters
            {
                Fields = "phoneNumbers"
            };
            var id = _supplierYahya.Id;

            // Act
            var result = await _repository.GetSupplier(id, parameters.Fields);

            // Assert
            result.Should().BeOfType<Supplier>();
            result.PhoneNumbers.Count.Should().NotBe(0);
        }


        //// ============================================================================
        //// EXISTS
        //// ============================================================================
        [Fact]
        public async Task Exists_ReturnsTrue_WhenSupplierExists()
        {
            var id = _supplierYahya.Id;

            // Act
            var result = await _repository.GetSupplier(id, null);

            // Assert
            result.Should().BeOfType<Supplier>();
            result.Should().NotBe(null);
        }

        [Fact]
        public async Task Exists_ReturnsFalse_WhenSupplierDoesNotExist()
        {
            var id = Guid.NewGuid();

            // Act
            var result = await _repository.GetSupplier(id, null);

            // Assert
            result.Should().Be(null);
        }


        //// ============================================================================
        //// PHONE NUMBER EXISTS
        //// ============================================================================

        [Fact]
        public async Task PhoneNumberExist_ReturnsTrue_WhenPhoneExists()
        {
            // Arrange
            var id = _supplierYahya.PhoneNumbers[0].Id;

            // Act
            var result = await _repository.PhoneNumberExist(id);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task PhoneNumberExist_ReturnsFalse_WhenPhoneDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var result = await _repository.PhoneNumberExist(id);

            // Assert
            result.Should().BeFalse();
        }


        //// ============================================================================
        //// CREATE
        //// ============================================================================

        [Fact]
        public async void Create_AddsSupplierToContext()
        {
            // Arrange
            var supplier = new Supplier
            {
                Id = Guid.Parse("1e965436-09be-4e93-86c6-6317e482962e"),
                Name = "New Supplier",
                RowVersion = [1, 1, 1, 0]
            };

            // Act
            _repository.Create(supplier);
            await _repository.SaveChangesAsync();

            // Assert
            Assert.True(_context.Suppliers.Any(s => s.Id == supplier.Id));
        }


        //// ============================================================================
        //// UPDATE SUPPLIER
        //// ============================================================================
        [Fact]
        public async Task Update_ThrowsConcurrencyException_WhenRowVersionMismatch()
        {
            // Arrange
            var updatedSupplier = new Supplier
            {
                Id = _supplierYahya.Id,
                Name = "Updated",
                RowVersion = [1, 1, 1],
            };

            // Act
            var exception = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
            {
                await _repository.Update(updatedSupplier);
            });

            // Assert
            Assert.Equal("The entity has been modified by another user.", exception.Message);
        }

        [Fact]
        public async Task Update_RemovesDeletedPhoneNumbers()
        {
            // Arrange
            var id = Guid.NewGuid();
            var firstPhoneNumber = Guid.NewGuid();
            var SecondPhoneNumber = Guid.NewGuid();
            var existingSupplier = new Supplier
            {
                Id = id,
                Name = "Yahya",
                RowVersion = [1, 2, 2, 1],
                PhoneNumbers =
                {
                    new SupplierPhoneNumber { Id =firstPhoneNumber, Number = "123" ,RowVersion = [6, 4, 2, 1]},
                    new SupplierPhoneNumber { Id = SecondPhoneNumber, Number = "456",RowVersion = [2, 4, 2, 1] }
                }
            };

            _context.Suppliers.Add(existingSupplier);
            await _context.SaveChangesAsync();

            var updatedSupplier = new Supplier
            {
                Id = id,
                Name = "Yahya",
                RowVersion = [1, 2, 2, 1],
                PhoneNumbers =
                [
                    new SupplierPhoneNumber { Id =SecondPhoneNumber, Number = "456" ,RowVersion = [2, 4, 2, 1]}
                ]
            };

            // Act
            await _repository.Update(updatedSupplier);
            await _context.SaveChangesAsync();

            // Assert

            var fromDb = await _context.Suppliers
                .Include(s => s.PhoneNumbers)
                .FirstAsync(s => s.Id == id);

            Assert.Single(fromDb.PhoneNumbers);
            Assert.Single(fromDb.PhoneNumbers);
            Assert.Equal("456", fromDb.PhoneNumbers.First().Number);
        }

        [Fact]
        public async Task Update_AddsNewPhoneNumbers()
        {
            // Arrange
            var id = _supplierYahya.Id;
            var phonenumber = new SupplierPhoneNumber
            {
                Id = Guid.Empty,
                SupplierId = _supplierYahya.Id,
                Number = "+44 44 55 25 000"
            };

            var updatedSupplier = new Supplier
            {
                Id = id,
                RowVersion = _supplierYahya.RowVersion,
                PhoneNumbers =
                {
                    new SupplierPhoneNumber
                    {
                        Id = _supplierYahya.PhoneNumbers[0].Id,
                        SupplierId = _supplierYahya.Id,
                        Number = _supplierYahya.PhoneNumbers[0].Number,
                        RowVersion = _supplierYahya.PhoneNumbers[0].RowVersion
                    },
                    phonenumber
                }
            };

            // Act
            await _repository.Update(updatedSupplier);
            await _repository.SaveChangesAsync();

            // Assert
            var fromDb = await _context.Suppliers
                .Include(s => s.PhoneNumbers)
                .FirstAsync(s => s.Id == id);
            Assert.Equal(2, fromDb.PhoneNumbers.Count);
            Assert.Contains(fromDb.PhoneNumbers, p => p.Number == "+44 44 55 25 000");
        }

        [Fact]
        public async Task Update_UpdatesExistingPhoneNumbers()
        {
            // Arrange
            var id = _supplierYahya.Id;
            var updatedSupplier = new Supplier
            {
                Id = id,
                RowVersion = _supplierYahya.RowVersion,
                PhoneNumbers =
                {
                    new SupplierPhoneNumber
                    {
                        Id = _supplierYahya.PhoneNumbers[0].Id,
                        SupplierId = _supplierYahya.Id,
                        Number = "+44 44 00 00 000",
                        RowVersion = _supplierYahya.PhoneNumbers[0].RowVersion
                    }
                }
            };
            // Act
            await _repository.Update(updatedSupplier);
            await _repository.SaveChangesAsync();

            var supplier = await _repository.GetSupplier(id, "phoneNumbers");

            // Assert
            var updatedPhone = supplier.PhoneNumbers.Single(pn => pn.Id == updatedSupplier.PhoneNumbers[0].Id);

            Assert.Equal("+44 44 00 00 000", updatedPhone.Number);
        }

        [Fact]
        public async Task Update_UpdatesSupplierFields()
        {
            // Arrange
            var id = _supplierYahya.Id;
            var updatedSupplier = new Supplier
            {
                Id = id,
                RowVersion = _supplierYahya.RowVersion,
                Name = "New Generation",
                Country = "Uk",
                City = "London"
            };
            // Act
            await _repository.Update(updatedSupplier);
            await _repository.SaveChangesAsync();

            var supplier = await _repository.GetSupplier(id, null);

            // Assert
            Assert.Equal("New Generation", supplier.Name);
            Assert.Equal("Uk", supplier.Country);
            Assert.Equal("London", supplier.City);
        }


        //// ============================================================================
        //// UPDATE PHONE NUMBERS ONLY
        //// ============================================================================

        [Fact]
        public async Task UpdatePhoneNumber_UpdatesExistingNumbers()
        {
            // Arrange
            var supplierId = _supplierYahya.Id;
            var existPhoneNumber = _supplierYahya.PhoneNumbers[0];
            var phoneNumbers = new List<SupplierPhoneNumberUpdateDto>()
            {
                new SupplierPhoneNumberUpdateDto {
                    Id = existPhoneNumber.Id,
                    Number = "+44 562 258 987 011",
                    RowVersion = existPhoneNumber.RowVersion,
                }
            };

            // Act
            await _repository.UpdatePhoneNumber(supplierId, phoneNumbers);
            await _repository.SaveChangesAsync();

            // Assert
            var supplier = await _repository.GetSupplier(supplierId, "phoneNumbers");
            Assert.Equal("+44 562 258 987 011", supplier.PhoneNumbers[0].Number);
        }

        [Fact]
        public async Task UpdatePhoneNumber_AddsNewNumbers()
        {
            // Arrange
            var supplierId = _supplierYahya.Id;
            var existPhoneNumber = _supplierYahya.PhoneNumbers[0];
            var phoneNumbers = new List<SupplierPhoneNumberUpdateDto>()
            {
                new SupplierPhoneNumberUpdateDto {
                    Id = existPhoneNumber.Id,
                    Number = "+44 562 258 987 011",
                    RowVersion = existPhoneNumber.RowVersion,
                },
                new SupplierPhoneNumberUpdateDto {
                    Number = "+44 110 258 987 011",
                }
            };

            // Act
            await _repository.UpdatePhoneNumber(supplierId, phoneNumbers);
            await _repository.SaveChangesAsync();

            // Assert
            var fromDb = await _context.Suppliers
                .Include(s => s.PhoneNumbers)
                .FirstAsync(s => s.Id == supplierId);
            Assert.Equal(2, fromDb.PhoneNumbers.Count);
            Assert.Contains(fromDb.PhoneNumbers, p => p.Number == "+44 110 258 987 011");
            Assert.Contains(fromDb.PhoneNumbers, p => p.Number == "+44 562 258 987 011");
        }

        [Fact]
        public async Task UpdatePhoneNumber_MarksMissingNumbersAsDeleted()
        {
            // Arrange
            var supplierId = _supplierYahya.Id;
            var existPhoneNumber = _supplierYahya.PhoneNumbers[0];
            var phoneNumbers = new List<SupplierPhoneNumberUpdateDto>()
            {
                new SupplierPhoneNumberUpdateDto {
                    Number = "+44 110 258 987 011",
                }
            };

            // Act
            await _repository.UpdatePhoneNumber(supplierId, phoneNumbers);
            await _repository.SaveChangesAsync();

            // Assert
            var fromDb = await _context.Suppliers
                .Include(s => s.PhoneNumbers)
                .FirstAsync(s => s.Id == supplierId);
            Assert.Equal(1, fromDb.PhoneNumbers.Count);
            Assert.DoesNotContain(fromDb.PhoneNumbers, p => p.Number == "+44 562 258 987 011");
            Assert.Contains(fromDb.PhoneNumbers, p => p.Number == "+44 110 258 987 011");
        }

        [Fact]
        public async Task UpdatePhoneNumber_ThrowsConcurrencyException_WhenRowVersionMismatch()
        {
            // Arrange
            var supplierId = _supplierYahya.Id;
            var existPhoneNumber = _supplierYahya.PhoneNumbers[0];
            var phoneNumbers = new List<SupplierPhoneNumberUpdateDto>()
            {
                new SupplierPhoneNumberUpdateDto {
                    Id = existPhoneNumber.Id,
                    Number = "+44 562 258 987 011",
                    RowVersion = [1,2,5,8],
                }
            };

            // Act
            var exception = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
            {
                await _repository.UpdatePhoneNumber(supplierId, phoneNumbers);
            });

            Assert.Equal("The entity has been modified by another user.", exception.Message);
        }


        //// ============================================================================
        //// DELETE SUPPLIER
        //// ============================================================================

        [Fact]
        public async Task Delete_SetsDeletedDate_WhenSupplierExists()
        {
            // Arrange
            var supplierId = _supplierYahya.Id;
            _context.ChangeTracker.Clear();
            // Act
            await _repository.Delete(supplierId);
            await _repository.SaveChangesAsync();

            // Assert
            var supplier = await _context.Suppliers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Id == supplierId);
            Assert.NotNull(supplier);
            Assert.NotNull(supplier.DeletedDate);
        }

        [Fact]
        public async Task Delete_DoesNothing_WhenSupplierDoesNotExist()
        {
            var supplierId = Guid.NewGuid();
            _context.ChangeTracker.Clear();
            // Act
            await _repository.Delete(supplierId);
            await _repository.SaveChangesAsync();

            // Assert
            var supplier = await _context.Suppliers
               .IgnoreQueryFilters()
               .FirstOrDefaultAsync(s => s.Id == supplierId);

            Assert.Null(supplier);
        }


        //// ============================================================================
        //// DELETE PHONE NUMBER
        //// ============================================================================

        [Fact]
        public async Task DeletePhoneNumber_SetsDeletedDate_WhenPhoneExists()
        {
            // Arrange
            var supplierId = _supplierYahya.Id;
            var phoneNumberId = _supplierYahya.PhoneNumbers[0].Id;
            _context.ChangeTracker.Clear();

            // Act
            await _repository.DeletePhoneNumber(supplierId, phoneNumberId);
            await _repository.SaveChangesAsync();

            // Assert
            var phoneNumber = await _context.SupplierPhoneNumbers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(pn => pn.Id == phoneNumberId);
            Assert.NotNull(phoneNumber.DeletedDate);
        }

        [Fact]
        public async Task DeletePhoneNumber_DoesNothing_WhenPhoneNotFound()
        {
            // Arrange
            var supplierId = _supplierYahya.Id;
            var phoneNumberId = Guid.NewGuid();
            _context.ChangeTracker.Clear();

            // Act
            await _repository.DeletePhoneNumber(supplierId, phoneNumberId);
            await _repository.SaveChangesAsync();

            // Assert
            var phoneNumber = await _context.SupplierPhoneNumbers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(pn => pn.Id == phoneNumberId);
            Assert.Null(phoneNumber);
        }

        [Fact]
        public async Task DeletePhoneNumber_DoesNothing_WhenSupplierNotFound()
        {
            // Arrange
            var supplierId = Guid.NewGuid();
            var phoneNumberId = _supplierYahya.PhoneNumbers[0].Id;
            _context.ChangeTracker.Clear();

            // Act
            await _repository.DeletePhoneNumber(supplierId, phoneNumberId);
            await _repository.SaveChangesAsync();

            // Assert
            var phoneNumber = await _context.SupplierPhoneNumbers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(pn => pn.Id == phoneNumberId);
            Assert.NotNull(phoneNumber);
            Assert.Null(phoneNumber.DeletedDate);
        }


        //// ============================================================================
        //// RESTORE SUPPLIER
        //// ============================================================================

        [Fact]
        public async Task RestoreSupplier_ReturnsFalse_WhenSupplierNotFound()
        {
            // Arrange
            var supplierId = Guid.NewGuid();
            _context.ChangeTracker.Clear();

            // Act
            var result = await _repository.RestoreSupplier(supplierId);
            await _repository.SaveChangesAsync();
            // Assert
            Assert.False(result);

        }

        [Fact]
        public async Task RestoreSupplier_ReturnsFalse_WhenSupplierIsNotDeleted()
        {
            // Arrange
            var supplierId = _supplierYahya.Id;
            _context.ChangeTracker.Clear();

            // Act
            var result = await _repository.RestoreSupplier(supplierId);
            await _repository.SaveChangesAsync();
            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RestoreSupplier_RestoresSupplierAndPhoneNumbers_WhenDeleted()
        {
            // Arrange
            var supplierId = _supplierYahya.Id;
            _context.ChangeTracker.Clear();
            await _repository.Delete(supplierId);
            await _repository.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            // Act
            var result = await _repository.RestoreSupplier(supplierId);
            await _repository.SaveChangesAsync();
            // Assert
            Assert.True(result);
        }


        //// ============================================================================
        //// RESTORE PHONE NUMBER
        //// ============================================================================

        [Fact]
        public async Task RestorePhoneNumber_ReturnsFalse_WhenPhoneNotFound()
        {
            // Arrange
            var phoneNumberId = Guid.NewGuid();

            // Act
            var result = await _repository.RestorePhoneNumber(phoneNumberId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RestorePhoneNumber_ReturnsFalse_WhenPhoneNotDeleted()
        {
            // Arrange
            var phoneNumberId = _supplierYahya.PhoneNumbers[0].Id;

            // Act
            var result = await _repository.RestorePhoneNumber(phoneNumberId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RestorePhoneNumber_RestoresPhoneNumber_WhenDeleted()
        {
            // Arrange
            var supplierId = _supplierYahya.Id;
            var phoneNumberId = _supplierYahya.PhoneNumbers[0].Id;
            _context.ChangeTracker.Clear();
            await _repository.DeletePhoneNumber(supplierId, phoneNumberId);
            await _repository.SaveChangesAsync();
            _context.ChangeTracker.Clear();
            // Act
            var result = await _repository.RestorePhoneNumber(phoneNumberId);
            await _repository.SaveChangesAsync();

            // Assert
            Assert.True(result);
        }


        //// ============================================================================
        //// SAVE CHANGES
        //// ============================================================================

        [Fact]
        public async Task SaveChangesAsync_ReturnsTrue_WhenChangesSaved()
        {
            // Arrange
            var supplier = new Supplier
            {
                Id = Guid.NewGuid(),
                Name = "Test Supplier",
                AddedDate = DateTime.Now,
                RowVersion = [1, 1, 1, 2]
            };

            _context.Suppliers.Add(supplier);

            // Act
            var result = await _repository.SaveChangesAsync();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task SaveChangesAsync_ReturnsFalse_WhenNoChangesSaved()
        {
            // Arrange
            _context.ChangeTracker.Clear();

            // Act
            var result = await _repository.SaveChangesAsync();

            // Assert
            Assert.False(result);
        }

    }
}
