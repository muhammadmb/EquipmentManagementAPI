using EquipmentAPI.Entities;
using EquipmentAPI.Helper;
using EquipmentAPI.Models.EquipmentModels.Read;
using EquipmentAPI.Models.EquipmentModels.Write;
using EquipmentAPI.Repositories.Equipment_Repository;
using EquipmentAPI.ResourceParameters;
using EquipmentAPI.Services;
using Mapster;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace EquipmentAPI.Controllers
{
    [ApiController]
    [Route("api/Equipment")]
    public class EquipmentController : ControllerBase
    {
        private readonly IEquipmentRepository _equipmentRepository;
        private readonly IPropertyCheckerService _propertyCheckerService;
        private readonly IMemoryCache _cache;

        public EquipmentController(IEquipmentRepository equipmentRepository, IPropertyCheckerService propertyCheckerService, IMemoryCache memoryCache)
        {
            _equipmentRepository = equipmentRepository ??
                throw new ArgumentNullException(nameof(equipmentRepository));
            _propertyCheckerService = propertyCheckerService ??
                throw new ArgumentNullException(nameof(propertyCheckerService));
            _cache = memoryCache ??
                throw new ArgumentNullException(nameof(memoryCache));
        }

        [HttpGet(Name = "GetEquipment")]
        [HttpHead]
        public async Task<IActionResult> GetEquipment([FromQuery] EquipmentResourceParameters parameters)
        {
            if (!_propertyCheckerService.TypeHasProperties<EquipmentDto>(parameters.Fields))
            {
                return BadRequest("Some requested fields are invalid.");
            }

            string cacheKey = $"equipment_{JsonSerializer.Serialize(parameters)}";

            if (!_cache.TryGetValue(cacheKey, out PagedList<Equipment> equipment))
            {
                equipment = await _equipmentRepository.GetEquipment(parameters);
                if (equipment == null) return NotFound();
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1),
                    SlidingExpiration = TimeSpan.FromMinutes(1)
                };
                _cache.Set(cacheKey, equipment, cacheOptions);
            }

            Response.Headers.Append("X-Pagination",
                JsonSerializer.Serialize(equipment.CreatePaginationMetadata()));

            var equipmentToReturn = equipment.Adapt<IEnumerable<EquipmentDto>>().ShapeData(parameters.Fields);

            return Ok(equipmentToReturn);
        }

        [HttpGet("{id}", Name = "GetAnEquipment")]
        [HttpHead("{id}")]
        public async Task<IActionResult> GetEquipmentById(Guid id, [FromQuery] string? fields)
        {
            if (!_propertyCheckerService.TypeHasProperties<EquipmentDto>(fields))
            {
                return BadRequest("Some requested fields are invalid.");
            }

            string cacheKey = $"equipment_{id}_{JsonSerializer.Serialize(fields)}";

            if (!_cache.TryGetValue(cacheKey, out Equipment equipment))
            {
                equipment = await _equipmentRepository.GetEquipmentById(id, fields);

                if (equipment == null) return NotFound($"Equipment with ID {id} not found.");

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1),
                    SlidingExpiration = TimeSpan.FromMinutes(1)
                };
                _cache.Set(cacheKey, equipment, cacheOptions);
            }

            var equipmentToReturn = equipment.Adapt<EquipmentDto>().ShapeData(fields);
            return Ok(equipmentToReturn);
        }

        [HttpPost()]
        public async Task<IActionResult> CreateEquipment([FromBody] EquipmentCreateDto equipment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdEquipment = equipment.Adapt<Equipment>();
            _equipmentRepository.Create(createdEquipment);
            await _equipmentRepository.SaveChangesAsync();
            return CreatedAtRoute(
                    "GetAnEquipment",
                    new { id = createdEquipment.Id },
                    $"The {equipment.Name} is Added Successfully"
                );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEquipment(Guid id, [FromBody] EquipmentUpdateDto equipment)
        {
            var EquipmentFromRepo = await _equipmentRepository.GetEquipmentById(id, null);
            if (EquipmentFromRepo == null) return NotFound($"Equipment with ID {id} not found.");

            try
            {
                equipment.Adapt(EquipmentFromRepo);
                await _equipmentRepository.Update(EquipmentFromRepo);
                await _equipmentRepository.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> EquipmentPartialUpdate(Guid id, JsonPatchDocument<EquipmentUpdateDto> patchDocument)
        {
            var EquipmentFromRepo = await _equipmentRepository.GetEquipmentById(id, null);
            if (EquipmentFromRepo == null) return NotFound($"Equipment with ID {id} not found.");

            var equipment = EquipmentFromRepo.Adapt<EquipmentUpdateDto>();
            patchDocument.ApplyTo(equipment, ModelState);

            if (!TryValidateModel(equipment))
            {
                return ValidationProblem(ModelState);
            }

            equipment.Adapt(EquipmentFromRepo);
            await _equipmentRepository.Update(EquipmentFromRepo);
            await _equipmentRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEquipment(Guid id)
        {
            var EquipmentFromRepo = await _equipmentRepository.GetEquipmentById(id, null);
            if (EquipmentFromRepo == null) return NotFound($"Equipment with ID {id} not found.");

            _equipmentRepository.Delete(id);
            await _equipmentRepository.SaveChangesAsync();
            return NoContent();
        }

        [HttpOptions()]
        public IActionResult GetEquipmentOptions()
        {
            Response.Headers.Add("Allow", "Get, Head, Post, Options");
            return Ok();
        }

        [HttpOptions("{id}")]
        public IActionResult GetAnEquipmentOptions()
        {
            Response.Headers.Add("Allow", "Get, Head, Put, Patch, Options");
            return Ok();
        }
    }
}
