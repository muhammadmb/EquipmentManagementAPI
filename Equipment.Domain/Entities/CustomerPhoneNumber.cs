using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class CustomerPhoneNumber : PhoneNumber
    {
        [Required]
        public Guid CustomerId { get; set; }

        public Customer Customer { get; set; }

    }
}
