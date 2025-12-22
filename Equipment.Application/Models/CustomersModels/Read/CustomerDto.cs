using Application.Models.PhoneNumberModels.Read;

namespace Application.Models.CustomersModels.Read
{
    public class CustomerDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public List<CustomerPhoneNumberDto> PhoneNumbers { get; set; } = new();

        public string Email { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string Address => $"{City}, {Country}";

        public DateTimeOffset AddedDate { get; set; } = DateTimeOffset.Now;

        public DateTimeOffset? DeletedDate { get; set; }

        public DateTimeOffset? UpdateDate { get; set; }

        public byte[] RowVersion { get; set; }
    }
}
