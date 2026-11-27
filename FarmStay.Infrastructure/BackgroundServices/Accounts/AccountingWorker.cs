using FarmStay.Application.Accounts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace FarmStay.Infrastructure.Accounts;
public sealed class AccountingWorker(IServiceScopeFactory scopes,ILogger<AccountingWorker> logger):BackgroundService
{
 protected override async Task ExecuteAsync(CancellationToken ct) {
  using var timer=new PeriodicTimer(TimeSpan.FromMinutes(1));
  do { try {using var scope=scopes.CreateScope();await scope.ServiceProvider.GetRequiredService<IAccountsRepository>().ProcessSourcesAsync(ct);}catch(OperationCanceledException) when(ct.IsCancellationRequested){break;}catch(Exception e){logger.LogError(e,"Accounts source synchronization failed; will retry next minute.");} }while(await timer.WaitForNextTickAsync(ct));
 }
}
