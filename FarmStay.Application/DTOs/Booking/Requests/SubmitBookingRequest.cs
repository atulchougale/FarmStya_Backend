
namespace FarmStay.Application.DTOs.Booking;

public record SubmitBookingRequest(Guid SelectionId, long[] ReservationIds, string FullName,
    string MobileNumber, string? Email, DateOnly CheckInDate, DateOnly CheckOutDate,
    string BookingType, DateOnly[] EventDates, int AdultCount, int ChildrenCount,
    int PersonCount, string? CustomerNotes, Guid IdempotencyKey, string PricingVersion);

public record SubmitBookingResponse(long BookingId, string BookingReference, string Status,
    DateOnly CheckInDate, DateOnly CheckOutDate, string BookingType, int StayDays,
    decimal TotalAmount, decimal AdvanceAmount, string CurrencyCode, DateTime OwnerDeadline);
