﻿using EquipmentAPI.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquipmentAPI.Entities
{
    public class Equipment : BasicEntity
    {

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(12)]
        public string InternalSerial { get; set; } = string.Empty;

        public EquipmentBrand EquipmentBrand { get; set; }

        public EquipmentType EquipmentType { get; set; }

        public Guid? SupplierId { get; set; }

        public Supplier? Supplier { get; set; }

        public Guid? TechnicalInformationId { get; set; }
        public TechnicalInformation? TechnicalInformation { get; set; }

        public List<MaintenanceRecord>? MaintenanceRecords { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Expenses { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ShippingPrice { get; set; } = 0;

        [NotMapped]
        public decimal TotalPrice => Price + Expenses + ShippingPrice;

        [Range(1900, 9999, ErrorMessage = "Year of manufacture must be after 1900.")]
        public int ManufactureDate { get; set; }

        [Required]
        public EquipmentStatus EquipmentStatus { get; set; } = EquipmentStatus.Available;

        public DateTime PurchaseDate { get; set; }
    }
}
