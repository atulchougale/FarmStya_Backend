using FarmStay.Application.Interfaces.Services.Booking;
using FarmStay.Application.Interfaces.Repositories;
namespace FarmStay.Application.Services.Booking;
public sealed class BookingStayApplicationService(IBookingActorContext access,IBookingStayRepository repository):IBookingStayOperations
{
 public async Task<StayResult> ExecuteAsync(int tenant,long bookingId,int actor,string action,CheckoutRequest? request,CancellationToken ct)
 {
  if(action is "customer-balance" or "order" or "verify") {
   await access.CustomerAsync(tenant,actor,ct);
   if(request!=null)return new(400,"Customer checkout amounts are determined by administration.");
  } else if(action is "balance" or "check-in" or "checkout") {
   await access.AdministrationAsync("Bookings.Decide",tenant,actor,ct);
   if(action=="checkout"&&request==null)return new(400,"Checkout payment details are required.");
   if(action!="checkout"&&request!=null)return new(400,"Unexpected checkout payment details.");
  } else return new(400,"Unknown booking operation.");
  return await repository.ExecuteAsync(tenant,bookingId,actor,action,request,ct);
 }
}
