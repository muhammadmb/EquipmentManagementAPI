using Domain.Enums;

namespace Application.Models.EquipmentModels.Read
{
    public class EquipmentForContractsDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string InternalSerial { get; set; }

        public EquipmentBrand EquipmentBrand { get; set; }

        public EquipmentType EquipmentType { get; set; }

        public Guid? SupplierId { get; set; }

        public decimal Price { get; set; }

        public decimal Expenses { get; set; }

        public decimal ShippingPrice { get; set; }

        public decimal TotalPrice { get; set; }

        public int ManufactureDate { get; set; }

        public EquipmentStatus EquipmentStatus { get; set; }
    }
}
