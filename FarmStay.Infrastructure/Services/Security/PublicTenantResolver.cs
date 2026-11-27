using FarmStay.Application.Interfaces.Common;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace FarmStay.Infrastructure.Services.Security;
public sealed class PublicTenantResolver(ITenantResolver domain,AppDbContext db):IPublicTenantResolver
{
 public async Task<int> ResolveAsync(CancellationToken ct=default){var name=domain.GetCurrentDomain();if(string.IsNullOrWhiteSpace(name))throw new TenantAccessException(404,"Public site not found.");var farm=await db.FarmHouses.AsNoTracking().SingleOrDefaultAsync(x=>(x.DomainName==name||x.SubDomain==name)&&x.IsActive&&!x.IsDeleted&&x.IsWebsitePublished,ct);if(farm==null||(farm.SubscriptionPlanId.HasValue&&(!farm.IsSubscriptionActive||farm.SubscriptionEndDate<DateTime.Now)))throw new TenantAccessException(404,"Public site not found.");return farm.FarmHouseId;}
}
