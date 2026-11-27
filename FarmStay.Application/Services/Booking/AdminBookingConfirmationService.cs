using FarmStay.Application.Interfaces.Services.Booking;
using FarmStay.Application.Interfaces.Repositories;
namespace FarmStay.Application.Services.Booking;
public sealed class AdminBookingConfirmationService(IBookingActorContext access,IAdminBookingConfirmationRepository repository):IAdminBookingConfirmation
{
 public async Task<AdminConfirmationResult> CollectCashAsync(int farmhouse,long bookingId,int actor,AdminAdvanceRequest request,CancellationToken ct)
 {await access.AdministrationAsync("Bookings.Confirm",farmhouse,actor,ct);return await repository.CollectCashAsync(farmhouse,bookingId,actor,request,ct);}
 public async Task<AdminConfirmationResult> SendPaymentReminderAsync(int farmhouse,long bookingId,int actor,CancellationToken ct)
 {await access.AdministrationAsync("Bookings.Confirm",farmhouse,actor,ct);return await repository.SendPaymentReminderAsync(farmhouse,bookingId,actor,ct);}
}
