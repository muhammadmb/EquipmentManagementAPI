using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class BasicEntity
    {
        [Key]
        public Guid Id { get; set; }
        public DateTimeOffset AddedDate { get; set; } = DateTimeOffset.Now;

        public DateTimeOffset? DeletedDate { get; set; }

        [NotMapped]
        public bool IsDeleted => DeletedDate.HasValue;

        public DateTimeOffset? UpdateDate { get; set; }

        public byte[] RowVersion { get; set; }

        public virtual void SoftDelete(Guid? userId = null)
        {
            if (IsDeleted)
                return;

            DeletedDate = DateTimeOffset.UtcNow;
        }

        public virtual void Restore(Guid? userId = null)
        {
            if (!IsDeleted)
                return;

            DeletedDate = null;
        }
    }
}
