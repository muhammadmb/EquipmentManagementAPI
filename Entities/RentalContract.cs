using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquipmentAPI.Entities
{
    public class RentalContract
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public Guid EquipmentId { get; set; }
        public Equipment? Equipment { get; set; }
        
        [Required]
        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }
        
        [Required]
        public DateTimeOffset StartDate { get; set; }
        
        [Required]
        public DateTimeOffset EndDate { get; set; }
        
        [Required]
        [Range(0, 1000)]
        public int Shifts{ get; set; }

        [Column(TypeName = "decimal(7,2)")]
        public decimal ShiftPrice {  get; set; }

        [NotMapped]
        public decimal RentalPrice => Shifts * ShiftPrice;

        public DateTimeOffset AddedDate { get; set; } = DateTimeOffset.Now;

        public DateTimeOffset? DeletedDate { get; set; }

        public DateTimeOffset? UpdateDate { get; set; }
    }
}
