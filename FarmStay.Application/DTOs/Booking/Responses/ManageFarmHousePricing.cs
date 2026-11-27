namespace FarmStay.Application.DTOs.Booking;

public record PricingRates(decimal BookingDaily, decimal NormalEventDaily, decimal DecorationEventDaily, decimal AdvanceDaily);

public record PricingEntryRequest(string PricingKey, string PricingName, decimal Price, bool IsActive);
public record PricingEntry(int PricingId, string PricingKey, string PricingName, decimal Price, string CurrencyCode, bool IsActive);
public record PricingChangeResult(int Status, string Message, PricingEntry? Entry = null);
