using System.ComponentModel.DataAnnotations;

namespace EquipmentAPI.Models.PhoneNumberModels.Write
{
    public class CustomerPhoneNumberCreateDto
    {
        [Phone]
        public string Number { get; set; } = string.Empty;
    }
}
