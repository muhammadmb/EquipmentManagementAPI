using System.ComponentModel.DataAnnotations;

namespace EquipmentAPI.Entities
{
    public abstract class PhoneNumber
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [Phone]
        public string Number { get; set; } = string.Empty;

        public DateTimeOffset AddedDate { get; set; } = DateTimeOffset.Now;

        public DateTimeOffset? DeletedDate { get; set; }

        public DateTimeOffset? UpdateDate { get; set; }
    }
}
