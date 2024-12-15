using EquipmentAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace EquipmentAPI.Contexts
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
        
        public DbSet<MaintenanceRecord> maintenanceRecords{ get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Equipment>().HasQueryFilter(e => e.DeletedDate == null);
            modelBuilder.Entity<Customer>().HasQueryFilter(c => c.DeletedDate == null);
            modelBuilder.Entity<Supplier>().HasQueryFilter(s => s.DeletedDate == null);
            modelBuilder.Entity<TechnicalInformation>().HasQueryFilter(t => t.DeletedDate == null);
            modelBuilder.Entity<RentalContract>().HasQueryFilter(r => r.DeletedDate == null);
            modelBuilder.Entity<SellingContract>().HasQueryFilter(se => se.DeletedDate == null);
            modelBuilder.Entity<MaintenanceRecord>().HasQueryFilter(ma => ma.DeletedDate == null);

            //Dumy data for the Demo

            modelBuilder.Entity<Equipment>().HasData(
                new Equipment
                {
                    Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d401"),
                    Name = "Caterpillar Excavator",
                    InternalSerial = "CAT-EX-001",
                    Brand = Enums.EquipmentBrand.Caterpillar,
                    EquipmentType = Enums.EquipmentType.Excavator,
                    SupplierId = Guid.NewGuid(),
                    TechnicalInformationId = Guid.NewGuid(),
                    MaintenanceRecordId = Guid.NewGuid(),
                    Price = 100000m,
                    Expenses = 5000m,
                    ManufactureDate = 1998,
                    PurchaseDate = new DateTime(2020, 12, 11),
                    EquipmentStatus = Enums.EquipmentStatus.Available
                },
                new Equipment
                {
                    Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d402"),
                    Name = "Komatsu Bulldozer",
                    InternalSerial = "KOM-BD-002",
                    Brand = Enums.EquipmentBrand.Komatsu,
                    EquipmentType = Enums.EquipmentType.Bulldozer,
                    SupplierId = Guid.NewGuid(),
                    TechnicalInformationId = Guid.NewGuid(),
                    MaintenanceRecordId = Guid.NewGuid(),
                    Price = 120000m,
                    Expenses = 6000m,
                    ManufactureDate = 2002,
                    PurchaseDate = new DateTime(2022, 12, 11),
                    EquipmentStatus = Enums.EquipmentStatus.Available
                },
                new Equipment
                {
                    Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d403"),
                    Name = "Volvo Crane",
                    InternalSerial = "VOL-CR-003",
                    Brand = Enums.EquipmentBrand.Volvo,
                    EquipmentType = Enums.EquipmentType.Crane,
                    SupplierId = Guid.NewGuid(),
                    TechnicalInformationId = Guid.NewGuid(),
                    MaintenanceRecordId = Guid.NewGuid(),
                    Price = 150000m,
                    Expenses = 7000m,
                    ManufactureDate = 2005,
                    PurchaseDate = new DateTime(2023, 12, 11),
                    EquipmentStatus = Enums.EquipmentStatus.Available
                },
                new Equipment
                {
                    Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d404"),
                    Name = "John Deere Loader",
                    InternalSerial = "JD-LO-004",
                    Brand = Enums.EquipmentBrand.JohnDeere,
                    EquipmentType = Enums.EquipmentType.Loader,
                    SupplierId = Guid.NewGuid(),
                    TechnicalInformationId = Guid.NewGuid(),
                    MaintenanceRecordId = Guid.NewGuid(),
                    Price = 110000m,
                    Expenses = 5500m,
                    ManufactureDate = 2000,
                    PurchaseDate = new DateTime(2024, 12, 11),
                    EquipmentStatus = Enums.EquipmentStatus.Sold
                },
                new Equipment
                {
                    Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d405"),
                    Name = "Hitachi Grader",
                    InternalSerial = "HIT-GR-005",
                    Brand = Enums.EquipmentBrand.Hitachi,
                    EquipmentType = Enums.EquipmentType.Grader,
                    SupplierId = Guid.NewGuid(),
                    TechnicalInformationId = Guid.NewGuid(),
                    MaintenanceRecordId = Guid.NewGuid(),
                    Price = 130000m,
                    Expenses = 6500m,
                    ManufactureDate = 2008,
                    PurchaseDate = new DateTime(2017, 12, 11),
                    EquipmentStatus = Enums.EquipmentStatus.Rented
                }
            );

            modelBuilder.Entity<Customer>().HasData(
                new Customer { Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c11111"), Name = "ACME Construction", Email = "contact@acme.com", PhoneNumbers = ["123-456-7890"], Country = "Egypt", City = "Alexandria" },
                new Customer { Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c11112"), Name = "BuildCorp", Email = "info@buildcorp.com", PhoneNumbers = ["987-654-3210"], Country = "Egypt", City = "Cairo" },
                new Customer { Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c11113"), Name = "MegaMachines", Email = "sales@megamachines.com", PhoneNumbers = ["555-555-5555"], Country = "Italy", City = "Roma" },
                new Customer { Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c11114"), Name = "HeavyDuty Co.", Email = "support@heavyduty.com", PhoneNumbers = ["444-444-4444"], Country = "USA", City = "L.A" },
                new Customer { Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c11115"), Name = "ExcavatePro", Email = "hello@excavatepro.com", PhoneNumbers = ["333-333-3333", "333-555-888"], Country = "Egypt", City = "Matroh" }
            );

            modelBuilder.Entity<Supplier>().HasData(
                new Supplier
                {
                    Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479"),
                    Name = "Tech Supplies Co.",
                    ContactPerson = "John Doe",
                    PhoneNumbers = ["123-456-7890", "987-654-3210"],
                    Email = "contact@techsupplies.com",
                    Country = "USA",
                    City = "New York"
                },
                new Supplier
                {
                    Id = Guid.Parse("c9bf9e57-1685-4c89-bafb-ff5af830be8a"),
                    Name = "Green Earth Products",
                    ContactPerson = "Sarah Green",
                    PhoneNumbers = ["555-123-4567", "555-765-4321"],
                    Email = "info@greenearth.com",
                    Country = "Canada",
                    City = "Toronto"
                },
                new Supplier
                {
                    Id = Guid.Parse("3d6f0ba4-82b8-4ac2-89f3-c0ecbab858ba"),
                    Name = "Quality Tools Inc.",
                    ContactPerson = "Michael Smith",
                    PhoneNumbers = ["800-123-4567", "800-765-4321"],
                    Email = "support@qualitytools.com",
                    Country = "UK",
                    City = "London"
                },
                new Supplier
                {
                    Id = Guid.Parse("7b9e54a8-cf68-42c8-a895-21b63d4b9dbd"),
                    Name = "Eco Friendly Packaging",
                    ContactPerson = "Linda Adams",
                    PhoneNumbers = ["600-123-4567", "600-765-4321"],
                    Email = "sales@ecofriendly.com",
                    Country = "Germany",
                    City = "Berlin"
                },
                new Supplier
                {
                    Id = Guid.Parse("d2c67f3a-37e7-4b9e-a117-3f3c82d7dc1e"),
                    Name = "Smart Electronics",
                    ContactPerson = "David Johnson",
                    PhoneNumbers = ["700-123-4567", "700-765-4321"],
                    Email = "inquiries@smartelectronics.com",
                    Country = "Australia",
                    City = "Sydney"
                }
            );

            modelBuilder.Entity<MaintenanceRecord>().HasData(
                new MaintenanceRecord
                {
                    Id = Guid.Parse("a1b2c3d4-e5f6-7890-a1b2-c3d4e5f67890"),
                    EquipmentId = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d401"),
                    MaintenanceDate = new DateTime(2024, 4, 4),
                    Description = "Routine maintenance for oil filter replacement.",
                    Cost = 150.0m,
                    Technician = "John Doe"
                },
                new MaintenanceRecord
                {
                    Id = Guid.Parse("b2c3d4e5-f678-90a1-b2c3-d4e5f6789012"),
                    EquipmentId = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d402"),
                    MaintenanceDate = new DateTime(2024, 3, 15),
                    Description = "Fixed hydraulic system leak.",
                    Cost = 250.0m,
                    Technician = "Sarah Green"
                },
                new MaintenanceRecord
                {
                    Id = Guid.Parse("c3d4e5f6-7890-a1b2-c3d4-e5f678901234"),
                    EquipmentId = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d403"),
                    MaintenanceDate = new DateTime(2024, 2, 20),
                    Description = "Replaced broken conveyor belt.",
                    Cost = 320.0m,
                    Technician = "Michael Smith"
                },
                new MaintenanceRecord
                {
                    Id = Guid.Parse("d4e5f678-90a1-b2c3-d4e5-f67890123456"),
                    EquipmentId = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d401"),
                    MaintenanceDate = new DateTime(2024, 1, 30),
                    Description = "Calibrated machine sensors.",
                    Cost = 75.0m,
                    Technician = "Linda Adams"
                },
                new MaintenanceRecord
                {
                    Id = Guid.Parse("e5f67890-a1b2-c3d4-e5f6-789012345678"),
                    EquipmentId = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d404"),
                    MaintenanceDate = new DateTime(2024, 5, 12),
                    Description = "Replaced faulty motor in assembly line.",
                    Cost = 500.0m,
                    Technician = "David Johnson"
                }
            );

            modelBuilder.Entity<TechnicalInformation>().HasData(
                new TechnicalInformation
                {
                    Id = Guid.Parse("a1b2c3d4-e5f6-7890-a1b2-c3d4e5f67890"),
                    EquipmentId = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d401"),
                    EngineType = Enums.EngineType.Diesel,
                    EnginePower = 150,
                    EnginePowerUnit = Enums.PowerUnit.HorsePower,
                    FuelCapacity = 100,
                    FuelCapacityUnit = Enums.FuelCapacityUnit.Gallons,
                    Weight = 5000,
                    WeightUnit = Enums.WeightUnit.Kilograms,
                    Dimensions = "2.5 x 3.0 x 1.8",
                    MaxSpeed = 25,
                    SpeedUnit = Enums.SpeedUnit.Mps
                },
                new TechnicalInformation
                {
                    Id = Guid.Parse("b2c3d4e5-f678-90a1-b2c3-d4e5f6789012"),
                    EquipmentId = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d402"),
                    EngineType = Enums.EngineType.Diesel,
                    EnginePower = 200,
                    EnginePowerUnit = Enums.PowerUnit.HorsePower,
                    FuelCapacity = 80,
                    FuelCapacityUnit = Enums.FuelCapacityUnit.Liters,
                    Weight = 3000,
                    WeightUnit = Enums.WeightUnit.Kilograms,
                    Dimensions = "2.0 x 3.5 x 2.0",
                    MaxSpeed = 30,
                    SpeedUnit = Enums.SpeedUnit.Kmh
                },
                new TechnicalInformation
                {
                    Id = Guid.Parse("c3d4e5f6-7890-a1b2-c3d4-e5f678901234"),
                    EquipmentId = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d403"),
                    EngineType = Enums.EngineType.Electric,
                    EnginePower = 75,
                    EnginePowerUnit = Enums.PowerUnit.KW,
                    FuelCapacity = 0, // Not applicable for electric engines
                    FuelCapacityUnit = Enums.FuelCapacityUnit.Gallons,
                    Weight = 2000,
                    WeightUnit = Enums.WeightUnit.Kilograms,
                    Dimensions = "1.8 x 2.8 x 1.6",
                    MaxSpeed = 20,
                    SpeedUnit = Enums.SpeedUnit.Mps
                },
                new TechnicalInformation
                {
                    Id = Guid.Parse("d4e5f678-90a1-b2c3-d4e5-f67890123456"),
                    EquipmentId = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d404"),
                    EngineType = Enums.EngineType.Hybrid,
                    EnginePower = 180,
                    EnginePowerUnit = Enums.PowerUnit.HorsePower,
                    FuelCapacity = 50,
                    FuelCapacityUnit = Enums.FuelCapacityUnit.Liters,
                    Weight = 4000,
                    WeightUnit = Enums.WeightUnit.Kilograms,
                    Dimensions = "2.2 x 3.2 x 1.9",
                    MaxSpeed = 28,
                    SpeedUnit = Enums.SpeedUnit.Kmh
                },
                new TechnicalInformation
                {
                    Id = Guid.Parse("e5f67890-a1b2-c3d4-e5f6-789012345678"),
                    EquipmentId = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d405"),
                    EngineType = Enums.EngineType.Diesel,
                    EnginePower = 220,
                    EnginePowerUnit = Enums.PowerUnit.HorsePower,
                    FuelCapacity = 120,
                    FuelCapacityUnit = Enums.FuelCapacityUnit.Gallons,
                    Weight = 6000,
                    WeightUnit = Enums.WeightUnit.Kilograms,
                    Dimensions = "2.8 x 3.8 x 2.1",
                    MaxSpeed = 35,
                    SpeedUnit = Enums.SpeedUnit.Kmh
                }
            );

            modelBuilder.Entity<SellingContract>().HasData(
                new SellingContract
                {
                    Id = Guid.Parse("1a2b3c4d-5e6f-7890-a1b2-c3d4e5f67890"),
                    EquipmentId = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d404"),
                    CustomerId = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c11111"),
                    SalePrice = 1500.0m,
                    SaleDate = new DateTime(2024, 12, 25)
                }
            );

            modelBuilder.Entity<RentalContract>().HasData(
                new RentalContract
                {
                    Id= Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c11abc"),
                    EquipmentId = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d405"),
                    CustomerId = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c11112"),
                    StartDate = new DateTime(2024, 5, 12),
                    EndDate = new DateTime(2024, 12, 12),
                    Shifts = 15,
                    ShiftPrice = 15000,
                    }
                );
        }
    }
}
