using Application.BulkOperations;
using Application.Interface.Repositories;
using Application.ResourceParameters;
using Domain.Entities;
using Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Shared.Extensions;
using Shared.Results;

namespace Infrastructure.Repositories
{
    public class SellingContractRepository : ISellingContractRepository
    {
        private readonly ApplicationDbContext _context;

        public SellingContractRepository(ApplicationDbContext context)
        {
            _context = context
                ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<PagedList<SellingContract>> GetSellingContracts(SellingContractResourceParameters parameters)
        {
            var collection = SellingContractsQuery(parameters, includeSoftDeleted: false);

            var result = await PagedList<SellingContract>.Create(collection,
                parameters.PageNumber,
                parameters.PageSize);

            return result;
        }

        public async Task<SellingContract> GetSellingContractById(Guid id, string? fields = null)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id));
            }

            IQueryable<SellingContract> collection = _context.Sellings.AsQueryable();

            if (!string.IsNullOrEmpty(fields))
            {
                collection = collection.ApplyInclude(fields);
            }

            var result = await collection
                .FirstOrDefaultAsync(sc => sc.Id == id);

            return result;
        }

        public async Task<SellingContract> GetSellingContractByYear(int year)
        {
            return await _context.Sellings
                .FirstOrDefaultAsync(sc => sc.SaleDate.Year == year);
        }

        public async Task<SellingContract> GetSellingContractForUpdate(Guid id, string? fields = null)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id));
            }

            IQueryable<SellingContract> collection = _context.Sellings.AsQueryable();

            if (!string.IsNullOrEmpty(fields))
            {
                collection = collection.ApplyInclude(fields);
            }

            var result = await collection
                .AsTracking()
                .FirstOrDefaultAsync(sc => sc.Id == id);

            return result;
        }

        public async Task<IEnumerable<SellingContract>> GetSellingContractsByCustomerId(Guid customerId, string? fields = null)
        {
            if (customerId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(customerId));
            }

            IQueryable<SellingContract> collection = _context.Sellings.AsQueryable();

            if (!string.IsNullOrEmpty(fields))
            {
                collection = collection.ApplyInclude(fields);
            }

            var result = await collection
                .Where(sc => sc.CustomerId == customerId)
                .ToListAsync();
            return result;
        }

        public async Task<SellingContract> GetSoftDeletedSellingContractsById(Guid id, string? fields = null)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id));
            }

            IQueryable<SellingContract> collection =
                _context.Sellings.IgnoreQueryFilters().AsQueryable();

            if (!string.IsNullOrEmpty(fields))
            {
                collection = collection.ApplyInclude(fields);
            }

            var result = await collection
                .FirstOrDefaultAsync(sc => sc.Id == id && sc.DeletedDate != null);
            return result;
        }

        public async Task<IEnumerable<SellingContract>> GetSellingContractsByEquipment(Guid equipmentId, string? fields = null)
        {
            if (equipmentId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(equipmentId));
            }
            IQueryable<SellingContract> collection = _context.Sellings.AsQueryable();
            if (!string.IsNullOrEmpty(fields))
            {
                collection = collection.ApplyInclude(fields);
            }
            var result = await collection
                .Where(sc => sc.EquipmentId == equipmentId)
                .ToListAsync();
            return result;
        }
        public async Task<IEnumerable<SellingContract>> GetSellingContractsByIds(IEnumerable<Guid> ids, string? fields = null)
        {
            if (ids == null || !ids.Any())
            {
                throw new ArgumentNullException(nameof(ids));
            }

            IQueryable<SellingContract> collection = _context.Sellings.AsQueryable();

            if (!string.IsNullOrEmpty(fields))
            {
                collection = collection.ApplyInclude(fields);
            }
            var result = await collection
                .Where(sc => ids.Contains(sc.Id))
                .ToListAsync();
            return result;
        }

        public async Task<IEnumerable<SellingContract>> GetSoftDeletedSellingContracts(SellingContractResourceParameters parameters)
        {
            var collection = SellingContractsQuery(parameters, includeSoftDeleted: true);

            collection = collection.Where(s => s.DeletedDate != null);

            return await collection.ToListAsync();
        }

        public async Task<IEnumerable<SellingContract>> GetSoftDeletedSellingContractsByIds(IEnumerable<Guid> ids, string? fields = null)
        {
            if (ids == null || !ids.Any())
            {
                throw new ArgumentNullException(nameof(ids));
            }

            IQueryable<SellingContract> collection = _context.Sellings
                .IgnoreQueryFilters()
                .AsQueryable();

            if (!string.IsNullOrEmpty(fields))
            {
                collection = collection.ApplyInclude(fields);
            }

            var result = await collection
                .Where(sc => ids.Contains(sc.Id))
                .ToListAsync();
            return result;
        }

        public async Task<bool> SellingContractExists(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id));
            }
            return await _context.Sellings.AnyAsync(sc => sc.Id == id);
        }

        public async Task<bool> CustomerHasContracts(Guid customerId)
        {
            if (customerId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(customerId));
            }

            return await _context.Sellings.AnyAsync(sc => sc.CustomerId == customerId);
        }

        public async Task CreateSellingContract(SellingContract sellingContract)
        {
            ArgumentNullException.ThrowIfNull(sellingContract);
            await _context.Sellings.AddAsync(sellingContract);
        }

        public async Task UpdateSellingContract(SellingContract sellingContract)
        {
            ArgumentNullException.ThrowIfNull(sellingContract);
            var existingSellingContract = await _context.Sellings
                .FirstOrDefaultAsync(sc => sc.Id == sellingContract.Id)
                ?? throw new KeyNotFoundException($"Selling contract with ID {sellingContract.Id} not found.");

            if (!existingSellingContract.RowVersion.SequenceEqual(sellingContract.RowVersion))
            {
                throw new DbUpdateConcurrencyException("The selling contract has been modified by another user.");
            }

            sellingContract.UpdateDate = DateTimeOffset.UtcNow;
            _context.Sellings.Update(sellingContract);
        }

        public async Task SoftDeleteSellingContract(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id cannot be empty.", nameof(id));

            var existingSellingContract = await _context.Sellings
                .FirstOrDefaultAsync(sc => sc.Id == id)
                ?? throw new KeyNotFoundException($"Selling contract with ID {id} not found.");

            if (existingSellingContract.DeletedDate != null)
            {
                throw new InvalidOperationException($"Selling contract with ID {id} is already deleted.");
            }

            existingSellingContract.DeletedDate = DateTimeOffset.UtcNow;
            _context.Sellings.Update(existingSellingContract);
        }

        public async Task RestoreSellingContract(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id cannot be empty.", nameof(id));

            var sellingContract = await _context.Sellings
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(sc => sc.Id == id) ??
                throw new KeyNotFoundException($"Selling contract with id {id} not found.");

            if (sellingContract.DeletedDate == null)
                throw new InvalidOperationException($"Selling contract with id {id} is not deleted.");

            sellingContract.DeletedDate = null;
            _context.Sellings.Update(sellingContract);
        }

        public async Task<BulkOperationResult> CreateSellingContracts(IEnumerable<SellingContract> sellingContracts)
        {
            if (sellingContracts == null || !sellingContracts.Any())
                throw new ArgumentNullException(nameof(sellingContracts));
            var result = new BulkOperationResult();
            var contracts = sellingContracts.ToList();
            foreach (var contract in contracts)
            {
                try
                {
                    await CreateSellingContract(contract);
                    result.SuccessCount++;
                    result.SuccessIds.Add(contract.Id);
                }
                catch (Exception ex)
                {
                    result.FailureCount++;
                    result.Errors.Add(new BulkOperationError
                    {
                        EntityId = contract.Id,
                        ErrorMessage = ex.Message
                    });
                }
            }
            return result;
        }

        public async Task<BulkOperationResult> SoftDeleteSellingContracts(IEnumerable<Guid> sellingContractsIds)
        {
            if (sellingContractsIds == null || !sellingContractsIds.Any())
                throw new ArgumentNullException(nameof(sellingContractsIds));
            var result = new BulkOperationResult();
            var ids = sellingContractsIds.ToList();
            foreach (var id in ids)
            {
                try
                {
                    await SoftDeleteSellingContract(id);
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

        public async Task<BulkOperationResult> RestoreSellingContracts(IEnumerable<Guid> sellingContractsIds)
        {
            if (sellingContractsIds == null || !sellingContractsIds.Any())
                throw new ArgumentNullException(nameof(sellingContractsIds));

            var result = new BulkOperationResult();
            var ids = sellingContractsIds.ToList();

            foreach (var id in ids)
            {
                try
                {
                    await RestoreSellingContract(id);
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

        private IQueryable<SellingContract> SellingContractsQuery(
            SellingContractResourceParameters parameters,
            bool includeSoftDeleted)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            IQueryable<SellingContract> collection = _context.Sellings;

            if (includeSoftDeleted)
                collection = collection.IgnoreQueryFilters();

            if (!string.IsNullOrEmpty(parameters.Fields))
                collection = collection.ApplyInclude(parameters.Fields);

            if (!string.IsNullOrEmpty(parameters.SearchQuery))
            {
                var searchQuery = parameters.SearchQuery.Trim().ToLowerInvariant();
                collection = collection.Where(sc =>
                    sc.Customer.Name.ToLower().Contains(searchQuery) ||
                    sc.Equipment.Name.ToLower().Contains(searchQuery));
            }

            if (parameters.CustomerId.HasValue && parameters.CustomerId != Guid.Empty)
                collection = collection.Where(sc => sc.CustomerId == parameters.CustomerId);

            if (parameters.EquipmentId.HasValue && parameters.EquipmentId != Guid.Empty)
                collection = collection.Where(sc => sc.EquipmentId == parameters.EquipmentId);

            if (parameters.FromDate.HasValue)
                collection = collection.Where(sc => sc.SaleDate >= parameters.FromDate);

            if (parameters.ToDate.HasValue)
                collection = collection.Where(sc => sc.SaleDate <= parameters.ToDate);

            if (parameters.Year.HasValue)
                collection = collection.Where(sc => sc.SaleDate.Year == parameters.Year);

            if (parameters.Month.HasValue)
                collection = collection.Where(sc => sc.SaleDate.Month == parameters.Month);

            if (!string.IsNullOrEmpty(parameters.SortBy))
                collection = collection.OrderByProperty(parameters.SortBy, parameters.SortDescending);

            return collection;
        }
    }
}
