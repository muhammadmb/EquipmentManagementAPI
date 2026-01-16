using Application.Interface.Services;
using Application.Models.SellingContract.Read;
using Domain.Enums;

namespace API.GraphQL.SellingContract.Queries
{
    [ExtendObjectType(Name = "Query")]
    public class SellingContractAnalyticsQueries
    {
        public async Task<decimal> GetTotalRevenue(
            DateTimeOffset? from,
            DateTimeOffset? to,
               [Service] ISellingContractAnalyticsService service)
        {
            return await service.GetTotalRevenue(from, to);
        }

        public async Task<decimal> GetAverageSalePrice(
          DateTimeOffset? from,
          DateTimeOffset? to,
             [Service] ISellingContractAnalyticsService service)
        {
            return await service.GetAverageSalePrice(from, to);
        }

        public async Task<IDictionary<Guid, decimal>> GetAverageSalePriceByEquipment(
          DateTimeOffset? from,
          DateTimeOffset? to,
          EquipmentBrand? equipmentBrand,
          EquipmentType? equipmentType,
             [Service] ISellingContractAnalyticsService service)
        {
            return await service.GetAverageSalePriceByEquipment(from, to, equipmentBrand, equipmentType);
        }

        public async Task<decimal> GetMinSalePrice(
          DateTimeOffset? from,
          DateTimeOffset? to,
             [Service] ISellingContractAnalyticsService service)
        {
            return await service.GetMinSalePrice(from, to);
        }

        public async Task<decimal> GetMaxSalePrice(
          DateTimeOffset? from,
          DateTimeOffset? to,
             [Service] ISellingContractAnalyticsService service)
        {
            return await service.GetMaxSalePrice(from, to);
        }

        public async Task<IDictionary<DateTime, decimal>> GetRevenueByDay(
          DateTimeOffset? from,
          DateTimeOffset? to,
             [Service] ISellingContractAnalyticsService service)
        {
            return await service.GetRevenueByDay(from, to);
        }

        public async Task<IDictionary<int, decimal>> GetRevenueByMonth(
          int year,
             [Service] ISellingContractAnalyticsService service)
        {
            if (year < 1900 || year > DateTime.Now.Year)
                throw new ArgumentOutOfRangeException(nameof(year), "Year must be between 1 and 9999.");
            return await service.GetRevenueByMonth(year);
        }

        public async Task<IDictionary<int, decimal>> GetRevenueByYear(
             [Service] ISellingContractAnalyticsService service)
        {
            return await service.GetRevenueByYear();
        }

        public async Task<decimal> GetSalesCount(
          DateTimeOffset? from,
          DateTimeOffset? to,
             [Service] ISellingContractAnalyticsService service)
        {
            return await service.GetSalesCount(from, to);
        }

        public async Task<IDictionary<Guid, decimal>> GetRevenueByCustomer(
          DateTimeOffset? from,
          DateTimeOffset? to,
             [Service] ISellingContractAnalyticsService service)
        {
            return await service.GetRevenueByCustomer(from, to);
        }

        public async Task<IEnumerable<TopCustomerResult>> GetTopCustomers(
          int top,
          DateTimeOffset? from,
          DateTimeOffset? to,
             [Service] ISellingContractAnalyticsService service)
        {
            if (top <= 0)
                throw new ArgumentException("The 'top' parameter must be greater than zero.", nameof(top));

            return await service.GetTopCustomers(top, from, to);
        }

        public async Task<IDictionary<Guid, int>> GetSalesCountByCustomer(
          DateTimeOffset? from,
          DateTimeOffset? to,
             [Service] ISellingContractAnalyticsService service)
        {
            return await service.GetSalesCountByCustomer(from, to);
        }

        public async Task<IDictionary<Guid, decimal>> GetRevenueByEquipment(
          DateTimeOffset? from,
          DateTimeOffset? to,
             [Service] ISellingContractAnalyticsService service)
        {
            return await service.GetRevenueByEquipment(from, to);
        }

        public async Task<IEnumerable<TopEquipmentResult>> GetTopSellingEquipment(
          int top,
          DateTimeOffset? from,
          DateTimeOffset? to,
             [Service] ISellingContractAnalyticsService service)
        {
            if (top <= 0)
                throw new ArgumentException("The 'top' parameter must be greater than zero.", nameof(top));

            return await service.GetTopSellingEquipment(top, from, to);
        }

        public async Task<IDictionary<Guid, int>> GetSalesCountByEquipment(
          DateTimeOffset? from,
          DateTimeOffset? to,
             [Service] ISellingContractAnalyticsService service)
        {
            return await service.GetSalesCountByEquipment(from, to);
        }

        public async Task<int> GetDeletedContractsCount(
             [Service] ISellingContractAnalyticsService service)
        {
            return await service.GetDeletedContractsCount();
        }

        public async Task<decimal> GetAverageSalePricePerEquipment(
          Guid equipmentId,
             [Service] ISellingContractAnalyticsService service)
        {
            return await service.GetAverageSalePricePerEquipment(equipmentId);
        }
    }
}
