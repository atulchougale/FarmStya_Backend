namespace FarmStay.Application.Interfaces.Services.Booking;

public record PaymentReceiptSummary(long ReceiptId, string ReceiptNumber, string PaymentType, decimal Amount, DateTime IssuedAt);
public record ReceiptPdf(byte[] Content, string FileName);
public interface IBookingReceipts
{
    Task<IReadOnlyList<PaymentReceiptSummary>> ListAsync(int farmHouseId, long bookingId, int? customerId, CancellationToken ct);
    Task<ReceiptPdf?> RenderAsync(int farmHouseId, long bookingId, long receiptId, int? customerId, CancellationToken ct);
}
