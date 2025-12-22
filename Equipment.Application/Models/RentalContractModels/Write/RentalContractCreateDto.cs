using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Application.Models.RentalContractModels.Write
{
    public class RentalContractCreateDto
    {
        [Required]
        public Guid EquipmentId { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public DateTimeOffset StartDate { get; set; }

        [Required]
        public DateTimeOffset EndDate { get; set; }

        [Required]
        [Range(0, 1000)]
        public int Shifts { get; set; }

        [Column(TypeName = "decimal(7,2)")]
        public decimal ShiftPrice { get; set; }
    }
}
