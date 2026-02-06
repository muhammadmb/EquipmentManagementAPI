using Application.Interface.Services.EquipmentService;
using Domain.Enums;
using Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.EquipmentService
{
    public class EquipmentAnalyticsService : IEquipmentAnalyticsService
    {
        private readonly ApplicationDbContext _context;
        public EquipmentAnalyticsService(ApplicationDbContext context)
        {
            _context = context ??
                throw new ArgumentNullException(nameof(context));
        }

        #region General statistics

        public async Task<int> GetTotalEquipmentCountAsync()
            => await _context.Equipments.CountAsync();

        public async Task<int> GetAvailableEquipmentCountAsync()
            => await _context.Equipments.CountAsync(e => e.EquipmentStatus == EquipmentStatus.Available);

        public async Task<int> GetSoldEquipmentCountAsync()
            => await _context.Equipments.CountAsync(e => e.EquipmentStatus == EquipmentStatus.Sold);

        public async Task<int> GetUnderMaintenanceEquipmentCountAsync()
            => await _context.Equipments.CountAsync(e => e.EquipmentStatus == EquipmentStatus.UnderMaintenance);

        #endregion

        #region Financial analytics

        public async Task<decimal> GetTotalPurchaseCostAsync()
            => await _context.Equipments.SumAsync(e => e.Price);

        public async Task<decimal> GetTotalExpensesAsync()
            => await _context.Equipments.SumAsync(e => e.Expenses);

        public async Task<decimal> GetTotalShippingCostAsync()
            => await _context.Equipments.SumAsync(e => e.Expenses);

        public async Task<decimal> GetTotalEquipmentValueAsync()
            => await _context.Equipments.SumAsync(e => e.TotalPrice);

        public async Task<decimal> GetAverageEquipmentPriceAsync()
            => await _context.Equipments.AverageAsync(e => e.Price);

        #endregion

        #region Grouped analytics

        public async Task<IDictionary<Guid, int>> GetEquipmentCountByBrandAsync()
            => await _context.Equipments
                .GroupBy(e => e.EquipmentBrand)
                .ToDictionaryAsync(g => (Guid)(object)g.Key, g => g.Count());

        public async Task<IDictionary<Guid, int>> GetEquipmentCountByTypeAsync()
            => await _context.Equipments
                .GroupBy(e => e.EquipmentType)
                .ToDictionaryAsync(g => (Guid)(object)g.Key, g => g.Count());

        public async Task<IDictionary<Guid, decimal>> GetTotalValueByBrandAsync()
            => await _context.Equipments
                .GroupBy(e => e.EquipmentBrand)
                .ToDictionaryAsync(g => (Guid)(object)g.Key, g => g.Sum(e => e.TotalPrice));

        public async Task<IDictionary<Guid, decimal>> GetTotalValueByTypeAsync()
            => await _context.Equipments
                .GroupBy(e => e.EquipmentType)
                .ToDictionaryAsync(g => (Guid)(object)g.Key, g => g.Sum(e => e.TotalPrice));

        #endregion

        #region Maintenance analytics

        public async Task<int> GetEquipmentWithMaintenanceCountAsync()
            => await _context.Equipments.CountAsync(e => e.MaintenanceRecords.Any());

        public async Task<int> GetEquipmentWithoutMaintenanceCountAsync()
            => await _context.Equipments.CountAsync(e => !e.MaintenanceRecords.Any());

        public async Task<IDictionary<Guid, int>> GetMaintenanceCountByEquipmentAsync()
            => await _context.MaintenanceRecords
                .GroupBy(m => m.EquipmentId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

        #endregion

        #region Time-based analytics

        public async Task<IDictionary<int, int>> GetEquipmentCountByManufactureYearAsync()
            => await _context.Equipments
                .GroupBy(e => e.ManufactureDate)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

        public async Task<IDictionary<int, decimal>> GetTotalPurchaseCostByYearAsync(int year)
            => await _context.Equipments
                .Where(e => e.PurchaseDate.Year == year)
                .GroupBy(e => e.PurchaseDate.Month)
                .ToDictionaryAsync(g => g.Key, g => g.Sum(e => e.Price));

        public async Task<IDictionary<int, int>> GetEquipmentPurchasedPerYearAsync()
            => await _context.Equipments
                .GroupBy(e => e.PurchaseDate.Year)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

        #endregion

        #region Operational insights

        public async Task<IEnumerable<Guid>> GetEquipmentIdsUnderMaintenanceAsync()
            => await _context.Equipments
                .Where(e => e.EquipmentStatus == EquipmentStatus.UnderMaintenance)
                .Select(e => e.Id)
                .ToListAsync();

        public async Task<IEnumerable<Guid>> GetIdleEquipmentIdsAsync()
            => await _context.Equipments
                .Where(e => e.EquipmentStatus == EquipmentStatus.Available)
                .Select(e => e.Id)
                .ToListAsync();

        public async Task<IEnumerable<Guid>> GetMostExpensiveEquipmentIdsAsync(int top)
            => await _context.Equipments
                .OrderByDescending(e => e.Price)
                .Take(top)
                .Select(e => e.Id)
                .ToListAsync();

        #endregion

        #region Supplier analytics

        public async Task<IDictionary<Guid, int>> GetEquipmentCountBySupplierAsync()
        {
            return await _context.Equipments
                .Where(e => e.SupplierId != null)
                .GroupBy(e => e.SupplierId!.Value)
                .Select(g => new
                {
                    SupplierId = g.Key,
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.SupplierId, x => x.Count);
        }

        public async Task<IDictionary<Guid, decimal>> GetTotalValueBySupplierAsync()
        {
            return await _context.Equipments
                .Where(e => e.SupplierId != null)
                .GroupBy(e => e.SupplierId!.Value)
                .Select(g => new
                {
                    SupplierId = g.Key,
                    TotalValue = g.Sum(e => e.TotalPrice)
                })
                .ToDictionaryAsync(x => x.SupplierId, x => x.TotalValue);
        }

        #endregion

        #region Status distribution

        public async Task<IDictionary<EquipmentStatus, int>> GetEquipmentStatusDistributionAsync()
            => await _context.Equipments
                .GroupBy(e => e.EquipmentStatus)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

        #endregion
    }
}
