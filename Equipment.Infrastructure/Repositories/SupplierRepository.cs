using Application.Interface.Repositories;
using Application.Models.PhoneNumberModels.Write;
using Application.ResourceParameters;
using Domain.Entities;
using Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Shared.Extensions;
using Shared.Results;

namespace Infrastructure.Repositories
{
    public class SupplierRepository : ISupplierRepository
    {

        private readonly ApplicationDbContext _context;

        public SupplierRepository(ApplicationDbContext context)
        {
            _context = context ??
                throw new ArgumentNullException(nameof(context));
        }

        public async Task<PagedList<Supplier>> GetSuppliers(SupplierResourceParameters parameters)
        {
            IQueryable<Supplier> Collection = _context.Suppliers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(parameters.Fields))
            {
                Collection = Collection.ApplyInclude(parameters.Fields);
            }

            if (!string.IsNullOrWhiteSpace(parameters.FilterByCountry))
            {
                Collection = Collection.Where(s => s.Country.ToLowerInvariant() == parameters.FilterByCountry);
            }

            if (!string.IsNullOrWhiteSpace(parameters.FilterByCity))
            {
                Collection = Collection.Where(s => s.City.ToLowerInvariant() == parameters.FilterByCity);
            }

            if (!string.IsNullOrWhiteSpace(parameters.SearchQuery))
            {
                Collection = Collection.Where(s => EF.Functions.Like(s.Name, $"%{parameters.SearchQuery}%"));
            }

            return await PagedList<Supplier>.Create(Collection, parameters.PageNumber, parameters.PageSize);
        }

        public async Task<Supplier> GetSupplier(Guid supplierId, string fields)
        {
            IQueryable<Supplier> Collection = _context.Suppliers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(fields))
            {
                Collection = Collection.ApplyInclude(fields);
            }

            return await Collection.FirstOrDefaultAsync(s => s.Id == supplierId);
        }

        public async Task<bool> Exists(Guid id)
        {
            return await _context.Suppliers.AnyAsync(s => s.Id == id);
        }

        public async Task<bool> PhoneNumberExist(Guid id)
        {
            return await _context.SupplierPhoneNumbers.AnyAsync(pn => pn.Id == id);
        }

        public void Create(Supplier supplier)
        {
            _context.Suppliers.Add(supplier);
        }

        public async Task Update(Supplier supplier)
        {
            var currentSupplier = await _context.Suppliers
                .Include(s => s.PhoneNumbers)
                .AsTracking()
                .FirstOrDefaultAsync(s => s.Id == supplier.Id);

            if (currentSupplier == null) return;
            if (!currentSupplier.RowVersion.SequenceEqual(supplier.RowVersion))
            {
                throw new DbUpdateConcurrencyException("The entity has been modified by another user.");
            }

            var distinctNumbers = supplier.PhoneNumbers
                .GroupBy(p => p.Number)
                .Select(g => g.First())
                .ToList();


            foreach (var phone in currentSupplier.PhoneNumbers)
            {
                if (!distinctNumbers.Any(np => np.Id == phone.Id))
                {
                    phone.DeletedDate = DateTime.Now;
                }
            }

            foreach (var phone in supplier.PhoneNumbers)
            {
                if (phone.Id != Guid.Empty)
                {
                    var existing = currentSupplier.PhoneNumbers.FirstOrDefault(p => p.Id == phone.Id);
                    if (existing != null)
                    {
                        existing.Number = phone.Number;
                        existing.UpdateDate = DateTime.Now;
                    }
                    else
                    {
                        currentSupplier.PhoneNumbers.Add(new SupplierPhoneNumber
                        {
                            Number = phone.Number,
                            SupplierId = currentSupplier.Id,
                            AddedDate = DateTime.Now,
                            RowVersion = [1, 1, 1, 1]
                        });
                    }
                }
                else
                {
                    currentSupplier.PhoneNumbers.Add(new SupplierPhoneNumber
                    {
                        Number = phone.Number,
                        SupplierId = currentSupplier.Id,
                        AddedDate = DateTime.Now,
                        RowVersion = [1, 1, 1, 1]
                    });
                }
            }

            currentSupplier.Name = supplier.Name;
            currentSupplier.ContactPerson = supplier.ContactPerson;
            currentSupplier.Email = supplier.Email;
            currentSupplier.Country = supplier.Country;
            currentSupplier.City = supplier.City;
            currentSupplier.UpdateDate = DateTime.Now;
        }

        public async Task UpdatePhoneNumber(Guid supplierId, List<SupplierPhoneNumberUpdateDto> phonenumbers)
        {
            var supplier = await _context.Suppliers
                .Include(s => s.PhoneNumbers)
                .IgnoreQueryFilters()
                .AsTracking()
                .FirstOrDefaultAsync(s => s.Id == supplierId);

            var distinctNumbers = phonenumbers
                .GroupBy(p => p.Number)
                .Select(g => g.First())
                .ToList();

            foreach (var phone in supplier.PhoneNumbers)
            {
                if (!distinctNumbers.Any(np => np.Id == phone.Id))
                {
                    phone.DeletedDate = DateTime.Now;
                }
            }

            foreach (var dnumber in distinctNumbers)
            {
                var existing = supplier.PhoneNumbers
                    .FirstOrDefault(p => p.Id == dnumber.Id || p.Number == dnumber.Number);

                if (existing != null)
                {
                    if (!existing.RowVersion.SequenceEqual(dnumber.RowVersion)) throw new DbUpdateConcurrencyException("The entity has been modified by another user.");

                    if (existing.Number != dnumber.Number)
                    {
                        existing.UpdateDate = DateTime.Now;
                        existing.Number = dnumber.Number;
                        existing.DeletedDate = null;
                    }
                }
                else
                {
                    supplier.PhoneNumbers.Add(new SupplierPhoneNumber
                    {
                        SupplierId = supplierId,
                        Number = dnumber.Number,
                        AddedDate = DateTime.Now,
                        RowVersion = [1, 1, 1, 1]
                    });
                }
            }
        }

        public async Task Delete(Guid supplierId)
        {
            var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == supplierId);
            if (supplier == null) return;
            supplier.DeletedDate = DateTime.Now;
            _context.Suppliers.Update(supplier);
        }

        public async Task DeletePhoneNumber(Guid supplierId, Guid phoneId)
        {
            var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == supplierId);
            if (supplier == null) return;
            var phoneNumber = await _context.SupplierPhoneNumbers.FirstOrDefaultAsync(s => s.Id == phoneId);
            if (phoneNumber == null) return;

            phoneNumber.DeletedDate = DateTime.Now;
            _context.SupplierPhoneNumbers.Update(phoneNumber);
        }

        public async Task<bool> RestoreSupplier(Guid id)
        {
            var supplier = await _context.Suppliers
                .IgnoreQueryFilters()
                .Include(s => s.PhoneNumbers)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (supplier == null || supplier.DeletedDate == null) return false;

            supplier.DeletedDate = null;
            supplier.UpdateDate = DateTime.Now;

            foreach (var phone in supplier.PhoneNumbers)
                phone.DeletedDate = null;

            _context.Suppliers.Update(supplier);

            return true;
        }

        public async Task<bool> RestorePhoneNumber(Guid phoneId)
        {
            var phone = await _context.SupplierPhoneNumbers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == phoneId);

            if (phone == null || phone.DeletedDate == null) return false;

            phone.DeletedDate = null;
            phone.UpdateDate = DateTime.Now;

            _context.SupplierPhoneNumbers.Update(phone);

            return true;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() > 0);
        }
    }
}
