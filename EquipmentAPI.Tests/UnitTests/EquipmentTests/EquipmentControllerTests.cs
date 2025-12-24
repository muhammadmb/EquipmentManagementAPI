using API.Controllers;
using Application.Interface.Repositories;
using Application.Interface.Services;
using Application.Models.EquipmentModels.Read;
using Application.Models.EquipmentModels.Write;
using Application.ResourceParameters;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Shared.Results;
namespace EquipmentAPI.Tests.UnitTests.EquipmentTests
{
    public class EquipmentControllerTests
    {
        private readonly Mock<IEquipmentRepository> _mockRepo;
        private readonly Mock<IPropertyCheckerService> _mockPropertyChecker;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly EquipmentController _controller;

        public EquipmentControllerTests()
        {
            _mockRepo = new Mock<IEquipmentRepository>();
            _mockPropertyChecker = new Mock<IPropertyCheckerService>();
            _mockCache = new Mock<IMemoryCache>();
            _controller = new EquipmentController(_mockRepo.Object, _mockPropertyChecker.Object, _mockCache.Object);
        }

        [Fact]
        public async Task GetEquipment_ShouldReturnOkResult_WhenDataExists()
        {
            // Arrange
            var parameters = new EquipmentResourceParameters { Fields = "Name" };
            var equipmentList = new PagedList<Equipment>(new List<Equipment>(), 1, 1, 10);

            _mockRepo.Setup(repo => repo.GetEquipment(parameters)).ReturnsAsync(equipmentList);
            _mockPropertyChecker.Setup(service => service.TypeHasProperties<EquipmentDto>(parameters.Fields)).Returns(true);

            _mockPropertyChecker
                .Setup(service => service.TypeHasProperties<EquipmentDto>(parameters.SortBy))
                .Returns(true);

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
            var result = await _controller.GetEquipment(parameters);

            // Assert
            Assert.IsType<OkObjectResult>(result);

            var responseHeaders = httpContext.Response.Headers;
            responseHeaders.Should().ContainKey("X-Pagination");
        }

        [Fact]
        public async Task GetEquipment_ReturnsBadRequest_ForInvalidFields()
        {
            // Arrange
            var parameters = new EquipmentResourceParameters { Fields = "InvalidField" };

            _mockPropertyChecker
                .Setup(service => service.TypeHasProperties<EquipmentDto>(parameters.Fields))
                .Returns(false);

            // Act
            var result = await _controller.GetEquipment(parameters);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetEquipment_ReturnsCachedResult()
        {
            // Arrange
            var parameters = new EquipmentResourceParameters { Fields = "Name, InternalSerial" };
            var equipmentList = new PagedList<Equipment>(new List<Equipment>(), 0, 1, 10);

            _mockPropertyChecker
                .Setup(service => service.TypeHasProperties<EquipmentDto>(parameters.Fields))
                .Returns(true);

            _mockPropertyChecker
                .Setup(service => service.TypeHasProperties<EquipmentDto>(parameters.SortBy))
                .Returns(true);

            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Mock the cache for TryGetValue (cache hit)
            object cachedValue = equipmentList;
            _mockCache
                .Setup(cache => cache.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(true);

            // Act
            var result = await _controller.GetEquipment(parameters);

            // Assert
            result.Should().BeOfType<OkObjectResult>();

            // Verify that the repository was not called
            _mockRepo.Verify(repo => repo.GetEquipment(parameters), Times.Never);
        }

        [Fact]
        public async Task GetEquipment_FetchesFromRepository_WhenCacheMiss()
        {
            // Arrange
            var parameters = new EquipmentResourceParameters { Fields = "Name" };
            var equipmentList = new PagedList<Equipment>(new List<Equipment>(), 0, 1, 10);

            _mockPropertyChecker
                .Setup(service => service.TypeHasProperties<EquipmentDto>(parameters.Fields))
                .Returns(true);

            _mockPropertyChecker
                .Setup(service => service.TypeHasProperties<EquipmentDto>(parameters.SortBy))
                .Returns(true);

            // Mock the cache for TryGetValue (cache miss)
            object cachedValue = null;
            _mockCache
                .Setup(cache => cache.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);

            // Mock the repository to return data
            _mockRepo
                .Setup(repo => repo.GetEquipment(parameters))
                .ReturnsAsync(equipmentList);

            // Mock the cache for Set (CreateEntry)
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
            var result = await _controller.GetEquipment(parameters);

            // Assert
            result.Should().BeOfType<OkObjectResult>();

            // Verify that the repository was called
            _mockRepo.Verify(repo => repo.GetEquipment(parameters), Times.Once);
        }

        [Fact]
        public async Task GetEquipmentById_ShouldReturnNotFound_WhenEquipmentDoesNotExist()
        {
            // Arrange
            var parameters = new EquipmentResourceParameters { Fields = "Name" };
            _mockRepo.Setup(repo => repo.GetEquipmentById(It.IsAny<Guid>(), parameters.Fields))
                     .ReturnsAsync((Equipment)null);
            _mockPropertyChecker.Setup(service => service.TypeHasProperties<EquipmentDto>(parameters.Fields)).Returns(true);

            // Mock the cache
            object cachedValue = null;
            _mockCache
                .Setup(cache => cache.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);

            var mockCacheEntry = new Mock<ICacheEntry>();
            _mockCache
                .Setup(cache => cache.CreateEntry(It.IsAny<object>()))
                .Returns(mockCacheEntry.Object);

            // Act
            var result = await _controller.GetEquipmentById(Guid.NewGuid(), parameters.Fields);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task CreateEquipment_ShouldReturnCreatedAtRoute()
        {
            // Arrange
            var equipmentDto = new EquipmentCreateDto { Name = "New Equipment" };
            var createdEquipment = new Equipment { Id = Guid.NewGuid(), Name = "New Equipment" };

            _mockRepo.Setup(repo => repo.Create(It.IsAny<Equipment>()));
            _mockRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

            // Act
            var result = await _controller.CreateEquipment(equipmentDto);

            // Assert
            var createdAtRouteResult = Assert.IsType<CreatedAtRouteResult>(result);
            Assert.Equal("GetAnEquipment", createdAtRouteResult.RouteName);
        }

        [Fact]
        public async Task CreateEquipment_ReturnsCreatedAtRoute_ForValidData()
        {
            // Arrange
            var equipmentDto = new EquipmentCreateDto
            {
                Name = "Excavator",
                InternalSerial = "123456",
                Price = 1000,
                Expenses = 200,
                ManufactureDate = 2020,
                EquipmentStatus = EquipmentStatus.Available,
                PurchaseDate = DateTime.UtcNow
            };

            // Act
            var result = await _controller.CreateEquipment(equipmentDto);

            // Assert
            result.Should().BeOfType<CreatedAtRouteResult>();
        }

        [Fact]
        public async Task CreateEquipment_ReturnsBadRequest_ForInvalidData()
        {
            // Arrange
            var equipmentDto = new EquipmentCreateDto { Name = null }; // Invalid data

            _controller.ModelState.AddModelError("Name", "The Name field is required.");

            // Act
            var result = await _controller.CreateEquipment(equipmentDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateEquipment_ReturnsBadRequest_ForMissingRequiredFields()
        {
            // Arrange
            var equipmentDto = new EquipmentCreateDto
            {
                // Missing required fields: InternalSerial, Price, Expenses, ManufactureDate, EquipmentStatus, PurchaseDate
            };

            _controller.ModelState.AddModelError("Name", "The Name field is required.");

            // Act
            var result = await _controller.CreateEquipment(equipmentDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateEquipment_ShouldReturnNotFound_WhenEquipmentDoesNotExist()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.GetEquipmentById(It.IsAny<Guid>(), null))
                     .ReturnsAsync((Equipment)null);

            // Act
            var result = await _controller.UpdateEquipment(Guid.NewGuid(), new EquipmentUpdateDto());

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateEquipment_ReturnsConflict_ForConcurrencyConflict()
        {
            // Arrange
            var equipmentId = Guid.NewGuid();
            var equipmentDto = new EquipmentUpdateDto { Name = "Updated Excavator", RowVersion = new byte[] { 1, 2, 3, 4 } };
            var equipment = new Equipment { Id = equipmentId, RowVersion = new byte[] { 5, 6, 7, 8 } };

            _mockRepo
                .Setup(repo => repo.GetEquipmentById(equipmentId, null))
                .ReturnsAsync(equipment);

            _mockRepo
                .Setup(repo => repo.Update(equipment))
                .Throws(new DbUpdateConcurrencyException("Concurrency conflict"));

            // Act
            var result = await _controller.UpdateEquipment(equipmentId, equipmentDto);

            // Assert
            result.Should().BeOfType<ConflictObjectResult>();
        }

        [Fact]
        public async Task DeleteEquipment_ShouldReturnNoContent()
        {
            // Arrange
            var equipment = new Equipment { Id = Guid.NewGuid(), Name = "To Be Deleted" };

            _mockRepo.Setup(repo => repo.GetEquipmentById(It.IsAny<Guid>(), null))
                     .ReturnsAsync(equipment);

            _mockRepo.Setup(repo => repo.Delete(It.IsAny<Guid>()));
            _mockRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteEquipment(Guid.NewGuid());

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteEquipment_ReturnsNoContent()
        {
            // Arrange
            var equipmentId = Guid.NewGuid();
            var equipment = new Equipment { Id = equipmentId };

            _mockRepo
                .Setup(repo => repo.GetEquipmentById(equipmentId, null))
                .ReturnsAsync(equipment);

            // Act
            var result = await _controller.DeleteEquipment(equipmentId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeleteEquipment_ReturnsNotFound_ForInvalidId()
        {
            // Arrange
            var equipmentId = Guid.NewGuid();

            _mockRepo
                .Setup(repo => repo.GetEquipmentById(equipmentId, null))
                .ReturnsAsync((Equipment)null);

            // Act
            var result = await _controller.DeleteEquipment(equipmentId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetEquipment_AddsPaginationHeader()
        {
            // Arrange
            var parameters = new EquipmentResourceParameters { Fields = "Name" };
            var equipmentList = new PagedList<Equipment>(new List<Equipment>(), 0, 1, 10);

            _mockRepo
                .Setup(repo => repo.GetEquipment(parameters))
                .ReturnsAsync(equipmentList);

            _mockPropertyChecker
                .Setup(service => service.TypeHasProperties<EquipmentDto>(parameters.Fields))
                .Returns(true);

            _mockPropertyChecker
                .Setup(service => service.TypeHasProperties<EquipmentDto>(parameters.SortBy))
                .Returns(true);

            // Mock the cache for TryGetValue (cache miss)
            object cachedValue = null;
            _mockCache
                .Setup(cache => cache.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);

            // Mock the cache for Set (CreateEntry)
            var mockCacheEntry = new Mock<ICacheEntry>();
            _mockCache
                .Setup(cache => cache.CreateEntry(It.IsAny<object>()))
                .Returns(mockCacheEntry.Object);

            // Mock the HTTP context and response
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await _controller.GetEquipment(parameters);

            // Assert
            result.Should().BeOfType<OkObjectResult>();

            // Verify the response headers
            var responseHeaders = httpContext.Response.Headers;
            responseHeaders.Should().ContainKey("X-Pagination");
        }
    }
}