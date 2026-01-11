using Application.BulkOperations;
using Application.Interface;
using Application.Interface.Repositories;
using Application.Interface.Services;
using Application.Models.SellingContract.Write;
using Application.ResourceParameters;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Services;
using Mapster;
using Microsoft.AspNetCore.JsonPatch;
using Moq;
using Shared.Results;
using System.ComponentModel.DataAnnotations;

namespace EquipmentAPI.Tests.UnitTests.SellingContractTests
{
    public class SellingContracrServiceTests
    {
        private readonly Mock<ISellingContractRepository> _contractRepo = new();
        private readonly Mock<IEquipmentRepository> _equipmentRepo = new();
        private readonly Mock<ICustomerRepository> _customerRepo = new();
        private readonly Mock<ICacheVersionProvider> _cache = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly SellingContractService _sellingContractService;

        public SellingContracrServiceTests()
        {
            _sellingContractService = new SellingContractService(
                _contractRepo.Object,
                _unitOfWork.Object,
                _customerRepo.Object,
                _equipmentRepo.Object,
                _cache.Object);
        }

        #region GetSellingContracts Tests

        [Fact]
        public async Task GetSellingContracts_ReturnsPagedList_WhenContractsExist()
        {
            // Arramge
            var parameters = new SellingContractResourceParameters();
            var contracts = new List<SellingContract>
            {
                new () {Id = Guid.NewGuid()}
            };

            var paged = new PagedList<SellingContract>(contracts, 1, 1, 10);

            _contractRepo.Setup(r => r.GetSellingContracts(parameters))
                .ReturnsAsync(paged);

            // Act
            var result = await _sellingContractService.GetSellingContracts(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetSellingContracts_ReturnsEmptyPagedList_WhenNoContractsExist()
        {
            // Arrange
            var parameters = new SellingContractResourceParameters();
            var contracts = new List<SellingContract>();

            var paged = new PagedList<SellingContract>(contracts, 0, 1, 10);

            _contractRepo.Setup(r => r.GetSellingContracts(parameters))
                .ReturnsAsync(paged);

            // Act
            var result = await _sellingContractService.GetSellingContracts(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetSellingContracts_ReturnsNull_WhenRepositoryReturnsNull()
        {
            // Arrange
            var parameters = new SellingContractResourceParameters();

            _contractRepo.Setup(r => r.GetSellingContracts(parameters))
                .ReturnsAsync((PagedList<SellingContract>?)null);

            // Act
            var result = await _sellingContractService.GetSellingContracts(parameters);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetSellingContracts_AppliesFiltersCorrectly_WhenParametersProvided()
        {
            // Arrange
            var parameters = new SellingContractResourceParameters
            {
                SearchQuery = "test"
            };

            var contracts = new List<SellingContract>
            {
                new() { Id = Guid.NewGuid() }
            };

            var paged = new PagedList<SellingContract>(contracts, 1, 1, 10);

            _contractRepo.Setup(r => r.GetSellingContracts(parameters))
                .ReturnsAsync(paged);

            // Act
            var result = await _sellingContractService.GetSellingContracts(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetSellingContracts_AppliesPagination_WhenPageSizeAndNumberProvided()
        {
            // Arrange
            var parameters = new SellingContractResourceParameters
            {
                PageNumber = 1,
                PageSize = 5
            };

            var contracts = new List<SellingContract>
            {
                new() { Id = Guid.NewGuid() }
            };

            var paged = new PagedList<SellingContract>(contracts, 1, 1, 5);

            _contractRepo.Setup(r => r.GetSellingContracts(parameters))
                .ReturnsAsync(paged);

            // Act
            var result = await _sellingContractService.GetSellingContracts(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetSellingContracts_AppliesSorting_WhenSortByProvided()
        {
            // Arrange
            var parameters = new SellingContractResourceParameters
            {
                SortBy = "createdDate"
            };

            var contracts = new List<SellingContract>
            {
                new() { Id = Guid.NewGuid() }
            };

            var paged = new PagedList<SellingContract>(contracts, 1, 1, 10);

            _contractRepo.Setup(r => r.GetSellingContracts(parameters))
                .ReturnsAsync(paged);

            // Act
            var result = await _sellingContractService.GetSellingContracts(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetSellingContracts_AppliesSearchTerm_WhenSearchQueryProvided()
        {
            // Arrange
            var parameters = new SellingContractResourceParameters
            {
                SearchQuery = "contract"
            };

            var contracts = new List<SellingContract>
            {
                new() { Id = Guid.NewGuid() }
            };

            var paged = new PagedList<SellingContract>(contracts, 1, 1, 10);

            _contractRepo.Setup(r => r.GetSellingContracts(parameters))
                .ReturnsAsync(paged);

            // Act
            var result = await _sellingContractService.GetSellingContracts(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetSellingContracts_HandlesDefaultParameters_WhenNoParametersProvided()
        {
            // Arrange
            var parameters = new SellingContractResourceParameters();

            var contracts = new List<SellingContract>
            {
                new() { Id = Guid.NewGuid() }
            };

            var paged = new PagedList<SellingContract>(contracts, 1, 1, 10);

            _contractRepo.Setup(r => r.GetSellingContracts(parameters))
                .ReturnsAsync(paged);

            // Act
            var result = await _sellingContractService.GetSellingContracts(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        #endregion

        #region GetSellingContractById Tests

        [Fact]
        public async Task GetSellingContractById_ReturnsDto_WhenExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contract = new SellingContract { Id = id };

            _contractRepo.Setup(r => r.GetSellingContractById(id, null))
                .ReturnsAsync(contract);

            // Act
            var result = await _sellingContractService.GetSellingContractById(id);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetSellingContractById_ReturnsNull_WhenNotExists()
        {
            // Arrange
            var id = Guid.NewGuid();

            _contractRepo.Setup(r => r.GetSellingContractById(id, null))
                .ReturnsAsync((SellingContract?)null);

            // Act
            var result = await _sellingContractService.GetSellingContractById(id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetSellingContractById_ReturnsNull_WhenRepositoryReturnsNull()
        {
            // Arrange
            var id = Guid.NewGuid();

            _contractRepo.Setup(r => r.GetSellingContractById(id, null))
                .ReturnsAsync((SellingContract?)null);

            // Act
            var result = await _sellingContractService.GetSellingContractById(id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetSellingContractById_AppliesFieldShaping_WhenFieldsProvided()
        {
            // Arrange
            var id = Guid.NewGuid();
            var fields = "id, AddedDate";
            var contract = new SellingContract { Id = id, AddedDate = DateTime.UtcNow };

            _contractRepo.Setup(r => r.GetSellingContractById(id, fields))
                .ReturnsAsync(contract);

            // Act
            var result = await _sellingContractService.GetSellingContractById(id, fields);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.AddedDate);
        }

        [Fact]
        public async Task GetSellingContractById_ReturnsFullDto_WhenFieldsIsNull()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contract = new SellingContract { Id = id };

            _contractRepo.Setup(r => r.GetSellingContractById(id, null))
                .ReturnsAsync(contract);

            // Act
            var result = await _sellingContractService.GetSellingContractById(id, null);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetSellingContractById_ReturnsFullDto_WhenFieldsIsEmpty()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contract = new SellingContract { Id = id };
            var fields = "";

            _contractRepo.Setup(r => r.GetSellingContractById(id, fields))
                .ReturnsAsync(contract);

            // Act
            var result = await _sellingContractService.GetSellingContractById(id, string.Empty);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetSellingContractById_ThrowsException_WhenIdIsEmpty()
        {
            // Arrange
            var id = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _sellingContractService.GetSellingContractById(id));
        }

        #endregion

        #region GetDeletedSellingContractById Tests

        [Fact]
        public async Task GetDeletedSellingContractById_ReturnsDto_WhenDeletedContractExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contract = new SellingContract
            {
                Id = id,
                DeletedDate = DateTime.UtcNow
            };

            _contractRepo.Setup(r => r.GetSoftDeletedSellingContractById(id, null))
                .ReturnsAsync(contract);

            // Act
            var result = await _sellingContractService.GetDeletedSellingContractById(id);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetDeletedSellingContractById_ReturnsNull_WhenDeletedContractNotExists()
        {
            // Arrange
            var id = Guid.NewGuid();

            _contractRepo.Setup(r => r.GetSoftDeletedSellingContractById(id, null))
                .ReturnsAsync((SellingContract?)null);

            // Act
            var result = await _sellingContractService.GetDeletedSellingContractById(id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetDeletedSellingContractById_ReturnsNull_WhenRepositoryReturnsNull()
        {
            // Arrange
            var id = Guid.NewGuid();

            _contractRepo.Setup(r => r.GetSoftDeletedSellingContractById(id, null))
                .ReturnsAsync((SellingContract?)null);

            // Act
            var result = await _sellingContractService.GetDeletedSellingContractById(id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetDeletedSellingContractById_AppliesFieldShaping_WhenFieldsProvided()
        {
            // Arrange
            var id = Guid.NewGuid();
            var fields = "id, addeddate";
            var contract = new SellingContract
            {
                Id = id,
                AddedDate = DateTime.UtcNow,
                DeletedDate = DateTime.UtcNow
            };

            _contractRepo.Setup(r => r.GetSoftDeletedSellingContractById(id, fields))
                .ReturnsAsync(contract);

            // Act
            var result = await _sellingContractService.GetDeletedSellingContractById(id, fields);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.AddedDate);
        }

        [Fact]
        public async Task GetDeletedSellingContractById_ReturnsFullDto_WhenFieldsIsNull()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contract = new SellingContract
            {
                Id = id,
                DeletedDate = DateTime.UtcNow
            };

            _contractRepo.Setup(r => r.GetSoftDeletedSellingContractById(id, null))
                .ReturnsAsync(contract);

            // Act
            var result = await _sellingContractService.GetDeletedSellingContractById(id, null);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetDeletedSellingContractById_ThrowsException_WhenIdIsEmpty()
        {
            // Arrange
            var id = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _sellingContractService.GetDeletedSellingContractById(id));
        }

        [Fact]
        public async Task GetDeletedSellingContractById_DoesNotReturnActiveContract_WhenContractIsNotDeleted()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contract = new SellingContract
            {
                Id = id,
                DeletedDate = null
            };

            _contractRepo.Setup(r => r.GetSoftDeletedSellingContractById(id, null))
                .ReturnsAsync((SellingContract?)null);

            // Act
            var result = await _sellingContractService.GetDeletedSellingContractById(id);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetSellingContractsByYear Tests

        [Fact]
        public async Task GetSellingContractsByYear_ReturnsContracts_WhenContractsExistForYear()
        {
            // Arrange
            var year = 2024;
            var contracts = new List<SellingContract>
            {
                new() { Id = Guid.NewGuid() }
            };

            _contractRepo.Setup(r => r.GetSellingContractsByYear(year))
                .ReturnsAsync(contracts);

            // Act
            var result = await _sellingContractService.GetSellingContractsByYear(year);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetSellingContractsByYear_ReturnsEmptyList_WhenNoContractsForYear()
        {
            // Arrange
            var year = 2024;
            var contracts = new List<SellingContract>();

            _contractRepo.Setup(r => r.GetSellingContractsByYear(year))
                .ReturnsAsync(contracts);

            // Act
            var result = await _sellingContractService.GetSellingContractsByYear(year);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetSellingContractsByYear_ReturnsNull_WhenRepositoryReturnsNull()
        {
            // Arrange
            var year = 2024;

            _contractRepo.Setup(r => r.GetSellingContractsByYear(year))
                .ReturnsAsync((IEnumerable<SellingContract>?)null);

            // Act
            var result = await _sellingContractService.GetSellingContractsByYear(year);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetSellingContractsByYear_FiltersCorrectly_WhenMultipleYearsExist()
        {
            // Arrange
            var year = 2023;
            var contracts = new List<SellingContract>
            {
                new() { Id = Guid.NewGuid() }
            };

            _contractRepo.Setup(r => r.GetSellingContractsByYear(year))
                .ReturnsAsync(contracts);

            // Act
            var result = await _sellingContractService.GetSellingContractsByYear(year);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetSellingContractsByYear_HandlesCurrentYear_WhenCurrentYearProvided()
        {
            // Arrange
            var year = DateTime.UtcNow.Year;
            var contracts = new List<SellingContract>
            {
                new() { Id = Guid.NewGuid() }
            };

            _contractRepo.Setup(r => r.GetSellingContractsByYear(year))
                .ReturnsAsync(contracts);

            // Act
            var result = await _sellingContractService.GetSellingContractsByYear(year);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetSellingContractsByYear_HandlesPastYear_WhenPastYearProvided()
        {
            // Arrange
            var year = 2019;
            var contracts = new List<SellingContract>
            {
                new() { Id = Guid.NewGuid() }
            };

            _contractRepo.Setup(r => r.GetSellingContractsByYear(year))
                .ReturnsAsync(contracts);

            // Act
            var result = await _sellingContractService.GetSellingContractsByYear(year);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetSellingContractsByYear_HandlesFutureYear_WhenFutureYearProvided()
        {
            // Arrange
            var year = DateTime.UtcNow.Year + 1;
            var contracts = new List<SellingContract>();

            _contractRepo.Setup(r => r.GetSellingContractsByYear(year))
                .ReturnsAsync(contracts);

            // Act
            var result = await _sellingContractService.GetSellingContractsByYear(year);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetSellingContractsByYear_ThrowsException_WhenYearIsInvalid()
        {
            // Arrange
            var year = -1;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _sellingContractService.GetSellingContractsByYear(year));
        }

        [Fact]
        public async Task GetSellingContractsByYear_ThrowsException_WhenYearIsZero()
        {
            // Arrange
            var year = 0;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _sellingContractService.GetSellingContractsByYear(year));
        }

        [Fact]
        public async Task GetSellingContractsByYear_ThrowsException_WhenYearIsNegative()
        {
            // Arrange
            var year = -2024;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _sellingContractService.GetSellingContractsByYear(year));
        }

        #endregion

        #region GetSellingContractsByCustomerId Tests

        [Fact]
        public async Task GetSellingContractsByCustomerId_ReturnsContracts_WhenCustomerHasContracts()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var contracts = new List<SellingContract>
            {
                new() { Id = Guid.NewGuid(), CustomerId = customerId }
            };

            _contractRepo.Setup(r => r.GetSellingContractsByCustomerId(customerId, null))
                .ReturnsAsync(contracts);

            // Act
            var result = await _sellingContractService.GetSellingContractsByCustomerId(customerId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetSellingContractsByCustomerId_ReturnsEmptyList_WhenCustomerHasNoContracts()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var contracts = new List<SellingContract>();

            _contractRepo.Setup(r => r.GetSellingContractsByCustomerId(customerId, null))
                .ReturnsAsync(contracts);

            // Act
            var result = await _sellingContractService.GetSellingContractsByCustomerId(customerId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetSellingContractsByCustomerId_ReturnsNull_WhenRepositoryReturnsNull()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            _contractRepo.Setup(r => r.GetSellingContractsByCustomerId(customerId, null))
                .ReturnsAsync((IEnumerable<SellingContract>?)null);

            // Act
            var result = await _sellingContractService.GetSellingContractsByCustomerId(customerId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetSellingContractsByCustomerId_AppliesFieldShaping_WhenFieldsProvided()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var fields = "id";
            var contracts = new List<SellingContract>
            {
                new() { Id = Guid.NewGuid(), CustomerId = customerId }
            };

            _contractRepo.Setup(r => r.GetSellingContractsByCustomerId(customerId, fields))
                .ReturnsAsync(contracts);

            // Act
            var result = await _sellingContractService.GetSellingContractsByCustomerId(customerId, fields);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.First().Id);
        }

        [Fact]
        public async Task GetSellingContractsByCustomerId_ReturnsFullDto_WhenFieldsIsNull()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var contracts = new List<SellingContract>
            {
                new() { Id = Guid.NewGuid(), CustomerId = customerId }
            };

            _contractRepo.Setup(r => r.GetSellingContractsByCustomerId(customerId, null))
                .ReturnsAsync(contracts);

            // Act
            var result = await _sellingContractService.GetSellingContractsByCustomerId(customerId, null);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetSellingContractsByCustomerId_ThrowsException_WhenCustomerIdIsEmpty()
        {
            // Arrange
            var customerId = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _sellingContractService.GetSellingContractsByCustomerId(customerId));
        }

        [Fact]
        public async Task GetSellingContractsByCustomerId_ReturnsMultipleContracts_WhenCustomerHasMultiple()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var contracts = new List<SellingContract>
            {
                new() { Id = Guid.NewGuid(), CustomerId = customerId },
                new() { Id = Guid.NewGuid(), CustomerId = customerId }
            };

            _contractRepo.Setup(r => r.GetSellingContractsByCustomerId(customerId, null))
                .ReturnsAsync(contracts);

            // Act
            var result = await _sellingContractService.GetSellingContractsByCustomerId(customerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetSellingContractsByCustomerId_DoesNotReturnDeletedContracts_WhenOnlyActiveRequested()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var contracts = new List<SellingContract>();

            _contractRepo.Setup(r => r.GetSellingContractsByCustomerId(customerId, null))
                .ReturnsAsync(contracts);

            // Act
            var result = await _sellingContractService.GetSellingContractsByCustomerId(customerId);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetSellingContractsByEquipmentId Tests

        [Fact]
        public async Task GetSellingContractsByEquipmentId_ReturnsContracts_WhenEquipmentHasContracts()
        {
            // Arrange
            var equipmentId = Guid.NewGuid();
            var contracts = new List<SellingContract>
            {
                new() { Id = Guid.NewGuid(), EquipmentId = equipmentId }
            };

            _contractRepo.Setup(r => r.GetSellingContractsByEquipment(equipmentId, null))
                .ReturnsAsync(contracts);

            // Act
            var result = await _sellingContractService.GetSellingContractsByEquipmentId(equipmentId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetSellingContractsByEquipmentId_ReturnsEmptyList_WhenEquipmentHasNoContracts()
        {
            // Arrange
            var equipmentId = Guid.NewGuid();
            var contracts = new List<SellingContract>();

            _contractRepo.Setup(r => r.GetSellingContractsByEquipment(equipmentId, null))
                .ReturnsAsync(contracts);

            // Act
            var result = await _sellingContractService.GetSellingContractsByEquipmentId(equipmentId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetSellingContractsByEquipmentId_ReturnsNull_WhenRepositoryReturnsNull()
        {
            // Arrange
            var equipmentId = Guid.NewGuid();

            _contractRepo.Setup(r => r.GetSellingContractsByEquipment(equipmentId, null))
                .ReturnsAsync((IEnumerable<SellingContract>?)null);

            // Act
            var result = await _sellingContractService.GetSellingContractsByEquipmentId(equipmentId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetSellingContractsByEquipmentId_AppliesFieldShaping_WhenFieldsProvided()
        {
            // Arrange
            var equipmentId = Guid.NewGuid();
            var fields = "id";
            var contracts = new List<SellingContract>
            {
                new() { Id = Guid.NewGuid(), EquipmentId = equipmentId }
            };

            _contractRepo.Setup(r => r.GetSellingContractsByEquipment(equipmentId, fields))
                .ReturnsAsync(contracts);

            // Act
            var result = await _sellingContractService.GetSellingContractsByEquipmentId(equipmentId, fields);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.First().Id);
        }

        [Fact]
        public async Task GetSellingContractsByEquipmentId_ReturnsFullDto_WhenFieldsIsNull()
        {
            // Arrange
            var equipmentId = Guid.NewGuid();
            var contracts = new List<SellingContract>
            {
                new() { Id = Guid.NewGuid(), EquipmentId = equipmentId }
            };

            _contractRepo.Setup(r => r.GetSellingContractsByEquipment(equipmentId, null))
                .ReturnsAsync(contracts);

            // Act
            var result = await _sellingContractService.GetSellingContractsByEquipmentId(equipmentId, null);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetSellingContractsByEquipmentId_ThrowsException_WhenEquipmentIdIsEmpty()
        {
            // Arrange
            var equipmentId = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _sellingContractService.GetSellingContractsByEquipmentId(equipmentId));
        }

        [Fact]
        public async Task GetSellingContractsByEquipmentId_ReturnsMultipleContracts_WhenEquipmentHasMultiple()
        {
            // Arrange
            var equipmentId = Guid.NewGuid();
            var contracts = new List<SellingContract>
            {
                new() { Id = Guid.NewGuid(), EquipmentId = equipmentId },
                new() { Id = Guid.NewGuid(), EquipmentId = equipmentId }
            };

            _contractRepo.Setup(r => r.GetSellingContractsByEquipment(equipmentId, null))
                .ReturnsAsync(contracts);

            // Act
            var result = await _sellingContractService.GetSellingContractsByEquipmentId(equipmentId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        #endregion

        #region GetSellingContractsByIds Tests

        [Fact]
        public async Task GetSellingContractsByIds_ReturnsContracts_WhenAllIdsExist()
        {
            // Arrange
            var ids = new[] { Guid.NewGuid(), Guid.NewGuid() };
            var contracts = ids.Select(id => new SellingContract { Id = id }).ToList();

            _contractRepo.Setup(r => r.GetSellingContractsByIds(ids, null))
                .ReturnsAsync(contracts);

            // Act
            var result = await _sellingContractService.GetSellingContractsByIds(ids);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetSellingContractsByIds_ReturnsPartialList_WhenSomeIdsExist()
        {
            // Arrange
            var ids = new[] { Guid.NewGuid(), Guid.NewGuid() };
            var contracts = new List<SellingContract>
            {
                new() { Id = ids[0] }
            };

            _contractRepo.Setup(r => r.GetSellingContractsByIds(ids, null))
                .ReturnsAsync(contracts);

            // Act
            var result = await _sellingContractService.GetSellingContractsByIds(ids);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetSellingContractsByIds_ReturnsEmptyList_WhenNoIdsExist()
        {
            // Arrange
            var ids = new[] { Guid.NewGuid(), Guid.NewGuid() };

            _contractRepo.Setup(r => r.GetSellingContractsByIds(ids, null))
                .ReturnsAsync(new List<SellingContract>());

            // Act
            var result = await _sellingContractService.GetSellingContractsByIds(ids);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetSellingContractsByIds_ReturnsNull_WhenRepositoryReturnsNull()
        {
            // Arrange
            var ids = new[] { Guid.NewGuid() };

            _contractRepo.Setup(r => r.GetSellingContractsByIds(ids, null))
                .ReturnsAsync((IEnumerable<SellingContract>?)null);

            // Act
            var result = await _sellingContractService.GetSellingContractsByIds(ids);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetSellingContractsByIds_ReturnsEmptyList_WhenIdsCollectionIsEmpty()
        {
            // Arrange
            var ids = Array.Empty<Guid>();

            // Act
            var result = await _sellingContractService.GetSellingContractsByIds(ids);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region GetDeletedSellingContracts Tests

        [Fact]
        public async Task GetDeletedSellingContracts_ReturnsDeletedContracts_WhenDeletedContractsExist()
        {
            // Arrange
            var contracts = new List<SellingContract>
            {
                new() { Id = Guid.NewGuid(), DeletedDate = DateTime.UtcNow }
            };

            _contractRepo.Setup(r => r.GetSoftDeletedSellingContracts(It.IsAny<SellingContractResourceParameters>()))
                .ReturnsAsync(contracts);

            // Act
            var result = await _sellingContractService.GetDeletedSellingContracts(new());

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetDeletedSellingContracts_ReturnsEmptyList_WhenNoDeletedContractsExist()
        {
            // Arrange
            _contractRepo.Setup(r => r.GetSoftDeletedSellingContracts(It.IsAny<SellingContractResourceParameters>()))
                .ReturnsAsync(new List<SellingContract>());

            // Act
            var result = await _sellingContractService.GetDeletedSellingContracts(new());

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetDeletedSellingContracts_ReturnsNull_WhenRepositoryReturnsNull()
        {
            // Arrange
            _contractRepo.Setup(r => r.GetSoftDeletedSellingContracts(It.IsAny<SellingContractResourceParameters>()))
                .ReturnsAsync((IEnumerable<SellingContract>?)null);

            // Act
            var result = await _sellingContractService.GetDeletedSellingContracts(new());

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region SellingContractExists Tests

        [Fact]
        public async Task SellingContractExists_ReturnsTrue_WhenContractExists()
        {
            // Arrange
            var id = Guid.NewGuid();

            _contractRepo.Setup(r => r.SellingContractExists(id))
                .ReturnsAsync(true);

            // Act
            var result = await _sellingContractService.SellingContractExists(id);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task SellingContractExists_ReturnsFalse_WhenContractDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();

            _contractRepo.Setup(r => r.SellingContractExists(id))
                .ReturnsAsync(false);

            // Act
            var result = await _sellingContractService.SellingContractExists(id);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task SellingContractExists_ThrowsException_WhenIdIsEmpty()
        {
            // Arrange
            var id = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _sellingContractService.SellingContractExists(id));
        }

        [Fact]
        public async Task SellingContractExists_ReturnsFalse_WhenContractIsDeleted()
        {
            // Arrange
            var id = Guid.NewGuid();

            _contractRepo.Setup(r => r.SellingContractExists(id))
                .ReturnsAsync(false);

            // Act
            var result = await _sellingContractService.SellingContractExists(id);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region CustomerHasContracts Tests

        [Fact]
        public async Task CustomerHasContracts_ReturnsTrue_WhenCustomerHasContracts()
        {
            var customerId = Guid.NewGuid();

            _contractRepo.Setup(r => r.CustomerHasContracts(customerId))
                .ReturnsAsync(true);

            var result = await _sellingContractService.CustomerHasContracts(customerId);

            Assert.True(result);
        }

        [Fact]
        public async Task CustomerHasContracts_ReturnsFalse_WhenCustomerHasNoContracts()
        {
            var customerId = Guid.NewGuid();

            _contractRepo.Setup(r => r.CustomerHasContracts(customerId))
                .ReturnsAsync(false);

            var result = await _sellingContractService.CustomerHasContracts(customerId);

            Assert.False(result);
        }

        [Fact]
        public async Task CustomerHasContracts_ThrowsException_WhenCustomerIdIsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _sellingContractService.CustomerHasContracts(Guid.Empty));
        }

        [Fact]
        public async Task CustomerHasContracts_ReturnsFalse_WhenCustomerDoesNotExist()
        {
            var customerId = Guid.NewGuid();

            _contractRepo.Setup(r => r.CustomerHasContracts(customerId))
                .ReturnsAsync(false);

            var result = await _sellingContractService.CustomerHasContracts(customerId);

            Assert.False(result);
        }

        #endregion

        #region CreateSellingContract Tests

        [Fact]
        public async Task CreateSellingContract_ReturnsDto_WhenCreatedSuccessfully()
        {
            // Arrange
            var contract = new SellingContract { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid(), EquipmentId = Guid.NewGuid() };

            _customerRepo.Setup(r => r.CustomerExists(contract.CustomerId))
                .ReturnsAsync(true);
            _equipmentRepo.Setup(r => r.EquipmentExists(contract.EquipmentId))
                .ReturnsAsync(true);
            _contractRepo.Setup(r => r.CreateSellingContract(contract))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sellingContractService.CreateSellingContract(contract.Adapt<SellingContractCreateDto>());

            // Assert
            Assert.NotNull(result);
            Assert.Equal(contract.CustomerId, result.CustomerId);
        }

        [Fact]
        public async Task CreateSellingContract_ThrowsException_WhenDtoIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sellingContractService.CreateSellingContract(null!));
        }

        [Fact]
        public async Task CreateSellingContract_ThrowsException_WhenCustomerDoesNotExist()
        {
            // Arrange
            var contract = new SellingContract { CustomerId = Guid.NewGuid(), EquipmentId = Guid.NewGuid() };
            _customerRepo.Setup(r => r.CustomerExists(contract.CustomerId)).ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sellingContractService.CreateSellingContract(contract.Adapt<SellingContractCreateDto>()));
        }

        [Fact]
        public async Task CreateSellingContract_ThrowsException_WhenEquipmentDoesNotExist()
        {
            // Arrange
            var contract = new SellingContract { CustomerId = Guid.NewGuid(), EquipmentId = Guid.NewGuid() };
            _customerRepo.Setup(r => r.CustomerExists(contract.CustomerId)).ReturnsAsync(true);
            _equipmentRepo.Setup(r => r.EquipmentExists(contract.EquipmentId)).ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sellingContractService.CreateSellingContract(contract.Adapt<SellingContractCreateDto>()));
        }
        #endregion

        #region UpdateSellingContract Tests

        [Fact]
        public async Task UpdateSellingContract_UpdatesSuccessfully_WhenValidDataProvided()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contract = new SellingContract { Id = id };
            var dto = new SellingContractUpdateDto { SalePrice = 5000 };

            _contractRepo.Setup(r => r.GetSellingContractForUpdate(id, null))
                .ReturnsAsync(contract);

            _equipmentRepo.Setup(e => e.EquipmentExists(contract.EquipmentId))
                .ReturnsAsync(true);

            _contractRepo.Setup(r => r.GetSellingContractById(id, "saleprice"))
                .ReturnsAsync(contract);

            // Act
            await _sellingContractService.UpdateSellingContract(id, dto);

            // Assert
            var updated = await _sellingContractService.GetSellingContractById(id, "saleprice");
            updated.SaleDate.Equals(5000);
            _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateSellingContract_ThrowsException_WhenIdIsEmpty()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sellingContractService.UpdateSellingContract(Guid.Empty, new()));
        }

        [Fact]
        public async Task UpdateSellingContract_ThrowsException_WhenDtoIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sellingContractService.UpdateSellingContract(Guid.NewGuid(), null!));
        }

        [Fact]
        public async Task UpdateSellingContract_ThrowsException_WhenContractDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();

            _contractRepo.Setup(r => r.GetSellingContractForUpdate(id, null))
                .ReturnsAsync((SellingContract?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _sellingContractService.UpdateSellingContract(id, new()));
        }

        [Fact]
        public async Task UpdateSellingContract_ThrowsException_WhenContractIsDeleted()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contract = new SellingContract { Id = id, DeletedDate = DateTime.UtcNow };

            _contractRepo.Setup(r => r.GetSellingContractForUpdate(id, null))
                .ReturnsAsync(contract);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sellingContractService.UpdateSellingContract(id, new()));
        }

        [Fact]
        public async Task UpdateSellingContract_ThrowsException_WhenRequiredFieldsAreMissing()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contract = new SellingContract { Id = id };
            var dto = new SellingContractUpdateDto();

            _contractRepo.Setup(r => r.GetSellingContractForUpdate(id, null))
                .ReturnsAsync(contract);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sellingContractService.UpdateSellingContract(id, dto));
        }

        [Fact]
        public async Task UpdateSellingContract_UpdatesAllProperties_WhenValidDataProvided()
        {
            // Arrange
            var id = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var equipmentId = Guid.NewGuid();

            var existing = new SellingContract
            {
                Id = id,
                SalePrice = 100,
                CustomerId = customerId,
                EquipmentId = equipmentId
            };

            var dto = new SellingContractUpdateDto
            {
                SalePrice = 200,
                CustomerId = customerId,
                EquipmentId = equipmentId
            };

            _contractRepo.Setup(r => r.GetSellingContractForUpdate(id, null))
                .ReturnsAsync(existing);

            _customerRepo.Setup(r => r.CustomerExists(customerId))
                .ReturnsAsync(true);

            _equipmentRepo.Setup(r => r.EquipmentExists(equipmentId))
                .ReturnsAsync(true);

            // Act
            await _sellingContractService.UpdateSellingContract(id, dto);

            // Assert
            Assert.Equal(200, existing.SalePrice);
            _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateSellingContract_ThrowsException_WhenCustomerDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            var contract = new SellingContract { Id = id };
            var dto = new SellingContractUpdateDto { CustomerId = customerId };

            _contractRepo.Setup(r => r.GetSellingContractForUpdate(id, null))
                .ReturnsAsync(contract);

            _customerRepo.Setup(r => r.CustomerExists(customerId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sellingContractService.UpdateSellingContract(id, dto));
        }

        [Fact]
        public async Task UpdateSellingContract_ThrowsException_WhenEquipmentDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            var equipmentId = Guid.NewGuid();

            var contract = new SellingContract { Id = id };
            var dto = new SellingContractUpdateDto { EquipmentId = equipmentId };

            _contractRepo.Setup(r => r.GetSellingContractForUpdate(id, null))
                .ReturnsAsync(contract);

            _equipmentRepo.Setup(r => r.EquipmentExists(equipmentId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sellingContractService.UpdateSellingContract(id, dto));
        }

        [Fact]
        public async Task UpdateSellingContract_ThrowsException_WhenValidationFails()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contract = new SellingContract { Id = id };
            var dto = new SellingContractUpdateDto { SalePrice = -100 };

            _contractRepo.Setup(r => r.GetSellingContractForUpdate(id, null))
                .ReturnsAsync(contract);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sellingContractService.UpdateSellingContract(id, dto));
        }

        [Fact]
        public async Task UpdateSellingContract_ThrowsException_WhenRepositorySaveFails()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contract = new SellingContract { Id = id };

            _contractRepo.Setup(r => r.GetSellingContractById(id, null))
                .ReturnsAsync(contract);

            _unitOfWork.Setup(u => u.SaveChangesAsync())
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _sellingContractService.UpdateSellingContract(id, new()));
        }

        [Fact]
        public async Task UpdateSellingContract_HandlesNullableFields_WhenProvided()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contract = new SellingContract { Id = id, SalePrice = 1500 };
            var dto = new SellingContractUpdateDto { SalePrice = 500 };

            _contractRepo.Setup(r => r.GetSellingContractForUpdate(id, null))
                .ReturnsAsync(contract);

            _equipmentRepo.Setup(e => e.EquipmentExists(contract.EquipmentId))
                .ReturnsAsync(true);

            _equipmentRepo.Setup(e => e.GetEquipmentStatus(contract.EquipmentId))
                .ReturnsAsync(EquipmentStatus.Available);

            _customerRepo.Setup(c => c.CustomerExists(contract.CustomerId))
                .ReturnsAsync(true);

            // Act
            await _sellingContractService.UpdateSellingContract(id, dto);

            // Assert
            Assert.Equal(contract.SalePrice, 500);
        }

        #endregion

        #region Patch Tests

        [Fact]
        public async Task Patch_AppliesPatchSuccessfully_WhenValidPatchDocumentProvided()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contract = new SellingContract { Id = id, SalePrice = 100 };

            var patch = new JsonPatchDocument<SellingContractUpdateDto>();
            patch.Replace(c => c.SalePrice, 200);

            _equipmentRepo.Setup(e => e.EquipmentExists(contract.EquipmentId))
               .ReturnsAsync(true);

            _customerRepo.Setup(c => c.CustomerExists(contract.CustomerId))
                .ReturnsAsync(true);

            _contractRepo.Setup(r => r.GetSellingContractForUpdate(id, null))
                .ReturnsAsync(contract);

            // Act
            await _sellingContractService.Patch(id, patch);

            // Assert
            Assert.Equal(200, contract.SalePrice);
            _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Patch_ThrowsException_WhenIdIsEmpty()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _sellingContractService.Patch(Guid.Empty, new JsonPatchDocument<SellingContractUpdateDto>()));
        }

        [Fact]
        public async Task Patch_ThrowsException_WhenPatchDocumentIsNull()
        {
            await Assert.ThrowsAsync<ValidationException>(() =>
                _sellingContractService.Patch(Guid.NewGuid(), null!));
        }

        [Fact]
        public async Task Patch_ThrowsException_WhenContractDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contract = new SellingContract
            {
                Id = id
            };

            _contractRepo.Setup(r => r.GetSellingContractForUpdate(id, null))
                .ReturnsAsync(contract);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sellingContractService.Patch(id, new JsonPatchDocument<SellingContractUpdateDto>()));
        }

        [Fact]
        public async Task Patch_ThrowsException_WhenContractIsDeleted()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contract = new SellingContract { Id = id, DeletedDate = DateTime.UtcNow };

            _contractRepo.Setup(r => r.GetSellingContractForUpdate(id, null))
                .ReturnsAsync(contract);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sellingContractService.Patch(id, new JsonPatchDocument<SellingContractUpdateDto>()));
        }

        [Fact]
        public async Task Patch_UpdatesSingleProperty_WhenSingleOperationProvided()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contract = new SellingContract { Id = id, SalePrice = 100 };

            var patch = new JsonPatchDocument<SellingContractUpdateDto>();
            patch.Replace(c => c.SalePrice, 150);

            _contractRepo.Setup(r => r.GetSellingContractForUpdate(id, null))
                .ReturnsAsync(contract);

            _customerRepo.Setup(c => c.CustomerExists(contract.CustomerId))
                .ReturnsAsync(true);

            // Act
            await _sellingContractService.Patch(id, patch);

            // Assert
            Assert.Equal(150, contract.SalePrice);
        }

        [Fact]
        public async Task Patch_UpdatesMultipleProperties_WhenMultipleOperationsProvided()
        {
            // Arrange
            var id = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var contract = new SellingContract
            {
                Id = id,
                SalePrice = 100,
                CustomerId = customerId
            };

            var patch = new JsonPatchDocument<SellingContractUpdateDto>();
            patch.Replace(c => c.SalePrice, 300);
            patch.Replace(c => c.CustomerId, customerId);

            _contractRepo.Setup(r => r.GetSellingContractForUpdate(id, null))
                .ReturnsAsync(contract);

            _customerRepo.Setup(c => c.CustomerExists(contract.CustomerId))
                .ReturnsAsync(true);

            // Act
            await _sellingContractService.Patch(id, patch);

            // Assert
            Assert.Equal(300, contract.SalePrice);
            Assert.Equal(customerId, contract.CustomerId);
        }

        [Fact]
        public async Task Patch_ThrowsException_WhenPatchViolatesValidation()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contract = new SellingContract { Id = id, SalePrice = 100 };

            var patch = new JsonPatchDocument<SellingContractUpdateDto>();
            patch.Replace(c => c.SalePrice, -50);

            _contractRepo.Setup(r => r.GetSellingContractForUpdate(id, null))
                .ReturnsAsync(contract);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sellingContractService.Patch(id, patch));
        }

        [Fact]
        public async Task Patch_HandlesReplaceOperation_WhenReplaceProvided()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contract = new SellingContract { Id = id, SalePrice = 250 };

            var patch = new JsonPatchDocument<SellingContractUpdateDto>();
            patch.Replace(c => c.SalePrice, 450);

            _customerRepo.Setup(c => c.CustomerExists(contract.CustomerId))
                .ReturnsAsync(true);

            _contractRepo.Setup(r => r.GetSellingContractForUpdate(id, null))
                .ReturnsAsync(contract);

            // Act
            await _sellingContractService.Patch(id, patch);

            // Assert
            Assert.Equal(450, contract.SalePrice);
        }

        [Fact]
        public async Task Patch_HandlesAddOperation_WhenAddProvided()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contract = new SellingContract { Id = id };

            var patch = new JsonPatchDocument<SellingContractUpdateDto>();
            patch.Add(c => c.SalePrice, 450);

            _customerRepo.Setup(c => c.CustomerExists(contract.CustomerId))
                .ReturnsAsync(true);

            _contractRepo.Setup(r => r.GetSellingContractForUpdate(id, null))
                .ReturnsAsync(contract);

            // Act
            await _sellingContractService.Patch(id, patch);

            // Assert
            Assert.Equal(450, contract.SalePrice);
        }

        [Fact]
        public async Task Patch_HandlesRemoveOperation_WhenRemoveProvided()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contract = new SellingContract { Id = id, SalePrice = 450 };

            var patch = new JsonPatchDocument<SellingContractUpdateDto>();
            patch.Remove(c => c.SalePrice);

            _customerRepo.Setup(c => c.CustomerExists(contract.CustomerId))
                .ReturnsAsync(true);

            _contractRepo.Setup(r => r.GetSellingContractForUpdate(id, null))
                .ReturnsAsync(contract);

            // Act
            await _sellingContractService.Patch(id, patch);

            // Assert
            Assert.Equal(0, contract.SalePrice);
        }

        [Fact]
        public async Task Patch_ThrowsException_WhenRepositorySaveFails()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contract = new SellingContract { Id = id };

            var patch = new JsonPatchDocument<SellingContractUpdateDto>();
            patch.Replace(c => c.SalePrice, 750);

            _contractRepo.Setup(r => r.GetSellingContractForUpdate(id, null))
                .ReturnsAsync(contract);

            _unitOfWork.Setup(u => u.SaveChangesAsync())
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sellingContractService.Patch(id, patch));
        }

        #endregion

        #region SoftDeleteSellingContract Tests

        [Fact]
        public async Task SoftDeleteSellingContract_DeletesSuccessfully_WhenContractExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contract = new SellingContract { Id = id };

            _contractRepo.Setup(r => r.GetSellingContractById(id, null))
                .ReturnsAsync(contract);

            // Act
            await _sellingContractService.SoftDeleteSellingContract(id);

            // Assert
            _contractRepo.Verify(r => r.SoftDeleteSellingContract(contract.Id), Times.Once);
        }

        [Fact]
        public async Task SoftDeleteSellingContract_ThrowsException_WhenIdIsEmpty()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _sellingContractService.SoftDeleteSellingContract(Guid.Empty));
        }

        [Fact]
        public async Task SoftDeleteSellingContract_ThrowsException_WhenContractDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();

            _contractRepo.Setup(r => r.GetSellingContractForUpdate(id, null))
                .ReturnsAsync((SellingContract?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _sellingContractService.SoftDeleteSellingContract(id));
        }

        [Fact]
        public async Task SoftDeleteSellingContract_ThrowsException_WhenContractAlreadyDeleted()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contract = new SellingContract
            {
                Id = id,
                DeletedDate = DateTime.UtcNow
            };

            _contractRepo.Setup(r => r.GetSellingContractForUpdate(id, null))
                .ReturnsAsync(contract);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _sellingContractService.SoftDeleteSellingContract(id));
        }

        [Fact]
        public async Task SoftDeleteSellingContract_MaintainsData_WhenDeleted()
        {
            // Arrange
            var id = Guid.NewGuid();
            var originalPrice = 1000m;

            var contract = new SellingContract
            {
                Id = id,
                SalePrice = originalPrice
            };

            _contractRepo.Setup(r => r.GetSellingContractById(id, null))
                .ReturnsAsync(contract);

            // Act
            await _sellingContractService.SoftDeleteSellingContract(id);

            // Assert
            Assert.Equal(originalPrice, contract.SalePrice);
        }
        #endregion

        #region RestoreSellingContract Tests

        [Fact]
        public async Task RestoreSellingContract_RestoresSuccessfully_WhenDeletedContractExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contract = new SellingContract
            {
                Id = id,
                DeletedDate = DateTimeOffset.UtcNow,
                EquipmentId = Guid.NewGuid()
            };

            _contractRepo.Setup(r => r.GetSoftDeletedSellingContractById(id, null))
                .ReturnsAsync(contract);

            _equipmentRepo.Setup(e => e.EquipmentExists(contract.EquipmentId))
                .ReturnsAsync(true);

            // Act
            await _sellingContractService.RestoreSellingContract(id);

            // Assert
            _contractRepo.Verify(r => r.RestoreSellingContract(id), Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
            _unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task RestoreSellingContract_ThrowsException_WhenIdIsEmpty()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sellingContractService.RestoreSellingContract(Guid.Empty));
        }

        [Fact]
        public async Task RestoreSellingContract_ThrowsException_WhenContractDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();

            _contractRepo.Setup(r => r.GetSoftDeletedSellingContractById(id, null))
                .ReturnsAsync((SellingContract?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _sellingContractService.RestoreSellingContract(id));
        }

        [Fact]
        public async Task RestoreSellingContract_ThrowsException_WhenContractIsNotDeleted()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contract = new SellingContract
            {
                Id = id,
                DeletedDate = null
            };

            _contractRepo.Setup(r => r.GetSoftDeletedSellingContractById(id, null))
                .ReturnsAsync(contract);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sellingContractService.RestoreSellingContract(id));
        }

        [Fact]
        public async Task RestoreSellingContract_ClearsDeletedFlag_WhenRestored()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contract = new SellingContract
            {
                Id = id,
            };

            _contractRepo.Setup(r => r.GetSoftDeletedSellingContractById(id, null))
                .ReturnsAsync(contract);

            _equipmentRepo.Setup(e => e.EquipmentExists(contract.EquipmentId))
                .ReturnsAsync(true);

            _contractRepo.Setup(r => r.RestoreSellingContract(id))
                .Returns(Task.CompletedTask);

            // Act
            await _sellingContractService.RestoreSellingContract(id);

            // Assert
            Assert.Null(contract.DeletedDate);
        }

        [Fact]
        public async Task RestoreSellingContract_ThrowsException_WhenRepositorySaveFails()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contract = new SellingContract
            {
                Id = id,
                DeletedDate = DateTimeOffset.UtcNow
            };

            _contractRepo.Setup(r => r.GetSoftDeletedSellingContractById(id, null))
                .ReturnsAsync(contract);

            _unitOfWork.Setup(u => u.SaveChangesAsync())
                .ThrowsAsync(new Exception("DB failure"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sellingContractService.RestoreSellingContract(id));
        }
        #endregion

        #region Bulk Operations Tests

        #region CreateSellingContracts

        [Fact]
        public async Task CreateSellingContracts_ReturnsSuccessResult_WhenAllContractsCreatedSuccessfully()
        {
            // Arrange
            var dtos = new List<SellingContractCreateDto>
            {
                new() { EquipmentId = Guid.NewGuid() },
                new() { EquipmentId = Guid.NewGuid() }
            };

            var result = new BulkOperationResult { SuccessCount = 2, FailureCount = 0 };

            _contractRepo.Setup(r => r.CreateSellingContracts(It.IsAny<IEnumerable<SellingContract>>()))
                .ReturnsAsync(result);

            // Act
            var res = await _sellingContractService.CreateSellingContracts(dtos);

            // Assert
            Assert.Equal(2, res.SuccessCount);
            _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
            _unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateSellingContracts_ThrowsException_WhenCollectionIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sellingContractService.CreateSellingContracts(null!));
        }

        [Fact]
        public async Task CreateSellingContracts_ThrowsException_WhenCollectionIsEmpty()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sellingContractService.CreateSellingContracts(new List<SellingContractCreateDto>()));
        }

        [Fact]
        public async Task CreateSellingContracts_ThrowsException_WhenRepositoryFails()
        {
            // Arrange
            var dtos = new List<SellingContractCreateDto>
            {
                new() { EquipmentId = Guid.NewGuid() }
            };

            _contractRepo.Setup(r => r.CreateSellingContracts(It.IsAny<IEnumerable<SellingContract>>()))
                .ThrowsAsync(new Exception("DB failure"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _sellingContractService.CreateSellingContracts(dtos));
        }

        #endregion

        #region SoftDeleteSellingContracts

        [Fact]
        public async Task SoftDeleteSellingContracts_ReturnsSuccessResult_WhenAllContractsDeletedSuccessfully()
        {
            // Arrange
            var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var contracts = ids.Select(id => new SellingContract { Id = id, EquipmentId = Guid.NewGuid() }).ToList();

            _contractRepo.Setup(r => r.GetSellingContractsByIds(ids, null))
                .ReturnsAsync(contracts);

            var result = new BulkOperationResult { SuccessCount = ids.Count, FailureCount = 0 };

            _contractRepo.Setup(r => r.SoftDeleteSellingContracts(ids))
                .ReturnsAsync(result);

            // Act
            var res = await _sellingContractService.SoftDeleteSellingContracts(ids);

            // Assert
            Assert.Equal(ids.Count, res.SuccessCount);
            _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
            _unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task SoftDeleteSellingContracts_ThrowsException_WhenCollectionIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sellingContractService.SoftDeleteSellingContracts(null!));
        }

        [Fact]
        public async Task SoftDeleteSellingContracts_ThrowsException_WhenCollectionIsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sellingContractService.SoftDeleteSellingContracts(new List<Guid>()));
        }

        [Fact]
        public async Task SoftDeleteSellingContracts_ThrowsException_WhenRepositoryFails()
        {
            var ids = new List<Guid> { Guid.NewGuid() };
            _contractRepo.Setup(r => r.GetSellingContractsByIds(ids, null))
                .ReturnsAsync(new List<SellingContract> { new() { Id = ids[0], EquipmentId = Guid.NewGuid() } });

            _contractRepo.Setup(r => r.SoftDeleteSellingContracts(ids))
                .ThrowsAsync(new Exception("DB failure"));

            await Assert.ThrowsAsync<Exception>(() =>
                _sellingContractService.SoftDeleteSellingContracts(ids));
        }

        #endregion

        #region RestoreSellingContracts

        [Fact]
        public async Task RestoreSellingContracts_ReturnsSuccessResult_WhenAllContractsRestoredSuccessfully()
        {
            // Arrange
            var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var contracts = ids.Select(id => new SellingContract { Id = id, EquipmentId = Guid.NewGuid() }).ToList();

            _contractRepo.Setup(r => r.GetSoftDeletedSellingContractsByIds(ids, null))
                .ReturnsAsync(contracts);

            var result = new BulkOperationResult { SuccessCount = ids.Count, FailureCount = 0 };
            _contractRepo.Setup(r => r.RestoreSellingContracts(ids)).ReturnsAsync(result);

            _equipmentRepo.Setup(e => e.GetEquipmentStatus(It.IsAny<Guid>()))
                .ReturnsAsync(EquipmentStatus.Available);

            // Act
            var res = await _sellingContractService.RestoreSellingContracts(ids);

            // Assert
            Assert.Equal(ids.Count, res.SuccessCount);
            _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
            _unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task RestoreSellingContracts_ReturnsFailureResult_WhenNoDeletedContractsExist()
        {
            // Arrange
            var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            _contractRepo.Setup(r => r.GetSoftDeletedSellingContractsByIds(ids, null))
                .ReturnsAsync(new List<SellingContract>());

            // Act
            var res = await _sellingContractService.RestoreSellingContracts(ids);

            // Assert
            Assert.Equal(0, res.SuccessCount);
            Assert.Equal(ids.Count, res.FailureCount);
        }

        [Fact]
        public async Task RestoreSellingContracts_ThrowsException_WhenCollectionIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sellingContractService.RestoreSellingContracts(null!));
        }

        [Fact]
        public async Task RestoreSellingContracts_ThrowsException_WhenCollectionIsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sellingContractService.RestoreSellingContracts(new List<Guid>()));
        }

        [Fact]
        public async Task RestoreSellingContracts_ThrowsException_WhenEquipmentNotAvailable()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contract = new SellingContract { Id = id, EquipmentId = Guid.NewGuid() };

            _contractRepo.Setup(r => r.GetSoftDeletedSellingContractsByIds(It.IsAny<IEnumerable<Guid>>(), null))
                .ReturnsAsync(new List<SellingContract> { contract });

            _equipmentRepo.Setup(e => e.GetEquipmentStatus(contract.EquipmentId))
                .ReturnsAsync(EquipmentStatus.Sold);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sellingContractService.RestoreSellingContracts(new[] { id }));
        }

        [Fact]
        public async Task RestoreSellingContracts_ThrowsException_WhenRepositoryFails()
        {
            var ids = new List<Guid> { Guid.NewGuid() };

            _contractRepo.Setup(r => r.GetSoftDeletedSellingContractsByIds(ids, null))
                .ThrowsAsync(new Exception("DB failure"));

            await Assert.ThrowsAsync<Exception>(() =>
                _sellingContractService.RestoreSellingContracts(ids));
        }

        #endregion

        #endregion

    }
}
