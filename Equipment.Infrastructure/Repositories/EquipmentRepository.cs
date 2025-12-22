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
            IQueryable<Equipment> Collection = _context.Equipments.AsQueryable();

            if (!string.IsNullOrWhiteSpace(parameters.Fields))
            {
                Collection = Collection.ApplyInclude(parameters.Fields);
            }

            if (!string.IsNullOrWhiteSpace(parameters.SortBy))
            {
                Collection = Collection.OrderByProperty(parameters.SortBy, parameters.SortDescending);
            }

            if (!string.IsNullOrWhiteSpace(parameters.SearchQuery))
            {
                Collection = Collection.Where(e => EF.Functions.Like(e.Name, $"%{parameters.SearchQuery}%"));
            }

            return await PagedList<Equipment>.Create(Collection, parameters.PageNumber, parameters.PageSize);
        }

        public async Task<Equipment> GetEquipmentById(Guid equipmentId, string? fields)
        {
            IQueryable<Equipment> Collection = _context.Equipments.AsQueryable();

            if (!string.IsNullOrWhiteSpace(fields))
            {
                Collection = Collection.ApplyInclude(fields);
            }

            return await Collection
               .FirstOrDefaultAsync(e => e.Id == equipmentId);
        }

        public async Task<EquipmentStatus> GetEquipmentStatus(Guid equipmentId)
        {
            if (equipmentId == Guid.Empty)
            {
                throw new ArgumentException("Id cannot be empty", nameof(equipmentId));
            }

            var equipment = await _context.Equipments
                .FirstOrDefaultAsync(e => e.Id == equipmentId);

            if (equipment == null)
            {
                throw new KeyNotFoundException(
                    $"Equipment with id {equipmentId} not found.");
            }

            return equipment.EquipmentStatus;
        }

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

        public async Task<BulkOperationResult> SetEquipmentBulkStatus(IEnumerable<Guid> equipmentIdsList, EquipmentStatus status)
        {
            if (equipmentIdsList == null || !equipmentIdsList.Any())
            {
                throw new ArgumentException("Equipment ids cannot be empty",
                    nameof(equipmentIdsList));
            }

            var result = new BulkOperationResult();
            var idsList = equipmentIdsList.Distinct().ToList();
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                foreach (var id in idsList)
                {
                    try
                    {
                        var equipment = await _context.Equipments
                            .FirstOrDefaultAsync(e => e.Id == id);

                        if (equipment != null)
                        {
                            equipment.EquipmentStatus = status;
                            _context.Equipments.Update(equipment);
                            result.SuccessCount++;
                            result.SuccessIds.Add(id);
                        }
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
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }

            return result;
        }

        public async Task<bool> EquipmentExists(Guid equipmentId)
        {
            return await _context.Equipments.AnyAsync(e => e.Id == equipmentId);
        }

        public void Create(Equipment equipment)
        {
            _context.Equipments.Add(equipment);
        }

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

        public void Delete(Guid equipmentId)
        {
            var equipment = _context.Equipments.FirstOrDefault(e => e.Id == equipmentId);
            equipment.DeletedDate = DateTime.Now;
            _context.Equipments.Update(equipment);
        }

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
    }
}
