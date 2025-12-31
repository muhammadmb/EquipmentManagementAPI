using Application.Interface.Services;
using Application.Models.RentalContractModels.Read;
using Application.Models.RentalContractModels.Write;
using Application.ResourceParameters;
using EquipmentAPI.ModelBinders;
using Mapster;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Extensions;
using Shared.Results;
using System.Text.Json;

namespace API.Controllers
{
    [ApiController]
    [Route("/api/RentalContracts")]
    public class RentalContractController : ControllerBase
    {
        private readonly IRentalContractService _rentalContractService;
        private readonly IRentalContractAnalyticsService _rentalContractAnalytics;
        private readonly IPropertyCheckerService _propertyChecker;
        private const int MaxBulkOperationSize = 500;

        public RentalContractController(
            IRentalContractService rentalContractService,
            IRentalContractAnalyticsService rentalContractAnalytics,
            IPropertyCheckerService propertyChecker)
        {
            _rentalContractService = rentalContractService ??
                throw new ArgumentNullException(nameof(rentalContractService));
            _rentalContractAnalytics = rentalContractAnalytics ??
                throw new ArgumentNullException(nameof(rentalContractAnalytics));
            _propertyChecker = propertyChecker ??
                throw new ArgumentNullException(nameof(propertyChecker));
        }

        #region Get requests

        [HttpGet(Name = "GetRentalContracts")]
        [HttpHead]
        public async Task<IActionResult> GetRentalContracts([FromQuery] RentalContractResourceParameters parameters)
        {
            if (!_propertyChecker.TypeHasProperties<RentalContractDto>(parameters.Fields))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Some requested fields are invalid: {parameters.Fields}"
                });
            }

            if (!string.IsNullOrEmpty(parameters.SortBy) && !_propertyChecker.TypeHasProperties<RentalContractDto>(parameters.SortBy))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Sort by is not field of the rental contract: {parameters.SortBy}",
                    Message = "Sort by should be one of the entity fields"
                });
            }

            var rentalContracts = await _rentalContractService.GetRentalContracts(parameters);
            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(rentalContracts.CreatePaginationMetadata()));

            var rentalContractsToReturn = rentalContracts.Adapt<IEnumerable<RentalContractDto>>()
                .ShapeData(parameters.Fields);

            return Ok(rentalContractsToReturn);
        }

        [HttpGet("collection/({ids})", Name = "GetRentalContractsByIds")]
        [HttpHead("collection/({ids})")]
        public async Task<IActionResult> GetRentalContractsByIds(
            [FromRoute][ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids,
            [FromQuery] string? fields)
        {
            if (ids == null || !ids.Any())
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "No IDs provided",
                    Message = "At least one rental contract ID must be provided"
                });
            }

            if (!_propertyChecker.TypeHasProperties<RentalContractDto>(fields))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Some requested fields are invalid: {fields}"
                });
            }

            var rentalContracts = await _rentalContractService.GetRentalContractsByIds(ids, fields);

            var rentalContractsToReturn = rentalContracts.Adapt<IEnumerable<RentalContractDto>>()
                .ShapeData(fields);

            return Ok(rentalContractsToReturn);
        }

        [HttpGet("{id}", Name = "GetRentalContractById")]
        [HttpHead("{id}")]
        public async Task<IActionResult> GetRentalContractById(Guid id, [FromQuery] string? fields)
        {
            if (!string.IsNullOrEmpty(fields) &&
                !_propertyChecker.TypeHasProperties<RentalContractDto>(fields))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Some requested fields are invalid: {fields}"
                });
            }

            var rentalContract = await _rentalContractService.GetRentalContractById(id, fields);
            if (rentalContract is null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = $"Rental contract with id: {id} not found"
                });
            }

            var rentalContractToReturn = rentalContract.Adapt<RentalContractDto>()
                .ShapeData(fields);

            return Ok(rentalContractToReturn);
        }

        [HttpGet("customers/{customerId}")]
        [HttpHead("customers/{customerId}")]
        public async Task<IActionResult> GetRentalContractsByCustomerId(
            Guid customerId,
            [FromQuery] string? fields)
        {
            if (!string.IsNullOrEmpty(fields) &&
                !_propertyChecker.TypeHasProperties<RentalContractDto>(fields))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Some requested fields are invalid: {fields}"
                });
            }
            var rentalContracts = await _rentalContractService.GetRentalContractsByCustomerId(customerId, fields);
            var rentalContractsToReturn = rentalContracts.Adapt<IEnumerable<RentalContractDto>>()
                .ShapeData(fields);
            return Ok(rentalContractsToReturn);
        }

        [HttpGet("equipment/{equipmentId}")]
        [HttpHead("equipment/{equipmentId}")]
        public async Task<IActionResult> GetRentalContractsByEquipmentId(
            Guid equipmentId,
            [FromQuery] string? fields)
        {
            if (!string.IsNullOrEmpty(fields) &&
                !_propertyChecker.TypeHasProperties<RentalContractDto>(fields))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Some requested fields are invalid: {fields}"
                });
            }
            var rentalContracts = await _rentalContractService.GetRentalContractsByEquipmentId(equipmentId, fields);
            var rentalContractsToReturn = rentalContracts.Adapt<IEnumerable<RentalContractDto>>()
                .ShapeData(fields);
            return Ok(rentalContractsToReturn);
        }

        [HttpGet("active")]
        [HttpHead("active")]
        public async Task<IActionResult> GetActiveContracts([FromQuery] string? fields)
        {
            if (!string.IsNullOrEmpty(fields) &&
                !_propertyChecker.TypeHasProperties<RentalContractDto>(fields))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Some requested fields are invalid: {fields}"
                });
            }
            var rentalContracts = await _rentalContractService.GetActiveContracts(fields);
            var rentalContractsToReturn = rentalContracts.Adapt<IEnumerable<RentalContractDto>>()
                .ShapeData(fields);
            return Ok(rentalContractsToReturn);
        }

        [HttpGet("expired")]
        [HttpHead("expired")]
        public async Task<IActionResult> GetExpiredContracts([FromQuery] int daysUntilExpiration, [FromQuery] string? fields)
        {
            if (!string.IsNullOrEmpty(fields) &&
                !_propertyChecker.TypeHasProperties<RentalContractDto>(fields))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Some requested fields are invalid: {fields}"
                });
            }

            var rentalContracts = await _rentalContractService.GetExpiredContracts(daysUntilExpiration, fields);
            var rentalContractsToReturn = rentalContracts.Adapt<IEnumerable<RentalContractDto>>()
                .ShapeData(fields);
            return Ok(rentalContractsToReturn);
        }
        #endregion

        #region Add requests
        [HttpPost]
        public async Task<IActionResult> CreateRentalContract([FromBody] RentalContractCreateDto rentalContract)
        {
            var createdReatalContract = await _rentalContractService.CreateRentalContract(rentalContract);

            return CreatedAtRoute(
                "GetRentalContractById",
                new { id = createdReatalContract.Id },
                createdReatalContract);
        }

        [HttpPost("collection")]
        public async Task<IActionResult> CreateRentalContracts(
            [FromBody] List<RentalContractCreateDto> rentalContracts)
        {
            if (rentalContracts is null || !rentalContracts.Any())
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "No customers provided for bulk creation",
                    Message = "You have to Provide at least one customer for creation"
                });
            }

            if (rentalContracts.Count > MaxBulkOperationSize)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Bulk create is limited to {MaxBulkOperationSize} customers at a time",
                    Message = $"Bulk create is {MaxBulkOperationSize}"
                });
            }

            var result = await _rentalContractService.CreateRentalContracts(rentalContracts);
            var idsAsString = string.Join(",", result.SuccessIds);
            return CreatedAtRoute("GetRentalContractsByIds",
                new { ids = idsAsString },
                result);
        }
        #endregion

        #region edit requests

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRentalContract(
            Guid id,
            [FromBody] RentalContractUpdateDto rentalContract)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var exists = await _rentalContractService.RentalContractExists(id);
            if (!exists)
            {
                return NotFound(new ErrorResponse
                {
                    Error = $"Rental contract with id: {id} not found"
                });
            }

            try
            {
                await _rentalContractService.UpdateRentalContract(id, rentalContract);
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new ErrorResponse
                {
                    Error = "The record you attempted to edit was modified by another user after you got the original value.",
                    Message = "The edit operation was canceled."
                });
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> ParicialUpdateRentalContract(
            Guid id,
            [FromBody] JsonPatchDocument<RentalContractUpdateDto> patchDocument)
        {
            if (patchDocument is null) return BadRequest(new ErrorResponse { Error = "invalid patch document." });
            try
            {
                await _rentalContractService.Patch(id, patchDocument);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new ErrorResponse
                {
                    Error = "The record you attempted to edit was modified by another user after you got the original value.",
                    Message = "The edit operation was canceled."
                });
            }
            return NoContent();
        }
        #endregion

        #region Delete requests

        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDeleteRentalContract(Guid id)
        {
            var exists = await _rentalContractService.RentalContractExists(id);
            if (!exists)
            {
                return NotFound(new ErrorResponse
                {
                    Error = $"Rental contract with id: {id} not found"
                });
            }
            await _rentalContractService.SoftDeleteRentalContract(id);
            return NoContent();
        }

        [HttpDelete("collection/({ids})")]
        public async Task<IActionResult> DeleteRentalContracts(
            [FromRoute][ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids is null || !ids.Any())
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "No rental contracts provided for bulk deletion",
                    Message = "You have to Provide at least one rental contract for deletion"
                });
            }
            if (ids.Count() > MaxBulkOperationSize)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Bulk delete is limited to {MaxBulkOperationSize} rental contracts at a time",
                    Message = $"Bulk delete is {MaxBulkOperationSize}"
                });
            }
            var result = await _rentalContractService.DeleteRentalContracts(ids);
            return Ok(result);
        }
        #endregion

        #region Restore requests

        [HttpPost("{id}/restore")]
        public async Task<IActionResult> RestoreRentalContract(Guid id)
        {
            var exists = await _rentalContractService.RentalContractExists(id);
            if (exists)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Rental contract with id: {id} is not deleted",
                    Message = "Only deleted rental contracts can be restored"
                });
            }
            await _rentalContractService.RestoreRentalContract(id);
            return NoContent();
        }

        [HttpPost("collection/({ids})/restore")]
        public async Task<IActionResult> RestoreRentalContracts(
            [FromRoute][ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids is null || !ids.Any())
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "No rental contracts provided for bulk restoration",
                    Message = "You have to Provide at least one rental contract for restoration"
                });
            }
            if (ids.Count() > MaxBulkOperationSize)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Bulk restore is limited to {MaxBulkOperationSize} rental contracts at a time",
                    Message = $"Bulk restore is {MaxBulkOperationSize}"
                });
            }
            var result = await _rentalContractService.RestoreRentalContracts(ids);
            return Ok(result);
        }
        #endregion

        #region Rental contract Actions

        [HttpPost("{id}/active")]
        public async Task<IActionResult> ActiveRentalContract(Guid id)
        {
            await _rentalContractService.ActiveRentalContract(id);
            return NoContent();
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelRentalContract(Guid id)
        {
            await _rentalContractService.CancelRentalContract(id);
            return NoContent();
        }

        [HttpPost("{id}/suspend")]
        public async Task<IActionResult> SuspendRentalContract(Guid id)
        {
            await _rentalContractService.SuspendRentalContract(id);
            return NoContent();
        }

        [HttpPost("{id}/resume")]
        public async Task<IActionResult> ResumeRentalContract(Guid id)
        {
            await _rentalContractService.ResumeRentalContract(id);
            return NoContent();
        }

        [HttpPost("{id}/finish")]
        public async Task<IActionResult> FinishRentalContract(Guid id)
        {
            await _rentalContractService.FinishRentalContract(id);
            return NoContent();
        }

        [HttpPost("finish-expired")]
        public async Task<IActionResult> FinishExpiredContracts()
        {
            await _rentalContractService.FinishExpiredContracts();
            return NoContent();
        }

        #endregion

        #region Existence checks

        [HttpHead("{id}/exists")]
        public async Task<IActionResult> RentalContractExists(Guid id)
        {
            var exists = await _rentalContractService.RentalContractExists(id);
            if (!exists)
            {
                return NotFound(new ErrorResponse
                {
                    Error = $"Rental contract with id: {id} not found"
                });
            }
            return NoContent();
        }

        [HttpHead("customer/{customerId}/has-Contracts")]
        public async Task<IActionResult> CustomerHasContracts(Guid customerId)
        {
            var hasContracts = await _rentalContractService.CustomerHasContracts(customerId);
            if (!hasContracts)
            {
                return NotFound(new ErrorResponse
                {
                    Error = $"Customer with id: {customerId} has no rental contracts"
                });
            }
            return NoContent();
        }

        [HttpHead("equipment/{equipmentId}/has-Contracts")]
        public async Task<IActionResult> EquipmentHasContracts(Guid equipmentId)
        {
            var hasContracts = await _rentalContractService.EquipmentHasContracts(equipmentId);
            if (!hasContracts)
            {
                return NotFound(new ErrorResponse
                {
                    Error = $"Equipment with id: {equipmentId} has no rental contracts"
                });
            }
            return NoContent();
        }

        [HttpHead("overlapping/{equipmentId}")]
        public async Task<IActionResult> HasOverlappingContracts(
            Guid equipmentId,
            [FromQuery] DateTimeOffset startDate,
            [FromQuery] DateTimeOffset endDate,
            [FromQuery] Guid? excludeContractId = null)
        {
            var hasOverlappingContracts = await _rentalContractService.HasOverlappingContracts(
                equipmentId,
                startDate,
                endDate,
                excludeContractId);
            if (!hasOverlappingContracts)
            {
                return NotFound(new ErrorResponse
                {
                    Error = $"No overlapping rental contracts found for equipment with id: {equipmentId}"
                });
            }
            return NoContent();
        }
        #endregion

        #region Analytics

        [HttpGet("analytics/count")]
        public async Task<IActionResult> GetRentalContractCount()
        {
            var rentalContractCount = await _rentalContractAnalytics.GetRentalContractCount();
            return Ok(rentalContractCount);
        }

        [HttpGet("analytics/active-count")]
        public async Task<IActionResult> GetTotalActiveCount()
        {
            var totalActiveCount = await _rentalContractAnalytics.GetTotalActiveCount();
            return Ok(totalActiveCount);
        }

        [HttpGet("analytics/customer/{customerId}/contracts-count")]
        public async Task<IActionResult> GetTotalContractsForCustomer(Guid customerId)
        {
            var totalContractsForCustomer = await _rentalContractAnalytics.GetTotalContractsForCustomer(customerId);
            return Ok(totalContractsForCustomer);
        }

        [HttpGet("analytics/equipment/{equipmentId}/contracts-count")]
        public async Task<IActionResult> GetTotalContractsForEquipment(Guid equipmentId)
        {
            var equipmentHasContract = await _rentalContractService.EquipmentHasContracts(equipmentId);
            if (!equipmentHasContract)
            {
                return NotFound(new ErrorResponse
                {
                    Error = $"The equipment with id: {equipmentId} dose not have contracts"
                });
            }
            var totalContractsForEquipment = await _rentalContractAnalytics.GetTotalContractsForEquipment(equipmentId);
            return Ok(totalContractsForEquipment);
        }

        [HttpGet("analytics/equipment/summary")]
        public async Task<IActionResult> GetEquipmentContractSummary()
        {
            var equipmentContractSummary = await _rentalContractAnalytics.GetEquipmentContractSummary();
            return Ok(equipmentContractSummary);
        }

        [HttpGet("analytics/revenue")]
        public async Task<IActionResult> GetTotalRevenue([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var totalRevenue = await _rentalContractAnalytics.GetTotalRevenue(from, to);
            return Ok(totalRevenue);
        }

        [HttpGet("analytics/revenue/by-customer")]
        public async Task<IActionResult> GetRevenueByCustomer(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var revenueByCustomer = await _rentalContractAnalytics.GetRevenueByCustomer(from, to);
            return Ok(revenueByCustomer);
        }

        [HttpGet("analytics/revenue/by-month/{year}")]
        public async Task<IActionResult> GetRevenueByMonth(int year)
        {
            var revenueByMonth = await _rentalContractAnalytics.GetRevenueByMonth(year);
            return Ok(revenueByMonth);
        }

        [HttpGet("analytics/price-stats")]
        public async Task<IActionResult> GetContractPriceStatistics()
        {
            var priceStatistics = await _rentalContractAnalytics.GetContractPriceStatistics();
            return Ok(priceStatistics);
        }

        [HttpGet("analytics/finished-count")]
        public async Task<IActionResult> GetFinishedContractsCount([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var finishedContractsCount = await _rentalContractAnalytics.GetFinishedContractsCount(from, to);
            return Ok(finishedContractsCount);
        }

        [HttpGet("analytics/cancelled-count")]
        public async Task<IActionResult> GetCancelledContractsCount([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var cancelledContractsCount = await _rentalContractAnalytics.GetCancelledContractsCount(from, to);
            return Ok(cancelledContractsCount);
        }

        [HttpGet("analytics/average-duration")]
        public async Task<IActionResult> GetAverageContractDurationInDays()
        {
            var averageDuration = await _rentalContractAnalytics.GetAverageContractDurationInDays();
            return Ok(averageDuration);
        }

        [HttpGet("analytics/average-revenue-per-customer")]
        public async Task<IActionResult> GetAverageRevenuePerCustomer()
        {
            var averageRevenuePerCustomer = await _rentalContractAnalytics.GetAverageRevenuePerCustomer();
            return Ok(averageRevenuePerCustomer);
        }

        [HttpGet("analytics/average-rental-price")]
        public async Task<IActionResult> GetAverageRentalPrice()
        {
            var averageRentalPrice = await _rentalContractAnalytics.GetAverageRentalPrice();
            return Ok(averageRentalPrice);
        }
        #endregion
    }
}
