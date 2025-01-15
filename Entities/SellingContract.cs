using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquipmentAPI.Entities
{
    public class SellingContract : BasicEntity
    {
        [Required]
        public Guid EquipmentId { get; set; }
        public Equipment? Equipment { get; set; }

        [Required]
        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SalePrice { get; set; }

        public DateTimeOffset SaleDate { get; set; }
    }
}
