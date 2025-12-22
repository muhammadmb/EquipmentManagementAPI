using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Supplier : BasicEntity
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string ContactPerson { get; set; } = string.Empty;

        [Required]
        [Phone]
        public List<SupplierPhoneNumber> PhoneNumbers { get; set; } = new();

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Country { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string City { get; set; } = string.Empty;

        [NotMapped]
        public string Address => $"{City}, {Country}";
    }
}