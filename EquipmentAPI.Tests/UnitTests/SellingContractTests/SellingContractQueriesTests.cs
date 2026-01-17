using API.GraphQL.SellingContract.Inputs;
using API.GraphQL.SellingContract.Queries;
using Application.Interface.Services;
using Application.Models.SellingContract.Read;
using Application.ResourceParameters;
using Moq;
using Shared.Results;

namespace EquipmentAPI.Tests.UnitTests.SellingContractTests
{
    public class SellingContractQueriesTests
    {
        private readonly Mock<ISellingContractService> _serviceMock;
        private readonly SellingContractQueries _queries;

        public SellingContractQueriesTests()
        {
            _serviceMock = new Mock<ISellingContractService>();
            _queries = new SellingContractQueries();
        }

        [Fact]
        public async Task GetSellingContracts_ReturnsContracts()
        {
            // Arrange
            var filterInput = new SellingContractFilterInput();
            var listOfContracts = new List<SellingContractDto>
            {
                new SellingContractDto(),
                new SellingContractDto()
            };
            var expectedContracts = new PagedList<SellingContractDto>(listOfContracts, 1, 1, 10);

            _serviceMock
                .Setup(s => s.GetSellingContracts(It.IsAny<SellingContractResourceParameters>()))
                .ReturnsAsync(expectedContracts);

            // Act
            var result = await _queries.GetSellingContracts(filterInput, _serviceMock.Object);

            // Assert
            Assert.Equal(expectedContracts.Count, result.Count());
            _serviceMock.Verify(
                s => s.GetSellingContracts(It.IsAny<SellingContractResourceParameters>()),
                Times.Once);
        }

        [Fact]
        public async Task GetSellingContractById_ReturnsContract()
        {
            // Arrange
            var contractId = Guid.NewGuid();
            var contractDto = new SellingContractDto { Id = contractId };

            _serviceMock
                .Setup(s => s.GetSellingContractById(contractId, null))
                .ReturnsAsync(contractDto);

            // Act
            var result = await _queries.GetSellingContractById(contractId, _serviceMock.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(contractId, result.Id);
            _serviceMock.Verify(
                s => s.GetSellingContractById(contractId, null),
                Times.Once);
        }

        [Fact]
        public async Task GetDeletedSellingContracts_ReturnsDeletedContracts()
        {
            // Arrange
            var filterInput = new SellingContractFilterInput();
            var deletedContracts = new List<SellingContractDto>
            {
                new SellingContractDto()
            };

            _serviceMock
                .Setup(s => s.GetDeletedSellingContracts(It.IsAny<SellingContractResourceParameters>()))
                .ReturnsAsync(deletedContracts);

            // Act
            var result = await _queries.GetDeletedSellingContracts(filterInput, _serviceMock.Object);

            // Assert
            Assert.Single(result);
            _serviceMock.Verify(
                s => s.GetDeletedSellingContracts(It.IsAny<SellingContractResourceParameters>()),
                Times.Once);
        }

        [Fact]
        public async Task GetDeletedSellingContractById_ReturnsDeletedContract()
        {
            // Arrange
            var contractId = Guid.NewGuid();
            var deletedContract = new SellingContractDto { Id = contractId };

            _serviceMock
                .Setup(s => s.GetDeletedSellingContractById(contractId, null))
                .ReturnsAsync(deletedContract);

            // Act
            var result = await _queries.GetDeletedSellingContractById(contractId, _serviceMock.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(contractId, result.Id);
            _serviceMock.Verify(
                s => s.GetDeletedSellingContractById(contractId, null),
                Times.Once);
        }

        [Fact]
        public async Task GetSellingContractsByYear_ReturnsContract_WhenYearIsValid()
        {
            // Arrange
            var year = DateTime.Now.Year;
            var contract = new List<SellingContractDto>();

            _serviceMock
                .Setup(s => s.GetSellingContractsByYear(year))
                .ReturnsAsync(contract);

            // Act
            var result = await _queries.GetSellingContractsByYear(year, _serviceMock.Object);

            // Assert
            Assert.NotNull(result);
            _serviceMock.Verify(
                s => s.GetSellingContractsByYear(year),
                Times.Once);
        }

        [Fact]
        public async Task GetSellingContractsByYear_ThrowsException_WhenYearIsInvalid()
        {
            // Arrange
            var invalidYear = DateTime.Now.Year + 1;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                _queries.GetSellingContractsByYear(invalidYear, _serviceMock.Object));

            _serviceMock.Verify(
                s => s.GetSellingContractsByYear(It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task GetSellingContractsByIds_ReturnsContracts()
        {
            // Arrange
            var ids = new[] { Guid.NewGuid(), Guid.NewGuid() };
            var contracts = new List<SellingContractDto>
            {
                new SellingContractDto(),
                new SellingContractDto()
            };

            _serviceMock
                .Setup(s => s.GetSellingContractsByIds(ids, null))
                .ReturnsAsync(contracts);

            // Act
            var result = await _queries.GetSellingContractsByIds(ids, _serviceMock.Object);

            // Assert
            Assert.Equal(2, result.Count());
            _serviceMock.Verify(
                s => s.GetSellingContractsByIds(ids, null),
                Times.Once);
        }

        [Fact]
        public async Task GetSellingContractsByCustomerId_ReturnsContracts()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var contracts = new List<SellingContractDto> { new SellingContractDto() };

            _serviceMock
                .Setup(s => s.GetSellingContractsByCustomerId(customerId, null))
                .ReturnsAsync(contracts);

            // Act
            var result = await _queries.GetSellingContractsByCustomerId(customerId, _serviceMock.Object);

            // Assert
            Assert.Single(result);
            _serviceMock.Verify(
                s => s.GetSellingContractsByCustomerId(customerId, null),
                Times.Once);
        }

        [Fact]
        public async Task GetSellingContractsByEquipmentId_ReturnsContracts()
        {
            // Arrange
            var equipmentId = Guid.NewGuid();
            var contracts = new List<SellingContractDto> { new SellingContractDto() };

            _serviceMock
                .Setup(s => s.GetSellingContractsByEquipmentId(equipmentId, null))
                .ReturnsAsync(contracts);

            // Act
            var result = await _queries.GetSellingContractsByEquipmentId(equipmentId, _serviceMock.Object);

            // Assert
            Assert.Single(result);
            _serviceMock.Verify(
                s => s.GetSellingContractsByEquipmentId(equipmentId, null),
                Times.Once);
        }
    }
}