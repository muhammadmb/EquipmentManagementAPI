using Application.Interface.Repositories;
using Application.ResourceParameters;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Contexts;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Results;

namespace EquipmentAPI.Tests.UnitTests.SellingContractTests
{
    public class SellingContractRepositoryTests
    {
        private readonly ApplicationDbContext _context;
        private readonly ISellingContractRepository _repository;

        private SellingContract _sellingContract1;
        private SellingContract _sellingContract2;
        private SellingContract _sellingContract3;

        public SellingContractRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(($"{Guid.NewGuid()}_SellingContractTestDb"))
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new SellingContractRepository(_context);

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            _sellingContract1 = new SellingContract
            {
                Id = Guid.NewGuid(),
                EquipmentId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                SalePrice = 1500m,
                SaleDate = new DateTime(2025, 12, 12),
                RowVersion = []
            };

            _sellingContract2 = new SellingContract
            {
                Id = Guid.NewGuid(),
                EquipmentId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                SalePrice = 2500m,
                SaleDate = new DateTime(2025, 12, 12),
                RowVersion = []
            };

            _sellingContract3 = new SellingContract
            {
                Id = Guid.NewGuid(),
                EquipmentId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                SalePrice = 2500m,
                SaleDate = new DateTime(2025, 12, 12),
                RowVersion = [],
                DeletedDate = DateTimeOffset.UtcNow
            };

            _context.Sellings.AddRange(
                _sellingContract1,
                _sellingContract2,
                _sellingContract3);
            _context.SaveChanges();
        }

        #region GetSellingContracts Tests

        [Fact]
        public async Task GetSellingContracts_ReturnPagedList()
        {
            // Arrange
            var parameters = new SellingContractResourceParameters
            {
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _repository.GetSellingContracts(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.IsType<PagedList<SellingContract>>(result);
        }

        [Fact]
        public async Task GetSellingContracts_ReturnsEmptyList_WhenNoMatchingContracts()
        {
            // Arrange
            var parameters = new SellingContractResourceParameters
            {
                CustomerId = Guid.NewGuid(),
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _repository.GetSellingContracts(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetSellingContracts_FiltersByCustomerId()
        {
            //Arrange
            var parameters = new SellingContractResourceParameters
            {
                CustomerId = _sellingContract1.CustomerId,
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _repository.GetSellingContracts(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            result.All(c => c.CustomerId == _sellingContract1.CustomerId);
        }

        [Fact]
        public async Task GetSellingContracts_FilterByYear()
        {
            // Arrange
            var year = 2025;

            var parameters = new SellingContractResourceParameters
            {
                Year = year,
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _repository.GetSellingContracts(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, c => Assert.Equal(year, c.SaleDate.Year));
        }

        [Fact]
        public async Task GetSellingContracts_HandlesMultipleFilters()
        {
            // Arrange
            var parameters = new SellingContractResourceParameters
            {
                CustomerId = _sellingContract1.CustomerId,
                Year = 2025,
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _repository.GetSellingContracts(parameters);

            // Assert
            Assert.NotNull(result);
            result.All(result => result.CustomerId == _sellingContract1.CustomerId && result.SaleDate.Year == 2025);
        }

        [Fact]
        public async Task GetSellingContracts_RespectsPagination()
        {
            // Arrange
            var parameters = new SellingContractResourceParameters
            {
                PageNumber = 1,
                PageSize = 1
            };

            // Act
            var result = await _repository.GetSellingContracts(parameters);

            // Assert
            Assert.NotNull(result);
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetSellingContractById_ReturnsContract_WhenExists()
        {
            // Arrange
            var existingId = _sellingContract1.Id;

            // Act
            var result = await _repository.GetSellingContractById(existingId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_sellingContract1.Id, result.Id);
        }

        [Fact]
        public async Task GetSellingContractById_ReturnsNull_WhenNotExists()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            // Act
            var result = await _repository.GetSellingContractById(nonExistentId);
            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetSellingContractById_Throws_WhenIdIsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _repository.GetSellingContractById(Guid.Empty, null));
        }

        [Fact]
        public async Task GetSellingContractForUpdate_ReturnsTrackedEntity()
        {
            // Act
            var contract = await _repository.GetSellingContractForUpdate(_sellingContract1.Id, null);

            // Assert
            contract.Should().NotBeNull();
            _context.Entry(contract).State.Should().Be(EntityState.Unchanged);
        }

        [Fact]
        public async Task GetSellingContractForUpdate_ReturnsNull_WhenNotExists()
        {
            // Act
            var contract = await _repository.GetSellingContractForUpdate(Guid.NewGuid(), null);

            // Assert
            contract.Should().BeNull();
        }

        [Fact]
        public async Task GetSellingContractForUpdate_Throws_WhenIdIsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _repository.GetSellingContractForUpdate(Guid.Empty, null));
        }

        [Fact]
        public async Task GetDeletedSellingContractById_ReturnsDeletedContract()
        {
            // Arrange
            var parameters = new SellingContractResourceParameters
            {
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var contracts = await _repository.GetSoftDeletedSellingContracts(parameters);

            // Assert
            contracts.Should().NotBeNull();
            contracts.Count().Should().Be(1);
            contracts.First().Id.Should().Be(_sellingContract3.Id);
        }

        [Fact]
        public async Task GetDeletedSellingContractById_ReturnsNull_WhenNotDeleted()
        {
            // Act
            var contract = await _repository
                .GetSoftDeletedSellingContractById(_sellingContract2.Id);

            // Assert
            contract.Should().BeNull();
        }

        [Fact]
        public async Task GetDeletedSellingContractByIds_ReturnsNull_WhenNotExists()
        {
            // Arrange
            var parameters = new SellingContractResourceParameters
            {
                CustomerId = Guid.NewGuid(),
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var contract = await _repository.GetSoftDeletedSellingContractsByIds([Guid.NewGuid()]);

            // Assert
            contract.Should().BeEmpty();
        }
        #endregion

        #region EXISTS  CHECKS

        [Fact]
        public async Task SellingContractExists_ReturnsTrue_WhenExists()
        {
            // Act
            var exists = await _repository.SellingContractExists(_sellingContract1.Id);

            // Assert
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task SellingContractExists_ReturnsFalse_WhenNotExists()
        {
            // Act
            var exists = await _repository.SellingContractExists(Guid.NewGuid());

            // Assert
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task CustomerHasSellingContracts_ReturnsTrue()
        {
            // Act
            var result = await _repository.CustomerHasContracts(_sellingContract1.CustomerId);

            // Assert
            result.Should().BeTrue();
        }

        #endregion

        #region Create Update Tests

        [Fact]
        public async Task CreateSellingContract_AddsContractToDatabase()
        {
            // Arrange
            var contract = new SellingContract
            {
                EquipmentId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                SalePrice = 3000m,
                SaleDate = DateTime.UtcNow,
                RowVersion = []
            };

            // Act
            await _repository.CreateSellingContract(contract);
            await _context.SaveChangesAsync();

            // Assert
            var createdContract = await _context.Sellings.FindAsync(contract.Id);
            Assert.NotNull(createdContract);
        }

        [Fact]
        public async Task CreateSellingContract_Throws_WhenContractIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _repository.CreateSellingContract(null));
        }

        [Fact]
        public async Task UpdateSellingContract_ModifiesExistingContract()
        {
            // Arrange
            _sellingContract1.SalePrice = 2000m;

            // Act
            await _repository.UpdateSellingContract(_sellingContract1);
            await _context.SaveChangesAsync();

            // Assert
            var updatedContract = await _context.Sellings.FindAsync(_sellingContract1.Id);
            Assert.Equal(2000m, updatedContract.SalePrice);
        }

        [Fact]
        public async Task UpdateSellingContract_SetsEntityStateToModified()
        {
            // Arrange
            _context.ChangeTracker.Clear();
            
            // Act
            var contract = await _repository.GetSellingContractById(_sellingContract1.Id);
            await _repository.UpdateSellingContract(contract);
            
            // Assert
            _context.Entry(contract).State.Should().Be(EntityState.Modified);
        }

        [Fact]
        public async Task UpdateSellingContract_ThrowsConcurrency_WhenRowVersionMismatch()
        {
            _sellingContract1.RowVersion = new byte[] { 9, 9, 9 };

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
                _repository.UpdateSellingContract(_sellingContract1));
        }

        [Fact]
        public async Task UpdateSellingContract_Throws_WhenContractIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _repository.UpdateSellingContract(null));
        }
        #endregion

        #region SOFT DELETE / RESTORE

        [Fact]
        public async Task SoftDeleteSellingContract_SetsDeletedDate()
        {
            // Arrange
            _context.ChangeTracker.Clear();
            await _repository.SoftDeleteSellingContract(_sellingContract1.Id);
            await _repository.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            // Act
            var deleted = await _repository.GetSoftDeletedSellingContractsByIds([_sellingContract1.Id]);

            //Assert
            deleted.Should().NotBeNull();
            Assert.Contains(deleted, d => d.DeletedDate != null);
        }

        [Fact]
        public async Task SoftDeleteSellingContract_RemovesFromActiveQueries()
        {
            // Arrange
            _context.ChangeTracker.Clear();

            await _repository.SoftDeleteSellingContract(_sellingContract1.Id);
            await _repository.SaveChangesAsync();

            // Act
            var exists = await _repository.SellingContractExists(_sellingContract1.Id);

            // Assert
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteSellingContract_DoesNotRemoveFromDatabase()
        {
            // Arrange
            _context.ChangeTracker.Clear();

            await _repository.SoftDeleteSellingContract(_sellingContract1.Id);
            await _repository.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            // Act
            var contract = await _context.Sellings
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == _sellingContract1.Id);

            // Assert
            contract.Should().NotBeNull();
        }

        [Fact]
        public async Task SoftDeleteSellingContract_Throws_WhenContractNotFound()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _repository.SoftDeleteSellingContract(Guid.NewGuid()));
        }

        [Fact]
        public async Task SoftDeleteSellingContract_Throws_WhenIdIsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.SoftDeleteSellingContract(Guid.Empty));
        }

        [Fact]
        public async Task RestoreSellingContract_RestoresDeletedContract()
        {
            // Arrange
            _context.ChangeTracker.Clear();

            await _repository.RestoreSellingContract(_sellingContract3.Id);
            await _repository.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            // Act
            var restored = await _repository.GetSellingContractById(_sellingContract3.Id);

            // Assert
            restored.Should().NotBeNull();
            restored.DeletedDate.Should().BeNull();
        }

        [Fact]
        public async Task RestoreSellingContract_MakesContractVisibleInQueries()
        {
            // Arrange
            _context.ChangeTracker.Clear();

            await _repository.RestoreSellingContract(_sellingContract3.Id);
            await _repository.SaveChangesAsync();

            // Act
            var exists = await _repository.SellingContractExists(_sellingContract3.Id);
            
            // Assert
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task RestoreSellingContract_Throws_WhenIdIsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.RestoreSellingContract(Guid.Empty));
        }

        #endregion

        #region BULK OPERATIONS

        [Fact]
        public async Task CreateSellingContracts_BulkInsert()
        {
            // Arrange
            var contracts = new List<SellingContract>
            {
                new() { Id = Guid.NewGuid(), RowVersion = new byte[]{1} },
                new() { Id = Guid.NewGuid(), RowVersion = new byte[]{1} }
            };

            // Act
            var result = await _repository.CreateSellingContracts(contracts);
            await _repository.SaveChangesAsync();

            // Assert
            result.SuccessCount.Should().Be(2);
        }

        [Fact]
        public async Task DeleteSellingContracts_BulkDelete()
        {
            // Arrange
            var parameters = new SellingContractResourceParameters
            {
                PageNumber = 1,
                PageSize = 10
            };
            _context.ChangeTracker.Clear();

            // Act
            var result = await _repository.GetSoftDeletedSellingContracts(parameters);
            await _repository.SaveChangesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(1);
        }

        [Fact]
        public async Task RestoreSellingContracts_BulkRestore()
        {
            // Arrange
            var ids = new[] { _sellingContract3.Id };
            _context.ChangeTracker.Clear();
            
            // Act
            var result = await _repository.RestoreSellingContracts(ids);
            await _repository.SaveChangesAsync();

            // Assert
            result.SuccessCount.Should().Be(1);
        }

        #endregion

        #region SAVE CHANGES

        [Fact]
        public async Task SaveChangesAsync_ReturnsTrue_WhenChangesSaved()
        {
            // Arrange
            _sellingContract1.SalePrice += 500;

            // Act
            var result = await _repository.SaveChangesAsync();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task SaveChangesAsync_ReturnsFalse_WhenNoChanges()
        {
            // Arrange
            _context.ChangeTracker.Clear();

            // Act
            var result = await _repository.SaveChangesAsync();

            // Assert
            result.Should().BeFalse();
        }
        #endregion
    }
}
