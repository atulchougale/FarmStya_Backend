namespace FarmStay.Application.Interfaces.Services.Booking;

public record CheckoutRequest(string Method, decimal Discount, string? Reason);
public record CheckoutBalance(decimal Total, decimal AdvancePaid, decimal Discount, decimal Payable, string Currency, string? PaymentStatus);
public record StayResult(int Status, string Message, object? Data = null);

public interface IBookingStayOperations
{
    Task<StayResult> ExecuteAsync(int tenant, long bookingId, int actor, string action,
        CheckoutRequest? request, CancellationToken ct);
}
