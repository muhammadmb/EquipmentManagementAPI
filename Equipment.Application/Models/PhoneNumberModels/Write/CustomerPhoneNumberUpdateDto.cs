using System.ComponentModel.DataAnnotations;

namespace Application.Models.PhoneNumberModels.Write
{
    public class CustomerPhoneNumberUpdateDto
    {
        public string Number { get; set; } = string.Empty;

        [Required]
        public byte[] RowVersion { get; set; }
    }
}
