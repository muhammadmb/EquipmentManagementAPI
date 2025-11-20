using System.ComponentModel.DataAnnotations;

namespace EquipmentAPI.Models.PhoneNumberModels.Write
{
    public class SupplierPhoneNumberCreateDto
    {
        [Phone]
        public string Number { get; set; } = string.Empty;
    }
}
