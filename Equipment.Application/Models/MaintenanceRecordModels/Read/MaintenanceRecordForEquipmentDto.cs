namespace Application.Models.MaintenanceRecordModels.Read
{
    public class MaintenanceRecordForEquipmentDto
    {
        public Guid Id { get; set; }

        public Guid EquipmentId { get; set; }

        public DateTime MaintenanceDate { get; set; }

        public string Description { get; set; } = string.Empty;

        public decimal Cost { get; set; }

        public string Technician { get; set; } = string.Empty;

        public DateTimeOffset AddedDate { get; set; } = DateTimeOffset.Now;

        public DateTimeOffset? UpdateDate { get; set; }
    }
}
