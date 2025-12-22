using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public DbSet<Equipment> Equipments { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<Supplier> Suppliers { get; set; }

        public DbSet<TechnicalInformation> TechnicalInformation { get; set; }

        public DbSet<RentalContract> RentalContracts { get; set; }

        public DbSet<SellingContract> Sellings { get; set; }

        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }

        public DbSet<CustomerPhoneNumber> CustomerPhoneNumbers { get; set; }
        public DbSet<SupplierPhoneNumber> SupplierPhoneNumbers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Equipment>().HasQueryFilter(e => e.DeletedDate == null);
            modelBuilder.Entity<Customer>().HasQueryFilter(c => c.DeletedDate == null);
            modelBuilder.Entity<Supplier>().HasQueryFilter(s => s.DeletedDate == null);
            modelBuilder.Entity<TechnicalInformation>().HasQueryFilter(t => t.DeletedDate == null);
            modelBuilder.Entity<RentalContract>().HasQueryFilter(r => r.DeletedDate == null);
            modelBuilder.Entity<SellingContract>().HasQueryFilter(se => se.DeletedDate == null);
            modelBuilder.Entity<MaintenanceRecord>().HasQueryFilter(ma => ma.DeletedDate == null);
            modelBuilder.Entity<CustomerPhoneNumber>().HasQueryFilter(cpn => cpn.DeletedDate == null);
            modelBuilder.Entity<SupplierPhoneNumber>().HasQueryFilter(spn => spn.DeletedDate == null);

            modelBuilder.Entity<Equipment>()
                .HasMany<MaintenanceRecord>(e => e.MaintenanceRecords)
                .WithOne(m => m.Equipment)
                .HasForeignKey(m => m.EquipmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Equipment>()
               .HasOne<TechnicalInformation>(e => e.TechnicalInformation)
               .WithOne(t => t.Equipment)
               .HasForeignKey<TechnicalInformation>(r => r.EquipmentId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Equipment>()
            .Property(e => e.EquipmentType)
            .HasConversion(
                v => v.ToString(),
                v => (EquipmentType)Enum.Parse(typeof(EquipmentType), v)
            );

            modelBuilder.Entity<Equipment>()
            .Property(e => e.EquipmentBrand)
            .HasConversion(
                v => v.ToString(),
                v => (EquipmentBrand)Enum.Parse(typeof(EquipmentBrand), v)
            );

            modelBuilder.Entity<Equipment>()
            .Property(e => e.EquipmentStatus)
            .HasConversion(
                v => v.ToString(),
                v => (EquipmentStatus)Enum.Parse(typeof(EquipmentStatus), v)
            );

            modelBuilder.Entity<Equipment>()
            .Property(e => e.RowVersion)
            .IsRowVersion();

            modelBuilder.Entity<TechnicalInformation>()
            .Property(t => t.RowVersion)
            .IsRowVersion();

            modelBuilder.Entity<TechnicalInformation>()
            .Property(e => e.EnginePowerUnit)
            .HasConversion(
                v => v.ToString(),
                v => (PowerUnit)Enum.Parse(typeof(PowerUnit), v)
            );

            modelBuilder.Entity<TechnicalInformation>()
            .Property(e => e.WeightUnit)
            .HasConversion(
                v => v.ToString(),
                v => (WeightUnit)Enum.Parse(typeof(WeightUnit), v)
            );

            modelBuilder.Entity<TechnicalInformation>()
            .Property(e => e.FuelCapacityUnit)
            .HasConversion(
                v => v.ToString(),
                v => (FuelCapacityUnit)Enum.Parse(typeof(FuelCapacityUnit), v)
            );

            modelBuilder.Entity<TechnicalInformation>()
            .Property(e => e.DimensionUnit)
            .HasConversion(
                v => v.ToString(),
                v => (DimensionUnit)Enum.Parse(typeof(DimensionUnit), v)
            );

            modelBuilder.Entity<TechnicalInformation>()
            .Property(e => e.EngineType)
            .HasConversion(
                v => v.ToString(),
                v => (EngineType)Enum.Parse(typeof(EngineType), v)
            );

            modelBuilder.Entity<TechnicalInformation>()
            .Property(e => e.SpeedUnit)
            .HasConversion(
                v => v.ToString(),
                v => (SpeedUnit)Enum.Parse(typeof(SpeedUnit), v)
            );

            modelBuilder.Entity<CustomerPhoneNumber>()
                .HasIndex(s => s.Number)
                .IsUnique();

            modelBuilder.Entity<CustomerPhoneNumber>()
            .Property(cpn => cpn.RowVersion)
            .IsRowVersion();

            modelBuilder.Entity<CustomerPhoneNumber>()
                .HasOne(cpn => cpn.Customer)
                .WithMany(c => c.PhoneNumbers)
                .HasForeignKey(cpn => cpn.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SupplierPhoneNumber>()
                .HasIndex(s => s.Number)
                .IsUnique();

            modelBuilder.Entity<SupplierPhoneNumber>()
                .HasOne(spn => spn.Supplier)
                .WithMany(s => s.PhoneNumbers)
                .HasForeignKey(spn => spn.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SupplierPhoneNumber>()
            .Property(spn => spn.RowVersion)
            .IsRowVersion();

            modelBuilder.Entity<Supplier>()
            .Property(s => s.RowVersion)
            .IsRowVersion();

            modelBuilder.Entity<Supplier>()
                .HasMany(s => s.PhoneNumbers)
                .WithOne(p => p.Supplier)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Customer>()
            .Property(c => c.RowVersion)
            .IsRowVersion();

            modelBuilder.Entity<Customer>()
                .HasMany(s => s.PhoneNumbers)
                .WithOne(p => p.Customer)
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MaintenanceRecord>()
            .Property(mr => mr.RowVersion)
            .IsRowVersion();

            modelBuilder.Entity<SellingContract>()
            .Property(sc => sc.RowVersion)
            .IsRowVersion();

            modelBuilder.Entity<RentalContract>()
            .Property(rc => rc.RowVersion)
            .IsRowVersion();
        }
    }
}
