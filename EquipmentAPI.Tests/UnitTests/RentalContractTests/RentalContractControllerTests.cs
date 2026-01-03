using API.Controllers;
using Application.BulkOperations;
using Application.Interface.Services;
using Application.Models.RentalContractModels.Read;
using Application.Models.RentalContractModels.Write;
using Application.ResourceParameters;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EquipmentAPI.Tests.UnitTests.RentalContractTests
{
    public class RentalContractControllerTests
    {
        private readonly Mock<IRentalContractService> _rentalContractServiceMock;
        private readonly Mock<IRentalContractAnalyticsService> _analyticsServiceMock;
        private readonly Mock<IPropertyCheckerService> _propertyCheckerMock;

        private readonly RentalContractController _controller;

        public RentalContractControllerTests()
        {
            _rentalContractServiceMock = new Mock<IRentalContractService>();
            _analyticsServiceMock = new Mock<IRentalContractAnalyticsService>();
            _propertyCheckerMock = new Mock<IPropertyCheckerService>();

            _controller = new RentalContractController(
                _rentalContractServiceMock.Object,
                _analyticsServiceMock.Object,
                _propertyCheckerMock.Object
            );
        }

        [Fact]
        public async Task GetRentalContracts_ReturnsOk_WhenParametersAreValid()
        {
            // Arrange
            var parameters = new RentalContractResourceParameters();

            _propertyCheckerMock
                .Setup(p => p.TypeHasProperties<RentalContractDto>(It.IsAny<string>()))
                .Returns(true);

            var contracts = new Shared.Results.PagedList<RentalContractDto>(new List<RentalContractDto>(), 1, 1, 10);

            _rentalContractServiceMock
                .Setup(s => s.GetRentalContracts(parameters))
                .ReturnsAsync(contracts);

            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await _controller.GetRentalContracts(parameters);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetRentalContracts_ReturnsBadRequest_WhenFieldsAreInvalid()
        {
            // Arrange
            var parameters = new RentalContractResourceParameters
            {
                Fields = "invalidField"
            };

            _propertyCheckerMock
                .Setup(p => p.TypeHasProperties<RentalContractDto>(parameters.Fields))
                .Returns(false);

            // Act
            var result = await _controller.GetRentalContracts(parameters);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetRentalContractById_ReturnsOk_WhenContractExists()
        {
            // Arrange
            var id = Guid.NewGuid();

            _propertyCheckerMock
                .Setup(p => p.TypeHasProperties<RentalContractDto>(It.IsAny<string>()))
                .Returns(true);

            var contract = new RentalContractDto { Id = id };

            _rentalContractServiceMock
                .Setup(s => s.GetRentalContractById(id, null))
                .ReturnsAsync(contract);

            // Act
            var result = await _controller.GetRentalContractById(id, null);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GetRentalContractById_ReturnsNotFound_WhenContractDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();

            _rentalContractServiceMock
                .Setup(s => s.GetRentalContractById(id, null))
                .ReturnsAsync((RentalContractDto?)null);

            // Act
            var result = await _controller.GetRentalContractById(id, null);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task CreateRentalContract_ReturnsCreatedAtRoute()
        {
            // Arrange
            var createDto = new RentalContractCreateDto();

            var createdDto = new RentalContractDto
            {
                Id = Guid.NewGuid()
            };

            _rentalContractServiceMock
                .Setup(s => s.CreateRentalContract(createDto))
                .ReturnsAsync(createdDto);

            // Act
            var result = await _controller.CreateRentalContract(createDto);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
            createdResult.RouteName.Should().Be("GetRentalContractById");
        }

        [Fact]
        public async Task UpdateRentalContract_ReturnsNoContent_WhenUpdateSucceeds()
        {
            // Arrange
            var id = Guid.NewGuid();
            var updateDto = new RentalContractUpdateDto();

            _rentalContractServiceMock
                .Setup(s => s.RentalContractExists(id))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateRentalContract(id, updateDto);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }


        [Fact]
        public async Task UpdateRentalContract_ReturnsNotFound_WhenContractDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            var updateDto = new RentalContractUpdateDto();

            _rentalContractServiceMock
                .Setup(s => s.RentalContractExists(id))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateRentalContract(id, updateDto);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task SoftDeleteRentalContract_ReturnsNoContent_WhenContractExists()
        {
            // Arrange
            var id = Guid.NewGuid();

            _rentalContractServiceMock
                .Setup(s => s.RentalContractExists(id))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.SoftDeleteRentalContract(id);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task RestoreRentalContract_ReturnsBadRequest_WhenNotDeleted()
        {
            var id = Guid.NewGuid();

            _rentalContractServiceMock.Setup(s => s.RentalContractExists(id))
                .ReturnsAsync(true);

            var result = await _controller.RestoreRentalContract(id);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task RestoreRentalContracts_ReturnsBadRequest_WhenIdsNullOrEmpty()
        {
            var result = await _controller.RestoreRentalContracts(null);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task RestoreRentalContracts_ReturnsBadRequest_WhenExceedsMaxSize()
        {
            var ids = Enumerable.Range(0, 501)
                         .Select(_ => Guid.NewGuid())
                         .ToList();

            var result = await _controller.RestoreRentalContracts(ids);

            result.Should().BeOfType<BadRequestObjectResult>();
        }


        [Fact]
        public async Task RestoreRentalContracts_ReturnsOk_WhenValid()
        {
            var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var restoredContracts = new BulkOperationResult
            {
                FailureCount = 0,
                SuccessCount = 2,
                SuccessIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
                Errors = new List<BulkOperationError>()
            };

            _rentalContractServiceMock.Setup(s => s.RestoreRentalContracts(ids))
                .ReturnsAsync(restoredContracts);

            var result = await _controller.RestoreRentalContracts(ids);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task FinishExpiredContracts_ReturnsNoContent()
        {
            var result = await _controller.FinishExpiredContracts();

            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task RentalContractExists_ReturnsNotFound_WhenFalse()
        {
            var id = Guid.NewGuid();

            _rentalContractServiceMock.Setup(s => s.RentalContractExists(id))
                .ReturnsAsync(false);

            var result = await _controller.RentalContractExists(id);

            result.Should().BeOfType<NotFoundObjectResult>();
        }


        [Fact]
        public async Task RentalContractExists_ReturnsNoContent_WhenTrue()
        {
            var id = Guid.NewGuid();

            _rentalContractServiceMock.Setup(s => s.RentalContractExists(id))
                .ReturnsAsync(true);

            var result = await _controller.RentalContractExists(id);

            result.Should().BeOfType<NoContentResult>();
        }


        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CustomerHasContracts_ReturnsExpectedResult(bool hasContracts)
        {
            var id = Guid.NewGuid();

            _rentalContractServiceMock.Setup(s => s.CustomerHasContracts(id))
                .ReturnsAsync(hasContracts);

            var result = await _controller.CustomerHasContracts(id);

            result.Should().BeOfType(
                hasContracts ? typeof(NoContentResult) : typeof(NotFoundObjectResult));
        }


        [Fact]
        public async Task HasOverlappingContracts_ReturnsNoContent_WhenExists()
        {
            _rentalContractServiceMock.Setup(s => s.HasOverlappingContracts(
                It.IsAny<Guid>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<Guid?>()))
                .ReturnsAsync(true);

            var result = await _controller.HasOverlappingContracts(
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddDays(1));

            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task HasOverlappingContracts_ReturnsNotFound_WhenNone()
        {
            _rentalContractServiceMock.Setup(s => s.HasOverlappingContracts(
                It.IsAny<Guid>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<Guid?>()))
                .ReturnsAsync(false);

            var result = await _controller.HasOverlappingContracts(
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddDays(1));

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetRentalContractCount_ReturnsOk()
        {
            _analyticsServiceMock.Setup(a => a.GetRentalContractCount())
                .ReturnsAsync(10);

            var result = await _controller.GetRentalContractCount();

            result.Should().BeOfType<OkObjectResult>();
        }


        [Fact]
        public async Task GetTotalContractsForEquipment_ReturnsNotFound_WhenNoContracts()
        {
            var id = Guid.NewGuid();

            _rentalContractServiceMock.Setup(s => s.EquipmentHasContracts(id))
                .ReturnsAsync(false);

            var result = await _controller.GetTotalContractsForEquipment(id);

            result.Should().BeOfType<NotFoundObjectResult>();
        }


        [Fact]
        public async Task GetTotalContractsForEquipment_ReturnsOk_WhenExists()
        {
            var id = Guid.NewGuid();

            _rentalContractServiceMock.Setup(s => s.EquipmentHasContracts(id))
                .ReturnsAsync(true);

            _analyticsServiceMock.Setup(a => a.GetTotalContractsForEquipment(id))
                .ReturnsAsync(5);

            var result = await _controller.GetTotalContractsForEquipment(id);

            result.Should().BeOfType<OkObjectResult>();
        }




    }
}
