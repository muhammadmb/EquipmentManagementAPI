using Application.BulkOperations;
using Application.Interface;
using Application.Interface.Repositories;
using Application.Interface.Services;
using Application.Models.SellingContract.Read;
using Application.Models.SellingContract.Write;
using Application.ResourceParameters;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Helpers;
using Mapster;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Shared.Results;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Services
{
    public class SellingContractService : ISellingContractService
    {
        private readonly ISellingContractRepository _sellingRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICustomerRepository _customerRepository;
        private readonly IEquipmentRepository _equipmentRepository;
        private readonly ICacheVersionProvider _cacheVersionProvider;

        public SellingContractService(
            ISellingContractRepository sellingRepository,
            IUnitOfWork unitOfWork,
            ICustomerRepository customerRepository,
            IEquipmentRepository equipmentRepository,
            ICacheVersionProvider cacheVersionProvider)
        {
            _sellingRepository = sellingRepository
                ?? throw new ArgumentNullException(nameof(sellingRepository));
            _unitOfWork = unitOfWork ??
                throw new ArgumentNullException(nameof(UnitOfWork));
            _customerRepository = customerRepository ??
                throw new ArgumentNullException(nameof(customerRepository));
            _equipmentRepository = equipmentRepository ??
                throw new ArgumentNullException(nameof(equipmentRepository));
            _cacheVersionProvider = cacheVersionProvider ??
                throw new ArgumentNullException(nameof(cacheVersionProvider));
        }

        #region Get Operations
        public async Task<PagedList<SellingContractDto>?> GetSellingContracts(SellingContractResourceParameters parameters)
        {

            var sellingContracts = await _sellingRepository.GetSellingContracts(parameters);

            if (sellingContracts is null)
                return null;

            var sellingContractsDto = sellingContracts.Adapt<List<SellingContractDto>>();
            return new PagedList<SellingContractDto>(
                sellingContractsDto,
                sellingContracts.Count,
                sellingContracts.CurrentPage,
                sellingContracts.PageSize);
        }

        public async Task<SellingContractDto?> GetSellingContractById(
            Guid id,
            string? fields = null)
        {
            if (id == Guid.Empty)
                throw new ArgumentNullException(nameof(id));

            var sellingContract = await _sellingRepository.GetSellingContractById(id, fields);
            return sellingContract.Adapt<SellingContractDto>();
        }

        public async Task<SellingContractDto?> GetDeletedSellingContractById(
            Guid id,
            string? fields = null)
        {
            if (id == Guid.Empty)
                throw new ArgumentNullException(nameof(id));

            var sellingContract = await _sellingRepository.GetSoftDeletedSellingContractById(id, fields);
            return sellingContract.Adapt<SellingContractDto>();
        }

        public async Task<IEnumerable<SellingContractDto>?> GetSellingContractsByYear(int year)
        {
            if (year < 1900 || year > DateTime.UtcNow.Year + 20)
                throw new ArgumentException("Invalid year spacified.", nameof(year));

            var sellingContract = await _sellingRepository.GetSellingContractsByYear(year);
            return sellingContract.Adapt<IEnumerable<SellingContractDto>>();
        }

        public async Task<IEnumerable<SellingContractDto>?> GetSellingContractsByCustomerId(
            Guid customerId,
            string? fields = null)
        {
            if (customerId == Guid.Empty)
                throw new ArgumentNullException(nameof(customerId));

            var sellingContracts = await _sellingRepository
                .GetSellingContractsByCustomerId(customerId, fields);
            return sellingContracts.Adapt<IEnumerable<SellingContractDto>>();
        }

        public async Task<IEnumerable<SellingContractDto>?> GetSellingContractsByEquipmentId(
            Guid equipmentId,
            string? fields = null)
        {
            if (equipmentId == Guid.Empty)
                throw new ArgumentNullException(nameof(equipmentId));

            var sellingContracts = await _sellingRepository
                .GetSellingContractsByEquipment(equipmentId, fields);
            return sellingContracts.Adapt<IEnumerable<SellingContractDto>>();
        }

        public async Task<IEnumerable<SellingContractDto>?> GetSellingContractsByIds(
            IEnumerable<Guid> ids,
            string? fields = null)
        {
            if (ids == null || !ids.Any()) return Enumerable.Empty<SellingContractDto>();

            var sellingContracts = await _sellingRepository.GetSellingContractsByIds(ids, fields);
            return sellingContracts.Adapt<IEnumerable<SellingContractDto>>();
        }

        public async Task<IEnumerable<SellingContractDto>?> GetDeletedSellingContracts(SellingContractResourceParameters parameters)
        {
            ArgumentNullException.ThrowIfNull(parameters);

            var sellingContracts = await _sellingRepository
                .GetSoftDeletedSellingContracts(parameters);
            return sellingContracts.Adapt<IEnumerable<SellingContractDto>>();
        }

        public async Task<bool> SellingContractExists(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentNullException(nameof(id));

            return await _sellingRepository.SellingContractExists(id);
        }
        public async Task<bool> CustomerHasContracts(Guid customerId)
        {
            if (customerId == Guid.Empty)
                throw new ArgumentNullException(nameof(customerId));

            return await _sellingRepository.CustomerHasContracts(customerId);
        }
        #endregion

        #region Create And Update
        public async Task<SellingContractDto> CreateSellingContract(SellingContractCreateDto sellingContractCreateDto)
        {
            ArgumentNullException.ThrowIfNull(sellingContractCreateDto, nameof(sellingContractCreateDto));

            await ValidateCustomerAsync(sellingContractCreateDto.CustomerId);
            await ValidateEquipmentAvailableAsync(sellingContractCreateDto.EquipmentId);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var sellingContract = sellingContractCreateDto.Adapt<SellingContract>();

                await _sellingRepository.CreateSellingContract(sellingContract);
                await _equipmentRepository.SetEquipmentStatus(
                    sellingContractCreateDto.EquipmentId,
                    EquipmentStatus.Sold);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                await InvalidateCache();

                return sellingContract.Adapt<SellingContractDto>();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateSellingContract(Guid id, SellingContractUpdateDto sellingContract)
        {
            if (id == Guid.Empty)
                throw new ArgumentNullException(nameof(id));

            ArgumentNullException.ThrowIfNull(sellingContract, nameof(sellingContract));

            var existingSellingContract = await _sellingRepository.GetSellingContractForUpdate(id) ??
                throw new KeyNotFoundException($"Selling contract with id {id} not found.");
            if (existingSellingContract.EquipmentId != sellingContract.EquipmentId)
            {
                await ValidateEquipmentAvailableAsync(sellingContract.EquipmentId);
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (existingSellingContract.EquipmentId != sellingContract.EquipmentId)
                {
                    await ValidateCustomerAsync(sellingContract.CustomerId);
                    await _equipmentRepository.SetEquipmentStatus(existingSellingContract.EquipmentId, EquipmentStatus.Available);
                    await _equipmentRepository.SetEquipmentStatus(sellingContract.EquipmentId, EquipmentStatus.Sold);
                }

                sellingContract.Adapt(existingSellingContract);
                await _sellingRepository.UpdateSellingContract(existingSellingContract);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                await InvalidateCache();

            }
            catch (DbUpdateConcurrencyException)
            {
                throw new DbUpdateConcurrencyException("The entity has been modified by another user.");
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task Patch(Guid id, JsonPatchDocument<SellingContractUpdateDto> patchDocument)
        {
            if (patchDocument is null)
                throw new ValidationException("Invalid patch document.");

            var sellingContractFromRepo = await _sellingRepository.GetSellingContractForUpdate(id) ??
                throw new KeyNotFoundException($"Selling contract with id {id} not found.");
            var sellingContract = sellingContractFromRepo.Adapt<SellingContractUpdateDto>();

            if (sellingContractFromRepo.EquipmentId != sellingContract.EquipmentId)
            {
                await ValidateEquipmentAvailableAsync(sellingContract.EquipmentId);
            }
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (sellingContractFromRepo.EquipmentId != sellingContract.EquipmentId)
                {
                    await ValidateEquipmentAvailableAsync(sellingContract.EquipmentId);
                    await _equipmentRepository.SetEquipmentStatus(sellingContractFromRepo.EquipmentId, EquipmentStatus.Available);
                    await _equipmentRepository.SetEquipmentStatus(sellingContract.EquipmentId, EquipmentStatus.Sold);
                }

                patchDocument.ApplyTo(sellingContract);
                sellingContract.Adapt(sellingContractFromRepo);
                await _sellingRepository.UpdateSellingContract(sellingContractFromRepo);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                await InvalidateCache();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new DbUpdateConcurrencyException("The entity has been modified by another user.");
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        #endregion

        #region delete and restore

        public async Task SoftDeleteSellingContract(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentNullException(nameof(id));

            var existingSellingContract = await GetRequiredContractAsync(id);

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                await _sellingRepository.SoftDeleteSellingContract(id);
                await _equipmentRepository.SetEquipmentStatus(existingSellingContract.EquipmentId, EquipmentStatus.Available);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                await InvalidateCache();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task RestoreSellingContract(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentNullException(nameof(id));

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var deletedSellingContract = await _sellingRepository.GetSoftDeletedSellingContractById(id)
                    ?? throw new KeyNotFoundException($"Deleted selling contract with id {id} not found.");

                await ValidateEquipmentAvailableAsync(deletedSellingContract.EquipmentId);
                await _sellingRepository.RestoreSellingContract(id);
                await _equipmentRepository.SetEquipmentStatus(deletedSellingContract.EquipmentId, EquipmentStatus.Sold);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                await InvalidateCache();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        #endregion

        #region Bulk Operations

        public async Task<BulkOperationResult> CreateSellingContracts(
            IEnumerable<SellingContractCreateDto> sellingContracts)
        {
            if (sellingContracts == null || !sellingContracts.Any())
            {
                throw new ArgumentException(
                    "Contract list cannot be null or empty.",
                    nameof(sellingContracts));
            }

            await _unitOfWork.BeginTransactionAsync();
            var equipmentIds = sellingContracts
                .Select(sc => sc.EquipmentId)
                .Distinct()
                .ToList();

            try
            {
                var sellingContractToCreate = sellingContracts.Adapt<IEnumerable<SellingContract>>();

                var result = await _sellingRepository.CreateSellingContracts(sellingContractToCreate);
                if (result.SuccessCount > 0)
                {
                    await _equipmentRepository.SetEquipmentBulkStatus(
                        equipmentIds,
                        EquipmentStatus.Sold);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                await InvalidateCache();
                return result;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<BulkOperationResult> SoftDeleteSellingContracts(IEnumerable<Guid> sellingContractsIds)
        {
            if (sellingContractsIds == null || !sellingContractsIds.Any())
            {
                throw new ArgumentNullException(nameof(sellingContractsIds),
                    "The list of selling contract IDs cannot be null or empty.");
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var contracts = await _sellingRepository.GetSellingContractsByIds(sellingContractsIds);
                var equipmentIds = contracts.Select(c => c.EquipmentId).Distinct().ToList();

                var result = await _sellingRepository.SoftDeleteSellingContracts(sellingContractsIds);
                if (result.SuccessCount > 0)
                {
                    await _equipmentRepository.SetEquipmentBulkStatus(
                        equipmentIds,
                        EquipmentStatus.Available);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                await InvalidateCache();
                return result;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<BulkOperationResult> RestoreSellingContracts(IEnumerable<Guid> sellingContractsIds)
        {
            if (sellingContractsIds == null || !sellingContractsIds.Any())
            {
                throw new ArgumentNullException(nameof(sellingContractsIds), "The list of selling contract IDs cannot be null or empty.");
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {

                var deletedContracts = await _sellingRepository
                    .GetSoftDeletedSellingContractsByIds(sellingContractsIds);

                if (!deletedContracts.Any())
                {
                    return new BulkOperationResult
                    {
                        SuccessCount = 0,
                        FailureCount = sellingContractsIds.Count(),
                    };
                }

                var equipmentIds = deletedContracts.Select(c => c.EquipmentId).Distinct().ToList();

                foreach (var deletedContract in deletedContracts)
                {
                    var equipmentStatus = await _equipmentRepository
                   .GetEquipmentStatus(deletedContract.EquipmentId);

                    if (equipmentStatus != EquipmentStatus.Available)
                    {
                        throw new InvalidOperationException(
                            $"Cannot restore contract. Equipment is currently {equipmentStatus}.");
                    }
                }

                var result = await _sellingRepository.RestoreSellingContracts(sellingContractsIds);

                if (result.SuccessCount > 0)
                {
                    await _equipmentRepository.SetEquipmentBulkStatus(
                        equipmentIds,
                        EquipmentStatus.Sold);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                await InvalidateCache();
                return result;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        #endregion

        #region Helpers

        private async Task<SellingContract> GetRequiredContractAsync(Guid id)
            => await _sellingRepository.GetSellingContractById(id)
               ?? throw new KeyNotFoundException($"Selling contract with id {id} not found.");

        private async Task ValidateCustomerAsync(Guid customerId)
        {
            if (!await _customerRepository.CustomerExists(customerId))
                throw new InvalidOperationException($"Customer with id {customerId} not found.");
        }

        private async Task ValidateEquipmentAvailableAsync(Guid equipmentId)
        {
            if (!await _equipmentRepository.EquipmentExists(equipmentId))
                throw new InvalidOperationException($"Equipment with id {equipmentId} not found.");

            var status = await _equipmentRepository.GetEquipmentStatus(equipmentId);
            if (status != EquipmentStatus.Available)
                throw new InvalidOperationException($"Equipment is currently {status}.");
        }

        private Task InvalidateCache()
             => _cacheVersionProvider.IncrementAsync(CacheScopes.SellingContracts);

        #endregion
    }
}
