using Application.BulkOperations;
using Application.Interface;
using Application.Interface.Repositories;
using Application.Interface.Services;
using Application.Interface.Services.EquipmentService;
using Application.Models.EquipmentModels.Read;
using Application.Models.EquipmentModels.Write;
using Application.ResourceParameters;
using Domain.Entities;
using Domain.Enums;
using Mapster;
using Microsoft.AspNetCore.JsonPatch;
using Shared.Results;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Services.EquipmentService
{
    public class EquipmentService : IEquipmentService
    {
        private readonly IEquipmentRepository _equipmentRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheVersionProvider _cacheVersionProvider;

        public EquipmentService(
            IEquipmentRepository equipmentRepository,
            ISupplierRepository supplierRepository,
            IUnitOfWork unitOfWork,
            ICacheVersionProvider cacheVersionProvider)
        {
            _equipmentRepository = equipmentRepository ??
                throw new ArgumentNullException(nameof(equipmentRepository));
            _supplierRepository = supplierRepository ??
                throw new ArgumentNullException(nameof(supplierRepository));
            _unitOfWork = unitOfWork ??
                throw new ArgumentNullException(nameof(unitOfWork));
            _cacheVersionProvider = cacheVersionProvider ??
                throw new ArgumentNullException(nameof(cacheVersionProvider));
        }

        #region Read
        public async Task<PagedList<EquipmentDto>> GetEquipmentsAsync(EquipmentResourceParameters parameters)
        {
            var equipment = await _equipmentRepository.GetEquipment(parameters);
            var equipmentDto = equipment.Adapt<List<EquipmentDto>>();
            return new PagedList<EquipmentDto>(
                equipmentDto,
                equipment.Count,
                equipment.CurrentPage,
                equipment.PageSize);
        }

        public async Task<EquipmentDto?> GetEquipmentByIdAsync(Guid equipmentId, string? fields = null)
        {
            await IsEquipmentExistsAsync(equipmentId);

            var equipment = await _equipmentRepository.GetEquipmentById(equipmentId, fields);
            return equipment.Adapt<EquipmentDto>();
        }

        public async Task<EquipmentDto?> GetSoftDeletedEquipmentByIdAsync(Guid equipmentId, string? fields = null)
        {
            if (equipmentId == Guid.Empty) throw new ArgumentException(nameof(equipmentId));
            var equipment = await _equipmentRepository.GetSoftDeletedEquipmentById(equipmentId, fields);
            return equipment.Adapt<EquipmentDto>();
        }

        public async Task<PagedList<EquipmentDto>> GetEquipmentsByStatusAsync(
            EquipmentStatus status,
            EquipmentResourceParameters parameters)
        {
            var equipment = await _equipmentRepository.GetEquipmentByStatus(status, parameters);
            var equipmentDto = equipment.Adapt<List<EquipmentDto>>();

            return new PagedList<EquipmentDto>(
                equipmentDto,
                equipment.Count,
                equipment.CurrentPage,
                equipment.PageSize);
        }

        public async Task<PagedList<EquipmentDto>> GetEquipmentsBySupplierAsync(
            Guid supplierId,
            EquipmentResourceParameters parameters)
        {
            if (supplierId == Guid.Empty) throw new ArgumentException(nameof(supplierId));

            var supplier = await _supplierRepository.GetSupplier(supplierId, null)
                ?? throw new KeyNotFoundException($"supplier with id: {supplierId} is not found");

            var equipment = await _equipmentRepository.GetEquipmentBySupplier(supplierId, parameters);
            var equipmentDto = equipment.Adapt<List<EquipmentDto>>();

            return new PagedList<EquipmentDto>(
                equipmentDto,
                equipment.Count,
                equipment.CurrentPage,
                equipment.PageSize);
        }

        public async Task<PagedList<EquipmentDto>> GetEquipmentsByBrandAsync(
            EquipmentBrand brand,
            EquipmentResourceParameters parameters)
        {
            var equipment = await _equipmentRepository.GetEquipmentByBrand(brand, parameters);
            var equipmentDto = equipment.Adapt<List<EquipmentDto>>();

            return new PagedList<EquipmentDto>(
                equipmentDto,
                equipment.Count,
                equipment.CurrentPage,
                equipment.PageSize);
        }

        public async Task<PagedList<EquipmentDto>> GetEquipmentsByTypeAsync(
            EquipmentType type,
            EquipmentResourceParameters parameters)
        {
            var equipment = await _equipmentRepository.GetEquipmentByType(type, parameters);
            var equipmentDto = equipment.Adapt<List<EquipmentDto>>();

            return new PagedList<EquipmentDto>(
                equipmentDto,
                equipment.Count,
                equipment.CurrentPage,
                equipment.PageSize);
        }

        public async Task<PagedList<EquipmentDto>> GetSoftDeletedEquipmentsAsync(
            EquipmentResourceParameters parameters)
        {
            var equipment = await _equipmentRepository.GetSoftDeletedEquipment(parameters);
            var equipmentDto = equipment.Adapt<List<EquipmentDto>>();

            return new PagedList<EquipmentDto>(
                equipmentDto,
                equipment.Count,
                equipment.CurrentPage,
                equipment.PageSize);
        }

        #endregion

        #region Create

        public async Task<EquipmentDto> CreateEquipmentAsync(EquipmentCreateDto equipmentCreateDto)
        {
            ArgumentNullException.ThrowIfNull(equipmentCreateDto);

            var equipment = equipmentCreateDto.Adapt<Equipment>();
            await _equipmentRepository.CreateEquipment(equipment);
            await _unitOfWork.SaveChangesAsync();
            return (await _equipmentRepository.GetEquipmentById(equipment.Id)).Adapt<EquipmentDto>();
        }
        #endregion

        #region Update

        public async Task UpdateEquipmentAsync(Guid equipmentId, EquipmentUpdateDto equipmentUpdateDto)
        {
            var equipmentFromRepo = await GetEquipmentOrThrow(equipmentId);

            equipmentUpdateDto.Adapt(equipmentFromRepo);
            await _equipmentRepository.UpdateEquipment(equipmentFromRepo);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task Patch(Guid equipmentId, JsonPatchDocument<EquipmentUpdateDto> patchDocumnet)
        {
            if (patchDocumnet is null) throw new ValidationException("Invalid patch document.");

            var equipment = await GetEquipmentOrThrow(equipmentId);
            var updatedEquipment = equipment.Adapt<EquipmentUpdateDto>();

            patchDocumnet.ApplyTo(updatedEquipment);
            updatedEquipment.Adapt(equipment);
            await _equipmentRepository.UpdateEquipment(equipment);
            await _unitOfWork.SaveChangesAsync();
        }
        #endregion

        #region Status Management
        public async Task MarkEquipmentAsAvailableAsync(Guid equipmentId)
        {
            var equipment = await GetEquipmentOrThrow(equipmentId);
            await _equipmentRepository.SetEquipmentStatus(equipment, EquipmentStatus.Available);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task MarkEquipmentAsSoldAsync(Guid equipmentId)
        {
            var equipment = await GetEquipmentOrThrow(equipmentId);
            await _equipmentRepository.SetEquipmentStatus(equipment, EquipmentStatus.Sold);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task MarkEquipmentAsUnderMaintenanceAsync(Guid equipmentId)
        {
            var equipment = await GetEquipmentOrThrow(equipmentId);
            await _equipmentRepository.SetEquipmentStatus(equipment, EquipmentStatus.UnderMaintenance);
            await _unitOfWork.SaveChangesAsync();
        }
        #endregion

        #region Soft Delete / Restore
        public async Task SoftDeleteEquipmentAsync(Guid equipmentId)
        {
            var equipment = await GetEquipmentOrThrow(equipmentId);
            await _equipmentRepository.SoftDeleteEquipment(equipment);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RestoreEquipmentAsync(Guid equipmentId)
        {
            var equipment = await GetEquipmentOrThrow(equipmentId);
            await _equipmentRepository.RestoreEquipment(equipment);
            await _unitOfWork.SaveChangesAsync();
        }
        #endregion

        #region Bulk operations
        public async Task<BulkOperationResult> CreateEquipmentCollectionAsync(
            IEnumerable<EquipmentCreateDto> equipment)
        {
            if (equipment == null || !equipment.Any())
                throw new ArgumentException(
                    "Equipment list cannot be null or empty.",
                    nameof(equipment));

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var equipmentToCreate = equipment.Adapt<IEnumerable<Equipment>>();
                var result = await _equipmentRepository.CreateEquipmentBulk(equipmentToCreate);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return result;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<BulkOperationResult> SoftDeleteEquipmentCollectionAsync(
            IEnumerable<Guid> equipmentIds)
        {
            if (equipmentIds == null || !equipmentIds.Any())
                throw new ArgumentException(
                    "Ids list cannot be null or empty.",
                    nameof(equipmentIds));

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result = await _equipmentRepository.SoftDeleteBulk(equipmentIds);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return result;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<BulkOperationResult> RestoreEquipmentCollectionAsync(
            IEnumerable<Guid> equipmentIds)
        {
            if (equipmentIds == null || !equipmentIds.Any())
                throw new ArgumentException(
                    "Ids list cannot be null or empty.",
                    nameof(equipmentIds));

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var result = await _equipmentRepository.RestoreBulk(equipmentIds);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return result;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
        #endregion

        #region Validation / Helpers
        public async Task<bool> IsEquipmentExistsAsync(Guid equipmentId)
        {
            if (equipmentId == Guid.Empty) throw new ArgumentException($"The equipment id: {equipmentId} can not be empty", nameof(equipmentId));
            var equipment = await _equipmentRepository.EquipmentExists(equipmentId);
            if (equipment == false) throw new KeyNotFoundException($"Equipment with id: {equipmentId} not found");
            return true;
        }
        public async Task<bool> IsEquipmentAvailableAsync(Guid equipmentId)
        {
            await IsEquipmentExistsAsync(equipmentId);
            var equipmentStatus = await _equipmentRepository.GetEquipmentStatus(equipmentId);
            if (equipmentStatus == EquipmentStatus.Available)
            {
                return true;
            }
            return false;
        }
        #endregion

        private async Task<Equipment> GetEquipmentOrThrow(Guid equipmentId)
        {
            if (equipmentId == Guid.Empty)
                throw new ArgumentException("Equipment id cannot be empty", nameof(equipmentId));

            var equipment = await _equipmentRepository.GetEquipmentForUpdate(equipmentId);
            return equipment ?? throw new KeyNotFoundException($"Equipment with id {equipmentId} not found");
        }
    }
}
