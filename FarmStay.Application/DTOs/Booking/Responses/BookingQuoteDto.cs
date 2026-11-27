namespace FarmStay.Application.DTOs.Booking;

public record BookingQuoteRequest(
    DateOnly CheckInDate, DateOnly CheckOutDate, string BookingType, DateOnly[]? EventDates);

public record BookingQuoteResponse(
    int StayDays, int EventDays, decimal StayRate, decimal EventRate, decimal AdvanceRate,
    decimal StayAmount, decimal EventAmount, decimal TotalAmount, decimal AdvanceAmount,
    decimal RemainingAmount, string CurrencyCode, string PricingVersion);
