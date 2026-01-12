using Application.Interface.Services;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Contexts;
using Infrastructure.Helpers;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;
using Moq;

namespace EquipmentAPI.Tests.UnitTests.SellingContractTests
{
    public class SellingContractAnalyticsServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IDistributedCache> _cache = new();
        private readonly Mock<ICacheVersionProvider> _versionProvider = new();
        private readonly SellingContractAnalyticsService _service;

        public SellingContractAnalyticsServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"{Guid.NewGuid()}_AnalyticsDb")
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new ApplicationDbContext(options);

            _versionProvider
                .Setup(v => v.GetVersionAsync(CacheScopes.SellingContracts))
                .ReturnsAsync("v1");

            _cache.Setup(c => c.GetAsync(
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync((byte[]?)null);

            _service = new SellingContractAnalyticsService(
                _context,
                _cache.Object,
                _versionProvider.Object);

            Seed();
        }

        private void Seed()
        {
            var customer1Id = Guid.NewGuid();
            var customer2Id = Guid.NewGuid();
            var equipment1Id = Guid.NewGuid();
            var equipment2Id = Guid.NewGuid();

            var customer1 = new Customer
            {
                Id = customer1Id,
                Name = "Customer A",
                RowVersion = []
            };

            var customer2 = new Customer
            {
                Id = customer2Id,
                Name = "Customer B",
                RowVersion = []
            };

            _context.Customers.AddRange(customer1, customer2);

            var equipment1 = new Equipment
            {
                Id = equipment1Id,
                EquipmentBrand = EquipmentBrand.Caterpillar,
                EquipmentType = EquipmentType.Excavator,
                RowVersion = []
            };

            var equipment2 = new Equipment
            {
                Id = equipment2Id,
                EquipmentBrand = EquipmentBrand.Komatsu,
                EquipmentType = EquipmentType.Bulldozer,
                RowVersion = []
            };

            _context.Equipments.AddRange(equipment1, equipment2);

            var contract1 = new SellingContract
            {
                Id = Guid.NewGuid(),
                CustomerId = customer1Id,
                EquipmentId = equipment1Id,
                SaleDate = DateTimeOffset.Parse("2025-01-01"),
                SalePrice = 77820m,
                RowVersion = []
            };

            var contract2 = new SellingContract
            {
                Id = Guid.NewGuid(),
                CustomerId = customer1Id,
                EquipmentId = equipment1Id,
                SaleDate = DateTimeOffset.Parse("2025-01-15"),
                SalePrice = 5550.00m,
                RowVersion = []
            };

            var contract3 = new SellingContract
            {
                Id = Guid.NewGuid(),
                CustomerId = customer2Id,
                EquipmentId = equipment2Id,
                SaleDate = DateTimeOffset.Parse("2025-02-01"),
                SalePrice = 100000m,
                RowVersion = []
            };

            var deletedContract = new SellingContract
            {
                Id = Guid.NewGuid(),
                CustomerId = customer2Id,
                EquipmentId = equipment2Id,
                SaleDate = DateTimeOffset.Parse("2025-03-01"),
                SalePrice = 50000m,
                DeletedDate = DateTimeOffset.UtcNow,
                RowVersion = []
            };

            _context.Sellings.AddRange(contract1, contract2, contract3, deletedContract);
            _context.SaveChanges();
        }


        #region Financial Analytics Tests

        [Fact]
        public async Task GetTotalRevenue_ReturnsCorrectTotal_WhenContractsExist()
        {
            // Arrange
            // Seeded data has contracts with prices: 77820 + 5550 + 100000 = 183370

            // Act
            var result = await _service.GetTotalRevenue(null, null);

            // Assert
            result.Should().Be(183370m);
        }

        [Fact]
        public async Task GetTotalRevenue_FiltersCorrectly_WhenFromDateProvided()
        {
            // Arrange
            var fromDate = DateTimeOffset.Parse("2025-01-15");

            // Act
            var result = await _service.GetTotalRevenue(fromDate, null);

            // Assert
            result.Should().Be(105550m); // 5550 + 100000
        }

        [Fact]
        public async Task GetTotalRevenue_FiltersCorrectly_WhenToDateProvided()
        {
            // Arrange
            var toDate = DateTimeOffset.Parse("2025-01-31");

            // Act
            var result = await _service.GetTotalRevenue(null, toDate);

            // Assert
            result.Should().Be(83370m); // 77820 + 5550
        }

        [Fact]
        public async Task GetTotalRevenue_FiltersCorrectly_WhenBothDatesProvided()
        {
            // Arrange
            var fromDate = DateTimeOffset.Parse("2025-01-01");
            var toDate = DateTimeOffset.Parse("2025-01-31");

            // Act
            var result = await _service.GetTotalRevenue(fromDate, toDate);

            // Assert
            result.Should().Be(83370m); // 77820 + 5550
        }

        [Fact]
        public async Task GetTotalRevenue_ExcludesDeletedContracts_WhenCalculatingRevenue()
        {
            // Arrange
            // Deleted contract has price 50000 but should not be included

            // Act
            var result = await _service.GetTotalRevenue(null, null);

            // Assert
            result.Should().Be(183370m); // Should not include deleted contract
        }

        [Fact]
        public async Task GetAverageSalePrice_ReturnsCorrectAverage_WhenContractsExist()
        {
            // Arrange
            // Average of 77820, 5550, 100000 = 61123.33

            // Act
            var result = await _service.GetAverageSalePrice(null, null);

            // Assert
            result.Should().BeApproximately(61123.33m, 0.01m);
        }

        [Fact]
        public async Task GetAverageSalePrice_FiltersCorrectly_WhenDateRangeProvided()
        {
            // Arrange
            var fromDate = DateTimeOffset.Parse("2025-01-01");
            var toDate = DateTimeOffset.Parse("2025-01-31");

            // Act
            var result = await _service.GetAverageSalePrice(fromDate, toDate);

            // Assert
            result.Should().Be(41685m); // (77820 + 5550) / 2
        }

        [Fact]
        public async Task GetAverageSalePriceByEquipment_FiltersCorrectly_WhenEquipmentBrandProvided()
        {
            // Arrange
            var brand = EquipmentBrand.Caterpillar;

            // Act
            var result = await _service.GetAverageSalePriceByEquipment(null, null, brand, null);

            // Assert
            result.Should().Be(41685m); // (77820 + 5550) / 2
        }

        [Fact]
        public async Task GetAverageSalePriceByEquipment_FiltersCorrectly_WhenEquipmentTypeProvided()
        {
            // Arrange
            var type = EquipmentType.Bulldozer;

            // Act
            var result = await _service.GetAverageSalePriceByEquipment(null, null, null, type);

            // Assert
            result.Should().Be(100000m);
        }

        [Fact]
        public async Task GetAverageSalePriceByEquipment_FiltersCorrectly_WhenBothFiltersProvided()
        {
            // Arrange
            var brand = EquipmentBrand.Caterpillar;
            var type = EquipmentType.Excavator;

            // Act
            var result = await _service.GetAverageSalePriceByEquipment(null, null, brand, type);

            // Assert
            result.Should().Be(41685m);
        }

        [Fact]
        public async Task GetMinSalePrice_ReturnsCorrectMinimum_WhenContractsExist()
        {
            // Arrange
            // Minimum price is 5550

            // Act
            var result = await _service.GetMinSalePrice(null, null);

            // Assert
            result.Should().Be(5550m);
        }

        [Fact]
        public async Task GetMinSalePrice_FiltersCorrectly_WhenDateRangeProvided()
        {
            // Arrange
            var fromDate = DateTimeOffset.Parse("2025-02-01");

            // Act
            var result = await _service.GetMinSalePrice(fromDate, null);

            // Assert
            result.Should().Be(100000m);
        }

        [Fact]
        public async Task GetMaxSalePrice_ReturnsCorrectMaximum_WhenContractsExist()
        {
            // Arrange
            // Maximum price is 100000

            // Act
            var result = await _service.GetMaxSalePrice(null, null);

            // Assert
            result.Should().Be(100000m);
        }

        [Fact]
        public async Task GetMaxSalePrice_FiltersCorrectly_WhenDateRangeProvided()
        {
            // Arrange
            var toDate = DateTimeOffset.Parse("2025-01-31");

            // Act
            var result = await _service.GetMaxSalePrice(null, toDate);

            // Assert
            result.Should().Be(77820m);
        }

        #endregion

        #region Revenue Analytics Tests

        [Fact]
        public async Task GetRevenueByDay_GroupsByDayCorrectly_WhenMultipleDaysHaveRevenue()
        {
            // Arrange
            // Expected: Jan 1 = 77820, Jan 15 = 5550, Feb 1 = 100000

            // Act
            var result = await _service.GetRevenueByDay(null, null);

            // Assert
            result.Should().HaveCount(3);
            result[new DateTime(2025, 1, 1)].Should().Be(77820m);
            result[new DateTime(2025, 1, 15)].Should().Be(5550m);
            result[new DateTime(2025, 2, 1)].Should().Be(100000m);
        }

        [Fact]
        public async Task GetRevenueByDay_FiltersCorrectly_WhenDateRangeProvided()
        {
            // Arrange
            var fromDate = DateTimeOffset.Parse("2025-01-15");
            var toDate = DateTimeOffset.Parse("2025-01-31");

            // Act
            var result = await _service.GetRevenueByDay(fromDate, toDate);

            // Assert
            result.Should().HaveCount(1);
            result[new DateTime(2025, 1, 15)].Should().Be(5550m);
        }

        [Fact]
        public async Task GetRevenueByMonth_GroupsByMonthCorrectly_WhenMultipleMonthsHaveRevenue()
        {
            // Arrange
            var year = 2025;

            // Act
            var result = await _service.GetRevenueByMonth(year);

            // Assert
            result.Should().HaveCount(2);
            result[1].Should().Be(83370m); // January: 77820 + 5550
            result[2].Should().Be(100000m); // February: 100000
        }

        [Fact]
        public async Task GetRevenueByMonth_ReturnsEmptyDictionary_WhenNoRevenueForYear()
        {
            // Arrange
            var year = 2020;

            // Act
            var result = await _service.GetRevenueByMonth(year);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetRevenueByYear_GroupsByYearCorrectly_WhenMultipleYearsHaveRevenue()
        {
            // Arrange
            // All seeded contracts are in 2025

            // Act
            var result = await _service.GetRevenueByYear();

            // Assert
            result.Should().HaveCount(1);
            result[2025].Should().Be(183370m);
        }

        [Fact]
        public async Task GetSalesCount_ReturnsCorrectCount_WhenContractsExist()
        {
            // Arrange
            // 3 non-deleted contracts

            // Act
            var result = await _service.GetSalesCount(null, null);

            // Assert
            result.Should().Be(3);
        }

        [Fact]
        public async Task GetSalesCount_FiltersCorrectly_WhenDateRangeProvided()
        {
            // Arrange
            var fromDate = DateTimeOffset.Parse("2025-01-01");
            var toDate = DateTimeOffset.Parse("2025-01-31");

            // Act
            var result = await _service.GetSalesCount(fromDate, toDate);

            // Assert
            result.Should().Be(2); // Only January contracts
        }

        #endregion

        #region Customer Analytics Tests

        [Fact]
        public async Task GetRevenueByCustomer_GroupsByCustomerCorrectly_WhenMultipleCustomersExist()
        {
            // Arrange
            // Customer1: 77820 + 5550 = 83370
            // Customer2: 100000

            // Act
            var result = await _service.GetRevenueByCustomer(null, null);

            // Assert
            result.Should().HaveCount(2);
            var customer1Revenue = result.Values.First(v => v == 83370m);
            var customer2Revenue = result.Values.First(v => v == 100000m);
            customer1Revenue.Should().Be(83370m);
            customer2Revenue.Should().Be(100000m);
        }

        [Fact]
        public async Task GetRevenueByCustomer_FiltersCorrectly_WhenDateRangeProvided()
        {
            // Arrange
            var fromDate = DateTimeOffset.Parse("2025-02-01");

            // Act
            var result = await _service.GetRevenueByCustomer(fromDate, null);

            // Assert
            result.Should().HaveCount(1);
            result.Values.First().Should().Be(100000m);
        }

        [Fact]
        public async Task GetTopCustomers_ReturnsCorrectCount_WhenTopIsValid()
        {
            // Arrange
            var top = 2;

            // Act
            var result = await _service.GetTopCustomers(top, null, null);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetTopCustomers_OrdersByRevenueDescending_WhenReturningResults()
        {
            // Arrange
            var top = 2;

            // Act
            var result = (await _service.GetTopCustomers(top, null, null)).ToList();

            // Assert
            result[0].TotalRevenue.Should().Be(100000m); // Customer2
            result[1].TotalRevenue.Should().Be(83370m);   // Customer1
        }

        [Fact]
        public async Task GetTopCustomers_IncludesSalesCount_WhenReturningResults()
        {
            // Arrange
            var top = 2;

            // Act
            var result = (await _service.GetTopCustomers(top, null, null)).ToList();

            // Assert
            result[0].SalesCount.Should().Be(1); // Customer2 has 1 sale
            result[1].SalesCount.Should().Be(2); // Customer1 has 2 sales
        }

        [Fact]
        public async Task GetSalesCountByCustomer_GroupsByCustomerCorrectly_WhenMultipleCustomersExist()
        {
            // Arrange
            // Customer1: 2 sales, Customer2: 1 sale

            // Act
            var result = await _service.GetSalesCountByCustomer(null, null);

            // Assert
            result.Should().HaveCount(2);
            result.Values.Should().Contain(2);
            result.Values.Should().Contain(1);
        }

        [Fact]
        public async Task GetSalesCountByCustomer_FiltersCorrectly_WhenDateRangeProvided()
        {
            // Arrange
            var fromDate = DateTimeOffset.Parse("2025-01-15");

            // Act
            var result = await _service.GetSalesCountByCustomer(fromDate, null);

            // Assert
            result.Should().HaveCount(2);
        }

        #endregion

        #region Equipment Analytics Tests

        [Fact]
        public async Task GetRevenueByEquipment_GroupsByEquipmentCorrectly_WhenMultipleEquipmentExist()
        {
            // Arrange
            // Equipment1: 77820 + 5550 = 83370
            // Equipment2: 100000

            // Act
            var result = await _service.GetRevenueByEquipment(null, null);

            // Assert
            result.Should().HaveCount(2);
            result.Values.Should().Contain(83370m);
            result.Values.Should().Contain(100000m);
        }

        [Fact]
        public async Task GetRevenueByEquipment_FiltersCorrectly_WhenDateRangeProvided()
        {
            // Arrange
            var fromDate = DateTimeOffset.Parse("2025-02-01");

            // Act
            var result = await _service.GetRevenueByEquipment(fromDate, null);

            // Assert
            result.Should().HaveCount(1);
            result.Values.First().Should().Be(100000m);
        }

        [Fact]
        public async Task GetTopSellingEquipment_ReturnsCorrectCount_WhenTopIsValid()
        {
            // Arrange
            var top = 2;

            // Act
            var result = await _service.GetTopSellingEquipment(top, null, null);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetTopSellingEquipment_OrdersByRevenueDescending_WhenReturningResults()
        {
            // Arrange
            var top = 2;

            // Act
            var result = (await _service.GetTopSellingEquipment(top, null, null)).ToList();

            // Assert
            result[0].TotalRevenue.Should().Be(100000m);
            result[1].TotalRevenue.Should().Be(83370m);
        }

        [Fact]
        public async Task GetTopSellingEquipment_IncludesSalesCount_WhenReturningResults()
        {
            // Arrange
            var top = 2;

            // Act
            var result = (await _service.GetTopSellingEquipment(top, null, null)).ToList();

            // Assert
            result[0].SalesCount.Should().Be(1);
            result[1].SalesCount.Should().Be(2);
        }

        [Fact]
        public async Task GetSalesCountByEquipment_GroupsByEquipmentCorrectly_WhenMultipleEquipmentExist()
        {
            // Arrange
            // Equipment1: 2 sales, Equipment2: 1 sale

            // Act
            var result = await _service.GetSalesCountByEquipment(null, null);

            // Assert
            result.Should().HaveCount(2);
            result.Values.Should().Contain(2);
            result.Values.Should().Contain(1);
        }

        [Fact]
        public async Task GetSalesCountByEquipment_FiltersCorrectly_WhenDateRangeProvided()
        {
            // Arrange
            var toDate = DateTimeOffset.Parse("2025-01-31");

            // Act
            var result = await _service.GetSalesCountByEquipment(null, toDate);

            // Assert
            result.Should().HaveCount(1);
            result.Values.First().Should().Be(2); // Only equipment1 in January
        }

        #endregion

        #region Operational Analytics Tests

        [Fact]
        public async Task GetDeletedContractsCount_ReturnsCorrectCount_WhenDeletedContractsExist()
        {
            // Arrange
            // 1 deleted contract in seed data

            // Act
            var result = await _service.GetDeletedContractsCount();

            // Assert
            result.Should().Be(1);
        }

        [Fact]
        public async Task GetDeletedContractsCount_ReturnsZero_WhenNoDeletedContractsExist()
        {
            // Arrange
            var newContext = new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseInMemoryDatabase($"{Guid.NewGuid()}_NoDeletedDb")
                    .Options);

            var newService = new SellingContractAnalyticsService(
                newContext,
                _cache.Object,
                _versionProvider.Object);

            // Act
            var result = await newService.GetDeletedContractsCount();

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public async Task GetAverageSalePricePerEquipment_CalculatesCorrectly_WhenEquipmentHasMultipleContracts()
        {
            // Arrange
            var equipment1Id = _context.Sellings.First().EquipmentId;

            // Act
            var result = await _service.GetAverageSalePricePerEquipment(equipment1Id);

            // Assert
            result.Should().Be(41685m); // (77820 + 5550) / 2
        }

        [Fact]
        public async Task GetAverageSalePricePerEquipment_ReturnsCorrectAverage_WhenEquipmentHasSingleContract()
        {
            // Arrange
            var equipment2Id = _context.Sellings
                .Where(s => s.SalePrice == 100000m)
                .First()
                .EquipmentId;

            // Act
            var result = await _service.GetAverageSalePricePerEquipment(equipment2Id);

            // Assert
            result.Should().Be(100000m);
        }

        #endregion

        #region Caching Tests

        [Fact]
        public async Task GetTotalRevenue_CachesResult_WhenCalculated()
        {
            // Arrange & Act
            await _service.GetTotalRevenue(null, null);

            // Assert
            _cache.Verify(c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetRevenueByCustomer_UsesCacheVersionProvider_WhenGeneratingCacheKey()
        {
            // Arrange & Act
            await _service.GetRevenueByCustomer(null, null);

            // Assert
            _versionProvider.Verify(v => v.GetVersionAsync(CacheScopes.SellingContracts), Times.Once);
        }

        #endregion
    }
}
