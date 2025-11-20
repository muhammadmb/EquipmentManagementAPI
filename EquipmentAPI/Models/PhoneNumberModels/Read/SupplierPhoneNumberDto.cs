namespace EquipmentAPI.Models.PhoneNumberModels.Read
{
    public class SupplierPhoneNumberDto
    {
        public Guid Id { get; set; }

        public Guid SupplierId { get; set; }

        public string Number { get; set; } = string.Empty;

        public DateTimeOffset AddedDate { get; set; } = DateTimeOffset.Now;

        public DateTimeOffset? UpdateDate { get; set; }

        public byte[] RowVersion { get; set; }
    }
}
