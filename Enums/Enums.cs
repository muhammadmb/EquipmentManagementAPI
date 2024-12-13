using System.ComponentModel;

namespace EquipmentAPI.Enums
{
    public enum EquipmentType
    {
        [Description("Unknown or unspecified equipment type.")]
        Unknown = 0, // Default value

        [Description("Excavators are used for digging and earthmoving.")]
        Excavator,

        [Description("Bulldozers are designed for pushing large amounts of material.")]
        Bulldozer,

        [Description("Loaders are used to move and load materials.")]
        Loader,

        [Description("Graders are used for fine grading and leveling.")]
        Grader,

        [Description("Dump Trucks transport loose materials like sand or gravel.")]
        DumpTruck,

        [Description("Cranes are used to lift and move heavy objects.")]
        Crane,

        [Description("Forklifts are used in warehouses to move materials.")]
        Forklift,

        [Description("Backhoes are versatile for digging and small-scale earthmoving.")]
        Backhoe,

        [Description("Compactors are used for compacting soil or asphalt.")]
        Compactor,

        [Description("Skid Steer Loaders are compact and versatile machines.")]
        SkidSteerLoader,

        [Description("Pavers are used to lay asphalt on roads and driveways.")]
        Paver,

        [Description("Trenchers are used to dig trenches for cables and pipelines.")]
        Trencher,

        [Description("Telehandlers are used for lifting and placing materials at height.")]
        Telehandler,

        [Description("Scrapers are used to scrape and transport soil or materials.")]
        Scraper

    }

    public enum EquipmentBrand
    {
        [Description("Unknown or unspecified brand.")]
        Unknown = 0, // Default value

        [Description("Caterpillar - A global leader in construction equipment.")]
        Caterpillar,

        [Description("Komatsu - Known for durable heavy machinery.")]
        Komatsu,

        [Description("Volvo - Offers innovative and fuel-efficient machines.")]
        Volvo,

        [Description("Hitachi - Specializes in reliable excavators and cranes.")]
        Hitachi,

        [Description("Liebherr - Renowned for cranes and mining equipment.")]
        Liebherr,

        [Description("John Deere - Trusted for construction and forestry machinery.")]
        JohnDeere,

        [Description("Doosan - Offers versatile and cost-effective machines.")]
        Doosan,

        [Description("JCB - Famous for backhoes and skid-steer loaders.")]
        JCB,

        [Description("Sany - A leader in heavy equipment manufacturing in Asia.")]
        Sany,

        [Description("Case Construction - Specializes in excavators and loaders.")]
        CaseConstruction,

        [Description("Kubota - Known for compact construction machinery.")]
        Kubota,

        [Description("Hyundai - Offers affordable and reliable equipment.")]
        Hyundai
    }

    public enum PowerUnit
    {
        [Description("HorsePower")]
        HorsePower,

        [Description("Kilowatt (KW)")]
        KW
    }
    public enum WeightUnit
    {
        [Description("Kilogram")]
        Kilograms,

        [Description("Pounds")]
        Pounds
    }

    public enum FuelCapacityUnit
    {
        [Description("Liters")]
        Liters,

        [Description("Gallons")]
        Gallons
    }

    public enum DimensionUnit
    {
        [Description("Meters")]
        Meters,

        [Description("Feet")]
        Feet
    }

    public enum EngineType
    {
        [Description("Diesel")]
        Diesel,

        [Description("Electric")]
        Electric,

        [Description("Gasoline")]
        Gasoline,

        [Description("Hybrid")]
        Hybrid
    }

    public enum SpeedUnit
    {
        [Description("Kilometers per Hour (km/h)")]
        Kmh,

        [Description("Miles per Hour (mph)")]
        Mph,

        [Description("Meters per Second (m/s)")]
        Mps
    }
}
