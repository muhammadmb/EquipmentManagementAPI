using API.Controllers;
using Application.Interface.Services;
using Application.Models.SellingContract.Read;
using Application.Models.SellingContract.Write;
using Application.ResourceParameters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shared.Results;

namespace EquipmentAPI.Tests.UnitTests.SellingContractTests
{
    public class SellingContractControllerTests
    {
        private readonly Mock<ISellingContractService> _serviceMock;
        private readonly Mock<ISellingContractAnalyticsService> _analyticsMock;
        private readonly Mock<IPropertyCheckerService> _propertyCheckerMock;

        private readonly SellingContractController _controller;

        public SellingContractControllerTests()
        {
            _serviceMock = new Mock<ISellingContractService>();
            _analyticsMock = new Mock<ISellingContractAnalyticsService>();
            _propertyCheckerMock = new Mock<IPropertyCheckerService>();

            _controller = new SellingContractController(
                _serviceMock.Object,
                _analyticsMock.Object,
                _propertyCheckerMock.Object);
        }

        [Fact]
        public async Task GetSellingContracts_ReturnsOk_WhenValidParameters()
        {
            // Arrange
            var parameters = new SellingContractResourceParameters();
            var contracts = new PagedList<SellingContractDto>(new List<SellingContractDto>(), 1, 1, 10);

            _propertyCheckerMock
                .Setup(p => p.TypeHasProperties<SellingContractDto>(It.IsAny<string>()))
                .Returns(true);

            _serviceMock
                .Setup(s => s.GetSellingContracts(parameters))
                .ReturnsAsync(contracts);

            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await _controller.GetSellingContracts(parameters);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetSellingContracts_ReturnsBadRequest_WhenInvalidFields()
        {
            // Arrange
            var parameters = new SellingContractResourceParameters
            {
                Fields = "invalidField"
            };

            _propertyCheckerMock
                .Setup(p => p.TypeHasProperties<SellingContractDto>(parameters.Fields))
                .Returns(false);

            // Act
            var result = await _controller.GetSellingContracts(parameters);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetSellingContractById_ReturnsOk_WhenExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new SellingContractDto();

            _propertyCheckerMock
                .Setup(p => p.TypeHasProperties<SellingContractDto>(It.IsAny<string>()))
                .Returns(true);

            _serviceMock
                .Setup(s => s.GetSellingContractById(id, null))
                .ReturnsAsync(dto);

            // Act
            var result = await _controller.GetSellingContractById(id, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task CreateSellingContract_ReturnsCreatedAtRoute()
        {
            // Arrange
            var dto = new SellingContractCreateDto();
            var created = new SellingContractDto { Id = Guid.NewGuid() };

            _serviceMock
                .Setup(s => s.CreateSellingContract(dto))
                .ReturnsAsync(created);

            // Act
            var result = await _controller.CreateSellingContract(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtRouteResult>(result);
            Assert.Equal("GetSellingContractById", createdResult.RouteName);
        }

        [Fact]
        public async Task CreateSellingContracts_ReturnsBadRequest_WhenEmpty()
        {
            // Act
            var result = await _controller.CreateSellingContracts(Enumerable.Empty<SellingContractCreateDto>());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateSellingContract_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new SellingContractUpdateDto();

            _serviceMock
                .Setup(s => s.SellingContractExists(id))
                .ReturnsAsync(true);

            _serviceMock
                .Setup(s => s.UpdateSellingContract(id, dto))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateSellingContract(id, dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }


        [Fact]
        public async Task UpdateSellingContract_ReturnsNotFound_WhenNotExists()
        {
            // Arrange
            var id = Guid.NewGuid();

            _serviceMock
                .Setup(s => s.SellingContractExists(id))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateSellingContract(id, new SellingContractUpdateDto());

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task PatchSellingContract_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var id = Guid.NewGuid();
            var patch = new JsonPatchDocument<SellingContractUpdateDto>();

            _serviceMock
                .Setup(s => s.SellingContractExists(id))
                .ReturnsAsync(true);

            _serviceMock
                .Setup(s => s.Patch(id, patch))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.PatchSellingContract(id, patch);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task SoftDeleteSellingContract_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var id = Guid.NewGuid();

            _serviceMock
                .Setup(s => s.SellingContractExists(id))
                .ReturnsAsync(true);

            _serviceMock
                .Setup(s => s.SoftDeleteSellingContract(id))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.SoftDeleteSellingContract(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task RestoreSellingContract_ReturnsNoContent_WhenDeleted()
        {
            // Arrange
            var id = Guid.NewGuid();

            _serviceMock
                .Setup(s => s.SellingContractExists(id))
                .ReturnsAsync(false);

            _serviceMock
                .Setup(s => s.RestoreSellingContract(id))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.RestoreSellingContract(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task GetTotalRevenue_ReturnsOk()
        {
            // Arrange
            _analyticsMock
                .Setup(a => a.GetTotalRevenue(null, null))
                .ReturnsAsync(1000m);

            // Act
            var result = await _controller.GetTotalRevenue(null, null);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1000m, ok.Value);
        }

        #region Total Revenue

        [Fact]
        public async Task GetTotalRevenue_ReturnsOk_WithValue()
        {
            // Arrange
            _analyticsMock
                .Setup(a => a.GetTotalRevenue(null, null))
                .ReturnsAsync(5000m);

            // Act
            var result = await _controller.GetTotalRevenue(null, null);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(5000m, ok.Value);
        }

        #endregion

        #region Average Sale Price

        [Fact]
        public async Task GetAverageSalePrice_ReturnsOk()
        {
            _analyticsMock
                .Setup(a => a.GetAverageSalePrice(null, null))
                .ReturnsAsync(1200m);

            var result = await _controller.GetAverageSalePrice(null, null);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1200m, ok.Value);
        }

        #endregion

        #region Average Sale Price By Equipment

        [Fact]
        public async Task GetAverageSalePriceByEquipment_ReturnsOk()
        {
            _analyticsMock
                .Setup(a => a.GetAverageSalePriceByEquipment(
                    null, null, null, null))
                .ReturnsAsync(900m);

            var result = await _controller
                .GetAverageSalePriceByEquipment(null, null, null, null);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(900m, ok.Value);
        }

        #endregion

        #region Min / Max Sale Price

        [Fact]
        public async Task GetMinSalePrice_ReturnsOk()
        {
            _analyticsMock
                .Setup(a => a.GetMinSalePrice(null, null))
                .ReturnsAsync(300m);

            var result = await _controller.GetMinSalePrice(null, null);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(300m, ok.Value);
        }

        [Fact]
        public async Task GetMaxSalePrice_ReturnsOk()
        {
            _analyticsMock
                .Setup(a => a.GetMaxSalePrice(null, null))
                .ReturnsAsync(5000m);

            var result = await _controller.GetMaxSalePrice(null, null);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(5000m, ok.Value);
        }

        #endregion

        #region Revenue By Day / Month / Year

        [Fact]
        public async Task GetRevenueByDay_ReturnsOk()
        {
            var data = new Dictionary<DateTime, decimal>();

            _analyticsMock
                .Setup(a => a.GetRevenueByDay(null, null))
                .ReturnsAsync(data);

            var result = await _controller.GetRevenueByDay(null, null);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(data, ok.Value);
        }

        [Fact]
        public async Task GetRevenueByMonth_ReturnsOk()
        {
            var data = new Dictionary<int, decimal>();

            _analyticsMock
                .Setup(a => a.GetRevenueByMonth(2024))
                .ReturnsAsync(data);

            var result = await _controller.GetRevenueByMonth(2024);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(data, ok.Value);
        }

        [Fact]
        public async Task GetRevenueByYear_ReturnsOk()
        {
            var data = new Dictionary<int, decimal>();

            _analyticsMock
                .Setup(a => a.GetRevenueByYear())
                .ReturnsAsync(data);

            var result = await _controller.GetRevenueByYear();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(data, ok.Value);
        }

        #endregion

        #region Sales Count

        [Fact]
        public async Task GetSalesCount_ReturnsOk()
        {
            _analyticsMock
                .Setup(a => a.GetSalesCount(null, null))
                .ReturnsAsync(25);

            var result = await _controller.GetSalesCount(null, null);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(25, ok.Value);
        }

        #endregion

        #region Revenue & Sales By Customer

        [Fact]
        public async Task GetRevenueByCustomer_ReturnsOk()
        {
            var data = new Dictionary<Guid, decimal>();

            _analyticsMock
                .Setup(a => a.GetRevenueByCustomer(null, null))
                .ReturnsAsync(data);

            var result = await _controller.GetRevenueByCustomer(null, null);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(data, ok.Value);
        }

        [Fact]
        public async Task GetSalesCountByCustomer_ReturnsOk()
        {
            var data = new Dictionary<Guid, int>();

            _analyticsMock
                .Setup(a => a.GetSalesCountByCustomer(null, null))
                .ReturnsAsync(data);

            var result = await _controller.GetSalesCountByCustomer(null, null);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(data, ok.Value);
        }

        #endregion

        #region Top Customers

        [Fact]
        public async Task GetTopCustomers_ReturnsBadRequest_WhenTopInvalid()
        {
            var result = await _controller.GetTopCustomers(0, null, null);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetTopCustomers_ReturnsOk_WhenValid()
        {
            var data = new List<TopCustomerResult>();

            _analyticsMock
                .Setup(a => a.GetTopCustomers(10, null, null))
                .ReturnsAsync(data);

            var result = await _controller.GetTopCustomers(5, null, null);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(data, ok.Value);
        }

        #endregion

        #region Equipment Analytics

        [Fact]
        public async Task GetRevenueByEquipment_ReturnsOk()
        {
            var data = new Dictionary<Guid, decimal>();

            _analyticsMock
                .Setup(a => a.GetRevenueByEquipment(null, null))
                .ReturnsAsync(data);

            var result = await _controller.GetRevenueByEquipment(null, null);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(data, ok.Value);
        }

        [Fact]
        public async Task GetTopSellingEquipment_ReturnsBadRequest_WhenTopInvalid()
        {
            var result = await _controller.GetTopSellingEquipment(0, null, null);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetTopSellingEquipment_ReturnsOk_WhenValid()
        {
            var data = new List<TopEquipmentResult>();

            _analyticsMock
                .Setup(a => a.GetTopSellingEquipment(5, null, null))
                .ReturnsAsync(data);

            var result = await _controller.GetTopSellingEquipment(5, null, null);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(data, ok.Value);
        }

        [Fact]
        public async Task GetSalesCountByEquipment_ReturnsOk()
        {
            var data = new Dictionary<Guid, int>();

            _analyticsMock
                .Setup(a => a.GetSalesCountByEquipment(null, null))
                .ReturnsAsync(data);

            var result = await _controller.GetSalesCountByEquipment(null, null);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(data, ok.Value);
        }

        #endregion

        #region Deleted Contracts

        [Fact]
        public async Task GetDeletedContractsCount_ReturnsOk()
        {
            _analyticsMock
                .Setup(a => a.GetDeletedContractsCount())
                .ReturnsAsync(3);

            var result = await _controller.GetDeletedContractsCount();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(3, ok.Value);
        }

        #endregion

        #region Average Sale Price Per Equipment

        [Fact]
        public async Task GetAverageSalePricePerEquipment_ReturnsOk()
        {
            var equipmentId = Guid.NewGuid();

            _analyticsMock
                .Setup(a => a.GetAverageSalePricePerEquipment(equipmentId))
                .ReturnsAsync(1500m);

            var result = await _controller.GetAverageSalePricePerEquipment(equipmentId);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1500m, ok.Value);
        }

        #endregion
    }
}
