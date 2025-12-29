namespace Application.Models.RentalContractModels.Read
{
    public class ContractPriceStatisticsDto
    {
        public decimal AveragePrice { get; set; }
        public decimal MedianPrice { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
