using System.Data;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FarmStay.Application.Configurations;
using FarmStay.Application.Interfaces.Services.Booking;
using FarmStay.Domain.Entities;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BookingEntity = FarmStay.Domain.Entities.Booking;

namespace FarmStay.Infrastructure.Repositories.Booking;

public sealed class BookingStayOperations(AppDbContext db, IBookingNotifications notifications,
    IHttpClientFactory clients, IOptions<RazorpaySettings> options, ILogger<BookingStayOperations> logger)
    : IBookingStayOperations
{
    public async Task<StayResult> ExecuteAsync(int tenant, long bookingId, int actor, string action,
        CheckoutRequest? request, CancellationToken ct)
    {
        var customer = action is "customer-balance" or "order" or "verify";
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            DECLARE @result int;
            EXEC @result = sys.sp_getapplock @Resource = {$"FarmStay:Checkout:{bookingId}"},
                @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 15000;
            IF @result < 0 THROW 50014, 'Booking operations are busy.', 1;
            """, ct);
        var booking = await db.Bookings.SingleOrDefaultAsync(b => b.BookingId == bookingId &&
            b.FarmHouseId == tenant && !b.IsDelete && (!customer || b.CustomerUserId == actor), ct);
        if (booking == null) return new(404, "Booking not found.");
        if (action == "check-in")
        {
            if (booking.Status == "CheckedIn") return new(200, "Checked in.");
            var now = DateTime.Now;
            if (booking.Status != "Confirmed" || now.Date < booking.CheckInDate.Date || now.Date > booking.CheckOutDate.Date)
                return new(409, "Check-in is available from the scheduled arrival through the scheduled departure date.");
            booking.ActualCheckInAt = now;
            ChangeStatus(booking, actor, "CheckedIn", false);
            await SaveAsync();
            return new(200, "Checked in.");
        }

        var payments = db.PaymentTransactions.Where(p => p.FarmHouseId == tenant && p.BookingId == bookingId && !p.IsDelete);
        var advance = await payments.Where(p => p.PaymentType == "Advance" && p.PaymentStatus == "Success")
            .SumAsync(p => p.Amount, ct);
        var payment = await payments.SingleOrDefaultAsync(p => p.IdempotencyKey == $"checkout:{bookingId}", ct);
        var discount = await db.CheckoutDiscounts.Where(d => d.BookingId == bookingId && d.FarmHouseId == tenant && !d.IsDelete)
            .SumAsync(d => d.Amount, ct);
        var currency = booking.CurrencyCode ?? "INR";
        var remaining = Math.Max(0, booking.TotalAmount - advance);
        var balance = new CheckoutBalance(booking.TotalAmount, advance, discount,
            payment?.Amount ?? Math.Max(0, remaining - discount), currency, payment?.PaymentStatus);
        if (action is "balance" or "customer-balance") return new(200, "Checkout balance.", balance);
        if (booking.Status == "CheckedOut" && payment?.PaymentStatus == "Success")
            return new(200, "Checkout complete.");
        if (booking.Status != "CheckedIn") return new(409, "The booking must be checked in first.");

        if (request != null)
        {
            if (payment != null) return new(409, "A checkout payment has already been prepared.");
            var reason = request.Reason?.Trim();
            if (request.Method is not ("Cash" or "Online") || request.Discount < 0 ||
                request.Discount > remaining || decimal.Round(request.Discount, 2) != request.Discount ||
                (request.Discount > 0 && (string.IsNullOrEmpty(reason) || reason.Length > 1000)))
                return new(400, "Enter a valid payment method and discount with a reason.");
            var payable = remaining - request.Discount;
            if (request.Method == "Online" && payable > 0 &&
                (!options.Value.Enabled || currency != "INR" || payable < 1))
                return new(400, "Online test checkout requires Razorpay and an INR balance of at least ₹1.");
            if (request.Discount > 0)
                db.CheckoutDiscounts.Add(new CheckoutDiscount
                { FarmHouseId = tenant, BookingId = bookingId, Amount = request.Discount, Reason = reason!, EnteredBy = actor });

            payment = new PaymentTransaction
            {
                FarmHouseId = tenant, BookingId = bookingId, PaymentType = "Checkout", PaymentMethod = request.Method,
                Amount = payable, CurrencyCode = currency, IdempotencyKey = $"checkout:{bookingId}", CreatedBy = actor,
                PaymentProvider = request.Method == "Online" && payable > 0 ? "RazorpayTest" : null
            };
            db.PaymentTransactions.Add(payment);
            if (request.Method == "Online" && payable > 0)
            {
                await notifications.StageAsync(booking, BookingNotificationEvent.CheckoutPaymentRequested, cancellationToken: ct);
                await SaveAsync();
                return new(200, "Online payment requested.");
            }
            await CompleteAsync(booking, payment, actor, false, ct);
            await SaveAsync();
            return new(200, "Checkout complete.");
        }

        if (payment is not { PaymentMethod: "Online", PaymentStatus: "Pending", PaymentProvider: "RazorpayTest" } ||
            !options.Value.Enabled)
            return new(409, "No online checkout payment is available.");
        using var client = clients.CreateClient(RazorpayAdvancePaymentOrders.ClientName);
        var settings = options.Value;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{settings.KeyId}:{settings.KeySecret}")));
        var paise = decimal.ToInt64(payment.Amount * 100);
        try
        {
            if (action == "order")
            {
                if (payment.ProviderOrderId == null)
                {
                    var receipt = $"fs-c-{tenant}-{bookingId}";
                    payment.ProviderOrderId = await RazorpayAdvancePaymentOrders.FindOrCreateAsync(client, receipt, paise, currency, ct);
                    await SaveAsync();
                }
                return new(200, "Checkout order ready.", new AdvancePaymentOrder(
                    payment.PaymentTransactionId, payment.ProviderOrderId, paise, currency, settings.KeyId));
            }

            if (action != "verify" || payment.ProviderOrderId is not { } orderId)
                return new(409, "No checkout order is available to verify.");
            using var response = await client.GetAsync($"orders/{Uri.EscapeDataString(orderId)}/payments", ct);
            response.EnsureSuccessStatusCode();
            using var data = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
            var captured = data.RootElement.GetProperty("items").EnumerateArray().Where(p =>
                p.GetProperty("captured").GetBoolean() && p.GetProperty("status").GetString() == "captured" &&
                p.GetProperty("order_id").GetString() == orderId && p.GetProperty("currency").GetString() == currency &&
                p.GetProperty("amount").GetInt64() == paise && p.GetProperty("amount_refunded").GetInt64() == 0)
                .Select(p => p.GetProperty("id").GetString()).SingleOrDefault();
            if (captured is null || !captured.StartsWith("pay_", StringComparison.Ordinal) || captured.Length > 150)
                return new(409, "No captured checkout payment was found.");
            payment.ProviderPaymentId = captured;
        }
        catch (Exception error) when (error is HttpRequestException or JsonException or InvalidOperationException
            or KeyNotFoundException or FormatException || (error is OperationCanceledException && !ct.IsCancellationRequested))
        {
            logger.LogWarning("Checkout gateway operation {Action} failed for booking {BookingId}: {ErrorType}.",
                action, bookingId, error.GetType().Name);
            return new(502, "Checkout payment could not be checked. Try again.");
        }
        await CompleteAsync(booking, payment, actor, true, ct);
        await SaveAsync();
        return new(200, "Checkout complete.");

        async Task SaveAsync()
        {
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            logger.LogInformation("Stay operation {Action} committed for booking {BookingId}, actor {ActorId}.", action, bookingId, actor);
        }
    }

    private async Task CompleteAsync(BookingEntity booking, PaymentTransaction payment, int actor, bool customer, CancellationToken ct)
    {
        payment.PaymentStatus = "Success";
        payment.PaidAt = DateTime.Now;
        payment.VerifiedAt = payment.PaidAt;
        booking.CheckoutPaidAt = payment.PaidAt;
        booking.ActualCheckOutAt = payment.PaidAt;
        ChangeStatus(booking, actor, "CheckedOut", customer);
        db.BookingReceipts.Add(new BookingReceipt
        {
            FarmHouseId = booking.FarmHouseId, BookingId = booking.BookingId, PaymentTransaction = payment,
            ReceiptNumber = $"CO-{booking.BookingId}", Amount = payment.Amount, CurrencyCode = payment.CurrencyCode, CreatedBy = actor
        });
        await notifications.StageAsync(booking, BookingNotificationEvent.CheckedOut, cancellationToken: ct);
    }

    private void ChangeStatus(BookingEntity booking, int actor, string status, bool customer)
    {
        db.BookingStatusHistories.Add(new BookingStatusHistory
        {
            FarmHouseId = booking.FarmHouseId, BookingId = booking.BookingId, OldStatus = booking.Status,
            NewStatus = status, ActorUserId = actor, ActorType = customer ? "Customer" : "Administration"
        });
        booking.Status = status;
        booking.ModifiedBy = actor;
        booking.ModifiedDate = DateTime.Now;
    }
}
