using Application.BulkOperations;
using Application.Interface.Repositories;
using Application.ResourceParameters;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Shared.Extensions;
using Shared.Results;

namespace Infrastructure.Repositories
{
    public class RentalContractRepository : IRentalContractRepository
    {
        private readonly ApplicationDbContext _context;

        public RentalContractRepository(ApplicationDbContext context)
        {
            _context = context ??
                throw new ArgumentNullException(nameof(context));
        }

        public async Task<PagedList<RentalContract>?> GetRentalContracts(RentalContractResourceParameters parameters)
        {
            IQueryable<RentalContract> collection = _context.RentalContracts.AsQueryable();

            if (!string.IsNullOrEmpty(parameters.Fields))
            {
                collection = collection.ApplyInclude(parameters.Fields);
            }

            if (parameters.CustomerId != null)
            {
                collection = collection.Where(rc => rc.CustomerId == parameters.CustomerId);
            }

            if (parameters.EquipmentId != null)
            {
                collection = collection.Where(rc => rc.EquipmentId == parameters.EquipmentId);
            }

            if (parameters.FromDate != null)
            {
                collection = collection.Where(rc => rc.StartDate >= parameters.FromDate);
            }

            if (parameters.ToDate != null)
            {
                collection = collection.Where(rc => rc.EndDate <= parameters.ToDate);
            }

            if (parameters.Year != null)
            {
                collection = collection.Where(rc => rc.StartDate.Year == parameters.Year);
            }

            if (parameters.Month != null)
            {
                collection = collection.Where(rc => rc.StartDate.Month == parameters.Month);
            }

            if (parameters.MinShifts != null)
            {
                collection = collection.Where(rc => rc.Shifts >= parameters.MinShifts);
            }

            if (parameters.MaxShifts != null)
            {
                collection = collection.Where(rc => rc.Shifts <= parameters.MaxShifts);
            }

            if (parameters.MinShiftPrice != null)
            {
                collection = collection.Where(rc => rc.ShiftPrice >= parameters.MinShiftPrice);
            }

            if (parameters.MaxShiftPrice != null)
            {
                collection = collection.Where(rc => rc.ShiftPrice <= parameters.MaxShiftPrice);
            }

            if (parameters.MinContractPrice != null)
            {
                collection = collection.Where(rc => rc.RentalPrice >= parameters.MinContractPrice);
            }

            if (parameters.MaxContractPrice != null)
            {
                collection = collection.Where(rc => rc.RentalPrice <= parameters.MaxContractPrice);
            }

            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                collection = collection.OrderByProperty(parameters.SortBy, parameters.SortDescending);
            }

            var result = await PagedList<RentalContract>.Create(
            collection,
            parameters.PageNumber,
            parameters.PageSize);

            return result;
        }

        public async Task<RentalContract> GetRentalContractById(Guid id, string? fields)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id cannot be empty", nameof(id));

            IQueryable<RentalContract> collection = _context.RentalContracts.AsQueryable();

            if (!string.IsNullOrEmpty(fields))
            {
                collection = collection.ApplyInclude(fields);
            }

            var result = await collection
                .FirstOrDefaultAsync(rc => rc.Id == id);

            return result;
        }

        public async Task<IEnumerable<RentalContract>> GetRentalContractsByCustomerId(Guid customerId, string? fields)
        {
            if (customerId == Guid.Empty)
                throw new ArgumentException("Id cannot be empty", nameof(customerId));

            IQueryable<RentalContract> collection = _context.RentalContracts.AsQueryable();

            if (!string.IsNullOrEmpty(fields))
            {
                collection = collection.ApplyInclude(fields);
            }

            var result = await collection
                .Where(rc => rc.CustomerId == customerId).ToListAsync();

            return result;
        }

        public async Task<IEnumerable<RentalContract>> GetRentalContractsByEquipmentId(Guid equipmentId, string? fields)
        {
            if (equipmentId == Guid.Empty)
                throw new ArgumentException("Id cannot be empty", nameof(equipmentId));

            IQueryable<RentalContract> collection = _context.RentalContracts.AsQueryable();

            if (!string.IsNullOrEmpty(fields))
            {
                collection = collection.ApplyInclude(fields);
            }

            var result = await collection
                .Where(rc => rc.EquipmentId == equipmentId).ToListAsync();

            return result;
        }

        public async Task<IEnumerable<RentalContract>> GetRentalContractsByIds(IEnumerable<Guid> ids, string? fields)
        {
            if (ids == null || !ids.Any()) return [];

            IQueryable<RentalContract> collection =
                _context.RentalContracts
                .Where(c => ids.Contains(c.Id))
                .AsQueryable();

            if (!string.IsNullOrEmpty(fields))
            {
                collection = collection.ApplyInclude(fields);
            }

            var rentalContracts = await collection.ToListAsync();

            return rentalContracts;
        }

        public async Task<IEnumerable<RentalContract>> GetActiveContracts(string? fields)
        {
            IQueryable<RentalContract> collection =
                _context.RentalContracts
                .Where(rc => rc.EndDate >= DateTimeOffset.UtcNow)
                .AsQueryable();

            if (!string.IsNullOrEmpty(fields))
            {
                collection = collection.ApplyInclude(fields);
            }

            return await collection.ToListAsync();
        }

        public async Task<IEnumerable<RentalContract>> GetExpiredContracts(
            int daysUntilExpiration = 30,
            string? fields = null)
        {
            var expirationDate = DateTimeOffset.UtcNow.AddDays(daysUntilExpiration);

            IQueryable<RentalContract> collection = _context.RentalContracts
               .Where(rc => rc.EndDate <= expirationDate
                         && rc.EndDate >= DateTimeOffset.UtcNow)
                .AsQueryable();

            if (!string.IsNullOrEmpty(fields))
            {
                collection = collection.ApplyInclude(fields);
            }
            return await collection.ToListAsync();
        }

        public async Task<RentalContract?> GetDeletedRentalContractById(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id cannot be empty", nameof(id));

            return await _context.RentalContracts
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(rc => rc.Id == id && rc.DeletedDate != null);
        }

        public async Task<bool> RentalContractExists(Guid id)
        {
            return await _context.RentalContracts.AnyAsync(rc => rc.Id == id);
        }

        public async Task<bool> CustomerHasContracts(Guid customerId)
        {
            return await _context.RentalContracts.AnyAsync(rc => rc.CustomerId == customerId);
        }

        public async Task<bool> EquipmentHasContracts(Guid equipmentId)
        {
            return await _context.RentalContracts.AnyAsync(rc => rc.EquipmentId == equipmentId);
        }

        public async Task<bool> HasOverlappingContracts(
            Guid equipmentId,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            Guid? excludeContractId = null)
        {
            var query = _context.RentalContracts
                .Where(rc => rc.EquipmentId == equipmentId
                          && rc.StartDate < endDate
                          && rc.EndDate > startDate);

            if (excludeContractId.HasValue)
            {
                query = query.Where(rc => rc.Id != excludeContractId.Value);
            }
            return await query.AnyAsync();
        }

        public async Task CreateRentalContract(RentalContract rentalContract)
        {
            ArgumentNullException.ThrowIfNull(rentalContract, nameof(rentalContract));
            await _context.RentalContracts.AddAsync(rentalContract);
        }

        public async Task<IEnumerable<RentalContract>> GetActiveContractsEndingBefore(DateTimeOffset date)
        {
            return await _context.RentalContracts
                .Where(rc =>
                    rc.Status == RentalContractStatus.Active &&
                    rc.DeletedDate == null &&
                    rc.EndDate <= date)
                .ToListAsync();
        }

        public async Task<BulkOperationResult> CreateRentalContracts(IEnumerable<RentalContract> rentalContracts)
        {
            if (rentalContracts == null || !rentalContracts.Any())
                throw new ArgumentException("Customers collection cannot be empty", nameof(rentalContracts));
            var result = new BulkOperationResult();
            var rentalContractsList = rentalContracts.ToList();

            foreach (var rentalContract in rentalContractsList)
            {
                try
                {
                    await CreateRentalContract(rentalContract);
                    result.SuccessCount++;
                    result.SuccessIds.Add(rentalContract.Id);
                }
                catch (Exception ex)
                {
                    result.FailureCount++;
                    result.Errors.Add(new BulkOperationError
                    {
                        EntityId = rentalContract.Id,
                        ErrorMessage = ex.Message
                    });
                }
            }
            return result;
        }

        public async Task UpdateRentalContract(RentalContract rentalContract)
        {
            ArgumentNullException.ThrowIfNull(rentalContract, nameof(rentalContract));
            var existingContract = await _context.RentalContracts
                .FirstOrDefaultAsync(rc => rc.Id == rentalContract.Id);

            if (existingContract == null)
                throw new KeyNotFoundException(
                    $"Rental contract with id {rentalContract.Id} not found.");

            if (!existingContract.RowVersion.SequenceEqual(rentalContract.RowVersion))
            {
                throw new DbUpdateConcurrencyException("The entity has been modified by another user.");
            }

            rentalContract.UpdateDate = DateTimeOffset.UtcNow;
            _context.RentalContracts.Update(rentalContract);
        }

        public async Task SoftDeleteRentalContract(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id cannot be empty", nameof(id));
            var contract = await _context.RentalContracts.FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new KeyNotFoundException($"Rental contract with id {id} not found.");
            contract.DeletedDate = DateTimeOffset.UtcNow;
            _context.RentalContracts.Update(contract);
        }

        public async Task<BulkOperationResult> DeleteRentalContracts(IEnumerable<Guid> rentalContractsIds)
        {
            if (rentalContractsIds == null || !rentalContractsIds.Any())
                throw new ArgumentException("Rental Contracts ids cannot be empty", nameof(rentalContractsIds));

            var result = new BulkOperationResult();
            var idsList = rentalContractsIds.ToList();

            foreach (var id in idsList)
            {
                try
                {
                    await SoftDeleteRentalContract(id);
                    result.SuccessCount++;
                    result.SuccessIds.Add(id);
                }
                catch (Exception ex)
                {
                    result.FailureCount++;
                    result.Errors.Add(new BulkOperationError
                    {
                        EntityId = id,
                        ErrorMessage = ex.Message
                    });
                }
            }

            return result;
        }

        public async Task<IEnumerable<RentalContract>> GetDeletedRentalContracts()
        {
            var contracts = await _context.RentalContracts
                .IgnoreQueryFilters()
                .Where(rc => rc.DeletedDate != null)
                .ToListAsync();

            return contracts;
        }

        public async Task<bool> RestoreRentalContract(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id cannot be empty", nameof(id));

            var contract = await _context
                .RentalContracts
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id) ??
                throw new KeyNotFoundException($"Rental contract with id {id} not found.");

            if (contract.DeletedDate == null)
            {
                throw new InvalidOperationException($"Rental contract with id {id} is not deleted.");
            }
            contract.DeletedDate = null;
            _context.RentalContracts.Update(contract);
            return true;
        }

        public async Task<BulkOperationResult> RestoreRentalContracts(IEnumerable<Guid> rentalContractsIds)
        {
            if (rentalContractsIds == null || !rentalContractsIds.Any())
                throw new ArgumentException("Rental Contracts ids cannot be empty", nameof(rentalContractsIds));

            var result = new BulkOperationResult();
            var idsList = rentalContractsIds.ToList();

            foreach (var id in idsList)
            {
                try
                {
                    await RestoreRentalContract(id);
                    result.SuccessCount++;
                    result.SuccessIds.Add(id);
                }
                catch (Exception ex)
                {
                    result.FailureCount++;
                    result.Errors.Add(new BulkOperationError
                    {
                        EntityId = id,
                        ErrorMessage = ex.Message
                    });
                }
            }

            return result;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() > 0);
        }
    }
}