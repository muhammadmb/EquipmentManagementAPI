using Application.BulkOperations;
using Application.Interface;
using Application.Interface.Repositories;
using Application.Interface.Services;
using Application.Models.RentalContractModels.Read;
using Application.Models.RentalContractModels.Write;
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
    public class RentalContractService : IRentalContractService
    {
        private readonly IRentalContractRepository _contractRepository;
        private readonly IEquipmentRepository _equipmentRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ICacheVersionProvider _cacheVersionProvider;
        private readonly IUnitOfWork _unitOfWork;

        public RentalContractService(
            IRentalContractRepository contractRepository,
            IEquipmentRepository equipmentRepository,
            ICustomerRepository customerRepository,
            ICacheVersionProvider versionProvider,
            IUnitOfWork unitOfWork)
        {
            _contractRepository = contractRepository ??
                throw new ArgumentNullException(nameof(contractRepository));
            _equipmentRepository = equipmentRepository ??
                throw new ArgumentNullException(nameof(equipmentRepository));
            _customerRepository = customerRepository ??
                throw new ArgumentNullException(nameof(customerRepository));
            _cacheVersionProvider = versionProvider ??
                throw new ArgumentNullException(nameof(versionProvider));
            _unitOfWork = unitOfWork ??
                throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<PagedList<RentalContractDto>?> GetRentalContracts(
            RentalContractResourceParameters parameters)
        {
            var rentalContracts = await _contractRepository.GetRentalContracts(parameters);
            var rentalContractsDto = rentalContracts.Adapt<List<RentalContractDto>>();

            return new PagedList<RentalContractDto>(rentalContractsDto, rentalContracts.TotalCount, rentalContracts.CurrentPage, rentalContracts.PageSize);
        }

        public async Task<RentalContractDto?> GetRentalContractById(Guid id, string? fields)
        {
            var rentalContract = await _contractRepository.GetRentalContractById(id, fields);
            var rentalContractDto = rentalContract.Adapt<RentalContractDto>();
            return rentalContractDto;
        }

        public async Task<IEnumerable<RentalContractDto>> GetRentalContractsByCustomerId(
            Guid customerId,
            string? fields)
        {
            var customerRentalContracts = await _contractRepository.GetRentalContractsByCustomerId(customerId, fields);
            var customerRentalContractsDto = customerRentalContracts.Adapt<IEnumerable<RentalContractDto>>();
            return customerRentalContractsDto;
        }

        public async Task<IEnumerable<RentalContractDto>> GetRentalContractsByEquipmentId(
            Guid equipmentId,
            string? fields)
        {
            var equipmentRentalContracts = await _contractRepository.GetRentalContractsByEquipmentId(equipmentId, fields);
            var equipmentRentalContractsDto = equipmentRentalContracts.Adapt<IEnumerable<RentalContractDto>>();
            return equipmentRentalContractsDto;
        }

        public async Task<IEnumerable<RentalContractDto>> GetRentalContractsByIds(IEnumerable<Guid> ids, string? fields)
        {
            var rentalContracts = await _contractRepository.GetRentalContractsByIds(ids, fields);
            var rentalContractsDto = rentalContracts.Adapt<IEnumerable<RentalContractDto>>();

            return rentalContractsDto;
        }

        public async Task<IEnumerable<RentalContractDto>> GetActiveContracts(string? fields)
        {
            var activeRentalContracts = await _contractRepository.GetActiveContracts(fields);
            var activeRentalContractsDto = activeRentalContracts.Adapt<IEnumerable<RentalContractDto>>();
            return activeRentalContractsDto;
        }

        public async Task<IEnumerable<RentalContractDto>> GetExpiredContracts(
            int daysUntilExpiration,
            string? fields)
        {
            var expiredRentalContracts = await _contractRepository.GetExpiredContracts(daysUntilExpiration, fields);
            var expiredRentalContractsDto = expiredRentalContracts.Adapt<IEnumerable<RentalContractDto>>();
            return expiredRentalContractsDto;
        }

        public async Task<RentalContractDto?> GetDeletedRentalContractById(Guid id)
        {
            var deletedRentalContract = await _contractRepository.GetDeletedRentalContractById(id);
            var deletedRentalContractDto = deletedRentalContract.Adapt<RentalContractDto>();
            return deletedRentalContractDto;
        }

        public async Task<IEnumerable<RentalContractDto>> GetDeletedRentalContracts()
        {
            var deletedRentalContracts = await _contractRepository.GetDeletedRentalContracts();
            var deletedRentalContractsDto = deletedRentalContracts.Adapt<IEnumerable<RentalContractDto>>();
            return deletedRentalContractsDto;
        }

        public async Task<bool> RentalContractExists(Guid id)
        {
            return await _contractRepository.RentalContractExists(id);
        }
        public async Task<bool> CustomerHasContracts(Guid customerId)
        {
            return await _contractRepository.CustomerHasContracts(customerId);
        }
        public async Task<bool> EquipmentHasContracts(Guid equipmentId)
        {
            return await _contractRepository.EquipmentHasContracts(equipmentId);
        }
        public async Task<bool> HasOverlappingContracts(
            Guid equipmentId,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            Guid? excludeContractId = null)
        {
            return await _contractRepository.HasOverlappingContracts(equipmentId, startDate, endDate, excludeContractId);
        }

        public async Task ActiveRentalContract(Guid contractId)
        {

            var contract = await _contractRepository.GetRentalContractForUpdate(contractId, null)
                         ?? throw new KeyNotFoundException($"Contract {contractId} not found.");

            contract.Activate();

            await _equipmentRepository.SetEquipmentStatus(
                contract.EquipmentId,
                EquipmentStatus.Rented);
            await _unitOfWork.SaveChangesAsync();
            await _cacheVersionProvider.IncrementAsync(CacheScopes.RentalContracts);
        }

        public async Task CancelRentalContract(Guid contractId)
        {
            var contract = await _contractRepository.GetRentalContractForUpdate(contractId, null)
                         ?? throw new KeyNotFoundException($"Contract {contractId} not found.");

            contract.Cancel();

            await _equipmentRepository.SetEquipmentStatus(
                contract.EquipmentId,
                EquipmentStatus.Available);
            await _unitOfWork.SaveChangesAsync();
            await _cacheVersionProvider.IncrementAsync(CacheScopes.RentalContracts);
        }
        public async Task SuspendRentalContract(Guid contractId)
        {
            var contract = await _contractRepository.GetRentalContractForUpdate(contractId, null)
                         ?? throw new KeyNotFoundException($"Contract {contractId} not found.");

            contract.Suspend();

            await _equipmentRepository.SetEquipmentStatus(
                contract.EquipmentId,
                EquipmentStatus.Available);
            await _unitOfWork.SaveChangesAsync();
            await _cacheVersionProvider.IncrementAsync(CacheScopes.RentalContracts);
        }
        public async Task ResumeRentalContract(Guid contractId)
        {
            var contract = await _contractRepository.GetRentalContractForUpdate(contractId, null)
                         ?? throw new KeyNotFoundException($"Contract {contractId} not found.");

            contract.Resume();

            await _equipmentRepository.SetEquipmentStatus(
                contract.EquipmentId,
                EquipmentStatus.Rented);
            await _unitOfWork.SaveChangesAsync();
            await _cacheVersionProvider.IncrementAsync(CacheScopes.RentalContracts);
        }
        public async Task FinishRentalContract(Guid contractId)
        {
            var contract = await _contractRepository.GetRentalContractForUpdate(contractId, null)
                         ?? throw new KeyNotFoundException($"Contract {contractId} not found.");

            contract.Finish();

            await _equipmentRepository.SetEquipmentStatus(
                contract.EquipmentId,
                EquipmentStatus.Available);
            await _unitOfWork.SaveChangesAsync();
            await _cacheVersionProvider.IncrementAsync(CacheScopes.RentalContracts);
        }
        public async Task FinishExpiredContracts()
        {
            var contracts = await _contractRepository.GetActiveContractsEndingBefore(DateTimeOffset.UtcNow);

            foreach (var contract in contracts)
            {
                contract.Finish();

                await _equipmentRepository.SetEquipmentStatus(
                    contract.EquipmentId,
                    EquipmentStatus.Available);
            }
            await _unitOfWork.SaveChangesAsync();
            await _cacheVersionProvider.IncrementAsync(CacheScopes.RentalContracts);
        }

        public async Task<RentalContractDto> CreateRentalContract(RentalContractCreateDto rentalContractCreateDto)
        {
            ArgumentNullException.ThrowIfNull(rentalContractCreateDto, nameof(rentalContractCreateDto));

            if (rentalContractCreateDto.StartDate >= rentalContractCreateDto.EndDate)
            {
                throw new InvalidOperationException(
                    "End date must be after start date.");
            }

            if (rentalContractCreateDto.StartDate < DateTimeOffset.UtcNow)
            {
                throw new InvalidOperationException(
                    "Cannot create contract with past start date.");
            }

            var customerExists = await _customerRepository.CustomerExists(rentalContractCreateDto.CustomerId);
            if (!customerExists)
            {
                throw new InvalidOperationException(
                    $"Customer with id {rentalContractCreateDto.CustomerId} not found.");
            }

            var equipmentExists = await _equipmentRepository.EquipmentExists(rentalContractCreateDto.EquipmentId);
            if (!equipmentExists)
            {
                throw new InvalidOperationException(
                    $"Equipment with id {rentalContractCreateDto.EquipmentId} not found.");
            }

            var hasOverlap = await _contractRepository.HasOverlappingContracts(
                rentalContractCreateDto.EquipmentId,
                rentalContractCreateDto.StartDate,
                rentalContractCreateDto.EndDate);

            if (hasOverlap)
            {
                throw new InvalidOperationException(
                    "Equipment is already booked for the selected dates.");
            }

            var equipmentStatus = await _equipmentRepository.GetEquipmentStatus(rentalContractCreateDto.EquipmentId);

            if (!equipmentStatus.Equals(EquipmentStatus.Available))
            {
                throw new InvalidOperationException(
                    $"Equipment is currently {equipmentStatus}. You must use available equipment.");
            }

            if (rentalContractCreateDto.Shifts <= 0)
            {
                throw new InvalidOperationException("Shifts must be greater than zero.");
            }

            var rentalEquipment = rentalContractCreateDto.Adapt<RentalContract>();

            await _contractRepository.CreateRentalContract(rentalEquipment);
            await _equipmentRepository.SetEquipmentStatus(rentalEquipment.EquipmentId, EquipmentStatus.Rented);
            await _unitOfWork.SaveChangesAsync();
            await _cacheVersionProvider.IncrementAsync(CacheScopes.RentalContracts);
            return rentalEquipment.Adapt<RentalContractDto>();
        }

        public async Task<BulkOperationResult> CreateRentalContracts(
            IEnumerable<RentalContractCreateDto> rentalContracts)
        {
            if (rentalContracts == null || !rentalContracts.Any())
            {
                throw new ArgumentException("Rental contracts collection cannot be empty",
                    nameof(rentalContracts));
            }

            foreach (var contract in rentalContracts)
            {
                if (contract.StartDate >= contract.EndDate)
                {
                    throw new InvalidOperationException(
                        $"Contract {contract}: End date must be after start date.");
                }

                var equipmentStatus = await _equipmentRepository.GetEquipmentStatus(contract.EquipmentId);

                if (!equipmentStatus.Equals(EquipmentStatus.Available))
                {
                    throw new InvalidOperationException(
                         $"Equipment {contract.EquipmentId} is currently {equipmentStatus}. " +
                        $"You must use available equipment.");
                }
            }

            var equipmentIdsList = rentalContracts
                .Select(rc => rc.EquipmentId)
                .Distinct()
                .ToList();

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var rentalConractToCreate = rentalContracts.Adapt<IEnumerable<RentalContract>>();

                var result = await _contractRepository.CreateRentalContracts(rentalConractToCreate);
                if (result.SuccessCount > 0)
                {
                    await _equipmentRepository.SetEquipmentBulkStatus(
                        equipmentIdsList,
                        EquipmentStatus.Rented);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                await _cacheVersionProvider.IncrementAsync(CacheScopes.RentalContracts);
                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateRentalContract(Guid id, RentalContractUpdateDto rentalContract)
        {
            ArgumentNullException.ThrowIfNull(rentalContract, nameof(rentalContract));

            var rentalContractFromRepo = await _contractRepository.GetRentalContractById(id, null);
            if (rentalContractFromRepo == null)
            {
                throw new KeyNotFoundException(
                    $"Rental contract with id {id} not found.");
            }

            if (rentalContract.StartDate >= rentalContract.EndDate)
            {
                throw new InvalidOperationException(
                    "End date must be after start date.");
            }

            var hasOverlap = await _contractRepository.HasOverlappingContracts(
                rentalContract.EquipmentId,
                rentalContract.StartDate,
                rentalContract.EndDate,
                id);

            if (hasOverlap)
            {
                throw new InvalidOperationException(
                    "Equipment is already booked for the selected dates.");
            }

            if (rentalContract.Shifts <= 0)
            {
                throw new InvalidOperationException("Shifts must be greater than zero.");
            }

            if (rentalContract.RentalPrice <= 0)
            {
                throw new InvalidOperationException("Rental price must be greater than zero.");
            }

            rentalContract.Adapt(rentalContractFromRepo);
            await _contractRepository.UpdateRentalContract(rentalContractFromRepo);
            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new DbUpdateConcurrencyException("The entity has been modified by another user.");
            }
            await _cacheVersionProvider.IncrementAsync(CacheScopes.RentalContracts);
        }

        public async Task Patch(Guid id, JsonPatchDocument<RentalContractUpdateDto> patchDocument)
        {
            if (patchDocument is null)
            {
                throw new ValidationException(
                    "Invalid patch document.");
            }

            var rentalContractFromRepo = await _contractRepository.GetRentalContractById(id, null);
            if (rentalContractFromRepo == null)
            {
                throw new KeyNotFoundException($"Rental contract with id {id} not found.");
            }

            var rentalContract = rentalContractFromRepo.Adapt<RentalContractUpdateDto>();
            patchDocument.ApplyTo(rentalContract);
            rentalContract.Adapt(rentalContractFromRepo);
            await _contractRepository.UpdateRentalContract(rentalContractFromRepo);
            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new DbUpdateConcurrencyException("The entity has been modified by another user.");
            }
            await _cacheVersionProvider.IncrementAsync(CacheScopes.RentalContracts);
        }

        public async Task SoftDeleteRentalContract(Guid id)
        {
            var rentalContract = await _contractRepository.GetRentalContractById(id, null);
            if (rentalContract == null)
            {
                throw new KeyNotFoundException($"Rental contract with id {id} not found.");
            }

            await _contractRepository.SoftDeleteRentalContract(id);
            await _equipmentRepository.SetEquipmentStatus(
                rentalContract.EquipmentId,
                EquipmentStatus.Available);
            await _unitOfWork.SaveChangesAsync();
            await _cacheVersionProvider.IncrementAsync(CacheScopes.RentalContracts);
        }

        public async Task<BulkOperationResult> DeleteRentalContracts(IEnumerable<Guid> ids)
        {
            if (ids == null || !ids.Any())
            {
                throw new ArgumentException("Ids collection cannot be empty", nameof(ids));
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var contracts = await _contractRepository.GetRentalContractsByIds(ids, null);
                var equipmentIdsList = contracts
                    .Select(c => c.EquipmentId)
                    .Distinct()
                    .ToList();

                var result = await _contractRepository.DeleteRentalContracts(ids);

                if (result.SuccessCount > 0 && equipmentIdsList.Any())
                {
                    await _equipmentRepository.SetEquipmentBulkStatus(
                        equipmentIdsList,
                        EquipmentStatus.Available);
                }
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                await _cacheVersionProvider.IncrementAsync(CacheScopes.RentalContracts);
                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task RestoreRentalContract(Guid id)
        {
            var rentalContract = await _contractRepository.GetDeletedRentalContractById(id);

            if (rentalContract == null)
            {
                throw new KeyNotFoundException($"Rental contract with id {id} not found.");
            }
            await _contractRepository.RestoreRentalContract(id);
            await _equipmentRepository.SetEquipmentStatus(
                rentalContract.EquipmentId,
                EquipmentStatus.Rented);
            await _unitOfWork.SaveChangesAsync();
            await _cacheVersionProvider.IncrementAsync(CacheScopes.RentalContracts);
        }

        public async Task<BulkOperationResult> RestoreRentalContracts(IEnumerable<Guid> ids)
        {
            if (ids == null || !ids.Any())
            {
                throw new ArgumentException("Ids collection cannot be empty", nameof(ids));
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var deletedContracts = await _contractRepository.GetDeletedRentalContracts();
                var contractsToRestore = deletedContracts
                    .Where(c => ids.Contains(c.Id))
                    .ToList();

                var equipmentIdsList = contractsToRestore
                    .Select(c => c.EquipmentId)
                    .Distinct()
                    .ToList();

                var result = await _contractRepository.RestoreRentalContracts(ids);

                if (result.SuccessCount > 0 && equipmentIdsList.Any())
                {
                    await _equipmentRepository.SetEquipmentBulkStatus(
                        equipmentIdsList,
                        EquipmentStatus.Rented);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                await _cacheVersionProvider.IncrementAsync(CacheScopes.RentalContracts);
                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}
