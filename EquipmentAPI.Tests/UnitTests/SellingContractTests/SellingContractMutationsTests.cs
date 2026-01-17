using API.GraphQL.SellingContract.Mutations;
using Application.BulkOperations;
using Application.Interface.Services;
using Application.Models.SellingContract.Read;
using Application.Models.SellingContract.Write;
using HotChocolate;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EquipmentAPI.Tests.UnitTests.SellingContractTests
{
    public class SellingContractMutationsTests
    {
        private readonly Mock<ISellingContractService> _serviceMock;
        private readonly SellingContractMutations _mutations;

        public SellingContractMutationsTests()
        {
            _serviceMock = new Mock<ISellingContractService>();
            _mutations = new SellingContractMutations();
        }

        [Fact]
        public async Task CreateSellingContract_ReturnsCreatedContract()
        {
            // Arrange
            var createDto = new SellingContractCreateDto();
            var createdContract = new SellingContractDto();

            _serviceMock
                .Setup(s => s.CreateSellingContract(createDto))
                .ReturnsAsync(createdContract);

            // Act
            var result = await _mutations.CreateSellingContract(createDto, _serviceMock.Object);

            // Assert
            Assert.Equal(createdContract, result);
            _serviceMock.Verify(
                s => s.CreateSellingContract(createDto),
                Times.Once);
        }

        [Fact]
        public async Task CreateRentalContracts_Throws_WhenCollectionIsNull()
        {
            // Arrange
            IEnumerable<SellingContractCreateDto> createDtos = null;

            // Act & Assert
            await Assert.ThrowsAsync<GraphQLException>(() =>
                _mutations.CreateRentalContracts(createDtos, _serviceMock.Object));
        }

        [Fact]
        public async Task CreateRentalContracts_Throws_WhenCollectionIsEmpty()
        {
            // Arrange
            var createDtos = Enumerable.Empty<SellingContractCreateDto>();

            // Act & Assert
            await Assert.ThrowsAsync<GraphQLException>(() =>
                _mutations.CreateRentalContracts(createDtos, _serviceMock.Object));
        }

        [Fact]
        public async Task CreateRentalContracts_Throws_WhenExceedsMaxBulkSize()
        {
            // Arrange
            var createDtos = Enumerable.Range(0, 501)
                .Select(_ => new SellingContractCreateDto())
                .ToList();

            // Act & Assert
            await Assert.ThrowsAsync<GraphQLException>(() =>
                _mutations.CreateRentalContracts(createDtos, _serviceMock.Object));
        }

        [Fact]
        public async Task CreateRentalContracts_ReturnsResult_WhenValid()
        {
            // Arrange
            var createDtos = new List<SellingContractCreateDto>
            {
                new SellingContractCreateDto()
            };

            var result = new BulkOperationResult();

            _serviceMock
                .Setup(s => s.CreateSellingContracts(createDtos))
                .ReturnsAsync(result);

            // Act
            var response = await _mutations.CreateRentalContracts(createDtos, _serviceMock.Object);

            // Assert
            Assert.Equal(result, response);
            _serviceMock.Verify(
                s => s.CreateSellingContracts(createDtos),
                Times.Once);
        }

        [Fact]
        public async Task UpdateSellingContract_Throws_WhenContractDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            var updateDto = new SellingContractUpdateDto();

            _serviceMock
                .Setup(s => s.SellingContractExists(id))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<GraphQLException>(() =>
                _mutations.UpdateSellingContract(id, updateDto, _serviceMock.Object));
        }

        [Fact]
        public async Task UpdateSellingContract_ReturnsUpdatedContract_WhenSuccessful()
        {
            // Arrange
            var id = Guid.NewGuid();
            var updateDto = new SellingContractUpdateDto();
            var updatedContract = new SellingContractDto { Id = id };

            _serviceMock
                .Setup(s => s.SellingContractExists(id))
                .ReturnsAsync(true);

            _serviceMock
                .Setup(s => s.UpdateSellingContract(id, updateDto))
                .Returns(Task.CompletedTask);

            _serviceMock
                .Setup(s => s.GetSellingContractById(id, null))
                .ReturnsAsync(updatedContract);

            // Act
            var result = await _mutations.UpdateSellingContract(id, updateDto, _serviceMock.Object);

            // Assert
            Assert.Equal(id, result.Id);
        }

        [Fact]
        public async Task UpdateSellingContract_ThrowsGraphQLException_OnConcurrencyConflict()
        {
            // Arrange
            var id = Guid.NewGuid();
            var updateDto = new SellingContractUpdateDto();

            _serviceMock
                .Setup(s => s.SellingContractExists(id))
                .ReturnsAsync(true);

            _serviceMock
                .Setup(s => s.UpdateSellingContract(id, updateDto))
                .ThrowsAsync(new DbUpdateConcurrencyException());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<GraphQLException>(() =>
                _mutations.UpdateSellingContract(id, updateDto, _serviceMock.Object));

            Assert.Equal("CONCURRENCY_CONFLICT", exception.Errors[0].Code);
        }

        [Fact]
        public async Task DeleteSellingContractById_Throws_WhenNotExists()
        {
            // Arrange
            var id = Guid.NewGuid();

            _serviceMock
                .Setup(s => s.SellingContractExists(id))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<GraphQLException>(() =>
                _mutations.DeleteSellingContractById(id, _serviceMock.Object));
        }

        [Fact]
        public async Task DeleteSellingContractById_ReturnsTrue_WhenSuccessful()
        {
            // Arrange
            var id = Guid.NewGuid();

            _serviceMock
                .Setup(s => s.SellingContractExists(id))
                .ReturnsAsync(true);

            // Act
            var result = await _mutations.DeleteSellingContractById(id, _serviceMock.Object);

            // Assert
            Assert.True(result);
            _serviceMock.Verify(
                s => s.SoftDeleteSellingContract(id),
                Times.Once);
        }

        [Fact]
        public async Task RestoreSellingContractById_Throws_WhenNotExists()
        {
            // Arrange
            var id = Guid.NewGuid();

            _serviceMock
                .Setup(s => s.SellingContractExists(id))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<GraphQLException>(() =>
                _mutations.RestoreSellingContractById(id, _serviceMock.Object));
        }

        [Fact]
        public async Task RestoreSellingContractById_ReturnsTrue_WhenSuccessful()
        {
            // Arrange
            var id = Guid.NewGuid();

            _serviceMock
                .Setup(s => s.SellingContractExists(id))
                .ReturnsAsync(true);

            // Act
            var result = await _mutations.RestoreSellingContractById(id, _serviceMock.Object);

            // Assert
            Assert.True(result);
            _serviceMock.Verify(
                s => s.RestoreSellingContract(id),
                Times.Once);
        }
    }
}