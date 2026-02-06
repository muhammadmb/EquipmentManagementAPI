using Application.Interface;
using Application.Interface.Repositories;
using Application.Interface.Services;
using Application.Models.EquipmentModels.Write;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Services.EquipmentService;
using Microsoft.AspNetCore.JsonPatch;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace EquipmentAPI.Tests.UnitTests.EquipmentTests
{
    public class EquipmentServiceTests
    {
        private readonly Mock<IEquipmentRepository> _equipmentRepo = new();
        private readonly Mock<ISupplierRepository> _supplierRepo = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<ICacheVersionProvider> _cache = new();

        private readonly EquipmentService _sut;

        public EquipmentServiceTests()
        {
            _sut = new EquipmentService(
                _equipmentRepo.Object,
                _supplierRepo.Object,
                _unitOfWork.Object,
                _cache.Object);
        }

        [Fact]
        public async Task GetEquipmentByIdAsync_WhenExists_ReturnsDto()
        {
            // Arrange
            var id = Guid.NewGuid();
            var equipment = new Equipment { Id = id };

            _equipmentRepo.Setup(r => r.EquipmentExists(id))
                          .ReturnsAsync(true);

            _equipmentRepo.Setup(r => r.GetEquipmentById(id, null))
                          .ReturnsAsync(equipment);

            // Act
            var result = await _sut.GetEquipmentByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            _equipmentRepo.Verify(r => r.GetEquipmentById(id, null), Times.Once);
        }

        [Fact]
        public async Task GetEquipmentByIdAsync_WhenNotExists_Throws()
        {
            var id = Guid.NewGuid();

            _equipmentRepo.Setup(r => r.EquipmentExists(id))
                          .ReturnsAsync(false);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _sut.GetEquipmentByIdAsync(id));
        }

        [Fact]
        public async Task CreateEquipmentAsync_CreatesAndReturnsDto()
        {
            // Arrange
            var dto = new EquipmentCreateDto();
            var equipment = new Equipment { Id = Guid.NewGuid() };

            _equipmentRepo.Setup(r => r.CreateEquipment(It.IsAny<Equipment>()))
                          .Returns(Task.CompletedTask);

            _equipmentRepo.Setup(r => r.GetEquipmentById(It.IsAny<Guid>(), null))
                          .ReturnsAsync(equipment);

            // Act
            var result = await _sut.CreateEquipmentAsync(dto);

            // Assert
            Assert.NotNull(result);
            _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateEquipmentAsync_WhenNull_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _sut.CreateEquipmentAsync(null));
        }

        [Fact]
        public async Task UpdateEquipmentAsync_UpdatesAndSaves()
        {
            var id = Guid.NewGuid();
            var equipment = new Equipment { Id = id };
            var dto = new EquipmentUpdateDto();

            _equipmentRepo.Setup(r => r.GetEquipmentForUpdate(id, null))
                          .ReturnsAsync(equipment);

            // Act
            await _sut.UpdateEquipmentAsync(id, dto);

            // Assert
            _equipmentRepo.Verify(r => r.UpdateEquipment(equipment), Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Patch_ValidPatch_UpdatesEquipment()
        {
            var id = Guid.NewGuid();
            var equipment = new Equipment { Id = id, Name = "Old" };

            var patch = new JsonPatchDocument<EquipmentUpdateDto>();
            patch.Replace(e => e.Name, "New");

            _equipmentRepo.Setup(r => r.GetEquipmentForUpdate(id, null))
                          .ReturnsAsync(equipment);

            // Act
            await _sut.Patch(id, patch);

            // Assert
            _equipmentRepo.Verify(r => r.UpdateEquipment(equipment), Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Patch_WhenNull_Throws()
        {
            await Assert.ThrowsAsync<ValidationException>(
                () => _sut.Patch(Guid.NewGuid(), null));
        }

        [Fact]
        public async Task MarkEquipmentAsSoldAsync_SetsStatusAndSaves()
        {
            var id = Guid.NewGuid();
            var equipment = new Equipment { Id = id };

            _equipmentRepo.Setup(r => r.GetEquipmentForUpdate(id, null))
                          .ReturnsAsync(equipment);

            // Act
            await _sut.MarkEquipmentAsSoldAsync(id);

            // Assert
            _equipmentRepo.Verify(r =>
                r.SetEquipmentStatus(equipment, EquipmentStatus.Sold),
                Times.Once);

            _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task SoftDeleteEquipmentAsync_DeletesAndSaves()
        {
            var id = Guid.NewGuid();
            var equipment = new Equipment { Id = id };

            _equipmentRepo.Setup(r => r.GetEquipmentForUpdate(id, null))
                          .ReturnsAsync(equipment);

            await _sut.SoftDeleteEquipmentAsync(id);

            _equipmentRepo.Verify(r => r.SoftDeleteEquipment(equipment), Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateEquipmentCollectionAsync_WhenFails_RollsBack()
        {
            _equipmentRepo.Setup(r => r.CreateEquipmentBulk(It.IsAny<IEnumerable<Equipment>>()))
                          .ThrowsAsync(new Exception());

            await Assert.ThrowsAsync<Exception>(() =>
                _sut.CreateEquipmentCollectionAsync(new[] { new EquipmentCreateDto() }));

            _unitOfWork.Verify(u => u.RollbackAsync(), Times.Once);
        }

        [Fact]
        public async Task IsEquipmentExistsAsync_WhenExists_ReturnsTrue()
        {
            var id = Guid.NewGuid();

            _equipmentRepo.Setup(r => r.EquipmentExists(id))
                          .ReturnsAsync(true);

            var result = await _sut.IsEquipmentExistsAsync(id);

            Assert.True(result);
        }

        [Fact]
        public async Task IsEquipmentAvailableAsync_WhenAvailable_ReturnsTrue()
        {
            var id = Guid.NewGuid();

            _equipmentRepo.Setup(r => r.EquipmentExists(id))
                          .ReturnsAsync(true);

            _equipmentRepo.Setup(r => r.GetEquipmentStatus(id))
                          .ReturnsAsync(EquipmentStatus.Available);

            Assert.True(await _sut.IsEquipmentAvailableAsync(id));
        }

    }
}
