using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FarmStay.Application.Configurations;
using FarmStay.Application.Interfaces.Services.Booking;
using FarmStay.Domain.Entities;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FarmStay.Infrastructure.Repositories.Booking;

public sealed class RazorpayAdvancePaymentOrders(AppDbContext db, IHttpClientFactory clients,
    IOptions<RazorpaySettings> configuration, ILogger<RazorpayAdvancePaymentOrders> logger,
    IBookingNotifications notifications) : IAdvancePaymentOrders
{
    public const string ClientName = "Razorpay";
    private const string Provider = "RazorpayTest";

    public async Task<(int StatusCode, string Message)> VerifyAsync(int farmHouseId, long bookingId,
        int customerId, CancellationToken ct)
    {
        var settings = configuration.Value;
        if (!settings.Enabled) return (503, "Test payments are disabled.");
        var bookings = db.Bookings.Where(b => b.BookingId == bookingId && b.FarmHouseId == farmHouseId &&
            b.CustomerUserId == customerId && !b.IsDelete);
        var booking = await bookings.AsNoTracking().SingleOrDefaultAsync(ct);
        if (booking == null) return (404, "Booking not found.");
        var payment = await db.PaymentTransactions.SingleOrDefaultAsync(p => p.BookingId == bookingId &&
            p.FarmHouseId == farmHouseId && p.PaymentType == "Advance" && p.PaymentProvider == Provider &&
            p.IdempotencyKey == $"razorpay-test-advance:{bookingId}" && !p.IsDelete, ct);
        if (payment?.PaymentStatus == "Success" && booking.Status == "Confirmed")
            return (200, "Booking confirmed.");
        if (payment?.ProviderOrderId is not { Length: > 0 } orderId || payment.PaymentStatus != "Pending")
            return (404, "No pending test order was found.");
        if (booking.Status != "Approved" || !(booking.CustomerDeadline > DateTime.Now))
            return (409, "The confirmation window has closed. Contact support if you already paid.");
        if (payment.Amount != booking.AdvanceAmount || payment.CurrencyCode != (booking.CurrencyCode ?? "INR"))
            return (409, "The saved order does not match the booking advance.");

        string? paymentId;
        try
        {
            using var client = clients.CreateClient(ClientName);
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{settings.KeyId}:{settings.KeySecret}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            using var response = await client.GetAsync($"orders/{Uri.EscapeDataString(orderId)}/payments", ct);
            response.EnsureSuccessStatusCode();
            using var result = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
            var paise = decimal.ToInt64(payment.Amount * 100m);
            paymentId = result.RootElement.GetProperty("items").EnumerateArray()
                .Where(p => p.GetProperty("status").GetString() == "captured" &&
                    p.GetProperty("captured").GetBoolean() && p.GetProperty("order_id").GetString() == orderId &&
                    p.GetProperty("amount").GetInt64() == paise &&
                    p.GetProperty("currency").GetString() == payment.CurrencyCode &&
                    p.GetProperty("amount_refunded").GetInt64() == 0)
                .Select(p => p.GetProperty("id").GetString()).SingleOrDefault();
        }
        catch (Exception error) when (error is HttpRequestException or JsonException or InvalidOperationException
            or KeyNotFoundException or FormatException || (error is OperationCanceledException && !ct.IsCancellationRequested))
        {
            logger.LogWarning("Razorpay verification unavailable for booking {BookingId}: {ErrorType}.",
                bookingId, error.GetType().Name);
            return (502, "Payment could not be checked. Retry verification before paying again.");
        }
        if (paymentId == null || !paymentId.StartsWith("pay_", StringComparison.Ordinal) || paymentId.Length > 150)
            return (409, "No captured test payment was found yet.");

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var now = DateTime.Now;
        var updated = await bookings.Where(b => b.Status == "Approved" && b.CustomerDeadline > now &&
                db.BookingDocuments.Any(d => d.BookingId == bookingId && d.FarmHouseId == farmHouseId &&
                    d.UserId == customerId && !d.IsDelete && d.DocumentType == "IdentityProof" &&
                    (d.VerificationStatus == "Uploaded" || d.VerificationStatus == "Verified")))
            .ExecuteUpdateAsync(set => set.SetProperty(b => b.Status, "Confirmed")
                .SetProperty(b => b.ModifiedBy, customerId).SetProperty(b => b.ModifiedDate, now), ct);
        if (updated == 0)
            return await bookings.AnyAsync(b => b.Status == "Confirmed", ct)
                ? (200, "Booking confirmed.")
                : (409, "Captured payment could not confirm this booking. Contact support.");

        payment.PaymentStatus = "Success";
        payment.PaidAt = now;
        db.BookingReceipts.Add(new BookingReceipt
        {
            FarmHouseId = farmHouseId, BookingId = bookingId, PaymentTransactionId = payment.PaymentTransactionId,
            ReceiptNumber = $"AD-{farmHouseId}-{payment.PaymentTransactionId}", Amount = payment.Amount,
            CurrencyCode = payment.CurrencyCode, IssuedAt = now, CreatedBy = customerId
        });
        payment.ProviderPaymentId = paymentId;
        payment.VerifiedAt = now;
        payment.ModifiedBy = customerId;
        payment.ModifiedDate = now;
        db.BookingStatusHistories.Add(new BookingStatusHistory
        {
            BookingId = bookingId, FarmHouseId = farmHouseId, ActorUserId = customerId,
            ActorType = "Customer", OldStatus = "Approved", NewStatus = "Confirmed", CreatedDate = now
        });
        var confirmed = await bookings.AsNoTracking().SingleAsync(ct);
        await notifications.StageAsync(confirmed, BookingNotificationEvent.Confirmed, cancellationToken: ct);
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        logger.LogInformation("Captured Razorpay test advance verified for booking {BookingId}, transaction {PaymentTransactionId}.",
            bookingId, payment.PaymentTransactionId);
        return (200, "Booking confirmed.");
    }

    public async Task<AdvanceOrderResult> CreateAsync(int farmHouseId, long bookingId, int customerId,
        CancellationToken ct)
    {
        var settings = configuration.Value;
        if (!settings.Enabled)
            return new(503, "Test payments are disabled.");

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            DECLARE @result int;
            EXEC @result = sys.sp_getapplock @Resource = {$"FarmStay:AdvanceOrder:{bookingId}"},
                @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 15000;
            IF @result < 0 THROW 50013, 'Advance order lock unavailable.', 1;
            """, ct);

        var booking = await db.Bookings.AsNoTracking().SingleOrDefaultAsync(b =>
            b.BookingId == bookingId && b.FarmHouseId == farmHouseId && b.CustomerUserId == customerId && !b.IsDelete, ct);
        if (booking == null)
            return new(404, "Booking not found.");
        if (booking.Status != "Approved" || booking.CustomerDeadline <= DateTime.Now ||
            !booking.CustomerDeadline.HasValue)
            return new(409, "This booking is outside its advance payment window.");

        var hasIdentity = await db.BookingDocuments.AnyAsync(d => d.FarmHouseId == farmHouseId &&
            d.BookingId == bookingId && d.UserId == customerId && !d.IsDelete && d.DocumentType == "IdentityProof" &&
            (d.VerificationStatus == "Uploaded" || d.VerificationStatus == "Verified"), ct);
        if (!hasIdentity)
            return new(409, "Upload an ID proof before paying the advance.");
        var currency = booking.CurrencyCode ?? "INR";
        if (currency != "INR" || booking.AdvanceAmount < 1m)
            return new(422, "Razorpay Test Mode requires an INR advance of at least ₹1.");
        var amount = decimal.ToInt64(booking.AdvanceAmount * 100m);

        var key = $"razorpay-test-advance:{bookingId}";
        var payments = db.PaymentTransactions.Where(p => p.FarmHouseId == farmHouseId &&
            p.BookingId == bookingId && p.PaymentType == "Advance" && !p.IsDelete);
        if (await payments.AnyAsync(p => p.PaymentStatus == "Success", ct))
            return new(409, "The advance has already been paid.");
        var existing = await payments.SingleOrDefaultAsync(p => p.IdempotencyKey == key, ct);
        if (existing != null)
        {
            if (existing.PaymentStatus != "Pending" || existing.Amount != booking.AdvanceAmount ||
                existing.CurrencyCode != currency || existing.ProviderOrderId is not { Length: > 0 } existingOrderId)
                return new(409, "The existing advance order needs reconciliation.");
            return new(200, "Existing test order returned.", new(existing.PaymentTransactionId, existingOrderId,
                amount, currency, settings.KeyId));
        }

        // Stable receipt permits recovery when Razorpay creates the order but the response or local commit is lost.
        // Preserve the receipt used by any Razorpay order created before the server-clock migration.
        var originalReceiptTicks = booking.CreatedDate.Ticks - TimeZoneInfo.Local.GetUtcOffset(booking.CreatedDate).Ticks;
        var receiptIdentity = FormattableString.Invariant(
            $"{farmHouseId}:{bookingId}:{originalReceiptTicks}:{booking.BookingReference}:{booking.SubmitIdempotencyKey:N}");
        var receipt = "fs-a-" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(receiptIdentity)))[..32];
        using var client = clients.CreateClient(ClientName);
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{settings.KeyId}:{settings.KeySecret}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        string orderId;
        try
        {
            orderId = await FindOrCreateAsync(client, receipt, amount, currency, ct);
        }
        catch (Exception error) when (error is HttpRequestException or JsonException or InvalidOperationException or KeyNotFoundException or FormatException ||
            (error is OperationCanceledException && !ct.IsCancellationRequested))
        {
            logger.LogWarning("Razorpay test order unavailable for booking {BookingId}. Type {ErrorType}, HTTP {StatusCode}.",
                bookingId, error.GetType().Name, (error as HttpRequestException)?.StatusCode);
            return new(502, "Could not prepare the Razorpay test order. Try again.");
        }

        var payment = new PaymentTransaction
        {
            FarmHouseId = farmHouseId, BookingId = bookingId, PaymentType = "Advance", PaymentMethod = "Online",
            Amount = booking.AdvanceAmount, CurrencyCode = currency, PaymentStatus = "Pending",
            PaymentProvider = Provider, ProviderOrderId = orderId, IdempotencyKey = key, CreatedBy = customerId
        };
        db.PaymentTransactions.Add(payment);
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        logger.LogInformation("Razorpay test order {OrderId} recorded as payment {PaymentTransactionId} for booking {BookingId}.",
            orderId, payment.PaymentTransactionId, bookingId);
        return new(201, "Razorpay test order ready.", new(payment.PaymentTransactionId, orderId, amount, currency, settings.KeyId));
    }

    internal static async Task<string> FindOrCreateAsync(HttpClient client, string receipt, long amount,
        string currency, CancellationToken ct)
    {
        using var lookup = await client.GetAsync($"orders?receipt={Uri.EscapeDataString(receipt)}", ct);
        lookup.EnsureSuccessStatusCode();
        using var orders = JsonDocument.Parse(await lookup.Content.ReadAsStringAsync(ct));
        var matching = orders.RootElement.GetProperty("items").EnumerateArray()
            .Where(item => item.GetProperty("receipt").GetString() == receipt).ToArray();
        if (matching.Length > 0)
            return ReadOrder(matching.Single(), receipt, amount, currency);

        using var response = await client.PostAsJsonAsync("orders", new { amount, currency, receipt }, ct);
        response.EnsureSuccessStatusCode();
        using var created = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return ReadOrder(created.RootElement, receipt, amount, currency);
    }

    private static string ReadOrder(JsonElement order, string receipt, long amount, string currency)
    {
        var id = order.GetProperty("id").GetString();
        if (id is null || id.Length > 150 || !id.StartsWith("order_", StringComparison.Ordinal) ||
            order.GetProperty("receipt").GetString() != receipt || order.GetProperty("amount").GetInt64() != amount ||
            order.GetProperty("currency").GetString() != currency || order.GetProperty("status").GetString() != "created")
            throw new InvalidOperationException("Unexpected Razorpay test order.");
        return id;
    }
}
