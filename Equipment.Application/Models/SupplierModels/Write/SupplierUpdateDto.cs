using Application.Models.PhoneNumberModels.Write;
using System.ComponentModel.DataAnnotations;

namespace Application.Models.SupplierModels.Write
{
    public class SupplierUpdateDto
    {
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        public string ContactPerson { get; set; } = string.Empty;

        public List<SupplierPhoneNumberUpdateDto> PhoneNumbers { get; set; } = new();

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(50)]
        public string Country { get; set; } = string.Empty;

        [StringLength(50)]
        public string City { get; set; } = string.Empty;

        [Required]
        public byte[] RowVersion { get; set; }
    }
}
