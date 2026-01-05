namespace Application.ResourceParameters
{
    public class SellingContractResourceParameters : ResourceParameters
    {
        public Guid? CustomerId { get; set; }
        public Guid? EquipmentId { get; set; }

        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }

        public int? Year { get; set; }

        public int? Month { get; set; }
    }
}
