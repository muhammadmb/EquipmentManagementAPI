using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public abstract class PhoneNumber : BasicEntity
    {
        [Required]
        [Phone]
        public string Number { get; set; } = string.Empty;
    }
}
