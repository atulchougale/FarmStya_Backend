namespace FarmStay.Application.Interfaces.Services.Booking;

public sealed record AdvancePaymentOrder(long PaymentTransactionId, string OrderId,
    long AmountInPaise, string Currency, string KeyId);

public sealed record AdvanceOrderResult(int StatusCode, string Message, AdvancePaymentOrder? Order = null);

public interface IAdvancePaymentOrders
{
    Task<(int StatusCode, string Message)> VerifyAsync(int farmHouseId, long bookingId, int customerId, CancellationToken ct);
    Task<AdvanceOrderResult> CreateAsync(int farmHouseId, long bookingId, int customerId, CancellationToken ct);
}
