using API.GraphQL.SellingContract.Queries;
using Application.Interface.Services;
using Application.Models.SellingContract.Read;
using Domain.Enums;
using Moq;

namespace EquipmentAPI.Tests.UnitTests.SellingContractTests
{
    public class SellingContractAnalyticsQueriesTests
    {
        private readonly Mock<ISellingContractAnalyticsService> _serviceMock;
        private readonly SellingContractAnalyticsQueries _queries;

        public SellingContractAnalyticsQueriesTests()
        {
            _serviceMock = new Mock<ISellingContractAnalyticsService>();
            _queries = new SellingContractAnalyticsQueries();
        }

        [Fact]
        public async Task GetTotalRevenue_ReturnsValue()
        {
            // Arrange
            var expected = 15000m;
            _serviceMock
                .Setup(s => s.GetTotalRevenue(null, null))
                .ReturnsAsync(expected);

            // Act
            var result = await _queries.GetTotalRevenue(null, null, _serviceMock.Object);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetAverageSalePrice_ReturnsValue()
        {
            // Arrange
            var expected = 500m;
            _serviceMock
                .Setup(s => s.GetAverageSalePrice(null, null))
                .ReturnsAsync(expected);

            // Act
            var result = await _queries.GetAverageSalePrice(null, null, _serviceMock.Object);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetAverageSalePriceByEquipment_ReturnsValue()
        {
            // Arrange
            var expected = 800m;

            _serviceMock
                .Setup(s => s.GetAverageSalePriceByEquipment(
                    null, null, EquipmentBrand.Caterpillar, EquipmentType.Excavator))
                .ReturnsAsync(expected);

            // Act
            var result = await _queries.GetAverageSalePriceByEquipment(
                null, null, EquipmentBrand.Caterpillar, EquipmentType.Excavator, _serviceMock.Object);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetMinSalePrice_ReturnsValue()
        {
            // Arrange
            var expected = 200m;
            _serviceMock
                .Setup(s => s.GetMinSalePrice(null, null))
                .ReturnsAsync(expected);

            // Act
            var result = await _queries.GetMinSalePrice(null, null, _serviceMock.Object);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetMaxSalePrice_ReturnsValue()
        {
            // Arrange
            var expected = 1200m;
            _serviceMock
                .Setup(s => s.GetMaxSalePrice(null, null))
                .ReturnsAsync(expected);

            // Act
            var result = await _queries.GetMaxSalePrice(null, null, _serviceMock.Object);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetRevenueByDay_ReturnsDictionary()
        {
            // Arrange
            var expected = new Dictionary<DateTime, decimal>
            {
                [DateTime.Today] = 1000m
            };

            _serviceMock
                .Setup(s => s.GetRevenueByDay(null, null))
                .ReturnsAsync(expected);

            // Act
            var result = await _queries.GetRevenueByDay(null, null, _serviceMock.Object);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task GetRevenueByMonth_ReturnsDictionary_WhenYearIsValid()
        {
            // Arrange
            var year = DateTime.Now.Year;
            var expected = new Dictionary<int, decimal> { [1] = 3000m };

            _serviceMock
                .Setup(s => s.GetRevenueByMonth(year))
                .ReturnsAsync(expected);

            // Act
            var result = await _queries.GetRevenueByMonth(year, _serviceMock.Object);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task GetRevenueByMonth_Throws_WhenYearIsInvalid()
        {
            // Arrange
            var invalidYear = 1800;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                _queries.GetRevenueByMonth(invalidYear, _serviceMock.Object));
        }

        [Fact]
        public async Task GetRevenueByYear_ReturnsDictionary()
        {
            // Arrange
            var expected = new Dictionary<int, decimal> { [2024] = 10000m };

            _serviceMock
                .Setup(s => s.GetRevenueByYear())
                .ReturnsAsync(expected);

            // Act
            var result = await _queries.GetRevenueByYear(_serviceMock.Object);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task GetSalesCount_ReturnsValue()
        {
            // Arrange
            var expected = 42;

            _serviceMock
                .Setup(s => s.GetSalesCount(null, null))
                .ReturnsAsync(expected);

            // Act
            var result = await _queries.GetSalesCount(null, null, _serviceMock.Object);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetRevenueByCustomer_ReturnsDictionary()
        {
            // Arrange
            var expected = new Dictionary<Guid, decimal>
            {
                [Guid.NewGuid()] = 5000m
            };

            _serviceMock
                .Setup(s => s.GetRevenueByCustomer(null, null))
                .ReturnsAsync(expected);

            // Act
            var result = await _queries.GetRevenueByCustomer(null, null, _serviceMock.Object);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task GetTopCustomers_Throws_WhenTopIsInvalid()
        {
            // Arrange
            var invalidTop = 0;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _queries.GetTopCustomers(invalidTop, null, null, _serviceMock.Object));
        }

        [Fact]
        public async Task GetTopCustomers_ReturnsResults_WhenTopIsValid()
        {
            // Arrange
            var expected = new List<TopCustomerResult>
            {
                new TopCustomerResult()
            };

            _serviceMock
                .Setup(s => s.GetTopCustomers(3, null, null))
                .ReturnsAsync(expected);

            // Act
            var result = await _queries.GetTopCustomers(3, null, null, _serviceMock.Object);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task GetTopSellingEquipment_Throws_WhenTopIsInvalid()
        {
            // Arrange
            var invalidTop = -1;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _queries.GetTopSellingEquipment(invalidTop, null, null, _serviceMock.Object));
        }

        [Fact]
        public async Task GetTopSellingEquipment_ReturnsResults_WhenTopIsValid()
        {
            // Arrange
            var expected = new List<TopEquipmentResult>
            {
                new TopEquipmentResult()
            };

            _serviceMock
                .Setup(s => s.GetTopSellingEquipment(5, null, null))
                .ReturnsAsync(expected);

            // Act
            var result = await _queries.GetTopSellingEquipment(5, null, null, _serviceMock.Object);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task GetDeletedContractsCount_ReturnsValue()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.GetDeletedContractsCount())
                .ReturnsAsync(7);

            // Act
            var result = await _queries.GetDeletedContractsCount(_serviceMock.Object);

            // Assert
            Assert.Equal(7, result);
        }

        [Fact]
        public async Task GetAverageSalePricePerEquipment_ReturnsValue()
        {
            // Arrange
            var equipmentId = Guid.NewGuid();
            var expected = 900m;

            _serviceMock
                .Setup(s => s.GetAverageSalePricePerEquipment(equipmentId))
                .ReturnsAsync(expected);

            // Act
            var result = await _queries.GetAverageSalePricePerEquipment(equipmentId, _serviceMock.Object);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}