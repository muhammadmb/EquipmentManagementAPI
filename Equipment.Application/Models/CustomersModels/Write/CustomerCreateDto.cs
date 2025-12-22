using Application.Models.PhoneNumberModels.Write;
using System.ComponentModel.DataAnnotations;

namespace Application.Models.CustomersModels.Write
{
    public class CustomerCreateDto
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public List<CustomerPhoneNumberCreateDto> PhoneNumbers { get; set; } = new();

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Country { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string City { get; set; } = string.Empty;
    }
}
