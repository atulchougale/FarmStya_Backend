using FluentValidation;

namespace FarmStay.Application.DTOs.Booking;

public record SubmitBookingRequest(Guid SelectionId, long[] ReservationIds, string FullName,
    string MobileNumber, string Email, DateOnly CheckInDate, DateOnly CheckOutDate,
    string BookingType, DateOnly[] EventDates, int AdultCount, int ChildrenCount,
    int PersonCount, string? CustomerNotes, Guid IdempotencyKey, string PricingVersion);

public record SubmitBookingResponse(long BookingId, string BookingReference, string Status,
    DateOnly CheckInDate, DateOnly CheckOutDate, string BookingType, int StayDays,
    decimal TotalAmount, decimal AdvanceAmount, string CurrencyCode, DateTime OwnerDeadline);

public sealed class SubmitBookingValidator : AbstractValidator<SubmitBookingRequest>
{
    public SubmitBookingValidator()
    {
        RuleFor(x => x.SelectionId).NotEmpty();
        RuleFor(x => x.IdempotencyKey).NotEmpty();
        RuleFor(x => x.ReservationIds).NotEmpty().Must(ids =>
            ids != null && ids.All(id => id > 0) && ids.Distinct().Count() == ids.Length);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.MobileNumber).NotEmpty().MaximumLength(30).Matches(@"^\+?[0-9 ()-]{7,29}$");
        RuleFor(x => x.Email).NotEmpty().MaximumLength(255).EmailAddress();
        RuleFor(x => x.CheckInDate).NotEmpty();
        RuleFor(x => x.CheckOutDate).GreaterThan(x => x.CheckInDate);
        RuleFor(x => x.AdultCount).GreaterThan(0);
        RuleFor(x => x.ChildrenCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PersonCount).Must((x, count) => count == (long)x.AdultCount + x.ChildrenCount);
        RuleFor(x => x.CustomerNotes).MaximumLength(500);
        RuleFor(x => x.PricingVersion).NotEmpty().Matches("^[A-Fa-f0-9]{64}$");
    }
}
