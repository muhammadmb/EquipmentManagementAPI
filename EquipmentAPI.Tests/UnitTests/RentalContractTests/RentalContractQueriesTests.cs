using API.GraphQL.RentalContract.Inputs;
using API.GraphQL.RentalContract.Queries;
using Application.Interface.Services;
using Application.Models.RentalContractModels.Read;
using Application.ResourceParameters;
using FluentAssertions;
using Moq;
using Shared.Results;

namespace EquipmentAPI.Tests.UnitTests.RentalContractTests
{
    public class RentalContractQueriesTests
    {
        private readonly Mock<IRentalContractService> _serviceMock;
        private readonly RentalContractQueries _queries;

        public RentalContractQueriesTests()
        {
            _serviceMock = new Mock<IRentalContractService>();
            _queries = new RentalContractQueries();
        }

        #region GetRentalContracts

        [Fact]
        public async Task GetRentalContracts_CallsServiceOnce()
        {
            // Arrange
            var filterInput = new RentalContractFilterInput();

            var pagedResult = new PagedList<RentalContractDto>(
                new List<RentalContractDto>(),
                count: 0,
                pageNumber: 1,
                pageSize: 10);

            _serviceMock
                .Setup(s => s.GetRentalContracts(It.IsAny<RentalContractResourceParameters>()))
                .ReturnsAsync(pagedResult);

            // Act
            await _queries.GetRentalContracts(filterInput, _serviceMock.Object);

            // Assert
            _serviceMock.Verify(
                s => s.GetRentalContracts(It.IsAny<RentalContractResourceParameters>()),
                Times.Once);
        }

        [Fact]
        public async Task GetRentalContracts_MapsFilterInputToResourceParameters()
        {
            // Arrange
            var filterInput = new RentalContractFilterInput
            {
                PageNumber = 2,
                PageSize = 5
            };

            var pagedResult = new PagedList<RentalContractDto>(
                new List<RentalContractDto>(),
                0, 2, 5);

            _serviceMock
                .Setup(s => s.GetRentalContracts(It.IsAny<RentalContractResourceParameters>()))
                .ReturnsAsync(pagedResult);

            // Act
            await _queries.GetRentalContracts(filterInput, _serviceMock.Object);

            // Assert
            _serviceMock.Verify(s =>
                s.GetRentalContracts(It.Is<RentalContractResourceParameters>(p =>
                    p.PageNumber == 2 &&
                    p.PageSize == 5)),
                Times.Once);
        }

        [Fact]
        public async Task GetRentalContracts_ReturnsEmpty_WhenServiceReturnsEmpty()
        {
            // Arrange
            var filterInput = new RentalContractFilterInput();

            var pagedResult = new PagedList<RentalContractDto>(
                new List<RentalContractDto>(),
                0, 1, 10);

            _serviceMock
                .Setup(s => s.GetRentalContracts(It.IsAny<RentalContractResourceParameters>()))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _queries.GetRentalContracts(filterInput, _serviceMock.Object);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetRentalContracts_ReturnsItems_FromService()
        {
            // Arrange
            var filterInput = new RentalContractFilterInput();

            var pagedResult = new PagedList<RentalContractDto>(
                new List<RentalContractDto>
                {
            new RentalContractDto { Id = Guid.NewGuid() },
            new RentalContractDto { Id = Guid.NewGuid() }
                },
                2, 1, 10);

            _serviceMock
                .Setup(s => s.GetRentalContracts(It.IsAny<RentalContractResourceParameters>()))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _queries.GetRentalContracts(filterInput, _serviceMock.Object);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetRentalContracts_ReturnsNull_WhenServiceReturnsNull()
        {
            // Arrange
            var filterInput = new RentalContractFilterInput();

            _serviceMock
                .Setup(s => s.GetRentalContracts(It.IsAny<RentalContractResourceParameters>()))
                .ReturnsAsync((PagedList<RentalContractDto>?)null);

            // Act
            var result = await _queries.GetRentalContracts(filterInput, _serviceMock.Object);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetRentalContractById

        [Fact]
        public async Task GetRentalContractById_ReturnsMappedDto()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contract = new RentalContractDto
            {
                Id = id,
                Shifts = 10
            };

            _serviceMock
                .Setup(s => s.GetRentalContractById(id, null))
                .ReturnsAsync(contract);

            // Act
            var result = await _queries.GetRentalContractById(id, _serviceMock.Object);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(id);
            result.Shifts.Should().Be(10);
        }

        #endregion

        #region GetRentalContractsByIds

        [Fact]
        public async Task GetRentalContractsByIds_ReturnsDtos()
        {
            // Arrange
            var ids = new[] { Guid.NewGuid(), Guid.NewGuid() };

            var contracts = ids.Select(id => new RentalContractDto { Id = id }).ToList();

            _serviceMock
                .Setup(s => s.GetRentalContractsByIds(ids, null))
                .ReturnsAsync(contracts);

            // Act
            var result = await _queries.GetRentalContractsByIds(ids, _serviceMock.Object);

            // Assert
            result.Should().HaveCount(2);
            result.Select(x => x.Id).Should().BeEquivalentTo(ids);
        }

        #endregion

        #region GetRentalContractsByCustomerId

        [Fact]
        public async Task GetRentalContractsByCustomerId_ReturnsDtos()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var contracts = new List<RentalContractDto>
        {
            new() { Id = Guid.NewGuid() }
        };

            _serviceMock
                .Setup(s => s.GetRentalContractsByCustomerId(customerId, null))
                .ReturnsAsync(contracts);

            // Act
            var result = await _queries.GetRentalContractsByCustomerId(customerId, _serviceMock.Object);

            // Assert
            result.Should().HaveCount(1);
            _serviceMock.Verify(
                s => s.GetRentalContractsByCustomerId(customerId, null),
                Times.Once);
        }

        #endregion

        #region GetRentalContractsByEquipmentId

        [Fact]
        public async Task GetRentalContractsByEquipmentId_ReturnsDtos()
        {
            // Arrange
            var equipmentId = Guid.NewGuid();
            var contracts = new List<RentalContractDto>
        {
            new() { Id = Guid.NewGuid() }
        };

            _serviceMock
                .Setup(s => s.GetRentalContractsByEquipmentId(equipmentId, null))
                .ReturnsAsync(contracts);

            // Act
            var result = await _queries.GetRentalContractsByEquipmentId(equipmentId, _serviceMock.Object);

            // Assert
            result.Should().HaveCount(1);
            _serviceMock.Verify(
                s => s.GetRentalContractsByEquipmentId(equipmentId, null),
                Times.Once);
        }

        #endregion

        #region GetActiveContracts

        [Fact]
        public async Task GetActiveContracts_ReturnsDtos()
        {
            // Arrange
            var contracts = new List<RentalContractDto>
        {
            new() { Id = Guid.NewGuid() }
        };

            _serviceMock
                .Setup(s => s.GetActiveContracts(null))
                .ReturnsAsync(contracts);

            // Act
            var result = await _queries.GetActiveContracts(_serviceMock.Object);

            // Assert
            result.Should().HaveCount(1);
            _serviceMock.Verify(s => s.GetActiveContracts(null), Times.Once);
        }

        #endregion

        #region GetExpiredContracts

        [Fact]
        public async Task GetExpiredContracts_ReturnsDtos()
        {
            // Arrange
            var days = 7;
            var contracts = new List<RentalContractDto>
        {
            new() { Id = Guid.NewGuid() }
        };

            _serviceMock
                .Setup(s => s.GetExpiredContracts(days, null))
                .ReturnsAsync(contracts);

            // Act
            var result = await _queries.GetExpiredContracts(days, _serviceMock.Object);

            // Assert
            result.Should().HaveCount(1);
            _serviceMock.Verify(
                s => s.GetExpiredContracts(days, null),
                Times.Once);
        }

        #endregion
    }
}
