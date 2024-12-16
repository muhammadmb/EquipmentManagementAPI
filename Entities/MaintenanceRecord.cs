using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquipmentAPI.Entities
{
    public class MaintenanceRecord
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid EquipmentId { get; set; }
        public Equipment? Equipment { get; set; }
        
        public DateTime MaintenanceDate { get; set; }
        
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5)")]
        public decimal Cost { get; set; }
        
        public string Technician { get; set; } = string.Empty;

        public DateTimeOffset AddedDate { get; set; } = DateTimeOffset.Now;

        public DateTimeOffset? DeletedDate { get; set; }

        public DateTimeOffset? UpdateDate { get; set; }
    }
}
