using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class MaintenanceRecord : BasicEntity
    {
        [Required]
        public Guid EquipmentId { get; set; }
        public Equipment? Equipment { get; set; }
        
        public DateTime MaintenanceDate { get; set; }
        
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5)")]
        public decimal Cost { get; set; }
        
        public string Technician { get; set; } = string.Empty;
    }
}
