﻿using EquipmentAPI.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquipmentAPI.Entities
{
    public class Equipment
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(6)]
        public string InternalSerial { get; set; } = string.Empty;

        public EquipmentBrand Brand { get; set; }
         
        public EquipmentType EquipmentType { get; set; }

        public Guid? SupplierId { get; set; }
        
        public Supplier? Supplier { get; set; }

        public Guid? TechnicalInformationId { get; set; }
        public TechnicalInformation? TechnicalInformation { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price {  get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Expenses { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ShippingPrice { get; set; } = 0;

        [NotMapped]
        public decimal TotalPrice => Price + Expenses + ShippingPrice;

        public DateTime ManufactureDate { get; set; }

        public DateTime PurchaseDate { get; set; }

        public DateTimeOffset AddedDate { get; set; } = DateTimeOffset.Now;

        public DateTimeOffset? DeletedDate { get; set; }

        public DateTimeOffset? UpdateDate { get; set; }
    }
}
