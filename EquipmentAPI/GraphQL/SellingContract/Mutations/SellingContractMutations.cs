using Application.BulkOperations;
using Application.Interface.Services;
using Application.Models.SellingContract.Read;
using Application.Models.SellingContract.Write;
using Microsoft.EntityFrameworkCore;

namespace API.GraphQL.SellingContract.Mutations
{
    [ExtendObjectType(Name = "Mutation")]
    public class SellingContractMutations
    {
        private const int MaxBulkOperationSize = 500;

        public async Task<SellingContractDto> CreateSellingContract(
            SellingContractCreateDto createDto,
            [Service] ISellingContractService service)
        {
            return await service.CreateSellingContract(createDto);
        }

        public async Task<BulkOperationResult> CreateRentalContracts(
            IEnumerable<SellingContractCreateDto> createDtos,
            [Service] ISellingContractService service)
        {
            if (createDtos is null || !createDtos.Any())
                throw new GraphQLException("You must provide at least one selling contract.");
            if (createDtos.Count() > MaxBulkOperationSize)
                throw new GraphQLException($"You cannot create more than {MaxBulkOperationSize} selling contracts at once.");
            return await service.CreateSellingContracts(createDtos);
        }

        public async Task<SellingContractDto> UpdateSellingContract(
            Guid id,
            SellingContractUpdateDto updateDto,
            [Service] ISellingContractService service)
        {
            if (!await service.SellingContractExists(id))
                throw new GraphQLException($"Selling contract {id} not found");
            try
            {
                await service.UpdateSellingContract(id, updateDto);
                return await service.GetSellingContractById(id, null);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new GraphQLException(
                    ErrorBuilder.New()
                        .SetMessage("The record was modified by another user.")
                        .SetCode("CONCURRENCY_CONFLICT")
                        .Build());
            }
        }

        public async Task<bool> DeleteSellingContractById(
            Guid id,
            [Service] ISellingContractService service)
        {
            if (!await service.SellingContractExists(id))
                throw new GraphQLException($"Selling contract {id} not found");
            await service.SoftDeleteSellingContract(id);

            return true;
        }

        public async Task<BulkOperationResult> DeleteSellingContractsByIds(
            IEnumerable<Guid> ids,
            [Service] ISellingContractService service)
        {
            if (ids is null || !ids.Any())
                throw new GraphQLException("You must provide at least one selling contract ID.");
            return await service.SoftDeleteSellingContracts(ids);
        }

        public async Task<bool> RestoreSellingContractById(
            Guid id,
            [Service] ISellingContractService service)
        {
            if (!await service.SellingContractExists(id))
                throw new GraphQLException($"Selling contract {id} not found");
            await service.RestoreSellingContract(id);
            return true;
        }

        public async Task<BulkOperationResult> RestoreSellingContractsByIds(
            IEnumerable<Guid> ids,
            [Service] ISellingContractService service)
        {
            if (ids is null || !ids.Any())
                throw new GraphQLException("You must provide at least one selling contract ID.");
            return await service.RestoreSellingContracts(ids);
        }
    }
}
