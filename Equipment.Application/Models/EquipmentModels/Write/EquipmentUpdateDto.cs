using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.Models.EquipmentModels.Write
{
    public class EquipmentUpdateDto
    {
        public string Name { get; set; } = string.Empty;

        public string InternalSerial { get; set; } = string.Empty;

        public EquipmentBrand EquipmentBrand { get; set; }

        public EquipmentType EquipmentType { get; set; }

        public Guid? SupplierId { get; set; }

        public Guid? TechnicalInformationId { get; set; }

        public decimal Price { get; set; }

        public decimal Expenses { get; set; }

        public decimal ShippingPrice { get; set; } = 0;

        public int ManufactureDate { get; set; }

        public EquipmentStatus EquipmentStatus { get; set; } = EquipmentStatus.Available;

        public DateTime PurchaseDate { get; set; }
        
        [Required]
        public byte[] RowVersion { get; set; }
    }
}
