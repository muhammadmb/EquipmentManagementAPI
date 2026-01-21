using Domain.Enums;

namespace Application.ResourceParameters
{
    public class EquipmentResourceParameters : ResourceParameters
    {
        private string _SortBy = "name";

        public new string SortBy
        {
            get => _SortBy;
            set => _SortBy = value?.Trim().ToLowerInvariant();
        }

        public EquipmentBrand? EquipmentBrand { get; set; }
        public EquipmentType? EquipmentType { get; set; }
        public EquipmentStatus? EquipmentStatus { get; set; }

        public bool? IsAvailable { get; set; }

        public DateTimeOffset? PurchaseDateFrom { get; set; }
        public DateTimeOffset? PurchaseDateTo { get; set; }

        public int? YearOfManufacturers { get; set; }
    }
}
