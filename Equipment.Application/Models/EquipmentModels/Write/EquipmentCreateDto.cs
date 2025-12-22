using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Application.Models.EquipmentModels.Write
{
    public class EquipmentCreateDto
    {
        [Required]
        [StringLength(50)]
        [NotNull]
        public string Name { get; set; }

        [Required]
        [StringLength(12)]
        public string InternalSerial { get; set; } = string.Empty;

        public EquipmentBrand EquipmentBrand { get; set; }

        public EquipmentType EquipmentType { get; set; }

        public Guid? SupplierId { get; set; }

        public Guid? TechnicalInformationId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Expenses { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ShippingPrice { get; set; } = 0;

        [Range(1900, 9999, ErrorMessage = "Year of manufacture must be after 1900.")]
        public int ManufactureDate { get; set; }

        [Required]
        public EquipmentStatus EquipmentStatus { get; set; } = EquipmentStatus.Available;

        public DateTime PurchaseDate { get; set; }
    }
}
