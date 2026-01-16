namespace API.GraphQL.SellingContract.Inputs
{
    public class SellingContractFilterInput
    {
        public int? PageSize = 10;

        public int? pageNumber = 1;

        public string? SearchQuery = "";

        public string? SortBy = "";

        public bool? SortDescending { get; set; } = false;

        public Guid? CustomerId { get; set; }

        public Guid? EquipmentId { get; set; }

        public DateTimeOffset? FromDate { get; set; }

        public DateTimeOffset? ToDate { get; set; }

        public int? Year { get; set; }

        public int? Month { get; set; }
    }
}
