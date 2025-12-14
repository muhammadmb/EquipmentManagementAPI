using EquipmentAPI.Controllers;
using EquipmentAPI.Entities;
using EquipmentAPI.Helper;
using EquipmentAPI.Helper.BulkOperations;
using EquipmentAPI.Models.CustomersModels.Read;
using EquipmentAPI.Models.CustomersModels.Write;
using EquipmentAPI.Models.PhoneNumberModels.Write;
using EquipmentAPI.Repositories.Customer_Repository;
using EquipmentAPI.ResourceParameters;
using EquipmentAPI.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Dynamic;

namespace EquipmentAPI.Tests.CustomerTests
{
    public class CustomerControllerTests
    {
        private readonly Mock<ICustomerRepository> _mockRepo;
        private readonly Mock<IPropertyCheckerService> _mockPropertyChecker;
        private readonly CustomerController _customerController;

        public CustomerControllerTests()
        {
            _mockRepo = new Mock<ICustomerRepository>();
            _mockPropertyChecker = new Mock<IPropertyCheckerService>();
            _customerController = new CustomerController(_mockRepo.Object, _mockPropertyChecker.Object);
        }

        [Fact]
        public async Task GetAllCustomers_ReturnsOkResult_WithListOfCustomers()
        {
            // Arrange
            var customers = new PagedList<Customer>(new List<Customer>(), 1, 1, 10);
            var parameters = new CustomerResourceParameters { Fields = "Id" };

            _mockRepo.Setup(repo => repo.GetCustomers(parameters))
                     .ReturnsAsync(customers);
            _mockPropertyChecker.Setup(service => service.TypeHasProperties<CustomerDto>(parameters.Fields))
                     .Returns(true);

            var httpContext = new DefaultHttpContext();
            _customerController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await _customerController.GetCustomers(parameters);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var responseHeaders = httpContext.Response.Headers;
            responseHeaders.Should().ContainKey("X-Pagination");
        }

        [Fact]
        public async Task GetAllCustomers_ReturnsBadRequest_WhenInvalidFields()
        {
            // Arrange
            var parameters = new CustomerResourceParameters { Fields = "InvalidField" };
            _mockPropertyChecker.Setup(service => service.TypeHasProperties<CustomerDto>(parameters.Fields))
                     .Returns(false);
            // Act
            var result = await _customerController.GetCustomers(parameters);
            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetAllCustomers_ReturnsOkOfEmptyList_WhenNoCustomers()
        {
            // Arrange
            var customers = new PagedList<Customer>(new List<Customer>(), 0, 0, 0);
            var parameters = new CustomerResourceParameters { Fields = "Id" };
            _mockRepo.Setup(repo => repo.GetCustomers(parameters))
                     .ReturnsAsync(customers);
            _mockPropertyChecker.Setup(service => service.TypeHasProperties<CustomerDto>(parameters.Fields))
                     .Returns(true);
            var httpConsept = new DefaultHttpContext();
            _customerController.ControllerContext = new ControllerContext
            {
                HttpContext = httpConsept
            };

            // Act
            var result = await _customerController.GetCustomers(parameters);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedList = Assert.IsAssignableFrom<List<ExpandoObject>>(okResult.Value);
            Assert.Empty(returnedList);
        }

        [Fact]
        public async Task GetCustomerById_ReturnOkResult()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new Customer
            {
                Id = customerId,
                Name = "Test Customer"
            };
            var parameters = new CustomerResourceParameters { Fields = "Id,Name" };

            _mockRepo.Setup(repo => repo.GetCustomerById(customerId, parameters.Fields))
                     .ReturnsAsync(customer);

            _mockPropertyChecker.Setup(service => service.TypeHasProperties<CustomerDto>(parameters.Fields))
                     .Returns(true);

            var httpConsept = new DefaultHttpContext();
            _customerController.ControllerContext = new ControllerContext
            {
                HttpContext = httpConsept
            };

            // Act
            var result = await _customerController.GetCustomerById(customerId, parameters.Fields);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCustomer = Assert.IsAssignableFrom<ExpandoObject>(okResult.Value);
            Assert.NotNull(returnedCustomer);
        }

        [Fact]
        public async Task GetCustomerById_ReturnsNotFound_WhenCustomerIdIsInvalid()
        {
            // Arrange
            var invalidCustomerId = Guid.Empty;
            var parameters = new CustomerResourceParameters { Fields = "Id,Name" };
            _mockPropertyChecker.Setup(service => service.TypeHasProperties<CustomerDto>(parameters.Fields))
                     .Returns(true);
            // Act
            var result = await _customerController.GetCustomerById(invalidCustomerId, parameters.Fields);
            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetCustomerById_ReturnsBadRequest_WhenInvalidFields()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var parameters = new CustomerResourceParameters { Fields = "InvalidField" };
            _mockPropertyChecker.Setup(service => service.TypeHasProperties<CustomerDto>(parameters.Fields))
                     .Returns(false);
            // Act
            var result = await _customerController.GetCustomerById(customerId, parameters.Fields);
            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetCustomerById_ReturnsNotFound_WhenCustomerDoesNotExist()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var parameters = new CustomerResourceParameters { Fields = "Id,Name" };
            _mockRepo.Setup(repo => repo.GetCustomerById(customerId, parameters.Fields))
                     .ReturnsAsync((Customer)null);
            _mockPropertyChecker.Setup(service => service.TypeHasProperties<CustomerDto>(parameters.Fields))
                     .Returns(true);

            // Act
            var result = await _customerController.GetCustomerById(customerId, parameters.Fields);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetCustomersCollection_ReturnsOkResult_WithListOfCustomers()
        {
            // Arrange
            var customerIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var customers = new List<Customer>
            {
                new Customer { Id = customerIds[0], Name = "Customer 1" },
                new Customer { Id = customerIds[1], Name = "Customer 2" }
            };
            var parameters = new CustomerResourceParameters { Fields = "Id,Name" };
            _mockRepo.Setup(repo => repo.GetCustomersByIds(customerIds, parameters.Fields))
                     .ReturnsAsync(customers);
            _mockPropertyChecker.Setup(service => service.TypeHasProperties<CustomerDto>(parameters.Fields))
                     .Returns(true);
            var httpConsept = new DefaultHttpContext();
            _customerController.ControllerContext = new ControllerContext
            {
                HttpContext = httpConsept
            };
            // Act
            var result = await _customerController.GetCustomersCollection(customerIds, parameters);
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedList = Assert.IsAssignableFrom<List<ExpandoObject>>(okResult.Value);
            Assert.Equal(2, returnedList.Count);
        }

        [Fact]
        public async Task GetCustomersCollection_ReturnsBadRequest_WhenInvalidFields()
        {
            // Arrange
            var customerIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var parameters = new CustomerResourceParameters { Fields = "InvalidField" };
            _mockPropertyChecker.Setup(service => service.TypeHasProperties<CustomerDto>(parameters.Fields))
                     .Returns(false);
            // Act
            var result = await _customerController.GetCustomersCollection(customerIds, parameters);
            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreateCustomer_ReturnCreatedAtRouteResult_WhenModelStateValid()
        {
            // Arrange
            var newCustomer = new CustomerCreateDto
            {
                Name = "New Customer"
            };

            // Act
            var result = await _customerController.CreateCustomer(newCustomer);

            var httpConsept = new DefaultHttpContext();
            _customerController.ControllerContext = new ControllerContext
            {
                HttpContext = httpConsept
            };

            // Assert
            var createdAtRoute = Assert.IsType<CreatedAtRouteResult>(result);
            Assert.Equal("GetCustomerById", createdAtRoute.RouteName);
            _mockRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateCustomer_ReturnBadRequest_WhenModelStateInvalid()
        {
            // Arrange
            var newCustomer = new CustomerCreateDto { };
            _customerController.ModelState.AddModelError("Name", "The Name field is required.");

            // Act
            var result = await _customerController.CreateCustomer(newCustomer);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            _mockRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task CustomerExists_ReturnsOkResult_WhenCustomerExists()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            _mockRepo.Setup(repo => repo.CustomerExists(customerId))
                     .ReturnsAsync(true);

            // Act
            var result = await _customerController.CustomerExists(customerId);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task CustomerExists_ReturnsNotFound_WhenCustomerNotExist()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            _mockRepo.Setup(repo => repo.CustomerExists(customerId))
                     .ReturnsAsync(false);

            // Act
            var result = await _customerController.CustomerExists(customerId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetCustomerByEmail_ReturnOkResult_WhenEmailValied()
        {
            // Arrange
            var email = "validEmail@example.com";
            var id = Guid.NewGuid();
            var fields = "name";
            var customer = new Customer
            {
                Id = id,
                Email = email,
                Name = "customer"
            };

            _mockRepo.Setup(repo => repo.GetCustomerByEmail(email, fields))
                .ReturnsAsync(customer);
            _mockPropertyChecker.Setup(service => service.TypeHasProperties<CustomerDto>(fields))
                .Returns(true);

            // Act
            var result = await _customerController.GetCustomerByEmail(email, fields);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetCustomerByEmail_ReturnBadRequest_WhenEmailInvalid()
        {
            // Arrange
            var email = "";
            var id = Guid.NewGuid();
            var fields = "name";
            var customer = new Customer
            {
                Id = id,
                Email = email,
                Name = "customer"
            };

            _mockRepo.Setup(repo => repo.GetCustomerByEmail(email, fields))
                .ReturnsAsync(customer);
            _mockPropertyChecker.Setup(service => service.TypeHasProperties<CustomerDto>(fields))
                .Returns(true);

            // Act
            var result = await _customerController.GetCustomerByEmail(email, fields);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetCustomerByEmail_ReturnBadRequest_WhenFieldsInvalid()
        {
            // Arrange
            var email = "valid@example.com";
            var id = Guid.NewGuid();
            var fields = "invalid";
            var customer = new Customer
            {
                Id = id,
                Email = email,
                Name = "customer"
            };

            _mockRepo.Setup(repo => repo.GetCustomerByEmail(email, fields))
                .ReturnsAsync(customer);
            _mockPropertyChecker.Setup(service => service.TypeHasProperties<CustomerDto>(fields))
                .Returns(false);

            // Act
            var result = await _customerController.GetCustomerByEmail(email, fields);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task EmailExists_ReturnsOkResult_WhenEmailExists()
        {
            // Arrange
            var email = "email@example.com";

            _mockRepo.Setup(repo => repo.EmailExists(email))
                .ReturnsAsync(true);

            // Act
            var result = await _customerController.EmailExists(email);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task EmailExists_ReturnsNotFound_WhenEmailNotExist()
        {
            // Arrange
            var email = "email@example.com";

            _mockRepo.Setup(repo => repo.EmailExists(email))
                .ReturnsAsync(false);

            // Act
            var result = await _customerController.EmailExists(email);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetCustomerCount_ReturnNumberOfCustomers_WhenCustomersExist()
        {
            // Arrange
            var parameters = new CustomerResourceParameters { Fields = "name" };

            _mockRepo.Setup(repo => repo.GetCustomersCount(parameters))
                .ReturnsAsync(5);
            _mockPropertyChecker.Setup(service => service.TypeHasProperties<CustomerDto>(parameters.Fields))
                .Returns(true);

            // Act
            var result = await _customerController.GetCustomersCount(parameters);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be("count: 5");
        }

        [Fact]
        public async Task GetCustomerCount_ReturnBadRequest_WhenFieldsAreInvalid()
        {
            // Arrange
            var parameters = new CustomerResourceParameters { Fields = "name" };

            _mockRepo.Setup(repo => repo.GetCustomersCount(parameters))
                .ReturnsAsync(5);
            _mockPropertyChecker.Setup(service => service.TypeHasProperties<CustomerDto>(parameters.Fields))
                .Returns(false);

            // Act
            var result = await _customerController.GetCustomersCount(parameters);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetCustomerCount_ReturnZero_WhenNoCustomersExist()
        {
            // Arrange
            var parameters = new CustomerResourceParameters { Fields = "name" };
            _mockRepo.Setup(repo => repo.GetCustomersCount(parameters))
                .ReturnsAsync(0);
            _mockPropertyChecker.Setup(service => service.TypeHasProperties<CustomerDto>(parameters.Fields))
                .Returns(true);
            // Act
            var result = await _customerController.GetCustomersCount(parameters);
            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be("count: 0");
        }

        [Fact]
        public async Task CreateCustomerCollection_ReturnsCreatedAtRouteResult_WhenModelStateValid()
        {
            // Arrange
            var customers = new List<CustomerCreateDto>
        {
            new CustomerCreateDto { Name = "Test1" },
            new CustomerCreateDto { Name = "Test2" }
        };

            var bulkResult = new BulkOperationResult
            {
                SuccessCount = 2,
                FailureCount = 0,
                SuccessIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
            };

            _mockRepo.Setup(r => r.CreateCustomers(It.IsAny<IEnumerable<Customer>>()))
                     .ReturnsAsync(bulkResult);

            // Act
            var result = await _customerController.CreateCustomerCollection(customers);

            // Assert
            var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
            created.RouteName.Should().Be("GetCustomerCollection");
            var response = created.Value.Should().BeOfType<BulkOperationResponse>().Subject;
            var createdAtRoute = Assert.IsType<CreatedAtRouteResult>(result);
            Assert.Equal("GetCustomerCollection", createdAtRoute.RouteName);
            _mockRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateCustomerCollection_ReturnsBadRequest_WhenInputIsNull()
        {
            // Arrange
            List<CustomerCreateDto>? newCustomers = null;

            // Act
            var result = await _customerController.CreateCustomerCollection(newCustomers);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            _mockRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateCustomerCollection_ReturnsBadRequest_WhenNoCustomersProvided()
        {
            // Arrange
            var newCustomers = new List<CustomerCreateDto>();

            // Act
            var result = await _customerController.CreateCustomerCollection(newCustomers);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            _mockRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateCustomer_ReturnsNoContent_WhenModelStateValid()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var updateDto = new CustomerUpdateDto
            {
                Name = "Updated Customer"
            };
            _mockRepo.Setup(repo => repo.GetCustomerById(customerId, It.IsAny<string>()))
                     .ReturnsAsync(new Customer { Id = customerId });

            // Act
            var result = await _customerController.UpdateCustomer(customerId, updateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateCustomer_ReturnsBadRequest_WhenModelStateInvalid()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var updateDto = new CustomerUpdateDto { };
            _customerController.ModelState.AddModelError("Name", "The Name field is required.");

            // Act
            var result = await _customerController.UpdateCustomer(customerId, updateDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            _mockRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateCustomer_ReturnsNotFound_WhenCustomerDoesNotExist()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var updateDto = new CustomerUpdateDto
            {
                Name = "Updated Customer"
            };
            _mockRepo.Setup(repo => repo.GetCustomerById(customerId, It.IsAny<string>()))
                     .ReturnsAsync((Customer)null);
            // Act
            var result = await _customerController.UpdateCustomer(customerId, updateDto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            _mockRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateCustomer_ReturnsNotFound_WhenCustomerIdIsInvalid()
        {
            // Arrange
            var invalidCustomerId = Guid.Empty;
            var updateDto = new CustomerUpdateDto
            {
                Name = "Updated Customer"
            };
            // Act
            var result = await _customerController.UpdateCustomer(invalidCustomerId, updateDto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task CustomerPartialUpdate_ReturnsBadRequest_WhenPatchDocumentIsNull()
        {
            // Arrange
            JsonPatchDocument<CustomerUpdateDto> patchDocument = null;

            // Act
            var result = await _customerController.CustomerPartialUpdate(Guid.NewGuid(), patchDocument);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result as BadRequestObjectResult;
            badRequest!.Value.Should().BeOfType<ErrorResponse>();
            ((ErrorResponse)badRequest.Value).Error.Should().Be("Invalid patch document.");
        }

        [Fact]
        public async Task CustomerPartialUpdate_ReturnsNotFound_WhenCustomerDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();

            _mockRepo.Setup(r => r.GetCustomerById(id, null))
                .ReturnsAsync((Customer)null);

            var patchDoc = new JsonPatchDocument<CustomerUpdateDto>();

            // Act
            var result = await _customerController.CustomerPartialUpdate(id, patchDoc);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task CustomerPartialUpdate_ReturnsValidationProblem_WhenModelValidationFails()
        {
            // Arrange
            var id = Guid.NewGuid();

            var customerEntity = new Customer { Id = id, Name = "OldName" };
            _mockRepo.Setup(r => r.GetCustomerById(id, null))
                .ReturnsAsync(customerEntity);

            var patchDoc = new JsonPatchDocument<CustomerUpdateDto>();
            patchDoc.Replace(c => c.Name, "");

            var objectValidator = new Mock<IObjectModelValidator>();
            objectValidator.Setup(o => o.Validate(
                It.IsAny<ActionContext>(),
                It.IsAny<ValidationStateDictionary>(),
                It.IsAny<string>(),
                It.IsAny<object>()))
                .Callback<ActionContext, ValidationStateDictionary, string, object>(
                    (context, state, prefix, model) =>
                    {
                        context.ModelState.AddModelError("Name", "Name cannot be empty");
                    });

            _customerController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _customerController.ObjectValidator = objectValidator.Object;

            // Act
            var result = await _customerController.CustomerPartialUpdate(id, patchDoc);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            _mockRepo.Verify(r => r.UpdateCustomer(It.IsAny<Customer>()), Times.Never);
        }

        [Fact]
        public async Task CustomerPartialUpdate_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var id = Guid.NewGuid();

            var customerEntity = new Customer
            {
                Id = id,
                Name = "Old Name",
                Email = "old@example.com",
                City = "Alexandria",
                Country = "Egypt",
                RowVersion = []
            };

            _mockRepo.Setup(r => r.GetCustomerById(id, null))
                .ReturnsAsync(customerEntity);

            var patchDoc = new JsonPatchDocument<CustomerUpdateDto>();
            patchDoc.Replace(c => c.Name, "New Name");

            var objectValidator = new Mock<IObjectModelValidator>();
            objectValidator.Setup(o => o.Validate(
                It.IsAny<ActionContext>(),
                It.IsAny<ValidationStateDictionary>(),
                It.IsAny<string>(),
                It.IsAny<object>()));

            _customerController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _customerController.ObjectValidator = objectValidator.Object;

            // Act
            var result = await _customerController.CustomerPartialUpdate(id, patchDoc);

            // Assert
            result.Should().BeOfType<NoContentResult>();

            _mockRepo.Verify(r => r.UpdateCustomer(customerEntity), Times.Once);
            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);

            customerEntity.Name.Should().Be("New Name");
        }

        [Fact]
        public async Task DeleteCustomer_ReturnsNotFound_WhenCustomerDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockRepo.Setup(r => r.CustomerExists(id)).ReturnsAsync(false);

            var result = await _customerController.DeleteCustomer(id);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task DeleteCustomer_ReturnsNoContent_WhenCustomerDeleted()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockRepo.Setup(r => r.CustomerExists(id)).ReturnsAsync(true);

            // Act
            var result = await _customerController.DeleteCustomer(id);

            result.Should().BeOfType<NoContentResult>();
            _mockRepo.Verify(r => r.DeleteCustomer(id), Times.Once);
            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RestoreCustomer_ReturnsNotFound_WhenRestoreFails()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockRepo.Setup(r => r.RestoreCustomer(id)).ReturnsAsync(false);

            var result = await _customerController.RestoreCustomer(id);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task RestoreCustomer_ReturnsNoContent_WhenRestored()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockRepo.Setup(r => r.RestoreCustomer(id)).ReturnsAsync(true);

            // Act
            var result = await _customerController.RestoreCustomer(id);

            result.Should().BeOfType<NoContentResult>();
            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CustomerDeleteCollection_ReturnsBadRequest_WhenIdsNullOrEmpty()
        {
            // Act
            var result = await _customerController.CustomerDeleteCollection(null);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CustomerDeleteCollection_ReturnsBadRequest_WhenMoreThan1000()
        {
            // Arrange
            var ids = Enumerable.Range(0, 1001).Select(_ => Guid.NewGuid());

            // Act
            var result = await _customerController.CustomerDeleteCollection(ids);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CustomerDeleteCollection_ReturnsNoContent_WhenValid()
        {
            // Arrange
            var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var operationResult = new BulkOperationResult { };

            _mockRepo.Setup(r => r.DeleteCustomers(ids)).ReturnsAsync(operationResult);

            // Act
            var result = await _customerController.CustomerDeleteCollection(ids);

            result.Should().BeOfType<NoContentResult>();
            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CustomerRestoreCollection_ReturnsBadRequest_WhenIdsNullOrEmpty()
        {
            // Act
            var result = await _customerController.CustomerRestoreCollection(null);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CustomerRestoreCollection_ReturnsNoContent_WhenRestored()
        {
            // Arrange
            var ids = new List<Guid> { Guid.NewGuid() };
            var operationResult = new BulkOperationResult { };
            _mockRepo.Setup(r => r.RestoreCustomers(ids)).ReturnsAsync(operationResult);

            // Act
            var result = await _customerController.CustomerRestoreCollection(ids);

            result.Should().BeOfType<NoContentResult>();
            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetCustomerPhoneNumber_ReturnsNotFound_WhenCustomerDoesNotExist()
        {
            // Arrange
            var cid = Guid.NewGuid();
            var pid = Guid.NewGuid();

            _mockRepo.Setup(r => r.CustomerExists(cid)).ReturnsAsync(false);

            // Act
            var result = await _customerController.GetCustomerPhoneNumber(cid, pid);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetCustomerPhoneNumber_ReturnsNotFound_WhenPhoneDoesNotExist()
        {
            // Arrange
            var cid = Guid.NewGuid();
            var pid = Guid.NewGuid();

            _mockRepo.Setup(r => r.CustomerExists(cid)).ReturnsAsync(true);
            _mockRepo.Setup(r => r.GetPhoneNumber(pid)).ReturnsAsync((CustomerPhoneNumber)null);

            // Act
            var result = await _customerController.GetCustomerPhoneNumber(cid, pid);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetCustomerPhoneNumber_ReturnsOk_WhenExists()
        {
            // Arrange
            var cid = Guid.NewGuid();
            var pid = Guid.NewGuid();

            var phone = new CustomerPhoneNumber { Id = pid, Number = "010" };

            _mockRepo.Setup(r => r.CustomerExists(cid)).ReturnsAsync(true);
            _mockRepo.Setup(r => r.GetPhoneNumber(pid)).ReturnsAsync(phone);

            // Act
            var result = await _customerController.GetCustomerPhoneNumber(cid, pid);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateCustomerPhoneNumber_ReturnsBadRequest_WhenModelInvalid()
        {
            // Arrange
            _customerController.ModelState.AddModelError("Number", "Required");

            // Act
            var result = await _customerController.CreateCustomerPhoneNumber(Guid.NewGuid(),
                new CustomerPhoneNumberCreateDto());

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateCustomerPhoneNumber_ReturnsNotFound_WhenCustomerDoesNotExist()
        {
            // Arrange
            var cid = Guid.NewGuid();
            var dto = new CustomerPhoneNumberCreateDto { Number = "010" };

            _mockRepo.Setup(r => r.CustomerExists(cid)).ReturnsAsync(false);

            // Act
            var result = await _customerController.CreateCustomerPhoneNumber(cid, dto);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task CreateCustomerPhoneNumber_ReturnsCreated_WhenSuccess()
        {
            // Arrange
            var cid = Guid.NewGuid();
            var dto = new CustomerPhoneNumberCreateDto { Number = "010" };

            _mockRepo.Setup(r => r.CustomerExists(cid)).ReturnsAsync(true);

            // Act
            var result = await _customerController.CreateCustomerPhoneNumber(cid, dto);

            result.Should().BeOfType<CreatedAtRouteResult>();
            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
        [Fact]
        public async Task UpdatePhoneNumbers_ReturnsBadRequest_WhenModelInvalid()
        {
            // Arrange
            _customerController.ModelState.AddModelError("Number", "Required");

            // Act
            var result = await _customerController.UpdatePhoneNumbers(Guid.NewGuid(), Guid.NewGuid(),
                new CustomerPhoneNumberUpdateDto());

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdatePhoneNumbers_ReturnsNotFound_WhenCustomerDoesNotExist()
        {
            // Arrange
            var cid = Guid.NewGuid();
            var pid = Guid.NewGuid();

            _mockRepo.Setup(r => r.CustomerExists(cid)).ReturnsAsync(false);

            // Act
            var result = await _customerController.UpdatePhoneNumbers(cid, pid, new CustomerPhoneNumberUpdateDto());

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task UpdatePhoneNumbers_ReturnsNotFound_WhenPhoneNotFound()
        {
            // Arrange
            var cid = Guid.NewGuid();
            var pid = Guid.NewGuid();

            _mockRepo.Setup(r => r.CustomerExists(cid)).ReturnsAsync(true);
            _mockRepo.Setup(r => r.GetPhoneNumber(pid)).ReturnsAsync((CustomerPhoneNumber)null);

            // Act
            var result = await _customerController.UpdatePhoneNumbers(cid, pid, new CustomerPhoneNumberUpdateDto());

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task UpdatePhoneNumbers_ReturnsConflict_OnConcurrencyError()
        {
            // Arrange
            var cid = Guid.NewGuid();
            var pid = Guid.NewGuid();
            var phone = new CustomerPhoneNumber();

            _mockRepo.Setup(r => r.CustomerExists(cid)).ReturnsAsync(true);
            _mockRepo.Setup(r => r.GetPhoneNumber(pid)).ReturnsAsync(phone);

            _mockRepo.Setup(r => r.UpdatePhoneNumber(cid, pid, phone))
                .ThrowsAsync(new DbUpdateConcurrencyException("conflict"));

            // Act
            var result = await _customerController.UpdatePhoneNumbers(cid, pid,
                new CustomerPhoneNumberUpdateDto());

            result.Should().BeOfType<ConflictObjectResult>();
        }

        [Fact]
        public async Task UpdatePhoneNumbers_ReturnsNoContent_WhenSuccess()
        {
            // Arrange
            var cid = Guid.NewGuid();
            var pid = Guid.NewGuid();
            var phone = new CustomerPhoneNumber();

            _mockRepo.Setup(r => r.CustomerExists(cid)).ReturnsAsync(true);
            _mockRepo.Setup(r => r.GetPhoneNumber(pid)).ReturnsAsync(phone);

            // Act
            var result = await _customerController.UpdatePhoneNumbers(cid, pid,
                new CustomerPhoneNumberUpdateDto());

            result.Should().BeOfType<NoContentResult>();
            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
        [Fact]
        public async Task PhoneNumberExists_ReturnsNotFound_WhenDoesNotExist()
        {
            // Arrange
            var pid = Guid.NewGuid();

            _mockRepo.Setup(r => r.PhoneNumberExists(pid)).ReturnsAsync(false);

            // Act
            var result = await _customerController.PhoneNumberExists(pid);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task PhoneNumberExists_ReturnsOk_WhenExists()
        {
            // Arrange
            var pid = Guid.NewGuid();

            _mockRepo.Setup(r => r.PhoneNumberExists(pid)).ReturnsAsync(true);

            // Act
            var result = await _customerController.PhoneNumberExists(pid);

            result.Should().BeOfType<OkResult>();
        }
        [Fact]
        public async Task DeletePhoneNumber_ReturnsNotFound_WhenCustomerNotFound()
        {
            // Arrange
            var cid = Guid.NewGuid();
            var pid = Guid.NewGuid();

            _mockRepo.Setup(r => r.CustomerExists(cid)).ReturnsAsync(false);

            // Act
            var result = await _customerController.DeletePhoneNumber(cid, pid);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task DeletePhoneNumber_ReturnsNotFound_WhenPhoneNotFound()
        {
            // Arrange
            var cid = Guid.NewGuid();
            var pid = Guid.NewGuid();

            _mockRepo.Setup(r => r.CustomerExists(cid)).ReturnsAsync(true);
            _mockRepo.Setup(r => r.PhoneNumberExists(pid)).ReturnsAsync(false);

            // Act
            var result = await _customerController.DeletePhoneNumber(cid, pid);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task DeletePhoneNumber_ReturnsNoContent_WhenSuccess()
        {
            var cid = Guid.NewGuid();
            var pid = Guid.NewGuid();

            _mockRepo.Setup(r => r.CustomerExists(cid)).ReturnsAsync(true);
            _mockRepo.Setup(r => r.PhoneNumberExists(pid)).ReturnsAsync(true);

            // Act
            var result = await _customerController.DeletePhoneNumber(cid, pid);

            result.Should().BeOfType<NoContentResult>();
            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
        [Fact]
        public async Task RestorePhoneNumber_ReturnsNotFound_WhenCustomerDoesNotExist()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var phoneId = Guid.NewGuid();

            _mockRepo.Setup(r => r.CustomerExists(customerId))
                .ReturnsAsync(false);

            // Act
            var result = await _customerController.RestorePhoneNumber(customerId, phoneId);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            var error = Assert.IsType<ErrorResponse>(notFound.Value);
            Assert.Contains("does not exist", error.Error);
        }

        [Fact]
        public async Task RestorePhoneNumber_ReturnsNotFound_WhenPhoneNotRestored()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var phoneId = Guid.NewGuid();

            _mockRepo.Setup(r => r.CustomerExists(customerId))
                .ReturnsAsync(true);

            _mockRepo.Setup(r => r.RestorePhoneNumber(phoneId))
                .ReturnsAsync(false);

            // Act
            var result = await _customerController.RestorePhoneNumber(customerId, phoneId);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            var error = Assert.IsType<ErrorResponse>(notFound.Value);
            Assert.Contains("not found or not deleted", error.Error);
        }

        [Fact]
        public async Task RestorePhoneNumber_ReturnsNoContent_WhenRestored()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var phoneId = Guid.NewGuid();

            _mockRepo.Setup(r => r.CustomerExists(customerId))
                .ReturnsAsync(true);

            _mockRepo.Setup(r => r.RestorePhoneNumber(phoneId))
                .ReturnsAsync(true);

            // Act
            var result = await _customerController.RestorePhoneNumber(customerId, phoneId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
        [Fact]
        public void GetCustomersOptions_ReturnsCorrectAllowHeader()
        {
            // Arrange
            _customerController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            // Act
            var result = _customerController.GetCustomersOptions();

            // Assert
            var ok = Assert.IsType<OkResult>(result);
            Assert.True(_customerController.Response.Headers.ContainsKey("Allow"));
        }

        [Fact]
        public void GetCustomerOptions_ReturnsCorrectAllowHeader()
        {
            // Arrange
            _customerController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _customerController.GetCustomerOptions();

            // Assert
            Assert.IsType<OkResult>(result);
            Assert.True(_customerController.Response.Headers.ContainsKey("Allow"));
        }

        [Fact]
        public void GetCustomerPhoneNumberOptions_ReturnsCorrectAllowHeader()
        {
            // Arrange
            _customerController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _customerController.GetCustomerPhoneNumberOptions();

            // Assert
            Assert.IsType<OkResult>(result);
            Assert.True(_customerController.Response.Headers.ContainsKey("Allow"));
        }

        [Fact]
        public void GetCustomersCollectionOptions_ReturnsCorrectAllowHeader()
        {
            // Arrange
            _customerController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _customerController.GetCustomersCollectionOptions();

            // Assert
            Assert.IsType<OkResult>(result);
            Assert.True(_customerController.Response.Headers.ContainsKey("Allow"));
        }

        [Fact]
        public void RestoreCustomersOptions_ReturnsCorrectAllowHeader()
        {
            // Arrange
            _customerController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _customerController.RestoreCustomersOptions();

            // Assert
            Assert.IsType<OkResult>(result);
            Assert.True(_customerController.Response.Headers.ContainsKey("Allow"));
        }

    }
}
