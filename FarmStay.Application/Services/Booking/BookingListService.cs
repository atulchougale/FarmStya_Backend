using FarmStay.Application.DTOs.Booking;
using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Application.Interfaces.Services.Booking;
using FarmStay.Application.Common.Constants;
using BookingEntity = FarmStay.Domain.Entities.Booking;

namespace FarmStay.Application.Services.Booking;

public sealed class BookingListService(IBookingListRepository bookings, IUserMembershipRepository memberships,
    IFarmHouseRepository farmHouses, IPermissionRepository permissions, IBookingActorContext access, IBookingIdentityDocuments documents)
    : IBookingListService
{
    public async Task<AdministrationBookingList> SearchAdministrationAsync(int farmHouseId, AdministrationBookingFilter filter, CancellationToken ct)
    {
        await access.AdministrationAsync("Bookings.View",farmHouseId,ct:ct);
        var result = await bookings.SearchAdministrationAsync(farmHouseId, filter, ct);
        return new(result.Items.Select(Map).ToArray(), result.TotalCount, result.Summary);
    }
    public async Task<bool> CanViewAsync(int userId, int farmHouseId, bool administration)
    {
        if (administration)
            return await HasAdministrationAccessAsync(userId, farmHouseId, BookingPermissions.View);

        var membership = await memberships.GetMembershipAsync(userId, farmHouseId);
        return membership?.User is { IsActive: true, IsDeleted: false } &&
            membership.FarmHouse is { IsActive: true, IsDeleted: false } &&
            membership.Role is { IsActive: true, IsDeleted: false } role &&
            role.RoleName == "Customer";
    }

    public Task<bool> CanDecideAsync(int userId, int farmHouseId) =>
        HasAdministrationAccessAsync(userId, farmHouseId, BookingPermissions.Decide);

    public Task<bool> CanConfirmAsync(int userId, int farmHouseId) =>
        HasAdministrationAccessAsync(userId, farmHouseId, "Bookings.Confirm");

    public async Task<bool> IsGlobalAdministratorAsync(int userId) =>
        (await memberships.GetByUserIdAsync(userId)).Any(m =>
            m.Role is { RoleName: "Super Admin", IsActive: true, IsDeleted: false } &&
            m.User is { IsActive: true, IsDeleted: false });

    private async Task<bool> HasAdministrationAccessAsync(int userId, int farmHouseId, string permission)
    {
        if (await farmHouses.GetByIdAsync(farmHouseId) is not { IsActive: true, IsDeleted: false })
            return false;
        if (await IsGlobalAdministratorAsync(userId))
            return true;

        var membership = await memberships.GetMembershipAsync(userId, farmHouseId);
        return membership?.User is { IsActive: true, IsDeleted: false } &&
            membership.Role is { IsActive: true, IsDeleted: false, RoleName: "Admin" or "Farm Owner" or "FarmOwner" } &&
            await permissions.HasPermissionAsync(userId, farmHouseId, permission);
    }

    public async Task<IReadOnlyList<BookingListResponseDto>> ListAsync(int farmHouseId, int? customerUserId,
        int page, int pageSize, string? status = null)
    {
        await ReadScope(farmHouseId,customerUserId);
        var records = customerUserId.HasValue
            ? await bookings.GetMyBookingsAsync(customerUserId.Value, farmHouseId, page, pageSize, status)
            : await bookings.GetAllBookingsAsync(farmHouseId, page, pageSize);
        return records.Select(Map).ToArray();
    }

    public async Task<BookingListResponseDto?> FindAsync(int farmHouseId, int? customerUserId, long bookingId)
    {
        var actor = await ReadScope(farmHouseId,customerUserId);
        var booking = await bookings.GetBookingByIdAsync(bookingId, farmHouseId, customerUserId);
        if (booking == null)
            return null;

        var result = Map(booking);
        var payments = await bookings.GetPaymentTotalsAsync(farmHouseId, bookingId);
        result.TotalPaid = payments.TotalPaid;
        result.RefundedAmount = payments.Refunded;
        result.AdvancePaid = payments.AdvancePaid;
        result.HasPendingAdvanceOrder = payments.PendingAdvance;
        result.DiscountAmount = payments.Discount;
        result.Discounts = await bookings.GetDiscountDetailsAsync(farmHouseId, bookingId);
        result.RemainingAmount = booking.Status=="CheckedOut" ? 0 : Math.Max(0, booking.TotalAmount - payments.Discount - payments.TotalPaid);
        var events = await bookings.GetEventDatesAsync(farmHouseId, [bookingId]);
        result.EventDates = events.Select(e => new BookingListResponseDto.EventDateDetails
        {
            BookingEventDateId = e.BookingEventDateId,
            EventDate = e.EventDate,
            EventType = e.EventType,
            DailyRateSnapshot = e.DailyRateSnapshot
        }).ToList();
        var history = await bookings.GetStatusHistoryAsync(farmHouseId, [bookingId]);
        result.StatusHistory = history.Select(h => new BookingListResponseDto.StatusHistoryDetails
        {
            OldStatus = h.OldStatus, NewStatus = h.NewStatus, Reason = h.Reason,
            ActorType = h.ActorType, CreatedDate = DateTime.SpecifyKind(h.CreatedDate, DateTimeKind.Local)
        }).ToList();
        if (!customerUserId.HasValue)
        {
            result.CanManageConfirmation = await CanConfirmAsync(actor.UserId, farmHouseId) &&
                ((result.Status == "Approved" && result.CustomerDeadline > DateTime.Now) || result.Status == "Confirmed");
            var identity = await documents.GetAsync(farmHouseId,bookingId,null,default);
            result.HasIdentityDocument = identity != null;
            result.IdentityDocumentType = identity?.DocumentType;
            result.OtherIdentityDocumentType = identity?.OtherDocumentType;
            result.IdentityFileCount = identity?.Files.Count ?? 0;
            var now = DateTime.Now;
            var authorized = await CanDecideAsync(actor.UserId,farmHouseId);
            result.CanDecide = authorized && result.Status == "Pending" && result.OwnerDeadline > now;
            result.CanCheckIn = authorized && result.Status == "Confirmed" && now.Date >= result.CheckInDate.Date && now.Date <= result.CheckOutDate.Date;
            result.CanCheckout = authorized && result.Status == "CheckedIn";
        }
        return result;
    }

    private async Task<FarmStay.Application.Interfaces.Common.VerifiedTenant> ReadScope(int farmhouse,int? customer)
    {
        return customer.HasValue ? await access.CustomerAsync(farmhouse,customer.Value)
            : await access.AdministrationAsync("Bookings.View",farmhouse);
    }

    private static BookingListResponseDto Map(BookingEntity b) => new()
    {
        BookingId = b.BookingId, BookingReference = b.BookingReference, FullName = b.FullName,
        Email = b.Email, BookingSource = b.BookingSource, CreatedBy = b.CreatedBy,
        MobileNumber = b.MobileNumber, AdultCount = b.AdultCount,
        ChildrenCount = b.ChildrenCount, PersonCount = b.PersonCount, CustomerNotes = b.CustomerNotes,
        CheckInDate = b.CheckInDate, CheckOutDate = b.CheckOutDate, StayDays = b.StayDays,
        ActualCheckInAt = b.ActualCheckInAt, ActualCheckOutAt = b.ActualCheckOutAt, CheckoutPaidAt = b.CheckoutPaidAt,
        BookingType = b.BookingType, Status = b.Status, CreatedDate = DateTime.SpecifyKind(b.CreatedDate, DateTimeKind.Local),
        BaseAmount = b.BaseAmount, EventAmount = b.EventAmount, TotalAmount = b.TotalAmount,
        AdvanceAmount = b.AdvanceAmount, AdvanceDailyRateSnapshot = b.AdvanceDailyRateSnapshot,
        BookingDailyRateSnapshot = b.BookingDailyRateSnapshot, EventDailyRateSnapshot = b.EventDailyRateSnapshot,
        CurrencyCode = b.CurrencyCode ?? "INR", RejectionReason = b.RejectionReason, CancellationReason = b.CancellationReason,
        OwnerDeadline = DateTime.SpecifyKind(b.OwnerDeadline, DateTimeKind.Local),
        CustomerDeadline = b.CustomerDeadline is { } deadline ? DateTime.SpecifyKind(deadline, DateTimeKind.Local) : null
    };
}
