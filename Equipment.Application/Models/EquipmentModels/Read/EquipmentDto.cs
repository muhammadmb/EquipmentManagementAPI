using Application.Models.MaintenanceRecordModels.Read;
using Application.Models.SupplierModels.Read;
using Application.Models.TechnicalInformationModels.Read;
using Domain.Enums;

namespace Application.Models.EquipmentModels.Read
{
    public class EquipmentDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string InternalSerial { get; set; }

        public EquipmentBrand EquipmentBrand { get; set; }

        public EquipmentType EquipmentType { get; set; }

        public Guid? SupplierId { get; set; }

        public SupplierForEquipmentDto? Supplier { get; set; }

        public Guid? TechnicalInformationId { get; set; }

        public TechnicalInformationForEquipmentDto? TechnicalInformation { get; set; }

        public List<MaintenanceRecordForEquipmentDto>? MaintenanceRecords { get; set; }

        public decimal Price { get; set; }

        public decimal Expenses { get; set; }

        public decimal ShippingPrice { get; set; }

        public decimal TotalPrice { get; set; }

        public int ManufactureDate { get; set; }

        public EquipmentStatus EquipmentStatus { get; set; }

        public DateTime PurchaseDate { get; set; }

        public byte[] RowVersion { get; set; }

        public DateTimeOffset AddedDate { get; set; }

        public DateTimeOffset? DeletedDate { get; set; }

        public DateTimeOffset? UpdateDate { get; set; }
    }
}
