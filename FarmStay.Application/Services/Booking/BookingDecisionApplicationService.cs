using FarmStay.Application.Interfaces.Services.Booking;
using FarmStay.Application.Interfaces.Repositories;
namespace FarmStay.Application.Services.Booking;
public sealed class BookingDecisionApplicationService(IBookingActorContext actor,IBookingDecisionRepository repository):IBookingDecisionService
{
 public async Task<BookingDecisionOutcome> DecideAsync(int farmHouseId,long bookingId,int actorId,string? rejectionReason,CancellationToken ct)
 {
  await actor.AdministrationAsync("Bookings.Decide",farmHouseId,actorId,ct);
  if(rejectionReason is not null&&(string.IsNullOrWhiteSpace(rejectionReason)||rejectionReason.Trim().Length>1000))return BookingDecisionOutcome.Conflict;
  return await repository.DecideAsync(farmHouseId,bookingId,actorId,rejectionReason?.Trim(),ct);
 }
}
