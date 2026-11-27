using FarmStay.Application.Interfaces.Common;
namespace FarmStay.Application.Interfaces.Services.Booking;
public interface IBookingActorContext
{
 Task<VerifiedTenant> CustomerAsync(int? farmhouse=null,int? actor=null,CancellationToken ct=default);
 Task<VerifiedTenant> AdministrationAsync(string permission,int? farmhouse=null,int? actor=null,CancellationToken ct=default);
}
