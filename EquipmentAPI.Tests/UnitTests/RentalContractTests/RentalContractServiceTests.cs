using Application.Interface;
using Application.Interface.Repositories;
using Application.Interface.Services;
using Application.Models.RentalContractModels.Write;
using Application.ResourceParameters;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Helpers;
using Infrastructure.Services;
using Moq;
using Shared.Results;

namespace EquipmentAPI.Tests.UnitTests.RentalContractTests
{
    public class RentalContractServiceTests
    {
        private readonly Mock<IRentalContractRepository> _contractRepo = new();
        private readonly Mock<IEquipmentRepository> _equipmentRepo = new();
        private readonly Mock<ICustomerRepository> _customerRepo = new();
        private readonly Mock<ICacheVersionProvider> _cache = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly IRentalContractService _service;

        public RentalContractServiceTests()
        {
            _service = new RentalContractService(
               _contractRepo.Object,
               _equipmentRepo.Object,
               _customerRepo.Object,
               _cache.Object,
               _unitOfWork.Object);
        }


        // =========================================================================
        // GET
        // =========================================================================

        [Fact]
        public async Task GetRentalContractById_ReturnsDto_WhenExists()
        {
            var contract = new RentalContract { Id = Guid.NewGuid() };

            _contractRepo
                .Setup(r => r.GetRentalContractById(contract.Id, null))
                .ReturnsAsync(contract);

            var result = await _service.GetRentalContractById(contract.Id, null);

            result.Should().NotBeNull();
            result!.Id.Should().Be(contract.Id);
        }

        [Fact]
        public async Task GetRentalContracts_ReturnsPagedList()
        {
            var parameters = new RentalContractResourceParameters();
            var contracts = new List<RentalContract>
            {
                new() { Id = Guid.NewGuid() }
            };

            var paged = new PagedList<RentalContract>(contracts, 1, 1, 10);

            _contractRepo
                .Setup(r => r.GetRentalContracts(parameters))
                .ReturnsAsync(paged);

            var result = await _service.GetRentalContracts(parameters);

            result.Should().NotBeNull();
            result!.Count.Should().Be(1);
        }

        // =========================================================================
        // CREATE
        // =========================================================================

        [Fact]
        public async Task CreateRentalContract_CreatesContract_WhenValid()
        {
            var dto = new RentalContractCreateDto
            {
                CustomerId = Guid.NewGuid(),
                EquipmentId = Guid.NewGuid(),
                StartDate = DateTimeOffset.UtcNow.AddDays(1),
                EndDate = DateTimeOffset.UtcNow.AddDays(5),
                Shifts = 5
            };

            _customerRepo.Setup(r => r.CustomerExists(dto.CustomerId))
                         .ReturnsAsync(true);

            _equipmentRepo.Setup(r => r.EquipmentExists(dto.EquipmentId))
                          .ReturnsAsync(true);

            _equipmentRepo.Setup(r => r.GetEquipmentStatus(dto.EquipmentId))
                          .ReturnsAsync(EquipmentStatus.Available);

            _contractRepo.Setup(r => r.HasOverlappingContracts(
                    dto.EquipmentId,
                    dto.StartDate,
                    dto.EndDate,
                    null))
                .ReturnsAsync(false);

            var result = await _service.CreateRentalContract(dto);

            result.Should().NotBeNull();

            _contractRepo.Verify(r => r.CreateRentalContract(It.IsAny<RentalContract>()), Times.Once);
            _equipmentRepo.Verify(r => r.SetEquipmentStatus(dto.EquipmentId, EquipmentStatus.Rented), Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
            _cache.Verify(c => c.IncrementAsync(CacheScopes.RentalContracts), Times.Once);
        }

        [Fact]
        public async Task CreateRentalContract_Throws_WhenCustomerNotFound()
        {
            var dto = new RentalContractCreateDto
            {
                CustomerId = Guid.NewGuid(),
                EquipmentId = Guid.NewGuid(),
                StartDate = DateTimeOffset.UtcNow.AddDays(1),
                EndDate = DateTimeOffset.UtcNow.AddDays(5),
                Shifts = 5
            };

            _customerRepo.Setup(r => r.CustomerExists(dto.CustomerId))
                         .ReturnsAsync(false);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateRentalContract(dto));
        }

        // =========================================================================
        // STATUS CHANGES
        // =========================================================================

        [Fact]
        public async Task ActiveRentalContract_ActivatesContract()
        {
            var contract = new RentalContract
            {
                Id = Guid.NewGuid(),
                EquipmentId = Guid.NewGuid()
            };

            _contractRepo.Setup(r => r.GetRentalContractForUpdate(contract.Id, null))
                         .ReturnsAsync(contract);

            await _service.ActiveRentalContract(contract.Id);

            _equipmentRepo.Verify(r =>
                r.SetEquipmentStatus(contract.EquipmentId, EquipmentStatus.Rented),
                Times.Once);

            _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
            _cache.Verify(c => c.IncrementAsync(CacheScopes.RentalContracts), Times.Once);
        }

        // =========================================================================
        // UPDATE
        // =========================================================================

        [Fact]
        public async Task UpdateRentalContract_Updates_WhenValid()
        {
            var id = Guid.NewGuid();
            var existing = new RentalContract
            {
                Id = id,
                EquipmentId = Guid.NewGuid()
            };

            var updateDto = new RentalContractUpdateDto
            {
                EquipmentId = existing.EquipmentId,
                StartDate = DateTimeOffset.UtcNow.AddDays(1),
                EndDate = DateTimeOffset.UtcNow.AddDays(3),
                Shifts = 3,
                ShiftPrice = 150,
            };

            _contractRepo.Setup(r => r.GetRentalContractById(id, null))
                         .ReturnsAsync(existing);

            _contractRepo.Setup(r => r.HasOverlappingContracts(
                updateDto.EquipmentId,
                updateDto.StartDate,
                updateDto.EndDate,
                id))
                .ReturnsAsync(false);

            await _service.UpdateRentalContract(id, updateDto);

            _contractRepo.Verify(r => r.UpdateRentalContract(existing), Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
            _cache.Verify(c => c.IncrementAsync(CacheScopes.RentalContracts), Times.Once);
        }

        // =========================================================================
        // DELETE / RESTORE
        // =========================================================================

        [Fact]
        public async Task SoftDeleteRentalContract_DeletesAndFreesEquipment()
        {
            var contract = new RentalContract
            {
                Id = Guid.NewGuid(),
                EquipmentId = Guid.NewGuid()
            };

            _contractRepo.Setup(r => r.GetRentalContractById(contract.Id, null))
                         .ReturnsAsync(contract);

            await _service.SoftDeleteRentalContract(contract.Id);

            _contractRepo.Verify(r => r.SoftDeleteRentalContract(contract.Id), Times.Once);
            _equipmentRepo.Verify(r =>
                r.SetEquipmentStatus(contract.EquipmentId, EquipmentStatus.Available),
                Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RestoreRentalContract_RestoresAndRentsEquipment()
        {
            var contract = new RentalContract
            {
                Id = Guid.NewGuid(),
                EquipmentId = Guid.NewGuid()
            };

            _contractRepo.Setup(r => r.GetDeletedRentalContractById(contract.Id))
                         .ReturnsAsync(contract);

            await _service.RestoreRentalContract(contract.Id);

            _contractRepo.Verify(r => r.RestoreRentalContract(contract.Id), Times.Once);
            _equipmentRepo.Verify(r =>
                r.SetEquipmentStatus(contract.EquipmentId, EquipmentStatus.Rented),
                Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}