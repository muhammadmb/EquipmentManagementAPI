using Application.Interface.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.BackgroundJobs
{
    public class ContractExpirationWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ContractExpirationWorker(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var contractService = scope.ServiceProvider
                    .GetRequiredService<IRentalContractService>();

                await contractService.FinishExpiredContracts();
                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }
    }
}