using Application.ResourceParameters;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Contexts;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Results;

namespace EquipmentAPI.Tests.UnitTests.RentalContractTests
{
    public class RentalContractRepositoryTests
    {
        private readonly ApplicationDbContext _context;
        private readonly RentalContractRepository _repository;

        private RentalContract _activeContract;
        private RentalContract _finishedContract;
        private RentalContract _deletedContract;

        public RentalContractRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"{Guid.NewGuid()}_RentalContractTestsDb")
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new RentalContractRepository(_context);

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            _activeContract = new RentalContract
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                EquipmentId = Guid.NewGuid(),
                StartDate = DateTimeOffset.UtcNow.AddDays(-5),
                EndDate = DateTimeOffset.UtcNow.AddDays(10),
                Shifts = 10,
                ShiftPrice = 1000,
                RowVersion = new byte[] { 1, 1, 1 }
            };
            _activeContract.Activate();

            _finishedContract = new RentalContract
            {
                Id = Guid.NewGuid(),
                CustomerId = _activeContract.CustomerId,
                EquipmentId = Guid.NewGuid(),
                StartDate = DateTimeOffset.UtcNow.AddDays(-30),
                EndDate = DateTimeOffset.UtcNow.AddDays(-1),
                Shifts = 5,
                ShiftPrice = 800,
                RowVersion = new byte[] { 1, 1, 2 }
            };
            _finishedContract.Activate();
            _finishedContract.Finish();

            _deletedContract = new RentalContract
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                EquipmentId = Guid.NewGuid(),
                StartDate = DateTimeOffset.UtcNow.AddDays(-20),
                EndDate = DateTimeOffset.UtcNow.AddDays(-10),
                DeletedDate = DateTimeOffset.UtcNow,
                RowVersion = new byte[] { 2, 2, 2 }
            };

            _context.RentalContracts.AddRange(
                _activeContract,
                _finishedContract,
                _deletedContract);

            _context.SaveChanges();
        }

        #region Get Tests

        [Fact]
        public async Task GetAllContracts_ReturnsPagedList()
        {
            // Act
            var parameters = new RentalContractResourceParameters
            {
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var contracts = await _repository.GetRentalContracts(parameters);

            // Assert

            Assert.NotNull(contracts);
            Assert.IsType<PagedList<RentalContract>>(contracts);
            Assert.Equal(2, contracts.TotalCount); // Excludes deleted contract
        }

        [Fact]
        public async Task GetRentalContracts_ReturnsEmptyList_WhenNoMatchingContracts()
        {
            // Arrange
            var parameters = new RentalContractResourceParameters
            {
                CustomerId = Guid.NewGuid()
            };

            //Act
            var result = await _repository.GetRentalContracts(parameters);

            //Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetRentalContracts_FiltersByCustomerId()
        {
            var parameters = new RentalContractResourceParameters
            {
                CustomerId = _activeContract.CustomerId
            };

            var result = await _repository.GetRentalContracts(parameters);

            result.Should().HaveCount(2);
            result.All(c => c.CustomerId == _activeContract.CustomerId).Should().BeTrue();
        }

        [Fact]
        public async Task GetRentalContracts_FiltersByYear()
        {
            var parameters = new RentalContractResourceParameters
            {
                Year = DateTimeOffset.UtcNow.Year
            };

            var result = await _repository.GetRentalContracts(parameters);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetRentalContracts_FiltersByMonth()
        {
            var parameters = new RentalContractResourceParameters
            {
                Month = DateTimeOffset.UtcNow.Month
            };

            var result = await _repository.GetRentalContracts(parameters);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetRentalContracts_HandlesMultipleFilters()
        {
            var parameters = new RentalContractResourceParameters
            {
                CustomerId = _activeContract.CustomerId,
                EquipmentId = _activeContract.EquipmentId
            };

            var result = await _repository.GetRentalContracts(parameters);

            result.Should().NotBeEmpty();
            result.All(c => c.CustomerId == _activeContract.CustomerId).Should().BeTrue();
        }

        [Fact]
        public async Task GetRentalContracts_RespectsPagination()
        {
            var parameters = new RentalContractResourceParameters
            {
                PageNumber = 1,
                PageSize = 1
            };

            var result = await _repository.GetRentalContracts(parameters);

            result.Should().ContainSingle();
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetRentalContracts_ExcludesDeletedContracts()
        {
            var parameters = new RentalContractResourceParameters
            {
                PageNumber = 1,
                PageSize = 100
            };

            var result = await _repository.GetRentalContracts(parameters);

            result.Should().NotContain(c => c.Id == _deletedContract.Id);
        }

        [Fact]
        public async Task GetRentalContractById_ReturnsContract_WhenExists()
        {
            var contract = await _repository.GetRentalContractById(_activeContract.Id, null);

            contract.Should().NotBeNull();
            contract.Id.Should().Be(_activeContract.Id);
        }

        [Fact]
        public async Task GetRentalContractById_ReturnsNull_WhenNotExists()
        {
            var contract = await _repository.GetRentalContractById(Guid.NewGuid(), null);

            contract.Should().BeNull();
        }

        [Fact]
        public async Task GetRentalContractById_ReturnsNull_WhenDeleted()
        {
            var contract = await _repository.GetRentalContractById(_deletedContract.Id, null);

            contract.Should().BeNull();
        }

        [Fact]
        public async Task GetRentalContractById_Throws_WhenIdIsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.GetRentalContractById(Guid.Empty, null));
        }

        [Fact]
        public async Task GetRentalContractForUpdate_ReturnsTrackedEntity()
        {
            var contract = await _repository.GetRentalContractForUpdate(_activeContract.Id, null);

            contract.Should().NotBeNull();
            _context.Entry(contract).State.Should().Be(EntityState.Unchanged);
        }

        [Fact]
        public async Task GetRentalContractForUpdate_ReturnsNull_WhenNotExists()
        {
            var contract = await _repository.GetRentalContractForUpdate(Guid.NewGuid(), null);

            contract.Should().BeNull();
        }

        [Fact]
        public async Task GetRentalContractForUpdate_Throws_WhenIdIsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.GetRentalContractForUpdate(Guid.Empty, null));
        }

        [Fact]
        public async Task GetDeletedRentalContractById_ReturnsDeletedContract()
        {
            var contract = await _repository.GetDeletedRentalContractById(_deletedContract.Id);

            contract.Should().NotBeNull();
            contract.Id.Should().Be(_deletedContract.Id);
            contract.DeletedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDeletedRentalContractById_ReturnsNull_WhenNotDeleted()
        {
            var contract = await _repository.GetDeletedRentalContractById(_activeContract.Id);

            contract.Should().BeNull();
        }

        [Fact]
        public async Task GetDeletedRentalContractById_ReturnsNull_WhenNotExists()
        {
            var contract = await _repository.GetDeletedRentalContractById(Guid.NewGuid());

            contract.Should().BeNull();
        }
        #endregion

        #region EXISTS  CHECKS

        [Fact]
        public async Task RentalContractExists_ReturnsTrue_WhenExists()
        {
            var exists = await _repository.RentalContractExists(_activeContract.Id);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task RentalContractExists_ReturnsFalse_WhenNotExists()
        {
            var exists = await _repository.RentalContractExists(Guid.NewGuid());

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task CustomerHasContracts_ReturnsTrue()
        {
            var result = await _repository.CustomerHasContracts(_activeContract.CustomerId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task EquipmentHasContracts_ReturnsTrue()
        {
            var result = await _repository.EquipmentHasContracts(_activeContract.EquipmentId);

            result.Should().BeTrue();
        }
        #endregion

        #region ACTIVE / EXPIRED

        [Fact]
        public async Task GetActiveContracts_ReturnsOnlyActiveContracts()
        {
            var result = await _repository.GetActiveContracts(null);

            result.Should().ContainSingle();
            result.First().Id.Should().Be(_activeContract.Id);
        }

        [Fact]
        public async Task GetActiveContracts_ExcludesFinishedContracts()
        {
            var result = await _repository.GetActiveContracts(null);

            result.Should().NotContain(c => c.Id == _finishedContract.Id);
        }

        [Fact]
        public async Task GetActiveContracts_ExcludesDeletedContracts()
        {
            var result = await _repository.GetActiveContracts(null);

            result.Should().NotContain(c => c.Id == _deletedContract.Id);
        }

        [Fact]
        public async Task GetActiveContracts_ReturnsEmptyList_WhenNoActiveContracts()
        {
            _context.ChangeTracker.Clear();
            await _repository.SoftDeleteRentalContract(_activeContract.Id);
            await _repository.SaveChangesAsync();

            var result = await _repository.GetActiveContracts(null);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetExpiredContracts_ReturnsContractsEndingSoon()
        {
            var result = await _repository.GetExpiredContracts(30);

            result.Should().ContainSingle();
            result.First().Id.Should().Be(_activeContract.Id);
        }

        [Fact]
        public async Task GetExpiredContracts_ReturnsEmpty_WhenNoContractsExpiring()
        {
            var result = await _repository.GetExpiredContracts(1);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetExpiredContracts_ExcludesDeletedContracts()
        {
            var result = await _repository.GetExpiredContracts(365);

            result.Should().NotContain(c => c.Id == _deletedContract.Id);
        }
        #endregion


        #region Create Update Tests

        [Fact]
        public async Task CreateRentalContract_AddsContractToDatabase()
        {
            var contract = new RentalContract
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                EquipmentId = Guid.NewGuid(),
                StartDate = DateTimeOffset.UtcNow,
                EndDate = DateTimeOffset.UtcNow.AddDays(5),
                RowVersion = new byte[] { 1 }
            };

            await _repository.CreateRentalContract(contract);
            await _repository.SaveChangesAsync();

            var exists = await _repository.RentalContractExists(contract.Id);
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task CreateRentalContract_SetsEntityStateToAdded()
        {
            var contract = new RentalContract
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                EquipmentId = Guid.NewGuid(),
                StartDate = DateTimeOffset.UtcNow,
                EndDate = DateTimeOffset.UtcNow.AddDays(5),
                RowVersion = new byte[] { 1 }
            };

            await _repository.CreateRentalContract(contract);

            _context.Entry(contract).State.Should().Be(EntityState.Added);
        }

        [Fact]
        public async Task CreateRentalContract_Throws_WhenContractIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _repository.CreateRentalContract(null));
        }

        [Fact]
        public async Task UpdateRentalContract_UpdatesContract_WhenRowVersionMatches()
        {
            _activeContract.Shifts = 15;

            await _repository.UpdateRentalContract(_activeContract);
            await _repository.SaveChangesAsync();

            var updated = await _repository.GetRentalContractById(_activeContract.Id, null);
            updated.Shifts.Should().Be(15);
        }

        [Fact]
        public async Task UpdateRentalContract_SetsEntityStateToModified()
        {
            _context.ChangeTracker.Clear();
            var contract = await _repository.GetRentalContractById(_activeContract.Id, null);

            await _repository.UpdateRentalContract(contract);

            _context.Entry(contract).State.Should().Be(EntityState.Modified);
        }

        [Fact]
        public async Task UpdateRentalContract_ThrowsConcurrency_WhenRowVersionMismatch()
        {
            _activeContract.RowVersion = new byte[] { 9, 9, 9 };

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
                _repository.UpdateRentalContract(_activeContract));
        }

        [Fact]
        public async Task UpdateRentalContract_Throws_WhenContractIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _repository.UpdateRentalContract(null));
        }

        #endregion

        #region SOFT DELETE / RESTORE


        [Fact]
        public async Task SoftDeleteRentalContract_SetsDeletedDate()
        {
            _context.ChangeTracker.Clear();

            await _repository.SoftDeleteRentalContract(_activeContract.Id);
            await _repository.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            var deleted = await _repository.GetDeletedRentalContractById(_activeContract.Id);
            deleted.Should().NotBeNull();
            deleted.DeletedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task SoftDeleteRentalContract_RemovesFromActiveQueries()
        {
            _context.ChangeTracker.Clear();

            await _repository.SoftDeleteRentalContract(_activeContract.Id);
            await _repository.SaveChangesAsync();

            var exists = await _repository.RentalContractExists(_activeContract.Id);
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteRentalContract_DoesNotRemoveFromDatabase()
        {
            _context.ChangeTracker.Clear();

            await _repository.SoftDeleteRentalContract(_activeContract.Id);
            await _repository.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            var contract = await _context.RentalContracts
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == _activeContract.Id);

            contract.Should().NotBeNull();
        }

        [Fact]
        public async Task SoftDeleteRentalContract_Throws_WhenContractNotFound()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _repository.SoftDeleteRentalContract(Guid.NewGuid()));
        }

        [Fact]
        public async Task SoftDeleteRentalContract_Throws_WhenIdIsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.SoftDeleteRentalContract(Guid.Empty));
        }

        [Fact]
        public async Task RestoreRentalContract_RestoresDeletedContract()
        {
            _context.ChangeTracker.Clear();

            await _repository.RestoreRentalContract(_deletedContract.Id);
            await _repository.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            var restored = await _repository.GetRentalContractById(_deletedContract.Id, null);
            restored.Should().NotBeNull();
            restored.DeletedDate.Should().BeNull();
        }

        [Fact]
        public async Task RestoreRentalContract_MakesContractVisibleInQueries()
        {
            _context.ChangeTracker.Clear();

            await _repository.RestoreRentalContract(_deletedContract.Id);
            await _repository.SaveChangesAsync();

            var exists = await _repository.RentalContractExists(_deletedContract.Id);
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task RestoreRentalContract_Throws_WhenIdIsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.RestoreRentalContract(Guid.Empty));
        }

        #endregion

        #region BULK OPERATIONS

        [Fact]
        public async Task CreateRentalContracts_BulkInsert()
        {
            var contracts = new List<RentalContract>
            {
                new() { Id = Guid.NewGuid(), RowVersion = new byte[]{1} },
                new() { Id = Guid.NewGuid(), RowVersion = new byte[]{1} }
            };

            var result = await _repository.CreateRentalContracts(contracts);
            await _repository.SaveChangesAsync();

            result.SuccessCount.Should().Be(2);
        }

        [Fact]
        public async Task DeleteRentalContracts_BulkDelete()
        {
            var ids = new[] { _activeContract.Id, _finishedContract.Id };
            _context.ChangeTracker.Clear();
            var result = await _repository.DeleteRentalContracts(ids);
            await _repository.SaveChangesAsync();

            result.SuccessCount.Should().Be(2);
        }

        [Fact]
        public async Task RestoreRentalContracts_BulkRestore()
        {
            var ids = new[] { _deletedContract.Id };
            _context.ChangeTracker.Clear();
            var result = await _repository.RestoreRentalContracts(ids);
            await _repository.SaveChangesAsync();

            result.SuccessCount.Should().Be(1);
        }

        #endregion

        #region SAVE CHANGES

        [Fact]
        public async Task SaveChangesAsync_ReturnsTrue_WhenChangesSaved()
        {
            _activeContract.Shifts += 1;

            var result = await _repository.SaveChangesAsync();

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SaveChangesAsync_ReturnsFalse_WhenNoChanges()
        {
            _context.ChangeTracker.Clear();

            var result = await _repository.SaveChangesAsync();

            result.Should().BeFalse();
        }
        #endregion
    }
}
