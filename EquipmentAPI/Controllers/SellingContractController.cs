using Application.Interface.Services;
using Application.Models.SellingContract.Read;
using Application.Models.SellingContract.Write;
using Application.ResourceParameters;
using EquipmentAPI.ModelBinders;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Shared.Extensions;
using Shared.Results;
using System.Data;
using System.Text.Json;

namespace API.Controllers
{
    [ApiController]
    [Route("/api/SellingContracts")]
    public class SellingContractController : ControllerBase
    {
        private readonly ISellingContractService _sellingContractService;
        private readonly ISellingContractAnalyticsService _sellingContractAnalyticsService;
        private readonly IPropertyCheckerService _propertyChecker;
        private const int MaxBulkOperationSize = 500;

        public SellingContractController(
            ISellingContractService sellingContractService,
            ISellingContractAnalyticsService sellingContractAnalyticsService,
            IPropertyCheckerService checkerService)
        {
            _sellingContractService = sellingContractService ??
                throw new ArgumentNullException(nameof(sellingContractService));
            _sellingContractAnalyticsService = sellingContractAnalyticsService ??
                throw new ArgumentNullException(nameof(sellingContractAnalyticsService));
            _propertyChecker = checkerService ??
                throw new ArgumentNullException(nameof(checkerService));
        }

        #region Get Requests
        [HttpGet(Name = "GetSellingContracts")]
        [HttpHead]
        public async Task<IActionResult> GetSellingContracts([FromQuery] SellingContractResourceParameters parameters)
        {
            if (!_propertyChecker.TypeHasProperties<SellingContractDto>(parameters.Fields))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Some requested fields are invalid: {parameters.Fields}"
                });
            }
            if (!string.IsNullOrEmpty(parameters.SortBy) &&
               !_propertyChecker.TypeHasProperties<SellingContractDto>(parameters.SortBy))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Sort by is not field of the selling contract: {parameters.SortBy}",
                    Message = "Sort by should be one of the entity fields"
                });
            }
            var sellingContracts = await _sellingContractService.GetSellingContracts(parameters);
            Response.Headers.Append("X-Pagination",
                JsonSerializer.Serialize(sellingContracts.CreatePaginationMetadata()));

            return Ok(sellingContracts.ShapeData(parameters.Fields));
        }

        [HttpGet("collection/({ids})", Name = "GetSellingContractsByIds")]
        [HttpHead("collection/({ids})")]

        public async Task<IActionResult> GetSellingContractsByIds(
            [FromRoute][ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids,
            [FromQuery] string? fields)
        {
            if (ids == null || !ids.Any())
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "No IDs provided",
                    Message = "At least one selling contract ID must be provided"
                });
            }

            if (!_propertyChecker.TypeHasProperties<SellingContractDto>(fields))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Some requested fields are invalid: {fields}"
                });
            }

            var sellingContracts = await _sellingContractService.GetSellingContractsByIds(ids, fields);
            return Ok(sellingContracts.ShapeData(fields));
        }

        [HttpGet("{id}", Name = "GetSellingContractById")]
        [HttpHead("{id}")]
        public async Task<IActionResult> GetSellingContractById([FromRoute] Guid id, string? fields)
        {
            if (!_propertyChecker.TypeHasProperties<SellingContractDto>(fields))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Some requested fields are invalid: {fields}"
                });
            }

            var sellingContract = await _sellingContractService.GetSellingContractById(id, fields);
            return Ok(sellingContract.ShapeData(fields));
        }

        [HttpGet("customer/{customerId}")]
        [HttpHead("customer/{customerId}")]
        public async Task<IActionResult> GetSellingContractsByCustomerId(
            [FromRoute] Guid customerId,
            [FromQuery] string? fields)
        {
            if (!_propertyChecker.TypeHasProperties<SellingContractDto>(fields))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Some requested fields are invalid: {fields}"
                });
            }

            var sellingContracts = await _sellingContractService
                .GetSellingContractsByCustomerId(customerId, fields);
            return Ok(sellingContracts.ShapeData(fields));
        }

        [HttpGet("equipment/{equipmentId}")]
        [HttpHead("equipment/{equipmentId}")]
        public async Task<IActionResult> GetSellingContractsByEquipmentId(
            [FromRoute] Guid equipmentId,
            [FromQuery] string? fields)
        {
            if (!_propertyChecker.TypeHasProperties<SellingContractDto>(fields))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Some requested fields are invalid: {fields}"
                });
            }
            var sellingContracts = await _sellingContractService
                .GetSellingContractsByEquipmentId(equipmentId, fields);
            return Ok(sellingContracts.ShapeData(fields));
        }

        [HttpGet("year", Name = "GetSellingContractByYear")]
        [HttpHead("year")]
        public async Task<IActionResult> GetSellingContractByYear([FromQuery] int year)
        {
            if (year < 0 || year > DateTimeOffset.UtcNow.Year)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Invalid year provided",
                    Message = "Year must be a positive integer and cannot be in the future"
                });
            }
            var sellingContracts = await _sellingContractService.GetSellingContractsByYear(year);
            return Ok(sellingContracts);
        }

        [HttpGet("deleted", Name = "GetDeletedSellingContracts")]
        [HttpHead("deleted")]
        public async Task<IActionResult> GetDeletedSellingContracts(
            [FromQuery] SellingContractResourceParameters parameters)
        {
            if (!_propertyChecker.TypeHasProperties<SellingContractDto>(parameters.Fields))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Some requested fields are invalid: {parameters.Fields}"
                });
            }

            if (!string.IsNullOrEmpty(parameters.SortBy) &&
               !_propertyChecker.TypeHasProperties<SellingContractDto>(parameters.SortBy))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Sort by is not field of the selling contract: {parameters.SortBy}",
                    Message = "Sort by should be one of the entity fields"
                });
            }

            var sellingContracts = await _sellingContractService.GetDeletedSellingContracts(parameters);
            return Ok(sellingContracts.ShapeData(parameters.Fields));
        }

        [HttpGet("deleted/{ id}")]
        [HttpHead("deleted/{ id}")]
        public async Task<IActionResult> GetDeletedSellingContractById(
            [FromRoute] Guid id,
            [FromQuery] string? fields)
        {
            if (!_propertyChecker.TypeHasProperties<SellingContractDto>(fields))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Some requested fields are invalid: {fields}"
                });
            }
            var sellingContract = await _sellingContractService.GetDeletedSellingContractById(id, fields);
            return Ok(sellingContract.ShapeData(fields));
        }
        #endregion

        #region Existence checks
        [HttpHead("{id}/exists")]
        public async Task<IActionResult> SellingContractExists([FromRoute] Guid id)
        {
            var exists = await _sellingContractService.SellingContractExists(id);
            return exists ? NoContent() : NotFound();
        }

        [HttpHead("customer/{customerId}/hascontracts")]
        public async Task<IActionResult> CustomerHasContracts([FromRoute] Guid customerId)
        {
            var hasContracts = await _sellingContractService.CustomerHasContracts(customerId);
            return hasContracts ? NoContent() : NotFound();
        }
        #endregion

        #region Add Requests
        [HttpPost]
        public async Task<IActionResult> CreateSellingContract(
            [FromBody] SellingContractCreateDto sellingContract)
        {
            var createdSellingContract = await _sellingContractService.CreateSellingContract(sellingContract);
            return CreatedAtRoute("GetSellingContractById",
                new { id = createdSellingContract.Id },
                createdSellingContract);
        }

        [HttpPost("collection")]
        public async Task<IActionResult> CreateSellingContracts(
            [FromBody] IEnumerable<SellingContractCreateDto> sellingContracts)
        {
            if (sellingContracts is null || !sellingContracts.Any())
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "No selling contracts provided for bulk creation",
                    Message = "You have to Provide at least one contract for creation"
                });
            }

            if (sellingContracts.Count() > MaxBulkOperationSize)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Bulk create is limited to {MaxBulkOperationSize} contracts at a time",
                    Message = $"Bulk create is {MaxBulkOperationSize}"
                });
            }

            var result = await _sellingContractService.CreateSellingContracts(sellingContracts);
            if (result.FailureCount > 0)
            {
                return StatusCode(207, result);
            }
            var idsAsString = string.Join(",", result.SuccessIds);
            return CreatedAtRoute("GetSellingContractsByIds",
                new { ids = idsAsString },
                result);
        }
        #endregion

        #region Update Requests
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSellingContract(
            [FromRoute] Guid id,
            [FromBody] SellingContractUpdateDto sellingContract)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var exists = await _sellingContractService.SellingContractExists(id);

            if (!exists)
                return NotFound(new ErrorResponse { Error = $"Selling contract with id: {id} not found" });
            try
            {
                await _sellingContractService.UpdateSellingContract(id, sellingContract);
                return NoContent();
            }
            catch (DBConcurrencyException)
            {
                return Conflict(new ErrorResponse
                {
                    Error = "The record you attempted to edit was modified by another user after you got the original value.",
                    Message = "The edit operation was canceled."
                });
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchSellingContract(
            [FromRoute] Guid id,
            [FromBody] JsonPatchDocument<SellingContractUpdateDto> patchDocument)
        {
            if (patchDocument is null) return BadRequest(new ErrorResponse { Error = "invalid patch document." });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var exists = await _sellingContractService.SellingContractExists(id);
            if (!exists)
                return NotFound(new ErrorResponse { Error = $"Selling contract with id: {id} not found" });

            try
            {
                await _sellingContractService.Patch(id, patchDocument);
                return NoContent();
            }
            catch (DBConcurrencyException)
            {
                return Conflict(new ErrorResponse
                {
                    Error = "The record you attempted to edit was modified by another user after you got the original value.",
                    Message = "The edit operation was canceled."
                });
            }
        }
        #endregion

        #region Delete Requests
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDeleteSellingContract([FromRoute] Guid id)
        {
            var exists = await _sellingContractService.SellingContractExists(id);
            if (!exists)
                return NotFound(new ErrorResponse { Error = $"Selling contract with id: {id} not found" });

            await _sellingContractService.SoftDeleteSellingContract(id);
            return NoContent();
        }

        [HttpDelete("collection/{ids}")]
        public async Task<IActionResult> SoftDeleteSellingContracts(
            [FromQuery][ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids is null || !ids.Any())
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "No IDs provided for bulk deletion",
                    Message = "At least one selling contract ID must be provided"
                });
            }
            if (ids.Count() > MaxBulkOperationSize)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Bulk delete is limited to {MaxBulkOperationSize} contracts at a time",
                    Message = $"Bulk delete is {MaxBulkOperationSize}"
                });
            }
            var result = await _sellingContractService.SoftDeleteSellingContracts(ids);
            if (result.FailureCount > 0)
            {
                return StatusCode(207, result);
            }
            return NoContent();
        }
        #endregion

        #region Restore requests
        [HttpPost("{id}/restore")]
        public async Task<IActionResult> RestoreSellingContract([FromRoute] Guid id)
        {
            var exists = await _sellingContractService.SellingContractExists(id);
            if (exists)
                return BadRequest(new ErrorResponse { Error = $"Selling contract with id: {id} is not deleted." });
            await _sellingContractService.RestoreSellingContract(id);
            return NoContent();
        }

        [HttpPost("collection/{ids}/restore")]
        public async Task<IActionResult> RestoreSellingContracts(
            [FromBody][ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids is null || !ids.Any())
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "No IDs provided for bulk restoration",
                    Message = "At least one selling contract ID must be provided"
                });
            }
            if (ids.Count() > MaxBulkOperationSize)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Bulk restore is limited to {MaxBulkOperationSize} contracts at a time",
                    Message = $"Bulk restore is {MaxBulkOperationSize}"
                });
            }
            var result = await _sellingContractService.RestoreSellingContracts(ids);
            if (result.FailureCount > 0)
            {
                return StatusCode(207, result);
            }
            return NoContent();
        }
        #endregion

        #region Analytics Requests
        [HttpGet("analytics/total-revenue")]
        public async Task<IActionResult> GetTotalRevenue([FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to)
        {
            return Ok(await _sellingContractAnalyticsService.GetTotalRevenue(from, to));
        }

        [HttpGet("analytics/average-sale-price")]
        public async Task<IActionResult> GetAverageSalePrice([FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to)
        {
            return Ok(await _sellingContractAnalyticsService.GetAverageSalePrice(from, to));
        }

        [HttpGet("analytics/average-sale-price-by-equipment")]
        public async Task<IActionResult> GetAverageSalePriceByEquipment(
            [FromQuery] DateTimeOffset? from,
            [FromQuery] DateTimeOffset? to,
            [FromQuery] Domain.Enums.EquipmentBrand? equipmentBrand,
            [FromQuery] Domain.Enums.EquipmentType? equipmentType)
        {
            return Ok(await _sellingContractAnalyticsService
                .GetAverageSalePriceByEquipment(from, to, equipmentBrand, equipmentType));
        }

        [HttpGet("analytics/min-sale-price")]
        public async Task<IActionResult> GetMinSalePrice([FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to)
        {
            return Ok(await _sellingContractAnalyticsService.GetMinSalePrice(from, to));
        }

        [HttpGet("analytics/max-sale-price")]
        public async Task<IActionResult> GetMaxSalePrice([FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to)
        {
            return Ok(await _sellingContractAnalyticsService.GetMaxSalePrice(from, to));
        }

        [HttpGet("analytics/revenue-by-day")]
        public async Task<IActionResult> GetRevenueByDay(
            [FromQuery] DateTimeOffset? from,
            [FromQuery] DateTimeOffset? to)
        {
            return Ok(await _sellingContractAnalyticsService.GetRevenueByDay(from, to));
        }

        [HttpGet("analytics/revenue-by-month")]
        public async Task<IActionResult> GetRevenueByMonth(
            [FromQuery] int year)
        {
            return Ok(await _sellingContractAnalyticsService.GetRevenueByMonth(year));
        }

        [HttpGet("analytics/revenue-by-year")]
        public async Task<IActionResult> GetRevenueByYear()
        {
            return Ok(await _sellingContractAnalyticsService.GetRevenueByYear());
        }

        [HttpGet("analytics/sales-count")]
        public async Task<IActionResult> GetSalesCount(
            [FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to)
        {
            return Ok(await _sellingContractAnalyticsService.GetSalesCount(from, to));
        }

        [HttpGet("analytics/revenue-by-customer")]
        public async Task<IActionResult> GetRevenueByCustomer(
            [FromQuery] DateTimeOffset? from,
            [FromQuery] DateTimeOffset? to)
        {
            return Ok(await _sellingContractAnalyticsService.GetRevenueByCustomer(from, to));
        }

        [HttpGet("analytics/top-customers")]
        public async Task<IActionResult> GetTopCustomers(
            [FromQuery] int top,
            [FromQuery] DateTimeOffset? from,
            [FromQuery] DateTimeOffset? to)
        {
            if (top <= 0 || top > 100) return BadRequest(new ErrorResponse { Error = "Top parameter must be between 1 and 100." });
            return Ok(await _sellingContractAnalyticsService.GetTopCustomers(top, from, to));
        }

        [HttpGet("analytics/sales-count-by-customer")]
        public async Task<IActionResult> GetSalesCountByCustomer(
            [FromQuery] DateTimeOffset? from,
            [FromQuery] DateTimeOffset? to)
        {
            return Ok(await _sellingContractAnalyticsService.GetSalesCountByCustomer(from, to));
        }

        [HttpGet("analytics/revenue-by-equipment")]
        public async Task<IActionResult> GetRevenueByEquipment(
            [FromQuery] DateTimeOffset? from,
            [FromQuery] DateTimeOffset? to)
        {
            return Ok(await _sellingContractAnalyticsService.GetRevenueByEquipment(from, to));
        }

        [HttpGet("analytics/top-selling-equipment")]
        public async Task<IActionResult> GetTopSellingEquipment(
            [FromQuery] int top,
            [FromQuery] DateTimeOffset? from,
            [FromQuery] DateTimeOffset? to)
        {
            if (top <= 0 || top > 100) return BadRequest(new ErrorResponse { Error = "Top parameter must be between 1 and 100." });
                return Ok(await _sellingContractAnalyticsService.GetTopSellingEquipment(top, from, to));
        }

        [HttpGet("analytics/sales-count-by-equipment")]
        public async Task<IActionResult> GetSalesCountByEquipment(
            [FromQuery] DateTimeOffset? from,
            [FromQuery] DateTimeOffset? to)
        {
            return Ok(await _sellingContractAnalyticsService.GetSalesCountByEquipment(from, to));
        }

        [HttpGet("analytics/deleted-contracts-count")]
        public async Task<IActionResult> GetDeletedContractsCount()
        {
            return Ok(await _sellingContractAnalyticsService.GetDeletedContractsCount());
        }

        [HttpGet("analytics/average-sale-price-per-equipment/{equipmentId}")]
        public async Task<IActionResult> GetAverageSalePricePerEquipment([FromRoute] Guid equipmentId)
        {
            return Ok(await _sellingContractAnalyticsService.GetAverageSalePricePerEquipment(equipmentId));
        }
        #endregion
    }
}
