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
    public class EquipmentRepository : IEquipmentRepository, IDisposable
    {
        private readonly ApplicationDbContext _context;
        public EquipmentRepository(ApplicationDbContext context)
        {
            _context = context ??
                throw new ArgumentNullException(nameof(context));
        }

        public async Task<PagedList<Equipment>> GetEquipment(EquipmentResourceParameters parameters)
        {
            var query = EquipmentQuery(parameters, includeSoftDeleted: false);

            return await PagedList<Equipment>.Create(query, parameters.PageNumber, parameters.PageSize);
        }

        public async Task<Equipment?> GetEquipmentById(Guid equipmentId, string? fields)
        {
            if (equipmentId == Guid.Empty)
                throw new ArgumentException("Id cannot be empty", nameof(equipmentId));

            IQueryable<Equipment> query = _context.Equipments.AsQueryable();

            if (!string.IsNullOrWhiteSpace(fields))
            {
                query = query.ApplyInclude(fields);
            }

            return await query
               .FirstOrDefaultAsync(e => e.Id == equipmentId);
        }

        public async Task<Equipment?> GetSoftDeletedEquipmentById(Guid equipmentId, string? fields = null)
        {
            IQueryable<Equipment> query = _context.Equipments
                .IgnoreQueryFilters()
                .AsQueryable();

            if (equipmentId == Guid.Empty)
                throw new ArgumentException("Id cannot be empty", nameof(equipmentId));

            if (!string.IsNullOrWhiteSpace(fields))
            {
                query = query.ApplyInclude(fields);
            }

            return await query
               .FirstOrDefaultAsync(e => e.Id == equipmentId);
        }

        public async Task<Equipment?> GetEquipmentForUpdate(Guid equipmentId, string? fields = null)
        {
            if (equipmentId == Guid.Empty)
                throw new ArgumentException("Id cannot be empty", nameof(equipmentId));

            IQueryable<Equipment> query = _context.Equipments
                .AsTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(fields))
            {
                query = query.ApplyInclude(fields);
            }
            return await query
               .FirstOrDefaultAsync(e => e.Id == equipmentId);
        }

        public async Task<PagedList<Equipment>> GetSoftDeletedEquipment(EquipmentResourceParameters parameters)
        {
            var query = EquipmentQuery(parameters, includeSoftDeleted: true);
            return await PagedList<Equipment>.Create(query, parameters.PageNumber, parameters.PageSize);
        }

        public async Task<PagedList<Equipment>> GetSoftDeletedEquipmentByIds(IEnumerable<Guid> ids, EquipmentResourceParameters parameters)
        {
            if (ids == null || !ids.Any())
                throw new ArgumentException("Ids cannot be empty", nameof(ids));

            var query = EquipmentQuery(parameters, includeSoftDeleted: true)
                 .Where(e => ids.Contains(e.Id));
            return await PagedList<Equipment>.Create(query, parameters.PageNumber, parameters.PageSize);
        }

        public async Task<PagedList<Equipment>> GetEquipmentByStatus(EquipmentStatus status, EquipmentResourceParameters parameters)
        {
            var query = EquipmentQuery(parameters, includeSoftDeleted: false)
                .Where(e => e.EquipmentStatus == status);
            return await PagedList<Equipment>.Create(query, parameters.PageNumber, parameters.PageSize);
        }

        public async Task<PagedList<Equipment>> GetEquipmentBySupplier(Guid supplierId, EquipmentResourceParameters parameters)
        {
            var query = EquipmentQuery(parameters, includeSoftDeleted: false)
                    .Where(e => e.SupplierId == supplierId);
            return await PagedList<Equipment>.Create(query, parameters.PageNumber, parameters.PageSize);
        }

        public async Task<PagedList<Equipment>> GetEquipmentByBrand(EquipmentBrand brand, EquipmentResourceParameters parameters)
        {
            var query = EquipmentQuery(parameters, includeSoftDeleted: false)
                .Where(e => e.EquipmentBrand == brand);
            return await PagedList<Equipment>.Create(query, parameters.PageNumber, parameters.PageSize);
        }

        public async Task<PagedList<Equipment>> GetEquipmentByType(EquipmentType type, EquipmentResourceParameters parameters)
        {
            var query = EquipmentQuery(parameters, includeSoftDeleted: false)
                .Where(e => e.EquipmentType == type);
            return await PagedList<Equipment>.Create(query, parameters.PageNumber, parameters.PageSize);
        }

        public async Task<EquipmentStatus?> GetEquipmentStatus(Guid equipmentId)
        {
            if (equipmentId == Guid.Empty)
                throw new ArgumentException("Id cannot be empty", nameof(equipmentId));

            var equipment = await _context.Equipments
                .FirstOrDefaultAsync(e => e.Id == equipmentId);

            return equipment?.EquipmentStatus;
        }

        public async Task<bool> EquipmentExists(Guid equipmentId)
        {
            if (equipmentId == Guid.Empty)
                throw new ArgumentException("Id cannot be empty", nameof(equipmentId));
            return await _context.Equipments.AnyAsync(e => e.Id == equipmentId);
        }

        public Task CreateEquipment(Equipment equipment)
        {
            ArgumentNullException.ThrowIfNull(equipment);
            _context.Equipments.Add(equipment);
            return Task.CompletedTask;
        }

        [Obsolete("Use CreateEquipment(Equipment equipment) instead.")]
        public void Create(Equipment equipment)
        {
            _context.Equipments.Add(equipment);
        }

        public async Task UpdateEquipment(Equipment equipment)
        {
            ArgumentNullException.ThrowIfNull(equipment, nameof(equipment));

            equipment.UpdateDate = DateTimeOffset.UtcNow;
            _context.Equipments.Update(equipment);
        }

        [Obsolete("Use UpdateEquipment(Equipment equipment) instead.")]
        public async Task Update(Equipment equipment)
        {
            var currentEquipment =
                await _context.Equipments
                .FirstOrDefaultAsync(e => e.Id == equipment.Id);

            if (!currentEquipment.RowVersion.SequenceEqual(equipment.RowVersion))
            {
                throw new DbUpdateConcurrencyException("The entity has been modified by another user.");
            }
            equipment.UpdateDate = DateTime.Now;
            _context.Equipments.Update(equipment);
        }

        public async Task SoftDeleteEquipment(Equipment equipment)
        {
            ArgumentNullException.ThrowIfNull(equipment, nameof(equipment));
            equipment.DeletedDate = DateTimeOffset.UtcNow;
            _context.Equipments.Update(equipment);
        }

        [Obsolete("Use SoftDeleteEquipment(Equipment equipment) instead.")]
        public void Delete(Guid equipmentId)
        {
            var equipment = _context.Equipments.FirstOrDefault(e => e.Id == equipmentId);
            equipment.DeletedDate = DateTime.Now;
            _context.Equipments.Update(equipment);
        }

        public async Task RestoreEquipment(Equipment equipment)
        {
            ArgumentNullException.ThrowIfNull(equipment, nameof(equipment));

            equipment.DeletedDate = null;
            _context.Equipments.Update(equipment);
        }

        public async Task SetEquipmentStatus(Equipment equipment, EquipmentStatus status)
        {
            ArgumentNullException.ThrowIfNull(equipment, nameof(equipment));

            if (equipment.EquipmentStatus == status)
                return;

            equipment.EquipmentStatus = status;
            _context.Equipments.Update(equipment);
        }

        [Obsolete("Use SetEquipmentStatus(Equipment equipment, EquipmentStatus status) instead.")]
        public async Task SetEquipmentStatus(Guid equipmentId, EquipmentStatus status)
        {
            if (equipmentId == Guid.Empty)
            {
                throw new ArgumentException("Id cannot be empty", nameof(equipmentId));
            }

            var equipment = await _context.Equipments.FirstOrDefaultAsync(e => e.Id == equipmentId);
            if (equipment == null)
            {
                throw new KeyNotFoundException(
                        $"Equipment with id {equipmentId} not found.");
            }

            if (equipment.EquipmentStatus == status)
                return;

            equipment.EquipmentStatus = status;
            _context.Equipments.Update(equipment);
        }

        public async Task<BulkOperationResult> CreateEquipmentBulk(IEnumerable<Equipment> equipmentList)
        {
            if (equipmentList == null || !equipmentList.Any())
                throw new ArgumentException(
                    "Equipment list cannot be null or empty.",
                    nameof(equipmentList));

            var result = new BulkOperationResult();
            var equipment = equipmentList.ToList();
            foreach (var equipmentItem in equipmentList)
            {
                try
                {
                    await CreateEquipment(equipmentItem);
                    result.SuccessCount++;
                    result.SuccessIds.Add(equipmentItem.Id);
                }
                catch (Exception ex)
                {
                    result.FailureCount++;
                    result.Errors.Add(new BulkOperationError
                    {
                        EntityId = equipmentItem.Id,
                        ErrorMessage = ex.Message
                    });
                }
            }
            return result;
        }

        public async Task<BulkOperationResult> SetEquipmentBulkStatus(
            IEnumerable<Guid> equipmentIdsList, EquipmentStatus status)
        {
            if (equipmentIdsList == null || !equipmentIdsList.Any())
            {
                throw new ArgumentException("Equipment ids cannot be empty",
                    nameof(equipmentIdsList));
            }

            var result = new BulkOperationResult();
            var idsList = equipmentIdsList.Distinct().ToList();

            var equipments = await _context.Equipments
            .Where(e => idsList.Contains(e.Id))
            .ToListAsync();

            var foundIds = new HashSet<Guid>(equipments.Select(e => e.Id));

            foreach (var equipment in equipments)
            {
                equipment.EquipmentStatus = status;
                equipment.UpdateDate = DateTimeOffset.UtcNow;
                _context.Equipments.Update(equipment);

                result.SuccessCount++;
                result.SuccessIds.Add(equipment.Id);
            }

            var notFoundIds = idsList.Except(foundIds);
            foreach (var id in notFoundIds)
            {
                result.FailureCount++;
                result.Errors.Add(new BulkOperationError
                {
                    EntityId = id,
                    ErrorMessage = "Equipment not found"
                });
            }

            return result;
        }

        public async Task<BulkOperationResult> SoftDeleteBulk(IEnumerable<Guid> equipmentIds)
        {
            if (equipmentIds == null || !equipmentIds.Any())
            {
                throw new ArgumentException("Equipment ids cannot be empty",
                    nameof(equipmentIds));
            }
            var result = new BulkOperationResult();
            var idsList = equipmentIds.Distinct().ToList();

            var equipments = await _context.Equipments
               .Where(e => idsList.Contains(e.Id))
               .ToListAsync();

            var foundIds = new HashSet<Guid>(equipments.Select(e => e.Id));

            foreach (var equipment in equipments)
            {
                equipment.DeletedDate = DateTimeOffset.UtcNow;
                _context.Equipments.Update(equipment);

                result.SuccessCount++;
                result.SuccessIds.Add(equipment.Id);
            }

            var notFoundIds = idsList.Except(foundIds);
            foreach (var id in notFoundIds)
            {
                result.FailureCount++;
                result.Errors.Add(new BulkOperationError
                {
                    EntityId = id,
                    ErrorMessage = "Equipment not found"
                });
            }

            return result;
        }

        public async Task<BulkOperationResult> RestoreBulk(IEnumerable<Guid> equipmentIds)
        {
            if (equipmentIds == null || !equipmentIds.Any())
            {
                throw new ArgumentException("Equipment ids cannot be empty",
                    nameof(equipmentIds));
            }
            var result = new BulkOperationResult();
            var idsList = equipmentIds.Distinct().ToList();

            var equipments = await _context.Equipments
                .IgnoreQueryFilters()
                .Where(e => idsList.Contains(e.Id))
                .ToListAsync();

            var foundIds = new HashSet<Guid>(equipments.Select(e => e.Id));

            foreach (var equipment in equipments)
            {
                if (equipment.DeletedDate.HasValue)
                {
                    equipment.DeletedDate = null;
                    _context.Equipments.Update(equipment);

                    result.SuccessCount++;
                    result.SuccessIds.Add(equipment.Id);
                }
                else
                {
                    result.FailureCount++;
                    result.Errors.Add(new BulkOperationError
                    {
                        EntityId = equipment.Id,
                        ErrorMessage = "Equipment is not deleted"
                    });
                }
            }
            var notFoundIds = idsList.Except(foundIds);
            foreach (var id in notFoundIds)
            {
                result.FailureCount++;
                result.Errors.Add(new BulkOperationError
                {
                    EntityId = id,
                    ErrorMessage = "Equipment not found"
                });
            }

            return result;
        }


        #region Analytics Support
        public async Task<int> GetTotalCount()
        {
            return await _context.Equipments.CountAsync();
        }
        public async Task<int> GetCountByStatus(EquipmentStatus status)
        {
            return await _context.Equipments.CountAsync(e => e.EquipmentStatus == status);
        }

        public async Task<decimal> GetTotalPurchaseCost()
        {
            if (!await _context.Equipments.AnyAsync())
                return 0;
            return await _context.Equipments.SumAsync(e => e.TotalPrice);
        }
        public async Task<decimal> GetTotalExpenses()
        {
            if (!await _context.Equipments.AnyAsync())
                return 0;
            return await _context.Equipments.SumAsync(e => e.Expenses);
        }
        public async Task<decimal> GetTotalShippingCost()
        {
            if (!await _context.Equipments.AnyAsync())
                return 0;
            return await _context.Equipments.SumAsync(e => e.ShippingPrice);
        }

        public async Task<decimal> GetAveragePrice()
        {
            if (!await _context.Equipments.AnyAsync())
                return 0;
            return await _context.Equipments.AverageAsync(e => e.TotalPrice);
        }

        public async Task<IDictionary<EquipmentBrand, int>> GetCountByBrand()
        {
            return await _context.Equipments
                .GroupBy(e => e.EquipmentBrand)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<IDictionary<EquipmentType, int>> GetCountByType()
        {
            return await _context.Equipments
                .GroupBy(e => e.EquipmentType)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<IDictionary<Guid, int>> GetCountBySupplier()
        {
            return await _context.Equipments
                .Where(e => e.SupplierId != null)
                .GroupBy(e => e.SupplierId)
                .ToDictionaryAsync(g => g.Key!.Value, g => g.Count());
        }

        public async Task<IDictionary<EquipmentBrand, decimal>> GetTotalValueByBrand()
        {
            return await _context.Equipments
                .GroupBy(e => e.EquipmentBrand)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Sum(e => e.Price + e.ShippingPrice)
                );
        }

        public async Task<IDictionary<EquipmentType, decimal>> GetTotalValueByType()
        {
            return await _context.Equipments
                .GroupBy(e => e.EquipmentType)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Sum(e => e.Price + e.ShippingPrice)
                );
        }

        public async Task<IDictionary<Guid, decimal>> GetTotalValueBySupplier()
        {
            return await _context.Equipments
                .Where(e => e.SupplierId != null)
                .GroupBy(e => e.SupplierId)
                .ToDictionaryAsync(
                    g => g.Key!.Value,
                    g => g.Sum(e => e.Price + e.ShippingPrice)
                );
        }

        public async Task<IDictionary<int, int>> GetCountByManufactureYear()
        {
            return await _context.Equipments
                .Where(e => e.ManufactureDate != null)
                .GroupBy(e => e.ManufactureDate)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<IDictionary<int, int>> GetPurchasedCountPerYear()
        {
            return await _context.Equipments
                .Where(e => e.PurchaseDate != null)
                .GroupBy(e => e.PurchaseDate.Year)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        #endregion

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() > 0);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose resources when needed
            }
        }

        private IQueryable<Equipment> EquipmentQuery(
            EquipmentResourceParameters parameters,
            bool includeSoftDeleted)
        {
            ArgumentNullException.ThrowIfNull(parameters);

            IQueryable<Equipment> query = _context.Equipments.AsQueryable();

            if (includeSoftDeleted)
            {
                query = query.IgnoreQueryFilters()
                    .Where(e => e.IsDeleted == true);
            }

            if (!string.IsNullOrWhiteSpace(parameters.Fields))
            {
                query = query.ApplyInclude(parameters.Fields);
            }

            if (!string.IsNullOrWhiteSpace(parameters.SearchQuery))
            {
                var search = parameters.SearchQuery.Trim();

                query = query.Where(e =>
                    EF.Functions.Like(e.Name, $"%{search}%") ||
                    EF.Functions.Like(e.InternalSerial, $"%{search}%"));
            }

            if (parameters.EquipmentBrand.HasValue)
            {
                query = query.Where(e => e.EquipmentBrand == parameters.EquipmentBrand);
            }

            if (parameters.EquipmentType.HasValue)
            {
                query = query.Where(e => e.EquipmentType == parameters.EquipmentType);
            }

            if (parameters.EquipmentStatus.HasValue)
            {
                query = query.Where(e => e.EquipmentStatus == parameters.EquipmentStatus);
            }

            if (parameters.IsAvailable.HasValue)
            {
                query = parameters.IsAvailable.Value
                    ? query.Where(e => e.EquipmentStatus == EquipmentStatus.Available)
                    : query.Where(e => e.EquipmentStatus != EquipmentStatus.Available);
            }

            if (parameters.PurchaseDateFrom.HasValue)
            {
                query = query.Where(e => e.PurchaseDate >= parameters.PurchaseDateFrom);
            }

            if (parameters.PurchaseDateTo.HasValue)
            {
                query = query.Where(e => e.PurchaseDate <= parameters.PurchaseDateTo);
            }

            if (parameters.YearOfManufacturers.HasValue)
            {
                query = query.Where(e =>
                    e.ManufactureDate == parameters.YearOfManufacturers.Value);
            }

            if (!string.IsNullOrWhiteSpace(parameters.SortBy))
            {
                query = query.OrderByProperty(parameters.SortBy, parameters.SortDescending);
            }

            return query;
        }
    }
}
