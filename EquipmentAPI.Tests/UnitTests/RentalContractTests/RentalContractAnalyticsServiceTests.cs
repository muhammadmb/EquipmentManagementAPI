using Application.Interface.Services;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Contexts;
using Infrastructure.Helpers;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using System.Text;

namespace EquipmentAPI.Tests.UnitTests.RentalContractTests
{
    public class RentalContractAnalyticsServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IDistributedCache> _cache = new();
        private readonly Mock<ICacheVersionProvider> _versionProvider = new();

        private readonly RentalContractAnalyticsService _service;

        public RentalContractAnalyticsServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"{Guid.NewGuid()}_AnalyticsDb")
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new ApplicationDbContext(options);

            _versionProvider
                .Setup(v => v.GetVersionAsync(CacheScopes.RentalContracts))
                .ReturnsAsync("v1");

            _cache.Setup(c => c.GetAsync(
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync((byte[]?)null);

            _service = new RentalContractAnalyticsService(
                _context,
                _cache.Object,
                _versionProvider.Object);

            Seed();
        }

        private void Seed()
        {
            var customer1Id = Guid.NewGuid();
            var customer2Id = Guid.NewGuid();
            var equipment1 = Guid.NewGuid();
            var equipment2 = Guid.NewGuid();

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

            var contract1 = new RentalContract
            {
                Id = Guid.NewGuid(),
                CustomerId = customer1Id,
                EquipmentId = equipment1,
                StartDate = DateTimeOffset.UtcNow.AddDays(-10),
                EndDate = DateTimeOffset.UtcNow.AddDays(5),
                Shifts = 10,
                ShiftPrice = 100,
                RowVersion = []
            };
            contract1.Activate();
            contract1.Finish();

            var contract2 = new RentalContract
            {
                Id = Guid.NewGuid(),
                CustomerId = customer1Id,
                EquipmentId = equipment1,
                StartDate = DateTimeOffset.UtcNow.AddDays(-20),
                EndDate = DateTimeOffset.UtcNow.AddDays(-5),
                Shifts = 5,
                ShiftPrice = 200,
                RowVersion = []
            };
            contract2.Activate();
            contract2.Finish();

            var contract3 = new RentalContract
            {
                Id = Guid.NewGuid(),
                CustomerId = customer2Id,
                EquipmentId = Guid.NewGuid(),
                StartDate = DateTimeOffset.UtcNow.AddDays(-15),
                EndDate = DateTimeOffset.UtcNow.AddDays(-1),
                Shifts = 2,
                ShiftPrice = 300,
                RowVersion = []
            };

            contract3.Activate();
            contract3.Finish();

            _context.RentalContracts.AddRange(
                contract1,
                contract2,
                contract3
            );

            _context.SaveChanges();
        }

        // =========================================================================
        // COUNT
        // =========================================================================

        [Fact]
        public async Task GetRentalContractCount_ReturnsCorrectCount_WhenCacheMiss()
        {
            var result = await _service.GetRentalContractCount();

            result.Should().Be(3);

            _cache.Verify(c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetRentalContractCount_ReturnsCachedValue_WhenCacheHit()
        {
            var cachedBytes = Encoding.UTF8.GetBytes(CacheHelper.Serialize(5));

            _cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(cachedBytes);

            var result = await _service.GetRentalContractCount();

            result.Should().Be(5);

            _cache.Verify(c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task GetTotalActiveCount_ReturnsActiveOnly()
        {
            var result = await _service.GetTotalActiveCount();

            result.Should().Be(1);
        }

        // =========================================================================
        // CUSTOMER / EQUIPMENT
        // =========================================================================

        [Fact]
        public async Task GetTotalContractsForCustomer_ReturnsCorrectCount()
        {
            var customerId = _context.RentalContracts.First().CustomerId;

            var result = await _service.GetTotalContractsForCustomer(customerId);

            result.Should().Be(2);
        }

        [Fact]
        public async Task GetTotalContractsForEquipment_ReturnsCorrectCount()
        {
            var equipmentId = _context.RentalContracts.First().EquipmentId;

            var result = await _service.GetTotalContractsForEquipment(equipmentId);

            result.Should().Be(2);
        }

        // =========================================================================
        // REVENUE
        // =========================================================================

        [Fact]
        public async Task GetTotalRevenue_ReturnsCorrectValue()
        {
            var result = await _service.GetTotalRevenue(null, null);

            result.Should().Be(2600);
        }

        [Fact]
        public async Task GetRevenueByCustomer_ReturnsGroupedRevenue()
        {
            var result = await _service.GetRevenueByCustomer(null, null);

            result.Should().HaveCount(2);
            result.Values.Sum().Should().Be(2600);
        }

        [Fact]
        public async Task GetRevenueByMonth_ReturnsRevenueForCurrentYear()
        {
            var year = DateTimeOffset.UtcNow.Year;

            var result = await _service.GetRevenueByMonth(year);

            result.Should().NotBeEmpty();
            result.Values.Sum().Should().Be(2600);
        }

        // =========================================================================
        // STATISTICS
        // =========================================================================

        [Fact]
        public async Task GetContractPriceStatistics_ReturnsCorrectStatistics()
        {
            var result = await _service.GetContractPriceStatistics();

            result.TotalRevenue.Should().Be(2600);
            result.AveragePrice.Should().Be(2600m / 3);
            result.MedianPrice.Should().Be(1000);
        }

        [Fact]
        public async Task GetAverageContractDurationInDays_ReturnsPositiveValue()
        {
            var result = await _service.GetAverageContractDurationInDays();

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetAverageRevenuePerCustomer_ReturnsCorrectValue()
        {
            // customer1 = 2000, customer2 = 600 → avg = 1300
            var result = await _service.GetAverageRevenuePerCustomer();

            result.Should().Be(1300);
        }

        [Fact]
        public async Task GetAverageRentalPrice_ReturnsCorrectValue()
        {
            var result = await _service.GetAverageRentalPrice();

            result.Should().Be(2600m / 3);
        }
    }
}
