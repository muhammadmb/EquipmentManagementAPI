using Application.Models.RentalContractModels.Read;

namespace Application.Interface.Services
{
    public interface IRentalContractAnalyticsService
    {
        Task<int> GetRentalContractCount();
        Task<int> GetTotalActiveCount();
        Task<int> GetTotalContractsForCustomer(Guid customerId);
        Task<int> GetTotalContractsForEquipment(Guid equipmentId);
        Task<IEnumerable<EquipmentContractSummaryDto>> GetEquipmentContractSummary();
        Task<decimal> GetTotalRevenue(DateTime? from, DateTime? to);
        Task<IDictionary<string, decimal>> GetRevenueByCustomer(DateTime? from, DateTime? to);
        Task<IDictionary<int, decimal>> GetRevenueByMonth(int year);
        Task<ContractPriceStatisticsDto> GetContractPriceStatistics();
        Task<int> GetFinishedContractsCount(DateTime? from, DateTime? to);
        Task<int> GetCancelledContractsCount(DateTime? from, DateTime? to);
        Task<double> GetAverageContractDurationInDays();
        Task<decimal> GetAverageRevenuePerCustomer();
        Task<decimal> GetAverageRentalPrice();
    }
}
