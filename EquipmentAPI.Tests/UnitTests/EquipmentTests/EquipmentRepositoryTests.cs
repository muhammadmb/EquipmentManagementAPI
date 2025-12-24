using Application.ResourceParameters;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Contexts;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EquipmentAPI.Tests.UnitTests.EquipmentTests
{
    public class EquipmentRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly EquipmentRepository _repository;

        public EquipmentRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "EquipmentTestDb")
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new EquipmentRepository(_context);

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            var equipment = new Equipment
            {
                Id = Guid.NewGuid(),
                Name = "Excavator",
                EquipmentStatus = EquipmentStatus.Available,
                RowVersion = new byte[] { 1, 2, 3, 4 }
            };

            _context.Equipments.Add(equipment);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetEquipment_ReturnsPagedList()
        {
            // Arrange
            var parameters = new EquipmentResourceParameters
            {
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _repository.GetEquipment(parameters);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetEquipmentById_ReturnsEquipment()
        {
            // Arrange
            var equipmentId = _context.Equipments.First().Id;

            // Act
            var result = await _repository.GetEquipmentById(equipmentId, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(equipmentId, result.Id);
        }

        [Fact]
        public async Task Create_AddsEquipmentToDatabase()
        {
            // Arrange
            var equipment = new Equipment
            {
                Id = Guid.NewGuid(),
                Name = "Bulldozer",
                EquipmentStatus = EquipmentStatus.Available,
                RowVersion = new byte[] { 1, 2, 3, 4 }
            };

            // Act
            _repository.Create(equipment);
            await _repository.SaveChangesAsync();

            // Assert
            var result = await _context.Equipments.FindAsync(equipment.Id);
            Assert.NotNull(result);
            Assert.Equal("Bulldozer", result.Name);
        }

        [Fact]
        public async Task Update_UpdatesEquipment()
        {
            // Arrange
            var equipment = _context.Equipments.First();
            equipment.Name = "Updated Excavator";

            // Act
            await _repository.Update(equipment);
            await _repository.SaveChangesAsync();

            // Assert
            var result = await _context.Equipments.FindAsync(equipment.Id);
            Assert.Equal("Updated Excavator", result.Name);
        }

        [Fact]
        public async Task Delete_MarksEquipmentAsDeleted()
        {
            // Arrange
            var equipment = new Equipment { Id = Guid.Parse("00000000-0000-0000-0000-000000000011"), Name = "To Be Deleted", RowVersion = new byte[] { 1, 2, 3, 4 } };
            _context.Equipments.Add(equipment);
            await _context.SaveChangesAsync();


            _context.ChangeTracker.Clear();

            // Act
            _repository.Delete(equipment.Id);
            await _repository.SaveChangesAsync();

            // Assert
            var deletedEquipment = await _context.Equipments
                                    .IgnoreQueryFilters()
                                    .FirstOrDefaultAsync(e => e.Id == equipment.Id);

            Assert.NotNull(deletedEquipment);
            Assert.NotNull(deletedEquipment.DeletedDate);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
