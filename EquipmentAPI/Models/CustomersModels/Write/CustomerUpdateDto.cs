using EquipmentAPI.Entities;
using System.ComponentModel.DataAnnotations;

namespace EquipmentAPI.Models.CustomersModels.Write
{
    public class CustomerUpdateDto
    {
        public string Name { get; set; } = string.Empty;

        public List<CustomerPhoneNumber> PhoneNumbers { get; set; } = new();

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(50)]
        public string Country { get; set; } = string.Empty;

        [StringLength(50)]
        public string City { get; set; } = string.Empty;

        public byte[] RowVersion { get; set; }
    }
}
