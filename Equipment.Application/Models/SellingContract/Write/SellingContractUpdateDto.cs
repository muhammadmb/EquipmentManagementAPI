using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Application.Models.SellingContract.Write
{
    public class SellingContractUpdateDto
    {
        [Required]
        public Guid EquipmentId { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SalePrice { get; set; }

        public DateTimeOffset SaleDate { get; set; }

        [Required]
        public byte[] RowVersion { get; set; }
    }
}
