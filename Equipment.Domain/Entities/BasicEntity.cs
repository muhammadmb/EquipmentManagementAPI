using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class BasicEntity
    {
        [Key]
        public Guid Id { get; set; }
        public DateTimeOffset AddedDate { get; set; } = DateTimeOffset.Now;

        public DateTimeOffset? DeletedDate { get; set; }

        public DateTimeOffset? UpdateDate { get; set; }

        public byte[] RowVersion { get; set; }
    }
}
