using FarmStay.Application.Interfaces.Common;
using FarmStay.Application.Interfaces.Services.Booking;
namespace FarmStay.Application.Services.Booking;
public sealed class BookingActorContext(ICurrentTenantContext tenant):IBookingActorContext
{
 public async Task<VerifiedTenant> CustomerAsync(int? farmhouse=null,int? actor=null,CancellationToken ct=default)
 {
  var scope=await tenant.RequireAsync(ct);Check(scope,farmhouse,actor);
  if(scope.Role!="Customer")throw new TenantAccessException(403,"An active customer membership is required.");
  return scope;
 }
 public async Task<VerifiedTenant> AdministrationAsync(string permission,int? farmhouse=null,int? actor=null,CancellationToken ct=default)
 {
  var scope=await tenant.RequireAsync(ct);Check(scope,farmhouse,actor);
  if(scope.Role is not ("Admin" or "Farm Owner" or "FarmOwner" or "Super Admin"))throw new TenantAccessException(403,"An active administration membership is required.");
  await tenant.DemandAsync(permission);return scope;
 }
 private static void Check(VerifiedTenant scope,int? farm,int? actor)
 {
  if((farm.HasValue&&scope.FarmHouseId!=farm)||(actor.HasValue&&scope.UserId!=actor))
   throw new TenantAccessException(403,"Booking scope conflicts with your authenticated membership.");
 }
}
