using Application.Interface.Services;
using Application.Models.RentalContractModels.Read;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Contexts;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Infrastructure.Services
{
    public class RentalContractAnalyticsService : IRentalContractAnalyticsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly ICacheVersionProvider _cacheVersionProvider;
        private const int CacheExpirationMinutes = 15;
        private static DistributedCacheEntryOptions CacheOptions =>
            new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes) };

        public RentalContractAnalyticsService(
            ApplicationDbContext context,
            IDistributedCache cache,
            ICacheVersionProvider versionProvider)
        {
            _context = context ??
                throw new ArgumentNullException(nameof(context));
            _cache = cache ??
                throw new ArgumentNullException(nameof(cache));
            _cacheVersionProvider = versionProvider ??
                throw new ArgumentException(nameof(versionProvider));
        }

        public async Task<int> GetRentalContractCount()
        {
            var version = await _cacheVersionProvider.GetVersionAsync(CacheScopes.RentalContracts);
            var cacheKey = CacheHelper.AnalyticsKey("rental-contract-count", version);

            var cachedValue = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedValue))
            {
                return CacheHelper.Deserialize<int>(cachedValue);
            }

            var count = await _context.RentalContracts.CountAsync();

            await _cache.SetStringAsync(
                cacheKey,
                CacheHelper.Serialize(count),
                CacheOptions);

            return count;
        }

        public async Task<int> GetTotalActiveCount()
        {
            var version = await _cacheVersionProvider.GetVersionAsync(CacheScopes.RentalContracts);
            var cacheKey = CacheHelper.AnalyticsKey("total-active-count", version);

            var cachedValue = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedValue))
            {
                return CacheHelper.Deserialize<int>(cachedValue);
            }

            var count = await _context.RentalContracts
                .CountAsync(rc => rc.EndDate >= DateTimeOffset.UtcNow);

            await _cache.SetStringAsync(
                cacheKey,
                CacheHelper.Serialize(count),
                CacheOptions);

            return count;
        }

        public async Task<int> GetTotalContractsForCustomer(Guid customerId)
        {
            var version = await _cacheVersionProvider.GetVersionAsync(CacheScopes.RentalContracts);
            var cacheKey = CacheHelper.AnalyticsKey(
                "total-contracts-for-customer",
                version,
                ("customerId", customerId));

            var cachedValue = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedValue))
            {
                return CacheHelper.Deserialize<int>(cachedValue);
            }

            var count = await _context.RentalContracts
                .CountAsync(rc => rc.CustomerId == customerId);

            await _cache.SetStringAsync(
                cacheKey,
                CacheHelper.Serialize(count),
               CacheOptions);

            return count;
        }

        public async Task<int> GetTotalContractsForEquipment(Guid equipmentId)
        {
            var version = await _cacheVersionProvider.GetVersionAsync(CacheScopes.RentalContracts);
            var cacheKey = CacheHelper.AnalyticsKey(
                "total-contracts-for-equipment",
                version,
                ("equipmentId", equipmentId));

            var cachedValue = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedValue))
            {
                return CacheHelper.Deserialize<int>(cachedValue);
            }

            var count = await _context.RentalContracts
                .CountAsync(rc => rc.EquipmentId == equipmentId);

            await _cache.SetStringAsync(
                cacheKey,
                CacheHelper.Serialize(count),
                CacheOptions);

            return count;
        }

        public async Task<IEnumerable<EquipmentContractSummaryDto>> GetEquipmentContractSummary()
        {
            var version = await _cacheVersionProvider.GetVersionAsync(CacheScopes.RentalContracts);
            var cacheKey = CacheHelper.AnalyticsKey("equipment-contract-summary", version);

            var cachedValue = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedValue))
            {
                var deserializedData = CacheHelper.Deserialize<IEnumerable<EquipmentContractSummaryDto>>(cachedValue);
                if (deserializedData != null)
                {
                    return deserializedData;
                }
            }

            var equipmentContractSummary = await _context.RentalContracts
                .GroupBy(rc => rc.EquipmentId)
                .Select(g => new EquipmentContractSummaryDto
                {
                    EquipmentId = g.Key,
                    ContractCount = g.Count()
                })
                .ToListAsync();

            await _cache.SetStringAsync(
                cacheKey,
                CacheHelper.Serialize(equipmentContractSummary),
                CacheOptions);

            return equipmentContractSummary;
        }

        public async Task<decimal> GetTotalRevenue(DateTime? from, DateTime? to)
        {
            var version = await _cacheVersionProvider.GetVersionAsync(CacheScopes.RentalContracts);
            var cacheKey = CacheHelper.AnalyticsKey(
                "total-revenue",
                version,
                ("from", from),
                ("to", to));

            var cachedValue = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedValue))
            {
                return CacheHelper.Deserialize<decimal>(cachedValue);
            }

            IQueryable<RentalContract> query = _context.RentalContracts.AsQueryable();

            if (from.HasValue)
                query = query.Where(rc => rc.StartDate >= from);

            if (to.HasValue)
                query = query.Where(rc => rc.EndDate <= to);

            var revenue = await query.SumAsync(r => r.Shifts * r.ShiftPrice);

            await _cache.SetStringAsync(
                cacheKey,
                CacheHelper.Serialize(revenue),
                CacheOptions);

            return revenue;
        }

        public async Task<IDictionary<string, decimal>> GetRevenueByCustomer(DateTime? from, DateTime? to)
        {
            var version = await _cacheVersionProvider.GetVersionAsync(CacheScopes.RentalContracts);
            var cacheKey = CacheHelper.AnalyticsKey(
                "revenue-by-customer",
                version,
                ("from", from),
                ("to", to));

            var cachedValue = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedValue))
            {
                return CacheHelper.Deserialize<Dictionary<string, decimal>>(cachedValue);
            }

            IQueryable<RentalContract> query = _context.RentalContracts
                    .AsQueryable();

            if (from.HasValue)
                query = query.Where(rc => rc.StartDate >= from);

            if (to.HasValue)
                query = query.Where(rc => rc.EndDate <= to);

            var revenueByCustomer = await query
                .Include(c => c.Customer)
                .GroupBy(rc => rc.Customer.Name)
                .Select(g => new
                {
                    Customer = g.Key,
                    Revenue = g.Sum(rc => rc.Shifts * rc.ShiftPrice)
                })
                .ToDictionaryAsync(x => x.Customer, x => x.Revenue);

            await _cache.SetStringAsync(
                cacheKey,
                CacheHelper.Serialize(revenueByCustomer),
                CacheOptions);

            return revenueByCustomer;
        }

        public async Task<IDictionary<int, decimal>> GetRevenueByMonth(int year)
        {
            var version = await _cacheVersionProvider.GetVersionAsync(CacheScopes.RentalContracts);
            var cacheKey = CacheHelper.AnalyticsKey(
                "revenue-by-month",
                version,
                ("year", year));

            var cachedValue = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedValue))
            {
                return CacheHelper.Deserialize<Dictionary<int, decimal>>(cachedValue) ?? new Dictionary<int, decimal>();
            }

            var revenueByMonth = await _context.RentalContracts
                .Where(rc => rc.StartDate.Year == year)
                .GroupBy(rc => rc.StartDate.Month)
                .Select(g => new { Month = g.Key, Revenue = g.Sum(rc => rc.Shifts * rc.ShiftPrice) })
                .ToDictionaryAsync(x => x.Month, x => x.Revenue);

            await _cache.SetStringAsync(
                cacheKey,
                CacheHelper.Serialize(revenueByMonth),
                CacheOptions);

            return revenueByMonth;
        }

        public async Task<ContractPriceStatisticsDto> GetContractPriceStatistics()
        {
            var version = await _cacheVersionProvider.GetVersionAsync(CacheScopes.RentalContracts);
            var cacheKey = CacheHelper.AnalyticsKey("contract-price-statistics", version);

            var cachedValue = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedValue))
            {
                return CacheHelper.Deserialize<ContractPriceStatisticsDto>(cachedValue);
            }

            var contracts = await _context.RentalContracts
                    .Select(rc => rc.Shifts * rc.ShiftPrice)
                    .OrderBy(p => p)
                    .ToListAsync();

            if (!contracts.Any())
            {
                var emptyResult = new ContractPriceStatisticsDto
                {
                    AveragePrice = 0,
                    MedianPrice = 0,
                    TotalRevenue = 0
                };
                await _cache.SetStringAsync(
                    cacheKey,
                    CacheHelper.Serialize(emptyResult),
                    CacheOptions);

                return emptyResult;
            }

            var average = contracts.Average();
            var total = contracts.Sum();
            var median = contracts.Count % 2 == 0
                ? (contracts[contracts.Count / 2 - 1] + contracts[contracts.Count / 2]) / 2
                : contracts[contracts.Count / 2];

            var result = new ContractPriceStatisticsDto
            {
                AveragePrice = average,
                MedianPrice = median,
                TotalRevenue = total
            };

            await _cache.SetStringAsync(
                cacheKey,
                CacheHelper.Serialize(result),
                CacheOptions);

            return result;
        }

        public async Task<int> GetFinishedContractsCount(DateTime? from, DateTime? to)
        {
            var version = await _cacheVersionProvider.GetVersionAsync(CacheScopes.RentalContracts);
            var cacheKey = CacheHelper.AnalyticsKey(
                "finished-contracts-count",
                version,
                ("from", from),
                ("to", to));

            var cachedValue = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedValue))
            {
                return CacheHelper.Deserialize<int>(cachedValue);
            }

            var query = _context.RentalContracts.AsQueryable()
                .Where(rc => rc.Status == RentalContractStatus.Finished);

            if (from.HasValue)
                query = query.Where(rc => rc.StartDate >= from.Value);

            if (to.HasValue)
                query = query.Where(rc => rc.EndDate <= to.Value);

            var count = await query.CountAsync();

            await _cache.SetStringAsync(
                cacheKey,
                CacheHelper.Serialize(count),
                CacheOptions);

            return count;
        }

        public async Task<int> GetCancelledContractsCount(DateTime? from, DateTime? to)
        {
            var version = await _cacheVersionProvider.GetVersionAsync(CacheScopes.RentalContracts);
            var cacheKey = CacheHelper.AnalyticsKey(
                "cancelled-contracts-count",
                version,
                ("from", from),
                ("to", to));

            var cachedValue = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedValue))
            {
                return CacheHelper.Deserialize<int>(cachedValue);
            }

            var query = _context.RentalContracts.AsQueryable()
                .Where(rc => rc.Status == RentalContractStatus.Cancelled);

            if (from.HasValue)
                query = query.Where(rc => rc.StartDate >= from.Value);

            if (to.HasValue)
                query = query.Where(rc => rc.EndDate <= to.Value);

            var count = await query.CountAsync();

            await _cache.SetStringAsync(
                cacheKey,
                CacheHelper.Serialize(count),
                CacheOptions);

            return count;
        }

        public async Task<double> GetAverageContractDurationInDays()
        {
            var version = await _cacheVersionProvider.GetVersionAsync(CacheScopes.RentalContracts);
            var cacheKey = CacheHelper.AnalyticsKey("average-contract-duration-in-days", version);

            var cachedValue = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedValue))
            {
                return CacheHelper.Deserialize<double>(cachedValue);
            }

            var durations = await _context.RentalContracts
                    .Select(rc => new
                    {
                        rc.StartDate,
                        rc.EndDate
                    })
                    .ToListAsync();

            if (!durations.Any())
            {
                await _cache.SetStringAsync(
                    cacheKey,
                    CacheHelper.Serialize(0d),
                    CacheOptions);

                return 0;
            }

            var average = durations
                .Select(d => (d.EndDate - d.StartDate).TotalDays)
                .Average();

            await _cache.SetStringAsync(
                cacheKey,
                CacheHelper.Serialize(average),
               CacheOptions);

            return average;
        }

        public async Task<decimal> GetAverageRevenuePerCustomer()
        {
            var version = await _cacheVersionProvider.GetVersionAsync(CacheScopes.RentalContracts);
            var cacheKey = CacheHelper.AnalyticsKey("average-revenue-per-customer", version);

            var cachedValue = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedValue))
            {
                return CacheHelper.Deserialize<decimal>(cachedValue);
            }

            var revenueByCustomer = await _context.RentalContracts
                .GroupBy(rc => rc.CustomerId)
                .Select(g => g.Sum(rc => rc.Shifts * rc.ShiftPrice))
                .ToListAsync();

            if (!revenueByCustomer.Any())
            {
                await _cache.SetStringAsync(
                    cacheKey,
                    CacheHelper.Serialize(0m),
                    CacheOptions);

                return 0;
            }

            var average = revenueByCustomer.Average();

            await _cache.SetStringAsync(
                cacheKey,
                CacheHelper.Serialize(average),
               CacheOptions);

            return average;
        }

        public async Task<decimal> GetAverageRentalPrice()
        {
            var version = await _cacheVersionProvider.GetVersionAsync(CacheScopes.RentalContracts);
            var cacheKey = CacheHelper.AnalyticsKey("average-rental-price", version);

            var cachedValue = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedValue))
            {
                return CacheHelper.Deserialize<decimal>(cachedValue);
            }

            var prices = await _context.RentalContracts
                .Select(rc => rc.Shifts * rc.ShiftPrice)
                .ToListAsync();

            if (!prices.Any())
            {
                await _cache.SetStringAsync(
                    cacheKey,
                    CacheHelper.Serialize(0m),
                    CacheOptions);

                return 0;
            }

            var average = prices.Average();

            await _cache.SetStringAsync(
                cacheKey,
                CacheHelper.Serialize(average),
                CacheOptions);

            return average;
        }
    }
}
