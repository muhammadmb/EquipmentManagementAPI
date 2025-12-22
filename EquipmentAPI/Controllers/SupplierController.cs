using Application.Interface.Repositories;
using Application.Interface.Services;
using Application.Models.PhoneNumberModels.Write;
using Application.Models.SupplierModels.Read;
using Application.Models.SupplierModels.Write;
using Application.ResourceParameters;
using Domain.Entities;
using Mapster;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Shared.Extensions;
using Shared.Results;
using System.Text.Json;

namespace API.Controllers
{
    [ApiController]
    [Route("api/Suppliers")]
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierRepository _supplierRepository;
        private readonly IPropertyCheckerService _propertyCheckerService;
        private readonly IMemoryCache _cache;

        public SupplierController(ISupplierRepository supplierRepository, IPropertyCheckerService propertyCheckerService, IMemoryCache memoryCache)
        {
            _supplierRepository = supplierRepository ??
                throw new ArgumentNullException(nameof(supplierRepository));
            _propertyCheckerService = propertyCheckerService ??
                throw new ArgumentNullException(nameof(propertyCheckerService));
            _cache = memoryCache ??
                throw new ArgumentNullException(nameof(memoryCache));
        }

        [HttpGet(Name = "Suppliers")]
        [HttpHead]
        public async Task<IActionResult> GetSuppliers([FromQuery] SupplierResourceParameters parameters)
        {

            if (!_propertyCheckerService.TypeHasProperties<SupplierDto>(parameters.Fields))
            {
                return BadRequest("Some requested fields are invalid.");
            }

            string cacheKey = $"supplier_{JsonSerializer.Serialize(parameters)}";

            if (!_cache.TryGetValue(cacheKey, out PagedList<Supplier> suppliers))
            {
                suppliers = await _supplierRepository.GetSuppliers(parameters);
                if (suppliers == null) return NotFound();
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1),
                    SlidingExpiration = TimeSpan.FromMinutes(1)
                };
                _cache.Set(cacheKey, suppliers, cacheOptions);
            }

            Response.Headers.Append("X-Pagination",
                JsonSerializer.Serialize(suppliers.CreatePaginationMetadata()));
            var suppliersToReturn = suppliers.Adapt<IEnumerable<SupplierDto>>().ShapeData(parameters.Fields);
            return Ok(suppliersToReturn);
        }

        [HttpGet("{id}", Name = "GetASupplier")]
        [HttpHead("{id}")]
        public async Task<IActionResult> GetASupplierById(Guid id, [FromQuery] string? fields)
        {
            if (!_propertyCheckerService.TypeHasProperties<SupplierDto>(fields))
            {
                return BadRequest("Some requested firlds are invalid.");
            }

            var cacheKey = ($"supplier_{id}_{JsonSerializer.Serialize(fields)}");
            if (!_cache.TryGetValue(cacheKey, out Supplier supplier))
            {
                supplier = await _supplierRepository.GetSupplier(id, fields);
                if (supplier == null) return NotFound($"Supplier with ID {id} not found.");

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1),
                    SlidingExpiration = TimeSpan.FromMinutes(1)
                };

                _cache.Set(cacheKey, supplier, cacheOptions);
            }
            var supplierToReturn = supplier.Adapt<SupplierDto>().ShapeData(fields);
            return Ok(supplierToReturn);
        }

        [HttpPost()]
        public async Task<IActionResult> CreateSupplier([FromBody] SupplierCreateDto supplier)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdSupplier = supplier.Adapt<Supplier>();
            _supplierRepository.Create(createdSupplier);
            await _supplierRepository.SaveChangesAsync();

            return CreatedAtRoute(
                "GetASupplier",
                new { id = createdSupplier.Id },
                $"The supplier {supplier.Name} is added successfully"
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSupplier(Guid id, [FromBody] SupplierUpdateDto supplier)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var supplierFromRepo = await _supplierRepository.GetSupplier(id, null);
            if (supplierFromRepo == null) return NotFound($"Supplier with ID {id} not found.");

            try
            {
                supplier.Adapt(supplierFromRepo);
                await _supplierRepository.Update(supplierFromRepo);
                await _supplierRepository.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> SupplierPartialUpdate(Guid id, JsonPatchDocument<SupplierUpdateDto> patchDocument)
        {
            var supplierFromRepo = await _supplierRepository.GetSupplier(id, null);
            if (supplierFromRepo == null) return NotFound($"Supplier with ID {id} not found.");

            var supplier = supplierFromRepo.Adapt<SupplierUpdateDto>();
            patchDocument.ApplyTo(supplier, ModelState);

            if (!TryValidateModel(supplier))
            {
                return ValidationProblem(ModelState);
            }

            supplier.Adapt(supplierFromRepo);
            await _supplierRepository.Update(supplierFromRepo);
            await _supplierRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{id}/phoneNumbers")]
        public async Task<IActionResult> UpdatePhoneNumbers(Guid id, List<SupplierPhoneNumberUpdateDto> phonenumbers)
        {
            if (phonenumbers == null || !phonenumbers.Any()) return BadRequest("Phone numbers list can not be empty.");

            var IsSupplierExist = await _supplierRepository.Exists(id);
            if (!IsSupplierExist) return NotFound($"Supplier with {id} is not found");

            await _supplierRepository.UpdatePhoneNumber(id, phonenumbers);
            await _supplierRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupplier(Guid id)
        {
            var IsSupplierExist = await _supplierRepository.Exists(id);
            if (!IsSupplierExist) return NotFound($"Supplier with ID {id} not found.");

            await _supplierRepository.Delete(id);
            await _supplierRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{supplierId}/phoneNumbers/{phoneId}")]
        public async Task<IActionResult> DeleteSupplierPhoneNumber(Guid supplierId, Guid phoneId)
        {
            var IsSupplierExist = await _supplierRepository.Exists(supplierId);
            if (!IsSupplierExist) return NotFound($"Supplier with ID {supplierId} not found.");

            var IsPhoneNumberExist = await _supplierRepository.PhoneNumberExist(phoneId);
            if (!IsPhoneNumberExist) return NotFound($"Phone number with ID {phoneId} not found");

            await _supplierRepository.DeletePhoneNumber(supplierId, phoneId);
            await _supplierRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id}/restore")]
        public async Task<IActionResult> RestoreSupplier(Guid id)
        {
            var restored = await _supplierRepository.RestoreSupplier(id);
            if (!restored) return BadRequest($"Supplier with ID {id} not found or not deleted.");

            await _supplierRepository.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{supplierId}/phoneNumbers/{phoneId}/restore")]
        public async Task<IActionResult> RestoreSupplierPhoneNumber(Guid supplierId, Guid phoneId)
        {
            var supplierExists = await _supplierRepository.Exists(supplierId);
            if (!supplierExists) return NotFound($"Supplier with ID {supplierId} not found.");

            var IsPhoneNumberExist = await _supplierRepository.PhoneNumberExist(phoneId);
            if (!IsPhoneNumberExist) return NotFound($"Phone number with ID {phoneId} not found");

            var restored = await _supplierRepository.RestorePhoneNumber(phoneId);
            if (!restored) return NotFound($"Phone number with ID {phoneId} not found or not deleted.");

            await _supplierRepository.SaveChangesAsync();
            return NoContent();
        }

        [HttpOptions]
        public IActionResult GetSuppliersOptions()
        {
            Response.Headers.Append("Allow", "Get, Head, Put, Patch, Post, Delete, Options");
            return Ok();
        }

        [HttpOptions("{id}")]
        public IActionResult GetASupplierOptions()
        {
            Response.Headers.Append("Allow", "Get, Head, Put, Patch, Post, Delete, Options");
            return Ok();
        }
    }
}
