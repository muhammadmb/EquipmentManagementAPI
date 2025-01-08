using EquipmentAPI.Helper;
using EquipmentAPI.Models.EquipmentModels.Read;
using EquipmentAPI.Repositories.Equipment_Repository;
using EquipmentAPI.ResourceParameters;
using EquipmentAPI.Services;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EquipmentAPI.Controllers
{
    [ApiController]
    [Route("api/Equipment")]
    public class EquipmentController : ControllerBase
    {

        private readonly IEquipmentRepository _equipmentRepository;
        private readonly IPropertyCheckerService _propertyCheckerService;

        public EquipmentController(IEquipmentRepository equipmentRepository, IPropertyCheckerService propertyCheckerService)
        {
            _equipmentRepository = equipmentRepository ??
                throw new ArgumentNullException(nameof(equipmentRepository));
            _propertyCheckerService = propertyCheckerService ??
                throw new ArgumentNullException(nameof(propertyCheckerService));
        }

        [HttpGet(Name = "GetEquipment")]
        [HttpHead]
        public async Task<IActionResult> GetEquipment([FromQuery] EquipmentResourceParameters parameters)
        {
            if (!_propertyCheckerService.TypeHasProperties<EquipmentDto>(parameters.Fields))
            {
                return NotFound("Some requested fields are invalid.");
            }

            var equipment = await _equipmentRepository.GetEquipment(parameters);

            if (equipment == null) return NoContent();

            var paginationMetadata = new
            {
                pageSize = equipment.PageSize,
                currentPage = equipment.CurrentPage,
                hasNext = equipment.HasNext,
                hasPrevious = equipment.HasPrevious,
                totalPages = equipment.TotalPages,
                totalCount = equipment.TotalCount
            };

            Response.Headers.Append("X-Pagination",
                JsonSerializer.Serialize(paginationMetadata));

            var equipmentToReturn = equipment.Adapt<IEnumerable<EquipmentDto>>().ShapeData(parameters.Fields);

            return Ok(equipmentToReturn);
        }
    }
}
