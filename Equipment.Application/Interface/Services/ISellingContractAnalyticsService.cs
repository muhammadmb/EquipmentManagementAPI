using Application.Models.SellingContract.Read;
using Domain.Enums;

namespace Application.Interface.Services
{
    public interface ISellingContractAnalyticsService
    {
        Task<decimal> GetTotalRevenue(DateTimeOffset? from, DateTimeOffset? to);
        Task<decimal> GetAverageSalePrice(DateTimeOffset? from, DateTimeOffset? to);
        Task<IDictionary<Guid, decimal>> GetAverageSalePriceByEquipment(DateTimeOffset? from, DateTimeOffset? to, EquipmentBrand? equipmentBrand, EquipmentType? equipmentType);
        Task<decimal> GetMinSalePrice(DateTimeOffset? from, DateTimeOffset? to);
        Task<decimal> GetMaxSalePrice(DateTimeOffset? from, DateTimeOffset? to);

        Task<IDictionary<DateTime, decimal>> GetRevenueByDay(DateTimeOffset? from, DateTimeOffset? to);
        Task<IDictionary<int, decimal>> GetRevenueByMonth(int year);
        Task<IDictionary<int, decimal>> GetRevenueByYear();
        Task<int> GetSalesCount(DateTimeOffset? from, DateTimeOffset? to);

        Task<IDictionary<Guid, decimal>> GetRevenueByCustomer(DateTimeOffset? from, DateTimeOffset? to);
        Task<IEnumerable<TopCustomerResult>> GetTopCustomers(int top, DateTimeOffset? from, DateTimeOffset? to);
        Task<IDictionary<Guid, int>> GetSalesCountByCustomer(DateTimeOffset? from, DateTimeOffset? to);

        Task<IDictionary<Guid, decimal>> GetRevenueByEquipment(DateTimeOffset? from, DateTimeOffset? to);
        Task<IEnumerable<TopEquipmentResult>> GetTopSellingEquipment(int top, DateTimeOffset? from, DateTimeOffset? to);
        Task<IDictionary<Guid, int>> GetSalesCountByEquipment(DateTimeOffset? from, DateTimeOffset? to);

        Task<int> GetDeletedContractsCount();
        Task<decimal> GetAverageSalePricePerEquipment(Guid equipmentId);
    }

}
