using EquipmentAPI.Contexts;
using EquipmentAPI.Entities;
using EquipmentAPI.Helper;
using EquipmentAPI.ResourceParameters;
using Microsoft.EntityFrameworkCore;

namespace EquipmentAPI.Repositories.Equipment_Repository
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
            IQueryable<Equipment> Collection = _context.Equipments;

            if (!string.IsNullOrWhiteSpace(fields))
            {
                Collection = Collection.ApplyInclude(fields);
            }
            
            return await Collection
               .FirstOrDefaultAsync(e => e.Id == equipmentId);
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
