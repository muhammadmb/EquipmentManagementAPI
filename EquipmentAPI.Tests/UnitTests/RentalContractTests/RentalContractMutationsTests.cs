using API.GraphQL.RentalContract.Mutations;
using Application.Interface.Services;
using Application.Models.RentalContractModels.Read;
using Application.Models.RentalContractModels.Write;
using FluentAssertions;
using HotChocolate;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquipmentAPI.Tests.UnitTests.RentalContractTests
{
    public class RentalContractMutationsTests
    {
        private readonly Mock<IRentalContractService> _serviceMock;
        private readonly RentalContractMutations _mutations;

        public RentalContractMutationsTests()
        {
            _serviceMock = new Mock<IRentalContractService>();
            _mutations = new RentalContractMutations();
        }

    [Fact]
        public async Task CreateRentalContract_Should_Return_Created_Contract()
        {
            // Arrange
            var createDto = new RentalContractCreateDto();
            var expected = new RentalContractDto { Id = Guid.NewGuid() };

            _serviceMock
                .Setup(s => s.CreateRentalContract(createDto))
                .ReturnsAsync(expected);

            // Act
            var result = await _mutations.CreateRentalContract(createDto, _serviceMock.Object);

            // Assert
            result.Should().BeEquivalentTo(expected);
            _serviceMock.Verify(s => s.CreateRentalContract(createDto), Times.Once);
        }

        [Fact]
        public async Task CreateRentalContracts_Should_Throw_When_Input_Is_Empty()
        {
            // Act
            var act = async () =>
                await _mutations.CreateRentalContracts(
                    Enumerable.Empty<RentalContractCreateDto>(),
                    _serviceMock.Object);

            // Assert
            await act.Should()
                .ThrowAsync<GraphQLException>()
                .WithMessage("*at least one rental contract*");
        }

        [Fact]
        public async Task UpdateRentalContract_Should_Throw_When_Not_Found()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new RentalContractUpdateDto();

            _serviceMock
                .Setup(s => s.RentalContractExists(id))
                .ReturnsAsync(false);

            // Act
            var act = async () =>
                await _mutations.UpdateRentalContract(id, dto, _serviceMock.Object);

            // Assert
            await act.Should()
                .ThrowAsync<GraphQLException>()
                .WithMessage($"*{id}*");
        }


        [Fact]
        public async Task UpdateRentalContract_Should_Throw_Concurrency_Error()
        {
            var id = Guid.NewGuid();
            var dto = new RentalContractUpdateDto();

            _serviceMock.Setup(s => s.RentalContractExists(id)).ReturnsAsync(true);
            _serviceMock
                .Setup(s => s.UpdateRentalContract(id, dto))
                .ThrowsAsync(new DbUpdateConcurrencyException());

            var act = async () =>
                await _mutations.UpdateRentalContract(id, dto, _serviceMock.Object);

            await act.Should()
                .ThrowAsync<GraphQLException>()
                .Where(e => e.Errors.First().Code == "CONCURRENCY_CONFLICT");
        }


        [Fact]
        public async Task DeleteRentalContractById_Should_Delete_And_Return_True()
        {
            var id = Guid.NewGuid();

            _serviceMock.Setup(s => s.RentalContractExists(id)).ReturnsAsync(true);

            var result = await _mutations.DeleteRentalContractById(id, _serviceMock.Object);

            result.Should().BeTrue();
            _serviceMock.Verify(s => s.SoftDeleteRentalContract(id), Times.Once);
        }


        [Fact]
        public async Task RestoreRentalContract_Should_Throw_When_Not_Deleted()
        {
            var id = Guid.NewGuid();

            _serviceMock.Setup(s => s.RentalContractExists(id)).ReturnsAsync(true);

            var act = async () =>
                await _mutations.RestoreRentalContract(id, _serviceMock.Object);

            await act.Should()
                .ThrowAsync<GraphQLException>()
                .WithMessage("*Only deleted rental contracts*");
        }


        [Fact]
        public async Task ActivateRentalContract_Should_Call_Service()
        {
            var id = Guid.NewGuid();

            var result = await _mutations.ActivateRentalContract(id, _serviceMock.Object);

            result.Should().BeTrue();
            _serviceMock.Verify(s => s.ActiveRentalContract(id), Times.Once);
        }


        [Theory]
        [InlineData("Activate")]
        [InlineData("Cancel")]
        [InlineData("Suspend")]
        public async Task Contract_State_Actions_Should_Call_Service(string action)
        {
            var id = Guid.NewGuid();

            switch (action)
            {
                case "Activate":
                    await _mutations.ActivateRentalContract(id, _serviceMock.Object);
                    _serviceMock.Verify(s => s.ActiveRentalContract(id), Times.Once);
                    break;
            }
        }

    }
}
