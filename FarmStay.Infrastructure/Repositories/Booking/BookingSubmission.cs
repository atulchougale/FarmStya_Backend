using System.Data;
using System.Security.Cryptography;
using System.Text.Json;
using FarmStay.Application.DTOs.Booking;
using FarmStay.Application.Interfaces.Services.Booking;
using FarmStay.Domain.Entities;
using FarmStay.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BookingEntity = FarmStay.Domain.Entities.Booking;

namespace FarmStay.Infrastructure.Repositories.Booking;

public sealed class BookingSubmission(AppDbContext db, IFarmHousePricingService pricing,
    IBookingNotifications notifications, ILogger<BookingSubmission> logger) : IBookingSubmission
{
    public async Task<IReadOnlyList<SubmitBookingResponse>> GetMineAsync(int farmHouseId, int userId,
        CancellationToken cancellationToken)
    {
        var bookings = await db.Bookings.AsNoTracking()
            .Where(b => b.FarmHouseId == farmHouseId && b.CustomerUserId == userId && !b.IsDelete &&
                db.UserMemberships.Any(m => m.FarmHouseId == farmHouseId && m.UserId == userId &&
                    m.IsActive && !m.IsDeleted && m.User!.IsActive && !m.User.IsDeleted &&
                    m.Role!.IsActive && !m.Role.IsDeleted && m.Role.RoleName == "Customer"))
            .OrderByDescending(b => b.CreatedDate).ThenByDescending(b => b.BookingId)
            .Take(50).ToListAsync(cancellationToken);
        return bookings.Select(ToResponse).ToArray();
    }

    public async Task<BookingSubmissionResult> SubmitAsync(int farmHouseId, int userId,
        SubmitBookingRequest request, CancellationToken cancellationToken)
    {
        var requestHash = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(request with
        {
            ReservationIds = request.ReservationIds.Order().ToArray(),
            EventDates = (request.EventDates ?? []).Order().ToArray()
        })));
        var existing = db.Bookings.AsNoTracking().Where(b => b.FarmHouseId == farmHouseId &&
            b.CustomerUserId == userId && b.SubmitIdempotencyKey == request.IdempotencyKey);
        var pendingHolds = db.BookingDateReservations.Where(r => r.FarmHouseId == farmHouseId &&
            r.UserId == userId && r.SelectionId == request.SelectionId);
        logger.LogInformation("Booking submit started for farmhouse {FarmHouseId}, customer {UserId}, selection {SelectionId}.",
            farmHouseId, userId, request.SelectionId);
        BookingEntity booking;
        var replay = false;

        try
        {
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            var referenceYear = DateTime.Now.Year;
            var referenceLock = $"FarmStay:BookingReference:{referenceYear}";
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                DECLARE @lockResult int;
                EXEC @lockResult = sys.sp_getapplock @Resource = {referenceLock},
                    @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 10000;
                IF @lockResult < 0 THROW 50010, 'Booking reference lock unavailable.', 1;
                """, cancellationToken);
            var authorized = await db.UserMemberships.AnyAsync(m => m.UserId == userId &&
                m.FarmHouseId == farmHouseId && m.IsActive && !m.IsDeleted &&
                m.User!.IsActive && !m.User.IsDeleted && m.Role!.IsActive && !m.Role.IsDeleted &&
                m.Role.RoleName == "Customer" && m.FarmHouse!.IsActive && !m.FarmHouse.IsDeleted, cancellationToken);
            var enabled = authorized && await db.farmHouseModules.AnyAsync(m => m.FarmHouseId == farmHouseId &&
                m.IsEnabled && !m.IsDeleted && m.Module!.IsActive && m.Module.ModuleName == "Bookings", cancellationToken);
            if (!enabled)
                return new(403, "Booking is unavailable for this farmhouse membership.", "BOOKING_FORBIDDEN");

            var previous = await existing.SingleOrDefaultAsync(cancellationToken);
            if (previous != null)
            {
                if (previous.SubmitRequestHash != requestHash)
                    return new(409, "This submission key was already used for another request.", "DUPLICATE_SUBMISSION");
                booking = previous;
                replay = true;
            }
            else
            {
                var start = request.CheckInDate.ToDateTime(TimeOnly.MinValue);
                var end = request.CheckOutDate.ToDateTime(TimeOnly.MinValue);
                var days = request.CheckOutDate.DayNumber - request.CheckInDate.DayNumber;
                var held = await pendingHolds.AsNoTracking().OrderBy(r => r.ReservationDate).ToListAsync(cancellationToken);
                if (held.Count == 0)
                {
                    if (await db.BookingDateReservations.AnyAsync(r => r.FarmHouseId == farmHouseId &&
                        r.SelectionId == request.SelectionId, cancellationToken))
                        return new(403, "The reservation is not owned by this customer.", "RESERVATION_NOT_OWNED");
                    return new(409, "Your date reservation has expired. Please select the dates again.", "HOLD_EXPIRED");
                }
                if (held.Count != days || !held.Select(r => r.ReservationId).Order().SequenceEqual(request.ReservationIds.Order()) ||
                    held.Where((r, index) => r.ReservationDate.Date != start.AddDays(index)).Any())
                    return new(422, "Reservation IDs must cover exactly the occupied stay dates.", "INVALID_SELECTION");
                if (held.Any(r => r.ExpiresAt <= DateTime.Now || r.CreatedDate.AddMinutes(30) <= DateTime.Now))
                    return new(409, "Your date reservation has expired. Please select the dates again.", "HOLD_EXPIRED");
                if (await db.BookingStayDates.AnyAsync(d => d.FarmHouseId == farmHouseId && !d.IsDelete &&
                    d.StayDate >= start && d.StayDate < end, cancellationToken))
                    return Unavailable();

                var eventDates = request.EventDates ?? [];
                if (request.BookingType is not ("BookingOnly" or "NormalEvent" or "DecorationEvent"))
                    return new(422, "Choose a supported booking type.", "INVALID_BOOKING_TYPE");
                if ((request.BookingType == "BookingOnly") != (eventDates.Length == 0) ||
                    eventDates.Distinct().Count() != eventDates.Length ||
                    eventDates.Any(d => d < request.CheckInDate || d >= request.CheckOutDate))
                    return new(422, "Choose valid event dates within the stay.", "INVALID_EVENT_DATE");
                var quote = await pricing.GetQuoteAsync(new(request.CheckInDate, request.CheckOutDate,
                    request.BookingType, eventDates));
                if (!quote.Success || quote.Data is not { } price ||
                    price.TotalAmount > 9999999999999999.99m || price.CurrencyCode.Length != 3)
                    return new(422, "Booking pricing is unavailable.", "PRICING_NOT_CONFIGURED");
                if (!string.Equals(price.PricingVersion, request.PricingVersion, StringComparison.OrdinalIgnoreCase))
                    return new(409, "Pricing changed. Review the updated quote before submitting again.", "PRICING_CHANGED", Quote: price);

                var now = DateTime.Now;
                if (held.Any(r => r.ExpiresAt <= now || r.CreatedDate.AddMinutes(30) <= now))
                    return new(409, "Your date reservation has expired. Please select the dates again.", "HOLD_EXPIRED");
                booking = new BookingEntity
                {
                    FarmHouseId = farmHouseId, CustomerUserId = userId,
                    BookingReference = await NextBookingReferenceAsync(referenceYear, cancellationToken),
                    FullName = request.FullName.Trim(), Email = request.Email.Trim(), MobileNumber = request.MobileNumber.Trim(),
                    CheckInDate = start, CheckOutDate = end, StayDays = days, BookingType = request.BookingType,
                    AdultCount = request.AdultCount, ChildrenCount = request.ChildrenCount, PersonCount = request.PersonCount,
                    CustomerNotes = request.CustomerNotes?.Trim(), BaseAmount = price.StayAmount, EventAmount = price.EventAmount,
                    TotalAmount = price.TotalAmount, AdvanceAmount = price.AdvanceAmount, CurrencyCode = price.CurrencyCode,
                    BookingDailyRateSnapshot = price.StayRate, AdvanceDailyRateSnapshot = price.AdvanceRate,
                    EventDailyRateSnapshot = eventDates.Length == 0 ? null : price.EventRate,
                    PricingVersion = price.PricingVersion, SubmitIdempotencyKey = request.IdempotencyKey,
                    SubmitRequestHash = requestHash, Status = "Pending",
                    OwnerDeadline = now.AddHours(48), CreatedBy = userId, CreatedDate = now
                };
                db.Bookings.Add(booking);
                db.BookingStayDates.AddRange(held.Select(r => new BookingStayDate
                {
                    Booking = booking, FarmHouseId = farmHouseId, StayDate = r.ReservationDate,
                    CreatedBy = userId, CreatedDate = now
                }));
                db.BookingEventDates.AddRange(eventDates.Select(d => new BookingEventDate
                {
                    Booking = booking, FarmHouseId = farmHouseId, EventDate = d.ToDateTime(TimeOnly.MinValue),
                    DailyRateSnapshot = price.EventRate, EventType = request.BookingType, CreatedBy = userId, CreatedDate = now
                }));
                db.BookingStatusHistories.Add(new BookingStatusHistory
                {
                    Booking = booking, FarmHouseId = farmHouseId, ActorUserId = userId,
                    ActorType = "Customer", NewStatus = "Pending", CreatedDate = now
                });
                await db.SaveChangesAsync(cancellationToken);
                await notifications.StageAsync(booking, BookingNotificationEvent.Submitted, cancellationToken: cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception error) when ((error.GetBaseException() is SqlException { Number: 1205 or 2601 or 2627 }))
        {
            var previous = await existing.SingleOrDefaultAsync(cancellationToken);
            if (previous == null)
                return Unavailable();
            if (previous.SubmitRequestHash != requestHash)
                return new(409, "This submission key was already used for another request.", "DUPLICATE_SUBMISSION");
            booking = previous;
            replay = true;
        }

        try
        {
            await pendingHolds.Where(r => request.ReservationIds.Contains(r.ReservationId)).ExecuteDeleteAsync(CancellationToken.None);
        }
        catch (Exception error)
        {
            logger.LogWarning(error, "Reservation cleanup failed for booking {BookingId}; a submit replay can retry cleanup.", booking.BookingId);
        }
        logger.LogInformation("Booking {BookingId} {BookingReference} persisted for farmhouse {FarmHouseId}; replay {Replay}.",
            booking.BookingId, booking.BookingReference, farmHouseId, replay);
        return new(replay ? 200 : 201, "Booking submitted successfully.", Booking: ToResponse(booking));
    }

    private static SubmitBookingResponse ToResponse(BookingEntity booking) => new(
            booking.BookingId, booking.BookingReference, booking.Status, DateOnly.FromDateTime(booking.CheckInDate),
            DateOnly.FromDateTime(booking.CheckOutDate), booking.BookingType, booking.StayDays,
            booking.TotalAmount, booking.AdvanceAmount, booking.CurrencyCode ?? "INR",
            DateTime.SpecifyKind(booking.OwnerDeadline, DateTimeKind.Local));

    private static BookingSubmissionResult Unavailable() =>
        new(409, "One or more selected dates are no longer available.", "DATE_UNAVAILABLE");

    private async Task<string> NextBookingReferenceAsync(int year, CancellationToken cancellationToken)
    {
        var prefix = $"FS-{year}-";
        var lastReference = await db.Bookings.AsNoTracking()
            .Where(booking => booking.BookingReference.StartsWith(prefix) &&
                booking.BookingReference.Length >= prefix.Length + 3 &&
                booking.BookingReference.Length <= prefix.Length + 19 &&
                !EF.Functions.Like(booking.BookingReference.Substring(prefix.Length), "%[^0-9]%"))
            .OrderByDescending(booking => booking.BookingReference.Length)
            .ThenByDescending(booking => booking.BookingReference)
            .Select(booking => booking.BookingReference)
            .FirstOrDefaultAsync(cancellationToken);
        var next = lastReference == null ? 1L : checked(long.Parse(lastReference.AsSpan(prefix.Length)) + 1);
        return FormattableString.Invariant($"{prefix}{next:D3}");
    }
}
