using Application.BulkOperations;
using Application.Interface.Services;
using Application.Models.RentalContractModels.Read;
using Application.Models.RentalContractModels.Write;
using Microsoft.EntityFrameworkCore;

namespace API.GraphQL.RentalContract.Mutations
{
    [ExtendObjectType(Name = "Mutation")]
    public class RentalContractMutations
    {
        private const int MaxBulkOperationSize = 500;
        public async Task<RentalContractDto> CreateRentalContract(
            RentalContractCreateDto createDto,
            [Service] IRentalContractService service
            )
        {
            return await service.CreateRentalContract(createDto);
        }

        public async Task<BulkOperationResult> CreateRentalContracts(
            IEnumerable<RentalContractCreateDto> createDtos,
            [Service] IRentalContractService service
            )
        {
            if (createDtos is null || !createDtos.Any())
                throw new GraphQLException("You must provide at least one rental contract.");
            return await service.CreateRentalContracts(createDtos);
        }

        public async Task<RentalContractDto> UpdateRentalContract(
            Guid id,
            RentalContractUpdateDto updateDto,
            [Service] IRentalContractService service
            )
        {
            if (!await service.RentalContractExists(id))
                throw new GraphQLException($"Rental contract {id} not found");

            try
            {
                await service.UpdateRentalContract(id, updateDto);
                return await service.GetRentalContractById(id, null);
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

        public async Task<bool> DeleteRentalContractById(
            Guid id,
            [Service] IRentalContractService service
            )
        {
            if (!await service.RentalContractExists(id))
                throw new GraphQLException($"Rental contract {id} not found");

            await service.SoftDeleteRentalContract(id);
            return true;
        }

        public async Task<BulkOperationResult> DeleteRentalContracts(
            IEnumerable<Guid> ids,
            [Service] IRentalContractService service
            )
        {
            if (ids is null || !ids.Any())
                throw new GraphQLException("You must provide at least one rental contract ID.");

            if (ids.Count() > MaxBulkOperationSize)
                throw new GraphQLException("Bulk delete limit exceeded");

            return await service.DeleteRentalContracts(ids);
        }

        public async Task<bool> RestoreRentalContract(
            Guid id,
            [Service] IRentalContractService service
            )
        {
            var exists = await service.RentalContractExists(id);
            if (exists)
                throw new GraphQLException("Only deleted rental contracts can be restored");

            await service.RestoreRentalContract(id);
            return true;
        }

        public async Task<BulkOperationResult> RestoreRentalContracts(
            IEnumerable<Guid> ids,
            [Service] IRentalContractService service
            )
        {
            if (ids is null || !ids.Any())
                throw new GraphQLException("You must provide at least one rental contract ID.");

            if (ids.Count() > MaxBulkOperationSize)
                throw new GraphQLException("Bulk restore limit exceeded");

            return await service.RestoreRentalContracts(ids);
        }

        public async Task<bool> ActivateRentalContract(
            Guid id,
            [Service] IRentalContractService service)
        {
            await service.ActiveRentalContract(id);
            return true;
        }

        public async Task<bool> CancelRentalContract(
            Guid id,
            [Service] IRentalContractService service)
        {
            await service.CancelRentalContract(id);
            return true;
        }

        public async Task<bool> SuspendRentalContract(
            Guid id,
            [Service] IRentalContractService service)
        {
            await service.SuspendRentalContract(id);
            return true;
        }

        public async Task<bool> ResumeRentalContract(
            Guid id,
            [Service] IRentalContractService service)
        {
            await service.ResumeRentalContract(id);
            return true;
        }

        public async Task<bool> FinishRentalContract(
            Guid id,
            [Service] IRentalContractService service)
        {
            await service.FinishRentalContract(id);
            return true;
        }

        public async Task<bool> FinishExpiredContracts(
            [Service] IRentalContractService service)
        {
            await service.FinishExpiredContracts();
            return true;
        }

    }
}
