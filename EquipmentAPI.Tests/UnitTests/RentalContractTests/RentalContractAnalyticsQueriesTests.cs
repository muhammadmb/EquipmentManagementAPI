using API.GraphQL.RentalContract.Queries;
using Application.Interface.Services;
using Application.Models.RentalContractModels.Read;
using FluentAssertions;
using Moq;

namespace EquipmentAPI.Tests.UnitTests.RentalContractTests
{
    public class RentalContractAnalyticsQueriesTests
    {
        private readonly Mock<IRentalContractAnalyticsService> _mockService;
        private readonly RentalContractAnalyticsQueries _queries;

        public RentalContractAnalyticsQueriesTests()
        {
            _mockService = new Mock<IRentalContractAnalyticsService>();
            _queries = new RentalContractAnalyticsQueries();
        }

        #region Count and Summary Queries Tests

        [Fact]
        public async Task GetRentalContractCount_ReturnsCount_WhenServiceReturnsCount()
        {
            // Arrange
            var expectedCount = 10;
            _mockService
                .Setup(s => s.GetRentalContractCount())
                .ReturnsAsync(expectedCount);

            // Act
            var result = await _queries.GetRentalContractCount(_mockService.Object);

            // Assert
            result.Should().Be(expectedCount);
            _mockService.Verify(s => s.GetRentalContractCount(), Times.Once);
        }

        [Fact]
        public async Task GetRentalContractCount_ReturnsZero_WhenServiceReturnsZero()
        {
            // Arrange
            _mockService
                .Setup(s => s.GetRentalContractCount())
                .ReturnsAsync(0);

            // Act
            var result = await _queries.GetRentalContractCount(_mockService.Object);

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public async Task GetTotalActiveCount_ReturnsCount_WhenServiceReturnsCount()
        {
            // Arrange
            var expectedCount = 5;
            _mockService
                .Setup(s => s.GetTotalActiveCount())
                .ReturnsAsync(expectedCount);

            // Act
            var result = await _queries.GetTotalActiveCount(_mockService.Object);

            // Assert
            result.Should().Be(expectedCount);
            _mockService.Verify(s => s.GetTotalActiveCount(), Times.Once);
        }

        [Fact]
        public async Task GetTotalActiveCount_ReturnsZero_WhenServiceReturnsZero()
        {
            // Arrange
            _mockService
                .Setup(s => s.GetTotalActiveCount())
                .ReturnsAsync(0);

            // Act
            var result = await _queries.GetTotalActiveCount(_mockService.Object);

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public async Task GetTotalContractsForCustomer_ReturnsCount_WhenCustomerHasContracts()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var expectedCount = 3;
            _mockService
                .Setup(s => s.GetTotalContractsForCustomer(customerId))
                .ReturnsAsync(expectedCount);

            // Act
            var result = await _queries.GetTotalContractsForCustomer(customerId, _mockService.Object);

            // Assert
            result.Should().Be(expectedCount);
            _mockService.Verify(s => s.GetTotalContractsForCustomer(customerId), Times.Once);
        }

        [Fact]
        public async Task GetTotalContractsForCustomer_ReturnsZero_WhenCustomerHasNoContracts()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            _mockService
                .Setup(s => s.GetTotalContractsForCustomer(customerId))
                .ReturnsAsync(0);

            // Act
            var result = await _queries.GetTotalContractsForCustomer(customerId, _mockService.Object);

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public async Task GetTotalContractsForEquipment_ReturnsCount_WhenEquipmentHasContracts()
        {
            // Arrange
            var equipmentId = Guid.NewGuid();
            var expectedCount = 7;
            _mockService
                .Setup(s => s.GetTotalContractsForEquipment(equipmentId))
                .ReturnsAsync(expectedCount);

            // Act
            var result = await _queries.GetTotalContractsForEquipment(equipmentId, _mockService.Object);

            // Assert
            result.Should().Be(expectedCount);
            _mockService.Verify(s => s.GetTotalContractsForEquipment(equipmentId), Times.Once);
        }

        [Fact]
        public async Task GetTotalContractsForEquipment_ReturnsZero_WhenEquipmentHasNoContracts()
        {
            // Arrange
            var equipmentId = Guid.NewGuid();
            _mockService
                .Setup(s => s.GetTotalContractsForEquipment(equipmentId))
                .ReturnsAsync(0);

            // Act
            var result = await _queries.GetTotalContractsForEquipment(equipmentId, _mockService.Object);

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public async Task GetEquipmentContractSummary_ReturnsSummary_WhenServiceReturnsSummary()
        {
            // Arrange
            var expectedSummary = new List<EquipmentContractSummaryDto>
        {
            new EquipmentContractSummaryDto { EquipmentId = Guid.NewGuid(), ContractCount = 5 },
            new EquipmentContractSummaryDto { EquipmentId = Guid.NewGuid(), ContractCount = 3 }
        };
            _mockService
                .Setup(s => s.GetEquipmentContractSummary())
                .ReturnsAsync(expectedSummary);

            // Act
            var result = await _queries.GetEquipmentContractSummary(_mockService.Object);

            // Assert
            result.Should().BeEquivalentTo(expectedSummary);
            _mockService.Verify(s => s.GetEquipmentContractSummary(), Times.Once);
        }

        [Fact]
        public async Task GetEquipmentContractSummary_ReturnsEmptyList_WhenServiceReturnsEmptyList()
        {
            // Arrange
            _mockService
                .Setup(s => s.GetEquipmentContractSummary())
                .ReturnsAsync(new List<EquipmentContractSummaryDto>());

            // Act
            var result = await _queries.GetEquipmentContractSummary(_mockService.Object);

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region Revenue Queries Tests

        [Fact]
        public async Task GetTotalRevenue_ReturnsRevenue_WhenServiceReturnsRevenue()
        {
            // Arrange
            var from = DateTime.Parse("2025-01-01");
            var to = DateTime.Parse("2025-12-31");
            var expectedRevenue = 100000m;
            _mockService
                .Setup(s => s.GetTotalRevenue(from, to))
                .ReturnsAsync(expectedRevenue);

            // Act
            var result = await _queries.GetTotalRevenue(from, to, _mockService.Object);

            // Assert
            result.Should().Be(expectedRevenue);
            _mockService.Verify(s => s.GetTotalRevenue(from, to), Times.Once);
        }

        [Fact]
        public async Task GetTotalRevenue_ReturnsZero_WhenServiceReturnsZero()
        {
            // Arrange
            var from = DateTime.Parse("2025-01-01");
            var to = DateTime.Parse("2025-12-31");
            _mockService
                .Setup(s => s.GetTotalRevenue(from, to))
                .ReturnsAsync(0m);

            // Act
            var result = await _queries.GetTotalRevenue(from, to, _mockService.Object);

            // Assert
            result.Should().Be(0m);
        }

        [Fact]
        public async Task GetRevenueByCustomer_ReturnsDictionary_WhenServiceReturnsDictionary()
        {
            // Arrange
            var from = DateTime.Parse("2025-01-01");
            var to = DateTime.Parse("2025-12-31");
            var expectedDictionary = new Dictionary<string, decimal>
        {
            { "Customer A", 50000m },
            { "Customer B", 30000m }
        };
            _mockService
                .Setup(s => s.GetRevenueByCustomer(from, to))
                .ReturnsAsync(expectedDictionary);

            // Act
            var result = await _queries.GetRevenueByCustomer(from, to, _mockService.Object);

            // Assert
            result.Should().BeEquivalentTo(expectedDictionary);
            _mockService.Verify(s => s.GetRevenueByCustomer(from, to), Times.Once);
        }

        [Fact]
        public async Task GetRevenueByCustomer_ReturnsEmptyDictionary_WhenServiceReturnsEmpty()
        {
            // Arrange
            DateTime? from = null;
            DateTime? to = null;
            _mockService
                .Setup(s => s.GetRevenueByCustomer(from, to))
                .ReturnsAsync(new Dictionary<string, decimal>());

            // Act
            var result = await _queries.GetRevenueByCustomer(from, to, _mockService.Object);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetRevenueByCustomer_HandlesNullDates_WhenDatesAreNull()
        {
            // Arrange
            DateTime? from = null;
            DateTime? to = null;
            var expectedDictionary = new Dictionary<string, decimal>
        {
            { "Customer A", 75000m }
        };
            _mockService
                .Setup(s => s.GetRevenueByCustomer(null, null))
                .ReturnsAsync(expectedDictionary);

            // Act
            var result = await _queries.GetRevenueByCustomer(from, to, _mockService.Object);

            // Assert
            result.Should().BeEquivalentTo(expectedDictionary);
        }

        [Fact]
        public async Task GetRevenueByMonth_ReturnsDictionary_WhenServiceReturnsDictionary()
        {
            // Arrange
            var year = 2025;
            var expectedDictionary = new Dictionary<int, decimal>
        {
            { 1, 10000m },
            { 2, 15000m },
            { 3, 20000m }
        };
            _mockService
                .Setup(s => s.GetRevenueByMonth(year))
                .ReturnsAsync(expectedDictionary);

            // Act
            var result = await _queries.GetRevenueByMonth(year, _mockService.Object);

            // Assert
            result.Should().BeEquivalentTo(expectedDictionary);
            _mockService.Verify(s => s.GetRevenueByMonth(year), Times.Once);
        }

        [Fact]
        public async Task GetRevenueByMonth_ReturnsEmptyDictionary_WhenServiceReturnsEmpty()
        {
            // Arrange
            var year = 2025;
            _mockService
                .Setup(s => s.GetRevenueByMonth(year))
                .ReturnsAsync(new Dictionary<int, decimal>());

            // Act
            var result = await _queries.GetRevenueByMonth(year, _mockService.Object);

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region Statistics Queries Tests

        [Fact]
        public async Task GetContractPriceStatistics_ReturnsStatistics_WhenServiceReturnsStatistics()
        {
            // Arrange
            var expectedStatistics = new ContractPriceStatisticsDto
            {
                AveragePrice = 1000m,
                MedianPrice = 4500m
            };
            _mockService
                .Setup(s => s.GetContractPriceStatistics())
                .ReturnsAsync(expectedStatistics);

            // Act
            var result = await _queries.GetContractPriceStatistics(_mockService.Object);

            // Assert
            result.Should().BeEquivalentTo(expectedStatistics);
            _mockService.Verify(s => s.GetContractPriceStatistics(), Times.Once);
        }

        [Fact]
        public async Task GetContractPriceStatistics_ReturnsNull_WhenServiceReturnsNull()
        {
            // Arrange
            _mockService
                .Setup(s => s.GetContractPriceStatistics())
                .ReturnsAsync((ContractPriceStatisticsDto)null);

            // Act
            var result = await _queries.GetContractPriceStatistics(_mockService.Object);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetFinishedContractsCount_ReturnsCount_WhenServiceReturnsCount()
        {
            // Arrange
            var from = DateTime.Parse("2025-01-01");
            var to = DateTime.Parse("2025-12-31");
            var expectedCount = 15;
            _mockService
                .Setup(s => s.GetFinishedContractsCount(from, to))
                .ReturnsAsync(expectedCount);

            // Act
            var result = await _queries.GetFinishedContractsCount(from, to, _mockService.Object);

            // Assert
            result.Should().Be(expectedCount);
            _mockService.Verify(s => s.GetFinishedContractsCount(from, to), Times.Once);
        }

        [Fact]
        public async Task GetFinishedContractsCount_ReturnsZero_WhenServiceReturnsZero()
        {
            // Arrange
            DateTime? from = null;
            DateTime? to = null;
            _mockService
                .Setup(s => s.GetFinishedContractsCount(from, to))
                .ReturnsAsync(0);

            // Act
            var result = await _queries.GetFinishedContractsCount(from, to, _mockService.Object);

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public async Task GetFinishedContractsCount_HandlesNullDates_WhenDatesAreNull()
        {
            // Arrange
            DateTime? from = null;
            DateTime? to = null;
            var expectedCount = 20;
            _mockService
                .Setup(s => s.GetFinishedContractsCount(null, null))
                .ReturnsAsync(expectedCount);

            // Act
            var result = await _queries.GetFinishedContractsCount(from, to, _mockService.Object);

            // Assert
            result.Should().Be(expectedCount);
        }

        [Fact]
        public async Task GetCancelledContractsCount_ReturnsCount_WhenServiceReturnsCount()
        {
            // Arrange
            var from = DateTime.Parse("2025-01-01");
            var to = DateTime.Parse("2025-12-31");
            var expectedCount = 8;
            _mockService
                .Setup(s => s.GetCancelledContractsCount(from, to))
                .ReturnsAsync(expectedCount);

            // Act
            var result = await _queries.GetCancelledContractsCount(from, to, _mockService.Object);

            // Assert
            result.Should().Be(expectedCount);
            _mockService.Verify(s => s.GetCancelledContractsCount(from, to), Times.Once);
        }

        [Fact]
        public async Task GetCancelledContractsCount_ReturnsZero_WhenServiceReturnsZero()
        {
            // Arrange
            DateTime? from = null;
            DateTime? to = null;
            _mockService
                .Setup(s => s.GetCancelledContractsCount(from, to))
                .ReturnsAsync(0);

            // Act
            var result = await _queries.GetCancelledContractsCount(from, to, _mockService.Object);

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public async Task GetCancelledContractsCount_HandlesNullDates_WhenDatesAreNull()
        {
            // Arrange
            DateTime? from = null;
            DateTime? to = null;
            var expectedCount = 12;
            _mockService
                .Setup(s => s.GetCancelledContractsCount(null, null))
                .ReturnsAsync(expectedCount);

            // Act
            var result = await _queries.GetCancelledContractsCount(from, to, _mockService.Object);

            // Assert
            result.Should().Be(expectedCount);
        }

        #endregion

        #region Average Queries Tests

        [Fact]
        public async Task GetAverageContractDurationInDays_ReturnsAverage_WhenServiceReturnsAverage()
        {
            // Arrange
            var expectedAverage = 45.5;
            _mockService
                .Setup(s => s.GetAverageContractDurationInDays())
                .ReturnsAsync(expectedAverage);

            // Act
            var result = await _queries.GetAverageContractDurationInDays(_mockService.Object);

            // Assert
            result.Should().Be(expectedAverage);
            _mockService.Verify(s => s.GetAverageContractDurationInDays(), Times.Once);
        }

        [Fact]
        public async Task GetAverageContractDurationInDays_ReturnsZero_WhenServiceReturnsZero()
        {
            // Arrange
            _mockService
                .Setup(s => s.GetAverageContractDurationInDays())
                .ReturnsAsync(0);

            // Act
            var result = await _queries.GetAverageContractDurationInDays(_mockService.Object);

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public async Task GetAverageRevenuePerCustomer_ReturnsAverage_WhenServiceReturnsAverage()
        {
            // Arrange
            var expectedAverage = 25000m;
            _mockService
                .Setup(s => s.GetAverageRevenuePerCustomer())
                .ReturnsAsync(expectedAverage);

            // Act
            var result = await _queries.GetAverageRevenuePerCustomer(_mockService.Object);

            // Assert
            result.Should().Be(expectedAverage);
            _mockService.Verify(s => s.GetAverageRevenuePerCustomer(), Times.Once);
        }

        [Fact]
        public async Task GetAverageRevenuePerCustomer_ReturnsZero_WhenServiceReturnsZero()
        {
            // Arrange
            _mockService
                .Setup(s => s.GetAverageRevenuePerCustomer())
                .ReturnsAsync(0m);

            // Act
            var result = await _queries.GetAverageRevenuePerCustomer(_mockService.Object);

            // Assert
            result.Should().Be(0m);
        }

        [Fact]
        public async Task GetAverageRentalPrice_ReturnsAverage_WhenServiceReturnsAverage()
        {
            // Arrange
            var expectedAverage = 3500m;
            _mockService
                .Setup(s => s.GetAverageRentalPrice())
                .ReturnsAsync(expectedAverage);

            // Act
            var result = await _queries.GetAverageRentalPrice(_mockService.Object);

            // Assert
            result.Should().Be(expectedAverage);
            _mockService.Verify(s => s.GetAverageRentalPrice(), Times.Once);
        }

        [Fact]
        public async Task GetAverageRentalPrice_ReturnsZero_WhenServiceReturnsZero()
        {
            // Arrange
            _mockService
                .Setup(s => s.GetAverageRentalPrice())
                .ReturnsAsync(0m);

            // Act
            var result = await _queries.GetAverageRentalPrice(_mockService.Object);

            // Assert
            result.Should().Be(0m);
        }

        #endregion
    }
}
