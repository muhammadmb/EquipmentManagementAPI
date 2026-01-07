using Domain.Entities;

namespace Application.Models.SellingContract.Read
{
    public class SellingContractDto
    {
        public Guid Id { get; set; }
        public Guid EquipmentId { get; set; }
        public Equipment Equipment { get; set; }
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; }
        public decimal SalePrice { get; set; }
        public DateTimeOffset SaleDate { get; set; }
        public DateTimeOffset AddedDate { get; set; }
        public DateTimeOffset? DeletedDate { get; set; }
        public DateTimeOffset? UpdateDate { get; set; }
        public byte[] RowVersion { get; set; }
    }
}
