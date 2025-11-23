using EquipmentAPI.Controllers;
using EquipmentAPI.Entities;
using EquipmentAPI.Helper;
using EquipmentAPI.Models.PhoneNumberModels.Write;
using EquipmentAPI.Models.SupplierModels.Read;
using EquipmentAPI.Models.SupplierModels.Write;
using EquipmentAPI.Repositories.SupplierRepository;
using EquipmentAPI.ResourceParameters;
using EquipmentAPI.Services;
using EquipmentAPI.Tests.Helper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Dynamic;

namespace EquipmentAPI.Tests.SupplierTests
{
    public class SupplierControllerTests
    {
        private readonly Mock<ISupplierRepository> _mockRepo;
        private readonly Mock<IPropertyCheckerService> _mockPropertyChecker;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly SupplierController _controller;

        public SupplierControllerTests()
        {
            _mockRepo = new Mock<ISupplierRepository>();
            _mockPropertyChecker = new Mock<IPropertyCheckerService>();
            _mockCache = new Mock<IMemoryCache>();
            _controller = new SupplierController(_mockRepo.Object, _mockPropertyChecker.Object, _mockCache.Object);
        }

        [Fact]
        public async Task GetSupplier_ShouldReturnOkResult_WhenDataExists()
        {
            //Arrange
            var parameters = new SupplierResourceParameters { Fields = "Name, Id" };
            var suppliersList = new PagedList<Supplier>(new List<Supplier>(), 1, 1, 10);

            _mockRepo.Setup(repo => repo.GetSuppliers(parameters)).ReturnsAsync(suppliersList);
            _mockPropertyChecker.Setup(service => service.TypeHasProperties<SupplierDto>(parameters.Fields)).Returns(true);

            // Mock the cache

            object cachedValue = null;
            _mockCache
                .Setup(cache => cache.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);

            var mockCacheEntry = new Mock<ICacheEntry>();
            _mockCache
                .Setup(cache => cache.CreateEntry(It.IsAny<object>()))
                .Returns(mockCacheEntry.Object);

            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await _controller.GetSuppliers(parameters);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var responseHeaders = httpContext.Response.Headers;
            responseHeaders.Should().ContainKey("X-Pagination");
        }

        [Fact]
        public async Task GetSuppliers_ShouldReturnsBadRequest_WhenFieldsInvalid()
        {
            //Arrange

            var parameters = new SupplierResourceParameters { Fields = "InvalidField" };
            _mockPropertyChecker.Setup(service => service.TypeHasProperties<SupplierDto>(parameters.Fields)).Returns(false);

            //Act
            var result = await _controller.GetSuppliers(parameters);

            //Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetSuppliers_ShouldReturnsFromCache_WhenPresent()
        {
            //Arrange

            var parameters = new SupplierResourceParameters { Fields = "name, id" };
            var suppliers = new PagedList<Supplier>(new List<Supplier>(), 0, 1, 10);

            _mockPropertyChecker
                .Setup(service => service.TypeHasProperties<SupplierDto>(parameters.Fields))
                .Returns(true);

            var httpConsept = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpConsept
            };

            object cacheValue = suppliers;
            _mockCache.Setup(cache => cache.TryGetValue(It.IsAny<object>(), out cacheValue))
                .Returns(true);

            // Act
            var result = await _controller.GetSuppliers(parameters);

            // Assert
            result.Should().BeOfType<OkObjectResult>();

            // verify that the repository was not called
            _mockRepo.Verify(repo => repo.GetSuppliers(parameters), Times.Never);
        }

        [Fact]
        public async Task GetSuppliers_ShouldReturnEmptyList_WhenNoSuppliersExist()
        {

            var parameters = new SupplierResourceParameters { Fields = "name" };
            var suppliersList = new PagedList<Supplier>(new List<Supplier>(), 1, 1, 10);

            _mockRepo.Setup(repo => repo.GetSuppliers(parameters)).ReturnsAsync(suppliersList);
            _mockPropertyChecker.Setup(service => service.TypeHasProperties<SupplierDto>(parameters.Fields)).Returns(true);

            object cachedValue = null;
            _mockCache
                .Setup(cache => cache.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);

            var mockCacheEntry = new Mock<ICacheEntry>();
            _mockCache
                .Setup(cache => cache.CreateEntry(It.IsAny<object>()))
                .Returns(mockCacheEntry.Object);

            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await _controller.GetSuppliers(parameters);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedList = Assert.IsAssignableFrom<List<ExpandoObject>>(okResult.Value);
            Assert.Empty(returnedList);
        }

        // GET SUPPLIER BY ID
        [Fact]
        public async Task GetASupplierById_ReturnsBadRequest_WhenFieldsInvalid()
        {
            // Arrange
            var fields = "invaliedField";
            var id = Guid.NewGuid();

            _mockPropertyChecker.Setup(service => service.TypeHasProperties<SupplierDto>(fields)).Returns(false);

            // Act
            var result = await _controller.GetASupplierById(id, fields);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetASupplierById_FetchesFromRepository_WhenCacheMiss()
        {
            // Arrange
            var fields = "name, id";
            var id = Guid.NewGuid();
            var supplier = new Supplier();

            _mockPropertyChecker.Setup(service => service.TypeHasProperties<SupplierDto>(fields)).Returns(true);

            object cachedValue = null;
            _mockCache.Setup(cache => cache.TryGetValue(It.IsAny<object>(), out cachedValue)).Returns(false);

            _mockRepo.Setup(repo => repo.GetSupplier(id, fields)).ReturnsAsync(supplier);

            var mockCacheEntry = new Mock<ICacheEntry>();
            _mockCache
                .Setup(cache => cache.CreateEntry(It.IsAny<object>()))
                .Returns(mockCacheEntry.Object);


            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await _controller.GetASupplierById(id, fields);

            // Assert
            result.Should().BeOfType<OkObjectResult>();

            _mockRepo.Verify(repo => repo.GetSupplier(id, fields), Times.Once);
        }

        [Fact]
        public async Task GetASupplierById_ReturnsFromCache_WhenPresent()
        {
            // Arrange
            var fields = "name, id";
            var id = Guid.NewGuid();
            var supplier = new Supplier();

            _mockPropertyChecker.Setup(service => service.TypeHasProperties<SupplierDto>(fields)).Returns(true);

            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            object cachedValue = supplier;
            _mockCache.Setup(cache => cache.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(true);

            // Act
            var result = await _controller.GetASupplierById(id, fields);

            // Assert
            result.Should().BeOfType<OkObjectResult>();

            _mockRepo.Verify(repo => repo.GetSupplier(id, fields), Times.Never);
        }

        [Fact]
        public async Task GetASupplierById_ReturnsNotFound_WhenSupplierDoesNotExist()
        {
            // Arrange
            var fields = "name, id";
            var id = Guid.NewGuid();

            _mockPropertyChecker.Setup(service => service.TypeHasProperties<SupplierDto>(fields)).Returns(true);
            _mockRepo.Setup(repo => repo.GetSupplier(id, fields)).ReturnsAsync((Supplier)null);

            object cachedValue = null;
            _mockCache.Setup(cache => cache.TryGetValue(It.IsAny<object>(), out cachedValue)).Returns(false);

            var cacheEntry = new Mock<ICacheEntry>();
            _mockCache.Setup(cache => cache.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntry.Object);

            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await _controller.GetASupplierById(id, fields);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }


        [Fact]
        public async Task GetASupplierById_ReturnsOk_WithSupplierData()
        {
            // Arrange
            var fields = "name, id";
            var id = Guid.NewGuid();
            var supplier = new Supplier();

            _mockPropertyChecker.Setup(service => service.TypeHasProperties<SupplierDto>(fields)).Returns(true);
            _mockRepo.Setup(repo => repo.GetSupplier(id, fields)).ReturnsAsync(supplier);

            object cachedValue = null;
            _mockCache.Setup(cache => cache.TryGetValue(It.IsAny<object>, out cachedValue))
                .Returns(false);

            var cacheEntry = new Mock<ICacheEntry>();
            _mockCache.Setup(cache => cache.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntry.Object);

            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await _controller.GetASupplierById(id, fields);

            // Assert

            var okResult = Assert.IsType<OkObjectResult>(result);
            var shapedData = Assert.IsType<ExpandoObject>(okResult.Value);
            _mockRepo.Verify(r => r.GetSupplier(id, fields), Times.Once);
        }

        // CREATE SUPPLIER
        [Fact]
        public async Task CreateSupplier_ReturnsBadRequest_WhenModelStateInvalid()
        {
            // Arrange
            var supplierToCreate = new SupplierCreateDto
            {
                Name = "New Supplier"
            };

            ModelValidationHelper.ValidateModel(_controller, supplierToCreate);

            // Act
            var result = await _controller.CreateSupplier(supplierToCreate);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }

        [Fact]
        public async Task CreateSupplier_ReturnsCreatedAtRoute_WhenValid()
        {
            // Arrange
            var supplierToCreate = new SupplierCreateDto
            {
                Name = "New Supplier",
                City = "Alex",
                ContactPerson = "Khaled",
                Country = "Egy",
                Email = "A@N.com",
                PhoneNumbers = new List<SupplierPhoneNumberCreateDto>()
            };

            ModelValidationHelper.ValidateModel(_controller, supplierToCreate);

            // Act
            var result = await _controller.CreateSupplier(supplierToCreate);

            // Assert
            var createdAtRoute = Assert.IsType<CreatedAtRouteResult>(result);
            Assert.Equal("GetASupplier", createdAtRoute.RouteName);
        }

        // UPDATE SUPPLIER (PUT)
        [Fact]
        public async Task UpdateSupplier_ReturnsBadRequest_WhenModelStateInvalid()
        {
            // Arrange
            var supplierToUpdate = new SupplierUpdateDto
            {
                Name = "updated supplier",
                Email = "A@example.com"
            };

            ModelValidationHelper.ValidateModel(_controller, supplierToUpdate);

            // Act
            var result = await _controller.UpdateSupplier(Guid.NewGuid(), supplierToUpdate);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateSupplier_ReturnsNotFound_WhenSupplierDoesNotExist()
        {
            // Arrange
            var supplierToUpdate = new SupplierUpdateDto
            {
                Name = "Updated Supplier",
                RowVersion = new byte[] { 0, 0, 2 },
                Email = "e@example.com"
            };
            var id = Guid.NewGuid();
            ModelValidationHelper.ValidateModel(_controller, supplierToUpdate);
            _mockRepo.Setup(repo => repo.GetSupplier(id, null)).ReturnsAsync((Supplier)null);

            // Act
            var result = await _controller.UpdateSupplier(id, supplierToUpdate);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
        [Fact]
        public async Task UpdateSupplier_ReturnsConflict_OnConcurrencyException()
        {
            var id = Guid.NewGuid();
            var supplierToUpdate = new SupplierUpdateDto
            {
                Name = "Updated Supplier",
                RowVersion = new byte[] { 0, 0, 2 },
                Email = "e@example.com"
            };
            var supplier = new Supplier
            {
                Id = id,
                RowVersion = new byte[] { 1, 0, 2 }
            };

            ModelValidationHelper.ValidateModel(_controller, supplierToUpdate);

            _mockRepo
                .Setup(repo => repo.GetSupplier(id, null))
                .ReturnsAsync(supplier);

            _mockRepo.Setup(repo => repo.Update(supplier))
                .Throws(new DbUpdateConcurrencyException("Concurrency conflict"));

            // Act
            var result = await _controller.UpdateSupplier(id, supplierToUpdate);

            // Assert
            result.Should().BeOfType<ConflictObjectResult>();
        }

        [Fact]
        public async Task UpdateSupplier_ReturnsNoContent_WhenSuccessful()
        {
            var id = Guid.NewGuid();
            var supplierToUpdate = new SupplierUpdateDto
            {
                Name = "updated Supplier",
                RowVersion = new byte[] { 1, 1, 1 },
                Email = "e@example.com"

            };

            ModelValidationHelper.ValidateModel(_controller, supplierToUpdate);

            var supplier = new Supplier
            {
                Id = id,
                RowVersion = new byte[] { 1, 1, 1 }
            };

            _mockRepo
                .Setup(repo => repo.GetSupplier(id, null))
                .ReturnsAsync(supplier);

            _mockRepo
                .Setup(repo => repo.Update(supplier))
                .Returns(Task.CompletedTask);

            _mockRepo
                .Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateSupplier(id, supplierToUpdate);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _mockRepo.Verify(repo => repo.Update(supplier), Times.Once);
            _mockRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        // PATCH SUPPLIER
        [Fact]
        public async Task SupplierPartialUpdate_ReturnsNotFound_WhenSupplierDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();

            var jsonPatch = new JsonPatchDocument<SupplierUpdateDto>();
            jsonPatch.Replace(d => d.Email, "A@example.com");

            _mockRepo
                .Setup(repo => repo.GetSupplier(id, null))
                .ReturnsAsync((Supplier)null);


            // Act
            var result = await _controller.SupplierPartialUpdate(id, jsonPatch);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task SupplierPartialUpdate_ReturnsValidationProblem_WhenModelInvalid()
        {
            // Arrange
            var id = Guid.NewGuid();

            var supplier = new Supplier
            {
                Id = id,
                RowVersion = new byte[] { 1, 1, 1 },
                Email = "a@example.com"
            };

            _mockRepo
                .Setup(repo => repo.GetSupplier(id, null))
                .ReturnsAsync(supplier);

            var jsonPatch = new JsonPatchDocument<SupplierUpdateDto>();
            jsonPatch.Replace(s => s.Email, "invalid-email");

            // 1. Create a service collection and add necessary framework services
            var serviceCollection = new ServiceCollection();
            // Add MVC services, which includes the Model Validator Provider
            serviceCollection.AddMvc();

            // 2. Build the IServiceProvider
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // 3. Configure the ControllerContext with the ServiceProvider
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = serviceProvider }
            };

            // Act
            var result = await _controller.SupplierPartialUpdate(id, jsonPatch);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task SupplierPartialUpdate_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var id = Guid.NewGuid();
            var supplier = new Supplier
            {
                Id = id,
                RowVersion = new byte[] { 1, 1, 1 },
                Email = "a@example.com"
            };

            _mockRepo
                .Setup(repo => repo.GetSupplier(id, null))
                .ReturnsAsync(supplier);

            var jsonPatch = new JsonPatchDocument<SupplierUpdateDto>();
            jsonPatch.Replace(d => d.City, "Alexandria");

            // 1. Create a service collection and add necessary framework services
            var serviceCollection = new ServiceCollection();
            // Add MVC services, which includes the Model Validator Provider
            serviceCollection.AddMvc();

            // 2. Build the IServiceProvider
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // 3. Configure the ControllerContext with the ServiceProvider
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = serviceProvider }
            };

            // Act
            var result = await _controller.SupplierPartialUpdate(id, jsonPatch);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        // PATCH PHONE NUMBERS
        [Fact]
        public async Task UpdatePhoneNumbers_ReturnsBadRequest_WhenListEmpty()
        {
            // Arrange
            var id = Guid.NewGuid();
            var phoneNumbersList = new List<SupplierPhoneNumberUpdateDto>();

            // Act
            var result = await _controller.UpdatePhoneNumbers(id, phoneNumbersList);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdatePhoneNumbers_ReturnsNotFound_WhenSupplierDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            var phoneNumbersList = new List<SupplierPhoneNumberUpdateDto>
            { new SupplierPhoneNumberUpdateDto { Number = "+44 44 444 444" } };
            _mockRepo.Setup(repo => repo.Exists(id)).ReturnsAsync(false);

            // Act
            var result = await _controller.UpdatePhoneNumbers(id, phoneNumbersList);

            // Assert
            result?.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task UpdatePhoneNumbers_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var id = Guid.NewGuid();
            var phoneNumbersList = new List<SupplierPhoneNumberUpdateDto>
            { new SupplierPhoneNumberUpdateDto { Number = "+44 44 444 444" } };
            _mockRepo.Setup(repo => repo.Exists(id)).ReturnsAsync(true);
            _mockRepo.Setup(repo => repo.UpdatePhoneNumber(id, phoneNumbersList));
            _mockRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

            // Act
            var result = await _controller.UpdatePhoneNumbers(id, phoneNumbersList);

            // Assert
            result?.Should().BeOfType<NoContentResult>();
            _mockRepo.Verify(repo => repo.UpdatePhoneNumber(id, phoneNumbersList), Times.Once);
            _mockRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        // DELETE SUPPLIER
        [Fact]
        public async Task DeleteSupplier_ReturnsNotFound_WhenSupplierDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockRepo.Setup(repo => repo.Exists(id)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteSupplier(id);

            // Arrange
            result?.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task DeleteSupplier_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockRepo.Setup(repo => repo.Exists(id)).ReturnsAsync(true);
            _mockRepo.Setup(repo => repo.Delete(id));
            _mockRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteSupplier(id);

            // Assert
            result?.Should().BeOfType<NoContentResult>();
            _mockRepo.Verify(repo => repo.Delete(id), Times.Once);
            _mockRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        // DELETE PHONE NUMBER
        [Fact]
        public async Task DeleteSupplierPhoneNumber_ReturnsNotFound_WhenSupplierNotExist()
        {
            // Arrange
            var supplierId = Guid.NewGuid();
            var phoneNumberId = Guid.NewGuid();
            _mockRepo.Setup(repo => repo.Exists(supplierId)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteSupplierPhoneNumber(supplierId, phoneNumberId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task DeleteSupplierPhoneNumber_ReturnsNotFound_WhenPhoneNotExist()
        {
            // Arrange
            var supplierId = Guid.NewGuid();
            var phoneNumberId = Guid.NewGuid();
            _mockRepo.Setup(repo => repo.Exists(supplierId)).ReturnsAsync(true);
            _mockRepo.Setup(repo => repo.PhoneNumberExist(phoneNumberId)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteSupplierPhoneNumber(supplierId, phoneNumberId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task DeleteSupplierPhoneNumber_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var supplierId = Guid.NewGuid();
            var phoneNumberId = Guid.NewGuid();
            _mockRepo.Setup(repo => repo.Exists(supplierId)).ReturnsAsync(true);
            _mockRepo.Setup(repo => repo.PhoneNumberExist(phoneNumberId)).ReturnsAsync(true);
            _mockRepo.Setup(repo => repo.DeletePhoneNumber(supplierId, phoneNumberId));
            _mockRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteSupplierPhoneNumber(supplierId, phoneNumberId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _mockRepo.Verify(repo => repo.Exists(supplierId), Times.Once);
            _mockRepo.Verify(repo => repo.PhoneNumberExist(phoneNumberId), Times.Once);
            _mockRepo.Verify(repo => repo.DeletePhoneNumber(supplierId, phoneNumberId), Times.Once);
            _mockRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        // RESTORE SUPPLIER
        [Fact]
        public async Task RestoreSupplier_ReturnsBadRequest_WhenCannotRestore()
        {
            // Arrange
            var supplierId = Guid.NewGuid();
            _mockRepo.Setup(repo => repo.RestoreSupplier(supplierId)).ReturnsAsync(false);

            // Act
            var result = await _controller.RestoreSupplier(supplierId);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>($"Supplier with ID {supplierId} not found or not deleted.");
        }

        [Fact]
        public async Task RestoreSupplier_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var supplierId = Guid.NewGuid();
            _mockRepo.Setup(repo => repo.RestoreSupplier(supplierId)).ReturnsAsync(true);
            _mockRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

            // Act
            var result = await _controller.RestoreSupplier(supplierId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _mockRepo.Verify(repo => repo.RestoreSupplier(supplierId), Times.Once);
            _mockRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        // RESTORE PHONE NUMBER

        [Fact]
        public async Task RestoreSupplierPhoneNumber_ReturnsNotFound_WhenSupplierNotExist()
        {
            // Arrange
            var supplierId = Guid.NewGuid();
            var phoneNumberId = Guid.NewGuid();
            _mockRepo.Setup(repo => repo.Exists(supplierId)).ReturnsAsync(false);

            // Act
            var result = await _controller.RestoreSupplierPhoneNumber(supplierId, phoneNumberId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>($"Supplier with ID {supplierId} not found.");
        }

        [Fact]
        public async Task RestoreSupplierPhoneNumber_ReturnsNotFound_WhenPhoneNumberNotExist()
        {
            // Arrange
            var supplierId = Guid.NewGuid();
            var phoneNumberId = Guid.NewGuid();
            _mockRepo.Setup(repo => repo.Exists(supplierId)).ReturnsAsync(true);
            _mockRepo.Setup(repo => repo.PhoneNumberExist(phoneNumberId)).ReturnsAsync(false);

            // Act
            var result = await _controller.RestoreSupplierPhoneNumber(supplierId, phoneNumberId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>($"Phone number with ID {phoneNumberId} not found");
        }

        [Fact]
        public async Task RestoreSupplierPhoneNumber_ReturnsNotFound_WhenRestoreFailed()
        {
            // Arrange
            var supplierId = Guid.NewGuid();
            var phoneNumberId = Guid.NewGuid();
            _mockRepo.Setup(repo => repo.Exists(supplierId)).ReturnsAsync(true);
            _mockRepo.Setup(repo => repo.PhoneNumberExist(phoneNumberId)).ReturnsAsync(true);
            _mockRepo.Setup(repo => repo.RestorePhoneNumber(phoneNumberId)).ReturnsAsync(false);

            // Act
            var result = await _controller.RestoreSupplierPhoneNumber(supplierId, phoneNumberId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>($"Phone number with ID {phoneNumberId} not found or not deleted.");
        }

        [Fact]
        public async Task RestoreSupplierPhoneNumber_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var supplierId = Guid.NewGuid();
            var phoneNumberId = Guid.NewGuid();
            _mockRepo.Setup(repo => repo.Exists(supplierId)).ReturnsAsync(true);
            _mockRepo.Setup(repo => repo.PhoneNumberExist(phoneNumberId)).ReturnsAsync(true);
            _mockRepo.Setup(repo => repo.RestorePhoneNumber(phoneNumberId)).ReturnsAsync(true);
            _mockRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

            // Act
            var result = await _controller.RestoreSupplierPhoneNumber(supplierId, phoneNumberId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _mockRepo.Verify(repo => repo.Exists(supplierId), Times.Once);
            _mockRepo.Verify(repo => repo.PhoneNumberExist(phoneNumberId), Times.Once);
            _mockRepo.Verify(repo => repo.RestorePhoneNumber(phoneNumberId), Times.Once);
            _mockRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        // OPTIONS

        [Fact]
        public void GetSuppliersOptions_ReturnsCorrectAllowHeader()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = _controller.GetSuppliersOptions();

            // Assert
            result.Should().BeOfType<OkResult>();
            httpContext.Response.Headers.ContainsKey("Allow").Should().BeTrue();
            httpContext.Response.Headers.Allow.ToString()
                .Should().Be("Get, Head, Put, Patch, Post, Delete, Options");

        }

        [Fact]
        public void GetASupplierOptions_ReturnsCorrectAllowHeader()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = _controller.GetSuppliersOptions();

            // Assert
            result.Should().BeOfType<OkResult>();
            httpContext.Response.Headers.ContainsKey("Allow").Should().BeTrue();
            httpContext.Response.Headers.Allow.ToString()
                .Should().Be("Get, Head, Put, Patch, Post, Delete, Options");
        }
    }
}
