namespace API.GraphQL.RentalContract.Inputs
{
    public class RentalContractFilterInput
    {
        public int? PageSize { get; set; }
        public int? PageNumber { get; set; }

        public string? SearchQuery { get; set; }

        public string? SortBy { get; set; }
        public bool? SortDescending { get; set; } = false;

        public Guid? CustomerId { get; set; }
        public Guid? EquipmentId { get; set; }

        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }

        public int? Year { get; set; }
        public int? Month { get; set; }

        public int? MinShifts { get; set; }
        public int? MaxShifts { get; set; }

        public decimal? MinShiftPrice { get; set; }
        public decimal? MaxShiftPrice { get; set; }

        public decimal? MinContractPrice { get; set; }
        public decimal? MaxContractPrice { get; set; }
    }
}
