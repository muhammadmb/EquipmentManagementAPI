namespace Application.Models.RentalContractModels.Write
{
    public class RentalContractUpdateDto
    {
        public Guid EquipmentId { get; set; }

        public Guid CustomerId { get; set; }

        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset EndDate { get; set; }

        public int Shifts { get; set; }

        public decimal ShiftPrice { get; set; }

        public decimal RentalPrice => Shifts * ShiftPrice;

        public byte[] RowVersion { get; set; }
    }
}
