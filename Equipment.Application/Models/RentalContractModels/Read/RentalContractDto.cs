using Domain.Entities;

namespace Application.Models.RentalContractModels.Read
{
    public class RentalContractDto
    {
        public Guid Id { get; set; }
        public Guid EquipmentId { get; set; }
        public Equipment? Equipment { get; set; }

        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset EndDate { get; set; }

        public int Shifts { get; set; }

        public decimal ShiftPrice { get; set; }

        public decimal RentalPrice => Shifts * ShiftPrice;

        public DateTimeOffset AddedDate { get; set; } = DateTimeOffset.Now;

        public DateTimeOffset? DeletedDate { get; set; }

        public DateTimeOffset? UpdateDate { get; set; }

        public byte[] RowVersion { get; set; }
    }
}
