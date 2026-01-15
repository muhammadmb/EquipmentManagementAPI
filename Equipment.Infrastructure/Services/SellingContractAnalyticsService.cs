using Application.Interface.Services;
using Application.Models.SellingContract.Read;
using Domain.Enums;
using Infrastructure.Contexts;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Infrastructure.Services
{
    public class SellingContractAnalyticsService : ISellingContractAnalyticsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly ICacheVersionProvider _cacheVersionProvider;
        private const int CacheExpirationMinutes = 15;

        private static DistributedCacheEntryOptions CacheOptions =>
            new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes) };

        public SellingContractAnalyticsService(
            ApplicationDbContext context,
            IDistributedCache cache,
            ICacheVersionProvider versionProvider)
        {
            _context = context ??
                throw new ArgumentNullException(nameof(context));
            _cache = cache ??
               throw new ArgumentNullException(nameof(cache));
            _cacheVersionProvider = versionProvider ??
                throw new ArgumentNullException(nameof(versionProvider));
        }

        #region Financial Analytics
        public async Task<decimal> GetTotalRevenue(DateTimeOffset? from, DateTimeOffset? to)
        {
            var version = await _cacheVersionProvider.GetVersionAsync(CacheScopes.SellingContracts);
            var cacheKey = CacheHelper.AnalyticsKey("total-revenue", version, ("from", from), ("to", to));

            var cachedValue = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedValue))
            {
                return CacheHelper.Deserialize<decimal>(cachedValue);
            }

            var query = _context.Sellings.AsQueryable();

            if (from.HasValue)
                query = query.Where(sc => sc.SaleDate >= from);

            if (to.HasValue)
                query = query.Where(sc => sc.SaleDate <= to);

            var totalRevenue = await query.SumAsync(sc => sc.SalePrice);

            await _cache.SetStringAsync(
                cacheKey,
                CacheHelper.Serialize(totalRevenue),
                CacheOptions);

            return totalRevenue;
        }

        public async Task<decimal> GetAverageSalePrice(DateTimeOffset? from, DateTimeOffset? to)
        {
            var version = await _cacheVersionProvider.GetVersionAsync(CacheScopes.SellingContracts);
            var cacheKey = CacheHelper.AnalyticsKey("average-sale-price", version, ("from", from), ("to", to));

            var cachedValue = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedValue))
            {
                return CacheHelper.Deserialize<decimal>(cachedValue);
            }

            var query = _context.Sellings.AsQueryable();
            if (from.HasValue)
            {
                query = query.Where(selling => selling.SaleDate >= from.Value);
            }
            if (to.HasValue)
            {
                query = query.Where(selling => selling.SaleDate <= to.Value);
            }

            var averageSalePrice = await query.AverageAsync(selling => selling.SalePrice);
            await _cache.SetStringAsync(
                cacheKey,
                CacheHelper.Serialize(averageSalePrice),
                CacheOptions);
            return averageSalePrice;
        }

        public async Task<IDictionary<Guid, decimal>> GetAverageSalePriceByEquipment(
            DateTimeOffset? from,
            DateTimeOffset? to,
            EquipmentBrand? equipmentBrand,
            EquipmentType? equipmentType)
        {
            var version = await _cacheVersionProvider.GetVersionAsync(CacheScopes.SellingContracts);
            var cacheKey = CacheHelper.AnalyticsKey(
                "average-sale-price-by-equipment",
                version,
                ("from", from), ("to", to), ("equipmentBrand", equipmentBrand), ("equipmentType", equipmentType));

            var cachedValue = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedValue))
            {
                return CacheHelper.Deserialize<IDictionary<Guid, decimal>>(cachedValue);
            }

            var query = _context.Sellings
                .Include(s => s.Equipment)
                .AsQueryable();

            if (from.HasValue)
            {
                query = query.Where(selling => selling.SaleDate >= from.Value);
            }
            if (to.HasValue)
            {
                query = query.Where(selling => selling.SaleDate <= to.Value);
            }
            if (equipmentBrand.HasValue)
            {
                query = query.Where(selling => selling.Equipment.EquipmentBrand == equipmentBrand);
            }
            if (equipmentType.HasValue)
            {
                query = query.Where(selling => selling.Equipment.EquipmentType == equipmentType);
            }
            var averageSalePriceByEquipment = await query
                .GroupBy(s => s.EquipmentId)
                .Select(g => new
                {
                    EquipmentId = g.Key,
                    AveragePrice = g.Average(x => x.SalePrice)
                })
                .ToDictionaryAsync(x => x.EquipmentId, x => x.AveragePrice);

            await _cache.SetStringAsync(
                 cacheKey,
                 CacheHelper.Serialize(averageSalePriceByEquipment),
                 CacheOptions);
            return averageSalePriceByEquipment;
        }

        public async Task<decimal> GetMinSalePrice(DateTimeOffset? from, DateTimeOffset? to)
        {
            var version = await _cacheVersionProvider.GetVersionAsync(CacheScopes.SellingContracts);
            var cacheKey = CacheHelper.AnalyticsKey("min-sale-price", version, ("from", from), ("to", to));

            var cachedValue = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedValue))
            {
                return CacheHelper.Deserialize<decimal>(cachedValue);
            }

            var query = _context.Sellings.AsQueryable();
            if (from.HasValue)
            {
                query = query.Where(selling => selling.SaleDate >= from.Value);
            }
            if (to.HasValue)
            {
                query = query.Where(selling => selling.SaleDate <= to.Value);
            }
            var minSalePrice = await query.MinAsync(selling => selling.SalePrice);

            await _cache.SetStringAsync(
                 cacheKey,
                 CacheHelper.Serialize(minSalePrice),
                 CacheOptions);

            return minSalePrice;
        }
        public async Task<decimal> GetMaxSalePrice(DateTimeOffset? from, DateTimeOffset? to)
        {
            var version = await _cacheVersionProvider.GetVersionAsync(CacheScopes.SellingContracts);
            var cacheKey = CacheHelper.AnalyticsKey("max-sale-price", version, ("from", from), ("to", to));
            var cachedValue = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                return CacheHelper.Deserialize<decimal>(cachedValue);
            }

            var query = _context.Sellings.AsQueryable();
            if (from.HasValue)
            {
                query = query.Where(selling => selling.SaleDate >= from.Value);
            }
            if (to.HasValue)
            {
                query = query.Where(selling => selling.SaleDate <= to.Value);
            }
            var maxSalePrice = await query.MaxAsync(selling => selling.SalePrice);

            await _cache.SetStringAsync(
                 cacheKey,
                 CacheHelper.Serialize(maxSalePrice),
                 CacheOptions);

            return maxSalePrice;
        }
        #endregion

        #region Revenue Analytics
        public async Task<IDictionary<DateTime, decimal>> GetRevenueByDay(DateTimeOffset? from, DateTimeOffset? to)
        {
            var version = await _cacheVersionProvider.GetVersionAsync(CacheScopes.SellingContracts);
            var cacheKey = CacheHelper.AnalyticsKey("revenue-by-day", version, ("from", from), ("to", to));
            var cachedValue = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                var deserializedData = CacheHelper.Deserialize<IDictionary<DateTime, decimal>>(cachedValue);
                if (deserializedData != null) return deserializedData;
            }

            var query = _context.Sellings.AsQueryable();
            if (from.HasValue)
            {
                query = query.Where(selling => selling.SaleDate >= from.Value);
            }
            if (to.HasValue)
            {
                query = query.Where(selling => selling.SaleDate <= to.Value);
            }

            var revenueByDay =await query
               .GroupBy(sc => sc.SaleDate.Date)
               .Select(g => new
               {
                   Day = g.Key,
                   Total = g.Sum(x => x.SalePrice)
               })
               .OrderBy(x => x.Day)
               .ToDictionaryAsync(x => x.Day, x => x.Total);

            await _cache.SetStringAsync(
                 cacheKey,
                 CacheHelper.Serialize(revenueByDay),
                 CacheOptions);

            return revenueByDay;
        }
        public async Task<IDictionary<int, decimal>> GetRevenueByMonth(int year)
        {
            var version = await _cacheVersionProvider.GetVersionAsync(CacheScopes.SellingContracts);
            var cacheKey = CacheHelper.AnalyticsKey("revenue-by-month", version, ("year", year));
            var cachedValue = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                var deserializedData = CacheHelper.Deserialize<IDictionary<int, decimal>>(cachedValue);
                if (deserializedData != null) return deserializedData;
            }

            var revenueByMonth = await _context.Sellings
                .Where(sc => sc.SaleDate.Year == year)
                .GroupBy(sc => sc.SaleDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Total = g.Sum(x => x.SalePrice)
                })
                .OrderBy(x => x.Month)
                .ToDictionaryAsync(x => x.Month, x => x.Total);

            await _cache.SetStringAsync(
                 cacheKey,
                 CacheHelper.Serialize(revenueByMonth),
                 CacheOptions);

            return revenueByMonth;
        }

        public async Task<IDictionary<int, decimal>> GetRevenueByYear()
        {
            var version = await _cacheVersionProvider.GetVersionAsync(CacheScopes.SellingContracts);
            var cacheKey = CacheHelper.AnalyticsKey("revenue-by-year", version);

            var cachedValue = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                var deserializedData = CacheHelper.Deserialize<IDictionary<int, decimal>>(cachedValue);
                if (deserializedData != null) return deserializedData;
            }

            var revenueByYear = await _context.Sellings
                .GroupBy(sc => sc.SaleDate.Year)
                .Select(g => new
                {
                    Year = g.Key,
                    Total = g.Sum(x => x.SalePrice)
                })
                .OrderBy(x => x.Year)
                .ToDictionaryAsync(x => x.Year, x => x.Total);

            await _cache.SetStringAsync(
                 cacheKey,
                 CacheHelper.Serialize(revenueByYear),
                 CacheOptions);

            return revenueByYear;
        }
        public async Task<int> GetSalesCount(DateTimeOffset? from, DateTimeOffset? to)
        {
            var version = await _cacheVersionProvider.GetVersionAsync(CacheScopes.SellingContracts);
            var cacheKey = CacheHelper.AnalyticsKey("sales-count", version, ("from", from), ("to", to));
            var cachedValue = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                return CacheHelper.Deserialize<int>(cachedValue);
            }

            var query = _context.Sellings.AsQueryable();

            if (from.HasValue)
                query = query.Where(sc => sc.SaleDate >= from.Value);

            if (to.HasValue)
                query = query.Where(sc => sc.SaleDate <= to.Value);

            var salesCount = await query.CountAsync();
            await _cache.SetStringAsync(
                 cacheKey,
                 CacheHelper.Serialize(salesCount),
                 CacheOptions);

            return salesCount;
        }
        #endregion

        #region Customer Analytics
        public async Task<IDictionary<Guid, decimal>> GetRevenueByCustomer(DateTimeOffset? from, DateTimeOffset? to)
        {
            var version = await _cacheVersionProvider.GetVersionAsync(CacheScopes.SellingContracts);
            var cacheKey = CacheHelper.AnalyticsKey("revenue-by-customer", version, ("from", from), ("to", to));
            var cachedValue = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                var deserializedData = CacheHelper.Deserialize<IDictionary<Guid, decimal>>(cachedValue);
                if (deserializedData != null) return deserializedData;
            }

            var query = _context.Sellings.AsQueryable();
            if (from.HasValue)
            {
                query = query.Where(selling => selling.SaleDate >= from.Value);
            }
            if (to.HasValue)
            {
                query = query.Where(selling => selling.SaleDate <= to.Value);
            }

            var revenueByCustomer = await query
                .GroupBy(selling => selling.CustomerId)
                .Select(g => new
                {
                    CustomerId = g.Key,
                    Total = g.Sum(selling => selling.SalePrice)
                })
                .ToDictionaryAsync(x => x.CustomerId, x => x.Total);

            await _cache.SetStringAsync(
                 cacheKey,
                 CacheHelper.Serialize(revenueByCustomer),
                 CacheOptions);

            return revenueByCustomer;
        }
        public async Task<IEnumerable<TopCustomerResult>> GetTopCustomers(int top, DateTimeOffset? from, DateTimeOffset? to)
        {
            var version = await _cacheVersionProvider.GetVersionAsync(CacheScopes.SellingContracts);
            var cacheKey = CacheHelper.AnalyticsKey("top-customers", version, ("from", from), ("to", to), ("top", top));
            var cachedValue = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                var deserializedData = CacheHelper.Deserialize<IEnumerable<TopCustomerResult>>(cachedValue);
                if (deserializedData != null) return deserializedData;
            }

            var query = _context.Sellings.AsQueryable();

            if (from.HasValue)
                query = query.Where(sc => sc.SaleDate >= from.Value);

            if (to.HasValue)
                query = query.Where(sc => sc.SaleDate <= to.Value);

            var topCustomers = await query
               .GroupBy(sc => sc.CustomerId)
               .Select(g => new TopCustomerResult
               {
                   CustomerId = g.Key,
                   TotalRevenue = g.Sum(x => x.SalePrice),
                   SalesCount = g.Count()
               })
               .OrderByDescending(x => x.TotalRevenue)
               .Take(top)
               .ToListAsync();

            await _cache.SetStringAsync(
                 cacheKey,
                 CacheHelper.Serialize(topCustomers),
                 CacheOptions);
            return topCustomers;
        }

        public async Task<IDictionary<Guid, int>> GetSalesCountByCustomer(DateTimeOffset? from, DateTimeOffset? to)
        {
            var version = await _cacheVersionProvider.GetVersionAsync(CacheScopes.SellingContracts);
            var cacheKey = CacheHelper.AnalyticsKey("sales-count-by-customer", version, ("from", from), ("to", to));
            var cachedValue = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                var deserializedData = CacheHelper.Deserialize<IDictionary<Guid, int>>(cachedValue);
                if (deserializedData != null) return deserializedData;
            }

            var query = _context.Sellings.AsQueryable();

            if (from.HasValue)
                query = query.Where(sc => sc.SaleDate >= from.Value);

            if (to.HasValue)
                query = query.Where(sc => sc.SaleDate <= to.Value);

            var salesCountByCustomers = await query
                .GroupBy(sc => sc.CustomerId)
                .Select(g => new
                {
                    CustomerId = g.Key,
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.CustomerId, x => x.Count);

            await _cache.SetStringAsync(
                 cacheKey,
                 CacheHelper.Serialize(salesCountByCustomers),
                 CacheOptions);
            return salesCountByCustomers;
        }
        #endregion

        #region Equipment Analytics
        public async Task<IDictionary<Guid, decimal>> GetRevenueByEquipment(DateTimeOffset? from, DateTimeOffset? to)
        {
            var version = await _cacheVersionProvider.GetVersionAsync(CacheScopes.SellingContracts);
            var cacheKey = CacheHelper.AnalyticsKey("revenue-by-equipment", version, ("from", from), ("to", to));
            var cachedValue = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                var deserializedData = CacheHelper.Deserialize<IDictionary<Guid, decimal>>(cachedValue);
                if (deserializedData != null) return deserializedData;
            }

            var query = _context.Sellings.AsQueryable();

            if (from.HasValue)
                query = query.Where(sc => sc.SaleDate >= from.Value);

            if (to.HasValue)
                query = query.Where(sc => sc.SaleDate <= to.Value);

            var revenueByEquipment = await query
                .GroupBy(sc => sc.EquipmentId)
                .Select(g => new
                {
                    EquipmentId = g.Key,
                    Total = g.Sum(x => x.SalePrice)
                })
                .ToDictionaryAsync(x => x.EquipmentId, x => x.Total);

            await _cache.SetStringAsync(
                cacheKey,
                CacheHelper.Serialize(revenueByEquipment),
                CacheOptions);
            return revenueByEquipment;
        }

        public async Task<IEnumerable<TopEquipmentResult>> GetTopSellingEquipment(
            int top,
            DateTimeOffset? from,
            DateTimeOffset? to)
        {
            var version = await _cacheVersionProvider.GetVersionAsync(CacheScopes.SellingContracts);
            var cacheKey = CacheHelper.AnalyticsKey("top-selling-equipment", version, ("from", from), ("to", to), ("top", top));
            var cachedValue = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedValue))
            {
                var deserializedData = CacheHelper.Deserialize<IEnumerable<TopEquipmentResult>>(cachedValue);
                if (deserializedData != null) return deserializedData;
            }

            var query = _context.Sellings.AsQueryable();

            if (from.HasValue)
                query = query.Where(sc => sc.SaleDate >= from);

            if (to.HasValue)
                query = query.Where(sc => sc.SaleDate <= to);

            var topSellingEquipment = await query
                .GroupBy(sc => sc.EquipmentId)
                .Select(g => new TopEquipmentResult
                {
                    EquipmentId = g.Key,
                    TotalRevenue = g.Sum(x => x.SalePrice),
                    SalesCount = g.Count()
                })
                .OrderByDescending(x => x.TotalRevenue)
                .Take(top)
                .ToListAsync();

            await _cache.SetStringAsync(
                cacheKey,
                CacheHelper.Serialize(topSellingEquipment),
                CacheOptions);

            return topSellingEquipment;
        }

        public async Task<IDictionary<Guid, int>> GetSalesCountByEquipment(DateTimeOffset? from, DateTimeOffset? to)
        {
            var version = await _cacheVersionProvider.GetVersionAsync(CacheScopes.SellingContracts);
            var cacheKey = CacheHelper.AnalyticsKey("sales-count-by-equipment", version, ("from", from), ("to", to));
            var cachedValue = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                var deserialize = CacheHelper.Deserialize<IDictionary<Guid, int>>(cachedValue);
                if (deserialize != null) return deserialize;
            }

            var query = _context.Sellings.AsQueryable();

            if (from.HasValue)
                query = query.Where(sc => sc.SaleDate >= from.Value);

            if (to.HasValue)
                query = query.Where(sc => sc.SaleDate <= to.Value);

            var salesCountByEquipment = await query
                .GroupBy(sc => sc.EquipmentId)
                .Select(g => new
                {
                    EquipmentId = g.Key,
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.EquipmentId, x => x.Count);

            await _cache.SetStringAsync(
                cacheKey,
                CacheHelper.Serialize(salesCountByEquipment),
                CacheOptions);

            return salesCountByEquipment;
        }
        #endregion

        #region Operational Analytics
        public async Task<int> GetDeletedContractsCount()
        {
            var version = await _cacheVersionProvider.GetVersionAsync(CacheScopes.SellingContracts);
            var cacheKey = CacheHelper.AnalyticsKey("deleted-contract-count", version);
            var cachedValue = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                return CacheHelper.Deserialize<int>(cachedValue);
            }

            var deletedContractsCount = await _context.Sellings
                .IgnoreQueryFilters()
                .Where(selling =>
                    selling.DeletedDate != null)
                .CountAsync();

            await _cache.SetStringAsync(
                cacheKey,
                CacheHelper.Serialize(deletedContractsCount),
                CacheOptions);

            return deletedContractsCount;
        }
        public async Task<decimal> GetAverageSalePricePerEquipment(Guid equipmentId)
        {
            var version = await _cacheVersionProvider.GetVersionAsync(CacheScopes.SellingContracts);
            var cacheKey = CacheHelper.AnalyticsKey("average-sale-price-per-equipment", version, ("equipmentId", equipmentId));
            var cachedValue = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                return CacheHelper.Deserialize<decimal>(cachedValue);
            }

            var averageSalePricePerEquipment = await _context.Sellings
                .Where(selling =>
                    selling.EquipmentId == equipmentId)
                .AverageAsync(selling => selling.SalePrice);

            await _cache.SetStringAsync(
                cacheKey,
                CacheHelper.Serialize(averageSalePricePerEquipment),
                CacheOptions);

            return averageSalePricePerEquipment;
        }
        #endregion
    }
}