using Application.BulkOperations;
using Application.Interface.Repositories;
using Application.Interface.Services;
using Application.Models.CustomersModels.Read;
using Application.Models.CustomersModels.Write;
using Application.Models.PhoneNumberModels.Read;
using Application.Models.PhoneNumberModels.Write;
using Application.ResourceParameters;
using Domain.Entities;
using EquipmentAPI.ModelBinders;
using Mapster;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Extensions;
using Shared.Results;
using System.Data;
using System.Text.Json;

namespace API.Controllers
{
    [ApiController]
    [Route("/api/customers")]
    public class CustomerController : ControllerBase
    {
        private readonly IPropertyCheckerService _propertyChecker;
        private readonly ICustomerRepository _customerRepo;

        public CustomerController(
            ICustomerRepository customerRepository,
            IPropertyCheckerService propertyChecker)
        {
            _customerRepo = customerRepository ??
                throw new ArgumentNullException(nameof(customerRepository));
            _propertyChecker = propertyChecker ??
                throw new ArgumentNullException(nameof(propertyChecker));
        }

        [HttpGet(Name = "GetCustomers")]
        [HttpHead]
        public async Task<IActionResult> GetCustomers([FromQuery] CustomerResourceParameters parameters)
        {
            if (!_propertyChecker.TypeHasProperties<CustomerDto>(parameters.Fields))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Some requested fields are invalid: {parameters.Fields}"
                });
            }

            if (parameters.SortBy != "" && !_propertyChecker.TypeHasProperties<CustomerDto>(parameters.SortBy))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Sort by is not field of the customer: {parameters.SortBy}",
                    Message = "Sort by should be one of the entity fields"
                });
            }

            var customers = await _customerRepo.GetCustomers(parameters);

            Response.Headers.Append("X-Pagination",
                JsonSerializer.Serialize(customers.CreatePaginationMetadata()));

            var customersToReturn = customers.Adapt<IEnumerable<CustomerDto>>().ShapeData(parameters.Fields);

            return Ok(customersToReturn);
        }

        [HttpGet("collection/({ids})", Name = "GetCustomerCollection")]
        [HttpHead("collection/({ids})")]
        public async Task<IActionResult> GetCustomersCollection(
            [FromRoute]
            [ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids,
            [FromQuery] CustomerResourceParameters? parameters)
        {
            if (ids == null || !ids.Any())
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Ids are invalid"
                });
            }

            var customers = await _customerRepo.GetCustomersByIds(ids, parameters.Fields);

            if (customers.Count() != ids.Count())
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Ids are invalid"
                });
            }

            var customersToReturn = customers.Adapt<IEnumerable<CustomerDto>>().ShapeData(parameters.Fields);

            return Ok(customersToReturn);
        }

        [HttpGet("{id:guid}", Name = "GetCustomerById")]
        [HttpHead("{id:guid}")]
        public async Task<IActionResult> GetCustomerById(Guid id, [FromQuery] string? fields)
        {
            if (!string.IsNullOrEmpty(fields) &&
                !_propertyChecker.TypeHasProperties<CustomerDto>(fields))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Some requested fields are invalid: {fields}"
                });
            }
            var customer = await _customerRepo.GetCustomerById(id, fields);
            if (customer == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = $"The customer with {id} does not exist"
                });
            }

            var customerToReturn = customer.Adapt<CustomerDto>().ShapeData(fields);

            return Ok(customerToReturn);
        }

        [HttpHead("{id}/exists")]
        public async Task<IActionResult> CustomerExists(Guid id)
        {
            var exists = await _customerRepo.CustomerExists(id);

            if (!exists)
                return NotFound(new ErrorResponse
                {
                    Error = $"No customer found with Id: {id}"
                });

            return Ok();
        }

        [HttpGet("by-email/{email}", Name = "GetCustomerByEmail")]
        [HttpHead("by-email/{email}")]
        public async Task<IActionResult> GetCustomerByEmail([FromRoute] string email, [FromQuery] string fields)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Email: {email} is invalid"
                });
            }

            if (!_propertyChecker.TypeHasProperties<CustomerDto>(fields))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Some requested fields are invalid: {fields}"
                });
            }

            var customer = await _customerRepo.GetCustomerByEmail(email, fields);

            if (customer == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = $"No customer found with email: {email}"
                });
            }

            var customerToReturn = customer.Adapt<CustomerDto>().ShapeData(fields);

            return Ok(customerToReturn);
        }

        [HttpHead("by-email/{email}/exists")]
        public async Task<IActionResult> EmailExists(string email)
        {
            var exists = await _customerRepo.EmailExists(email);

            if (!exists)
                return NotFound();

            return Ok();
        }

        [HttpGet("count")]
        [HttpHead("count")]
        public async Task<IActionResult> GetCustomersCount([FromQuery] CustomerResourceParameters? parameters)
        {
            if (parameters.Fields != null && !_propertyChecker.TypeHasProperties<CustomerDto>(parameters.Fields))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Some requested fields are invalid: {parameters.Fields}"
                });
            }

            var count = await _customerRepo.GetCustomersCount(parameters);

            return Ok($"count: {count}");
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CustomerCreateDto customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await _customerRepo.EmailExists(customer.Email))
            {
                return Conflict(new ErrorResponse
                {
                    Error = "Duplicate email",
                    Message = $"A customer with email '{customer.Email}' already exists"
                });
            }

            var createdCustomer = customer.Adapt<Customer>();
            _customerRepo.CreateCustomer(createdCustomer);
            await _customerRepo.SaveChangesAsync();

            return CreatedAtRoute(
                "GetCustomerById",
                new { id = createdCustomer.Id },
                createdCustomer
                );
        }

        [HttpPost("collection")]
        public async Task<IActionResult> CreateCustomerCollection([FromBody] List<CustomerCreateDto> customers)
        {
            if (customers == null || !customers.Any())
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "No customers provided for bulk creation",
                    Message = "You have to Provide at least one customer for creation"
                });
            }

            if (customers.Count > 1000)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Bulk create is limited to 1000 customers at a time",
                    Message = "Bulk create is 1000"
                });
            }

            var customersToCreate = customers.Adapt<IEnumerable<Customer>>();

            var result = await _customerRepo.CreateCustomers(customersToCreate);
            await _customerRepo.SaveChangesAsync();

            var response = new BulkOperationResponse
            {
                TotalRequested = customers.Count,
                SuccessCount = result.SuccessCount,
                FailureCount = result.FailureCount,
                SuccessRate = result.SuccessRate,
                SuccessfulIds = result.SuccessIds,
                Errors = result.Errors.Select(e => new BulkOperationError
                {
                    EntityId = e.EntityId,
                    ErrorMessage = e.ErrorMessage
                }).ToList(),
                Message = result.HasErrors
                        ? $"Bulk create completed with {result.FailureCount} errors"
                        : "All customers created successfully"
            };

            var idsAsString = string.Join(",", result.SuccessIds);
            return result.HasErrors
                ? StatusCode(207, response) // 207 Multi-Status
                : CreatedAtRoute("GetCustomerCollection",
                new { ids = idsAsString }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] CustomerUpdateDto customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var customerFromRepo = await _customerRepo.GetCustomerById(id, null);
            if (customerFromRepo == null) return NotFound(
                new ErrorResponse
                {
                    Error = $"Customer with ID {id} not found"
                });

            try
            {
                customer.Adapt(customerFromRepo);
                await _customerRepo.UpdateCustomer(customerFromRepo);
                await _customerRepo.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> CustomerPartialUpdate(Guid id, [FromBody] JsonPatchDocument<CustomerUpdateDto> patchDocument)
        {
            if (patchDocument is null)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Invalid patch document."
                });
            }
            var customerFromRepo = await _customerRepo.GetCustomerById(id, null);
            if (customerFromRepo == null) return NotFound(
                new ErrorResponse
                {
                    Error = $"Customer with ID {id} not found"
                });

            var customer = customerFromRepo.Adapt<CustomerUpdateDto>();
            patchDocument.ApplyTo(customer, ModelState);

            if (!TryValidateModel(customer))
            {
                return ValidationProblem(ModelState);
            }

            customer.Adapt(customerFromRepo);
            await _customerRepo.UpdateCustomer(customerFromRepo);
            await _customerRepo.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(Guid id)
        {
            var isCustomerExist = await _customerRepo.CustomerExists(id);
            if (!isCustomerExist) return NotFound(
                 new ErrorResponse
                 {
                     Error = $"Customer with ID {id} not found"
                 });

            await _customerRepo.DeleteCustomer(id);
            await _customerRepo.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id}/restore")]
        public async Task<IActionResult> RestoreCustomer(Guid id)
        {
            var restored = await _customerRepo.RestoreCustomer(id);
            if (!restored) return NotFound(
                 new ErrorResponse
                 {
                     Error = $"Customer with ID {id} not found"
                 });
            await _customerRepo.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("collection/({ids})")]
        public async Task<IActionResult> CustomerDeleteCollection(
            [FromRoute][ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids == null || !ids.Any())
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Invalid Ids",
                    Message = "No IDs were provided for deletion."
                });
            }

            if (ids.Count() > 1000)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Too many items",
                    Message = "Bulk delete is limited to 1000 customers at a time"
                });
            }

            var result = await _customerRepo.DeleteCustomers(ids);
            await _customerRepo.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("collection/({ids})/restore")]
        public async Task<IActionResult> CustomerRestoreCollection(
            [FromRoute][ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids == null || !ids.Any())
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Invalid Ids",
                    Message = "No IDs were provided for deletion."
                });
            }

            var result = await _customerRepo.RestoreCustomers(ids);
            await _customerRepo.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{customerId}/phoneNumbers/{phoneNumberId}", Name = "GetCustomerPhoneNumber")]
        [HttpHead("{customerId}/phoneNumbers/{phoneNumberId}")]
        public async Task<IActionResult> GetCustomerPhoneNumber(Guid customerId, Guid phoneNumberId)
        {
            var isCustomerExist = await _customerRepo.CustomerExists(customerId);
            if (!isCustomerExist) return NotFound(
                 new ErrorResponse
                 {
                     Error = $"Customer with Id {customerId} does not exist"
                 });

            var phoneNumberFromRepo = await _customerRepo.GetPhoneNumber(phoneNumberId);
            if (phoneNumberFromRepo == null) return NotFound(
                new ErrorResponse
                {
                    Error = $"Phone number with Id {phoneNumberId} does not exist"
                });

            var phoneNumber = phoneNumberFromRepo.Adapt<CustomerPhoneNumberDto>();

            return Ok(phoneNumber);
        }

        [HttpPost("{customerId}/phoneNumbers")]
        public async Task<IActionResult> CreateCustomerPhoneNumber(Guid customerId, CustomerPhoneNumberCreateDto phoneNumber)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var isCustomerExist = await _customerRepo.CustomerExists(customerId);
            if (!isCustomerExist) return NotFound(
                 new ErrorResponse
                 {
                     Error = $"Customer with Id {customerId} does not exist"
                 });

            var phoneNumberToCreate = phoneNumber.Adapt<CustomerPhoneNumber>();
            _customerRepo.CreateCustomerPhoneNumber(customerId, phoneNumberToCreate);
            await _customerRepo.SaveChangesAsync();

            return CreatedAtRoute("GetCustomerPhoneNumber",
                new { customerId, phoneNumberId = phoneNumberToCreate.Id }
                , phoneNumberToCreate.Adapt<CustomerPhoneNumberDto>());
        }

        [HttpPut("{customerId}/phoneNumbers/{phoneNumberId}")]
        public async Task<IActionResult> UpdatePhoneNumbers(
            Guid customerId,
            Guid phoneNumberId,
            [FromBody] CustomerPhoneNumberUpdateDto phonenumber)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var isCustomerExist = await _customerRepo.CustomerExists(customerId);
            if (!isCustomerExist) return NotFound(
                 new ErrorResponse
                 {
                     Error = $"Customer with Id {customerId} does not exist"
                 });

            var phoneNumberFromRepo = await _customerRepo.GetPhoneNumber(phoneNumberId);
            if (phoneNumberFromRepo == null) return NotFound(
                new ErrorResponse
                {
                    Error = $"Phone number with Id {phoneNumberId} does not exist"
                });

            try
            {
                phonenumber.Adapt(phoneNumberFromRepo);
                await _customerRepo.UpdatePhoneNumber(customerId, phoneNumberId, phoneNumberFromRepo);
                await _customerRepo.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Conflict(ex.Message);
            }

            return NoContent();
        }

        [HttpHead("phonenumbers/{phoneNumberId}/exists")]
        public async Task<IActionResult> PhoneNumberExists(Guid phoneNumberId)
        {
            var IsPhoneNumberExist = await _customerRepo.PhoneNumberExists(phoneNumberId);
            if (!IsPhoneNumberExist) return NotFound(
                new ErrorResponse
                {
                    Error = $"Phone number with Id {phoneNumberId} does not exist"
                });

            return Ok();
        }

        [HttpDelete("{customerId}/phonenumbers/{phoneNumberId}")]
        public async Task<IActionResult> DeletePhoneNumber(Guid customerId, Guid phoneNumberId)
        {
            var isCustomerExist = await _customerRepo.CustomerExists(customerId);
            if (!isCustomerExist) return NotFound(
                 new ErrorResponse
                 {
                     Error = $"Customer with Id {customerId} does not exist"
                 });


            var IsPhoneNumberExist = await _customerRepo.PhoneNumberExists(phoneNumberId);
            if (!IsPhoneNumberExist) return NotFound(
                new ErrorResponse
                {
                    Error = $"Phone number with Id {phoneNumberId} does not exist"
                });

            await _customerRepo.DeletePhoneNumber(customerId, phoneNumberId);
            await _customerRepo.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{customerId}/phonenumbers/{phoneNumberId}/restore")]
        public async Task<IActionResult> RestorePhoneNumber(Guid customerId, Guid phoneNumberId)
        {
            var isCustomerExist = await _customerRepo.CustomerExists(customerId);
            if (!isCustomerExist) return NotFound(
                 new ErrorResponse
                 {
                     Error = $"Customer with Id {customerId} does not exist"
                 });

            var restored = await _customerRepo.RestorePhoneNumber(phoneNumberId);
            if (!restored) return NotFound(
                new ErrorResponse
                {
                    Error = $"Phone number with ID {phoneNumberId} not found or not deleted."
                });

            await _customerRepo.SaveChangesAsync();
            return NoContent();
        }

        [HttpOptions()]
        public IActionResult GetCustomersOptions()
        {
            Response.Headers.Append("Allow", "Get, Head, Put, Patch, Post, Delete, Options");
            return Ok();
        }

        [HttpOptions("{id:guid}")]
        public IActionResult GetCustomerOptions()
        {
            Response.Headers.Append("Allow", "Get, Head, Put, Patch, Post, Delete, Options");
            return Ok();
        }

        [HttpOptions("{customerId}/phonenumbers/{phoneNumberId}")]
        public IActionResult GetCustomerPhoneNumberOptions()
        {
            Response.Headers.Append("Allow", "Get, Head, Put, Patch, Delete, Options");
            return Ok();
        }

        [HttpOptions("collection/{ids}")]
        public IActionResult GetCustomersCollectionOptions()
        {
            Response.Headers.Append("Allow", "GET, DELETE, OPTIONS");
            return Ok();
        }

        [HttpOptions("{id}/resrote")]
        public IActionResult RestoreCustomersOptions()
        {
            Response.Headers.Append("Allow", "POST,OPTIONS");
            return Ok();
        }
    }
}