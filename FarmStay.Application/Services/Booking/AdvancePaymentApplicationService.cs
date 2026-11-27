using FarmStay.Application.Interfaces.Services.Booking;
using FarmStay.Application.Interfaces.Repositories;
namespace FarmStay.Application.Services.Booking;
public sealed class AdvancePaymentApplicationService(IBookingActorContext access,IAdvancePaymentRepository repository):IAdvancePaymentOrders
{
 public async Task<AdvanceOrderResult> CreateAsync(int farmhouse,long bookingId,int customer,CancellationToken ct)
 {await access.CustomerAsync(farmhouse,customer,ct);return await repository.CreateAsync(farmhouse,bookingId,customer,ct);}
 public async Task<(int StatusCode,string Message)> VerifyAsync(int farmhouse,long bookingId,int customer,CancellationToken ct)
 {await access.CustomerAsync(farmhouse,customer,ct);return await repository.VerifyAsync(farmhouse,bookingId,customer,ct);}
}
