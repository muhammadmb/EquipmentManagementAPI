using Application.Models.CustomersModels.Read;
using Application.Models.EquipmentModels.Read;
using Domain.Enums;

namespace Application.Models.RentalContractModels.Read
{
    public class RentalContractDto
    {
        public Guid Id { get; set; }
        public Guid EquipmentId { get; set; }
        public EquipmentForContractsDto? Equipment { get; set; }

        public Guid CustomerId { get; set; }
        public CustomerForContractDto? Customer { get; set; }

        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset EndDate { get; set; }
        public DateTimeOffset? CancelledDate { get; private set; } = null;
        public DateTimeOffset? FinishedDate { get; private set; } = null;
        public DateTimeOffset? SuspendedDate { get; private set; } = null;

        public RentalContractStatus Status { get; private set; }
        public int Shifts { get; set; }

        public decimal ShiftPrice { get; set; }

        public decimal RentalPrice => Shifts * ShiftPrice;

        public DateTimeOffset AddedDate { get; set; } = DateTimeOffset.Now;

        public DateTimeOffset? DeletedDate { get; set; }

        public DateTimeOffset? UpdateDate { get; set; }

        public byte[] RowVersion { get; set; }
    }
}
