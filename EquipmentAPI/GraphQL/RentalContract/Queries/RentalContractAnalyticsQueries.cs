using Application.Interface.Services;
using Application.Models.RentalContractModels.Read;

namespace API.GraphQL.RentalContract.Queries
{
    [ExtendObjectType(Name = "Query")]
    public class RentalContractAnalyticsQueries
    {
        public async Task<int> GetRentalContractCount(
            [Service] IRentalContractAnalyticsService service)
        {
            var rentalContractCount = await service.GetRentalContractCount();
            return rentalContractCount;
        }

        public async Task<int> GetTotalActiveCount(
            [Service] IRentalContractAnalyticsService service)
        {
            var activeRentalContractCount = await service.GetTotalActiveCount();
            return activeRentalContractCount;
        }

        public async Task<int> GetTotalContractsForCustomer(
            Guid customerId,
            [Service] IRentalContractAnalyticsService service)
        {
            var totalContractsForCustomer = await service.GetTotalContractsForCustomer(customerId);
            return totalContractsForCustomer;
        }

        public async Task<int> GetTotalContractsForEquipment(
            Guid equipmentId,
            [Service] IRentalContractAnalyticsService service
            )
        {
            var totalContractsForEquipment = await service.GetTotalContractsForEquipment(equipmentId);
            return totalContractsForEquipment;
        }

        public async Task<IEnumerable<EquipmentContractSummaryDto>> GetEquipmentContractSummary(
            [Service] IRentalContractAnalyticsService service)
        {
            var equipmentContractSummary = await service.GetEquipmentContractSummary();
            return equipmentContractSummary;
        }

        public async Task<decimal> GetTotalRevenue(
            DateTime from,
            DateTime to,
            [Service] IRentalContractAnalyticsService service)
        {
            var totalRevenue = await service.GetTotalRevenue(from, to);
            return totalRevenue;
        }

        public async Task<IDictionary<string, decimal>> GetRevenueByCustomer(
            DateTime? from,
            DateTime? to,
            [Service] IRentalContractAnalyticsService service)
        {
            var revenueByCustomer = await service.GetRevenueByCustomer(from, to);
            return revenueByCustomer;
        }

        public async Task<IDictionary<int, decimal>> GetRevenueByMonth(
            int year,
            [Service] IRentalContractAnalyticsService service)
        {
            var revenueByMonth = await service.GetRevenueByMonth(year);
            return revenueByMonth;
        }

        public async Task<ContractPriceStatisticsDto> GetContractPriceStatistics(
            [Service] IRentalContractAnalyticsService service)
        {
            var priceStatistics = await service.GetContractPriceStatistics();
            return priceStatistics;
        }

        public async Task<int> GetFinishedContractsCount(
            DateTime? from,
            DateTime? to,
            [Service] IRentalContractAnalyticsService service)
        {
            var finishedContractsCount = await service.GetFinishedContractsCount(from, to);
            return finishedContractsCount;
        }

        public async Task<int> GetCancelledContractsCount(
            DateTime? from,
            DateTime? to,
            [Service] IRentalContractAnalyticsService service)
        {
            var cancelledContractsCount = await service.GetCancelledContractsCount(from, to);
            return cancelledContractsCount;
        }

        public async Task<double> GetAverageContractDurationInDays(
            [Service] IRentalContractAnalyticsService service)
        {
            var averageDuration = await service.GetAverageContractDurationInDays();
            return averageDuration;
        }

        public async Task<decimal> GetAverageRevenuePerCustomer(
            [Service] IRentalContractAnalyticsService service)
        {
            var averageRevenuePerCustomer = await service.GetAverageRevenuePerCustomer();
            return averageRevenuePerCustomer;
        }

        public async Task<decimal> GetAverageRentalPrice(
            [Service] IRentalContractAnalyticsService service)
        {
            var averageRentalPrice = await service.GetAverageRentalPrice();
            return averageRentalPrice;
        }
    }
}
