namespace Application.Models.SellingContract.Read
{
    public class TopCustomerResult
    {
        public Guid CustomerId { get; set; }
        public decimal TotalRevenue { get; set; }
        public int SalesCount { get; set; }
    }
}
