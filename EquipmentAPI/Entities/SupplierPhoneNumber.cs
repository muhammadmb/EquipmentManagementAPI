using System.ComponentModel.DataAnnotations;

namespace EquipmentAPI.Entities
{
    public class SupplierPhoneNumber : PhoneNumber
    {
        [Required]
        public Guid SupplierId { get; set; }

        public Supplier Supplier { get; set; }

    }
}
