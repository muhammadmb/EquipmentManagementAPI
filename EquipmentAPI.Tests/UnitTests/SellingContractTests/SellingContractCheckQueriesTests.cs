using API.GraphQL.SellingContract.Queries;
using Application.Interface.Services;
using Moq;

namespace EquipmentAPI.Tests.UnitTests.SellingContractTests
{
    public class SellingContractCheckQueriesTests
    {
        private readonly Mock<ISellingContractService> _serviceMock;
        private readonly SellingContractCheckQueries _queries;

        public SellingContractCheckQueriesTests()
        {
            _serviceMock = new Mock<ISellingContractService>();
            _queries = new SellingContractCheckQueries();
        }

        [Fact]
        public async Task SellingContractExists_ReturnsTrue_WhenContractExists()
        {
            // Arrange
            var contractId = Guid.NewGuid();
            _serviceMock
                .Setup(s => s.SellingContractExists(contractId))
                .ReturnsAsync(true);

            // Act
            var result = await _queries.SellingContractExists(contractId, _serviceMock.Object);

            // Assert
            Assert.True(result);
            _serviceMock.Verify(
                s => s.SellingContractExists(contractId),
                Times.Once);
        }

        [Fact]
        public async Task SellingContractExists_ReturnsFalse_WhenContractDoesNotExist()
        {
            // Arrange
            var contractId = Guid.NewGuid();
            _serviceMock
                .Setup(s => s.SellingContractExists(contractId))
                .ReturnsAsync(false);

            // Act
            var result = await _queries.SellingContractExists(contractId, _serviceMock.Object);

            // Assert
            Assert.False(result);
            _serviceMock.Verify(
                s => s.SellingContractExists(contractId),
                Times.Once);
        }

        [Fact]
        public async Task CustomerHasContracts_ReturnsTrue_WhenCustomerHasContracts()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            _serviceMock
                .Setup(s => s.CustomerHasContracts(customerId))
                .ReturnsAsync(true);

            // Act
            var result = await _queries.CustomerHasContracts(customerId, _serviceMock.Object);

            // Assert
            Assert.True(result);
            _serviceMock.Verify(
                s => s.CustomerHasContracts(customerId),
                Times.Once);
        }

        [Fact]
        public async Task CustomerHasContracts_ReturnsFalse_WhenCustomerHasNoContracts()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            _serviceMock
                .Setup(s => s.CustomerHasContracts(customerId))
                .ReturnsAsync(false);

            // Act
            var result = await _queries.CustomerHasContracts(customerId, _serviceMock.Object);

            // Assert
            Assert.False(result);
            _serviceMock.Verify(
                s => s.CustomerHasContracts(customerId),
                Times.Once);
        }
    }
}
