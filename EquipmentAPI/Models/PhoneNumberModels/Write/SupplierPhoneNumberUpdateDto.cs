using System.ComponentModel.DataAnnotations;

namespace EquipmentAPI.Models.PhoneNumberModels.Write
{
    public class SupplierPhoneNumberUpdateDto
    {
        [Required(ErrorMessage = "Phone number ID is required.")]
        public Guid? Id { get; set; }
        public string Number { get; set; } = string.Empty;

        [Required]
        public byte[] RowVersion { get; set; }
    }
}
