using Domain.Enums;

namespace Application.Interface.Services.EquipmentService
{
    public interface IEquipmentAnalyticsService
    {
        #region General statistics
        Task<int> GetTotalEquipmentCountAsync();
        Task<int> GetAvailableEquipmentCountAsync();
        Task<int> GetSoldEquipmentCountAsync();
        Task<int> GetUnderMaintenanceEquipmentCountAsync();
        #endregion

        #region Financial analytics
        Task<decimal> GetTotalPurchaseCostAsync();
        Task<decimal> GetTotalExpensesAsync();
        Task<decimal> GetTotalShippingCostAsync();
        Task<decimal> GetTotalEquipmentValueAsync();
        Task<decimal> GetAverageEquipmentPriceAsync();
        #endregion

        #region Grouped analytics
        Task<IDictionary<Guid, int>> GetEquipmentCountByBrandAsync();
        Task<IDictionary<Guid, int>> GetEquipmentCountByTypeAsync();
        Task<IDictionary<Guid, decimal>> GetTotalValueByBrandAsync();
        Task<IDictionary<Guid, decimal>> GetTotalValueByTypeAsync();
        #endregion

        #region Maintenance analytics
        Task<int> GetEquipmentWithMaintenanceCountAsync();
        Task<int> GetEquipmentWithoutMaintenanceCountAsync();
        Task<IDictionary<Guid, int>> GetMaintenanceCountByEquipmentAsync();
        #endregion

        #region Time-based analytics
        Task<IDictionary<int, int>> GetEquipmentCountByManufactureYearAsync();
        Task<IDictionary<int, decimal>> GetTotalPurchaseCostByYearAsync(int year);
        Task<IDictionary<int, int>> GetEquipmentPurchasedPerYearAsync();
        #endregion

        #region Operational insights
        Task<IEnumerable<Guid>> GetEquipmentIdsUnderMaintenanceAsync();
        Task<IEnumerable<Guid>> GetIdleEquipmentIdsAsync();
        Task<IEnumerable<Guid>> GetMostExpensiveEquipmentIdsAsync(int top);
        #endregion

        #region Supplier analytics
        Task<IDictionary<Guid, int>> GetEquipmentCountBySupplierAsync();
        Task<IDictionary<Guid, decimal>> GetTotalValueBySupplierAsync();
        #endregion

        #region Status distribution
        Task<IDictionary<EquipmentStatus, int>> GetEquipmentStatusDistributionAsync();
        #endregion
    }
}
