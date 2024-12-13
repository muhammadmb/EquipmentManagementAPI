using System.ComponentModel.DataAnnotations;

namespace EquipmentAPI.Entities
{
    public class Supplier
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string ContactPerson { get; set; } = string.Empty ;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Address { get; set; } = string.Empty;

        public DateTimeOffset AddedDate { get; set; } = DateTimeOffset.Now;

        public DateTimeOffset? DeletedDate { get; set; }

        public DateTimeOffset? UpdateDate { get; set; }
    }
}