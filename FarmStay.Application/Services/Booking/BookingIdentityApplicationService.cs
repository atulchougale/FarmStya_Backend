using FarmStay.Application.Interfaces.Common;
using FarmStay.Application.Interfaces.Services.Booking;
using FarmStay.Application.Interfaces.Repositories;
namespace FarmStay.Application.Services.Booking;
public sealed class BookingIdentityApplicationService(IBookingActorContext access,ICurrentTenantContext tenant,IBookingIdentityRepository repository):IBookingIdentityDocuments
{
 public async Task<BookingIdentityDocument?> GetAsync(int farm,long booking,int? owner,CancellationToken ct)
 {if(owner.HasValue)await access.CustomerAsync(farm,owner,ct);else await access.AdministrationAsync("Bookings.View",farm,null,ct);return await repository.GetAsync(farm,booking,owner,ct);}
 public async Task<(Stream Content,string ContentType)?> OpenAsync(int farm,long booking,long document,CancellationToken ct)
 {
  var scope=await tenant.RequireAsync(ct);
  var identity=await GetAsync(farm,booking,scope.Role=="Customer"?scope.UserId:null,ct);
  if(identity==null||!identity.Files.Any(file=>file.Id==document))return null;
  return await repository.OpenAsync(farm,booking,document,ct);
 }
 public async Task<BookingIdentityDocument?> UploadAsync(int farm,long booking,int owner,string type,string? other,IReadOnlyList<IdentityProofUpload> files,CancellationToken ct)
 {await access.CustomerAsync(farm,owner,ct);return await repository.UploadAsync(farm,booking,owner,type,other,files,ct);}
 public async Task<BookingIdentityDocument?> UploadForCustomerAsync(int farm,long booking,int actor,string type,string? other,IReadOnlyList<IdentityProofUpload> files,CancellationToken ct)
 {await access.AdministrationAsync("Bookings.Confirm",farm,actor,ct);return await repository.UploadForCustomerAsync(farm,booking,actor,type,other,files,ct);}
 public async Task<bool> ReviewAsync(int farm,long booking,long document,int reviewer,string? reason,CancellationToken ct)
 {await access.AdministrationAsync("Bookings.Decide",farm,reviewer,ct);return await repository.ReviewAsync(farm,booking,document,reviewer,reason,ct);}
}
