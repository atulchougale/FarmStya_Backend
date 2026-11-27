using FarmStay.Application.Interfaces.Services.Booking;
using FarmStay.Application.Interfaces.Repositories;
namespace FarmStay.Application.Services.Booking;
using FarmStay.Application.DTOs.Booking;
using FluentValidation;
public sealed class BookingSubmissionService(IBookingSubmissionRepository repository,IBookingActorContext access,IValidator<SubmitBookingRequest> validator):IBookingSubmission
{
 public async Task<IReadOnlyList<SubmitBookingResponse>> GetMineAsync(int farmhouse,int user,CancellationToken ct)
 {await access.CustomerAsync(farmhouse,user,ct);return await repository.GetMineAsync(farmhouse,user,ct);}
 public async Task<BookingSubmissionResult> SubmitAsync(int farmhouse,int user,SubmitBookingRequest request,CancellationToken ct)
 {await access.CustomerAsync(farmhouse,user,ct);return await Submit(request,()=>repository.SubmitAsync(farmhouse,user,request,ct),ct);}
 public async Task<BookingSubmissionResult> SubmitForCustomerAsync(int farmhouse,int customer,int actor,string source,SubmitBookingRequest request,CancellationToken ct)
 {
  await access.AdministrationAsync("Bookings.Create",farmhouse,actor,ct);
  if(customer<=0||source is not ("WalkIn" or "Phone"))return new(400,"Select a customer and valid assisted booking source.","INVALID_BOOKING_SOURCE");
  return await Submit(request,()=>repository.SubmitForCustomerAsync(farmhouse,customer,actor,source,request,ct),ct);
 }
 private async Task<BookingSubmissionResult> Submit(SubmitBookingRequest request,Func<Task<BookingSubmissionResult>> operation,CancellationToken ct)
 {
  var result=await validator.ValidateAsync(request,ct);
  if(!result.IsValid)return new(422,"Check the booking details.","INVALID_BOOKING");
  return await operation();
 }
}
