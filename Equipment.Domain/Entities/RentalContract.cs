using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class RentalContract : BasicEntity
    {
        [Required]
        public Guid EquipmentId { get; set; }
        public Equipment? Equipment { get; set; }

        [Required]
        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }

        [Required]
        public DateTimeOffset StartDate { get; set; }

        [Required]
        public DateTimeOffset EndDate { get; set; }
        public DateTimeOffset? CancelledDate { get; private set; } = null;
        public DateTimeOffset? FinishedDate { get; private set; } = null;
        public DateTimeOffset? SuspendedDate { get; private set; } = null;

        public RentalContractStatus Status { get; private set; }

        [Required]
        [Range(0, 1000)]
        public int Shifts { get; set; }

        [Column(TypeName = "decimal(7,2)")]
        public decimal ShiftPrice { get; set; }

        [NotMapped]
        public decimal RentalPrice => Shifts * ShiftPrice;

        public void Activate()
        {
            if (Status != RentalContractStatus.Draft)
                throw new InvalidOperationException("Only draft contracts can be activated.");

            Status = RentalContractStatus.Active;
        }

        public void Cancel()
        {
            if (Status == RentalContractStatus.Finished)
                throw new InvalidOperationException("Finished contracts cannot be cancelled.");

            if (Status == RentalContractStatus.Cancelled)
                return;

            Status = RentalContractStatus.Cancelled;
            CancelledDate = DateTimeOffset.UtcNow;
        }

        public void Suspend()
        {
            if (Status != RentalContractStatus.Active)
                throw new InvalidOperationException("Only active contracts can be suspended.");

            Status = RentalContractStatus.Suspended;
            SuspendedDate = DateTimeOffset.UtcNow;
        }

        public void Resume()
        {
            if (Status != RentalContractStatus.Suspended)
                throw new InvalidOperationException("Only suspended contracts can be resumed.");

            Status = RentalContractStatus.Active;
        }

        public void Finish()
        {
            if (Status == RentalContractStatus.Finished)
                return;

            if (Status != RentalContractStatus.Active &&
                Status != RentalContractStatus.Suspended)
                throw new InvalidOperationException("Only active or suspended contracts can be finished.");

            Status = RentalContractStatus.Finished;
            FinishedDate = DateTimeOffset.UtcNow;
        }

        public bool IsActive()
            => Status == RentalContractStatus.Active;

        public bool IsFinished()
            => Status == RentalContractStatus.Finished;

        public bool IsCancelled()
            => Status == RentalContractStatus.Cancelled;
    }
}
