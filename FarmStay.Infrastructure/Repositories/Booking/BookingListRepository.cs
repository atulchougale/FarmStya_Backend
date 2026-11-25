
using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Domain.Entities;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FarmStay.Infrastructure.Repositories.Booking
{
    public class BookingListRepository : IBookingListRepository
    {
        private readonly AppDbContext _context;

        public BookingListRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<FarmStay.Domain.Entities.Booking> Items, long TotalCount, FarmStay.Application.DTOs.Booking.AdministrationBookingSummary Summary)> SearchAdministrationAsync(
            int farmHouseId, FarmStay.Application.DTOs.Booking.AdministrationBookingFilter filter, CancellationToken ct)
        {
            var scope = _context.Bookings.AsNoTracking().Where(b => b.FarmHouseId == farmHouseId && !b.IsDelete);
            var counts = await scope.GroupBy(b => b.Status).Select(g => new { Status = g.Key, Count = g.LongCount() }).ToListAsync(ct);
            long Count(string status) => counts.Where(c => c.Status == status).Sum(c => c.Count);
            var summary = new FarmStay.Application.DTOs.Booking.AdministrationBookingSummary(counts.Sum(c => c.Count), Count("Confirmed"), Count("CheckedIn"), Count("CheckedOut"), Count("Pending"));
            var query = scope;
            var search = filter.Search?.Trim();
            if (!string.IsNullOrEmpty(search))
                query = query.Where(b => b.BookingReference.Contains(search) || b.FullName.Contains(search) || b.MobileNumber.Contains(search));
            if (!string.IsNullOrEmpty(filter.Status)) query = query.Where(b => b.Status == filter.Status);
            if (!string.IsNullOrEmpty(filter.Source)) query = query.Where(b => b.BookingSource == filter.Source);
            // A stay overlaps the inclusive filter range; checkout is exclusive.
            if (filter.From.HasValue) query = query.Where(b => b.CheckOutDate > filter.From.Value.Date);
            if (filter.To.HasValue) query = query.Where(b => b.CheckInDate <= filter.To.Value.Date);
            var total = await query.LongCountAsync(ct);
            var items = await query.OrderByDescending(b => b.CreatedDate).ThenByDescending(b => b.BookingId)
                .Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync(ct);
            return (items, total, summary);
        }

        // Get all bookings with pagination
        public async Task<List<FarmStay.Domain.Entities.Booking>>
            GetAllBookingsAsync(
                int farmHouseId,
                int page,
                int pageSize)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            return await _context.Bookings
                .AsNoTracking()
                .Where(b =>
                    b.FarmHouseId == farmHouseId &&
                    !b.IsDelete)
                .OrderByDescending(b => b.CreatedDate)
                .ThenByDescending(b => b.BookingId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Get booking by ID
        public async Task<FarmStay.Domain.Entities.Booking?>
            GetBookingByIdAsync(
                long bookingId,
                int farmHouseId,
                int? customerUserId = null)
        {
            return await _context.Bookings
                .AsNoTracking()
                .FirstOrDefaultAsync(b =>
                    b.BookingId == bookingId &&
                    b.FarmHouseId == farmHouseId &&
                    (!customerUserId.HasValue || b.CustomerUserId == customerUserId.Value) &&
                    !b.IsDelete);
        }

        // Get booking event dates
        public async Task<List<BookingEventDate>>
            GetEventDatesAsync(
                int farmHouseId,
                List<long> bookingIds)
        {
            if (bookingIds == null || bookingIds.Count == 0)
                return new List<BookingEventDate>();

            return await _context.BookingEventDates
                .AsNoTracking()
                .Where(e =>
                    e.FarmHouseId == farmHouseId &&
                    !e.IsDelete &&
                    bookingIds.Contains(e.BookingId))
                .OrderBy(e => e.EventDate)
                .ToListAsync();
        }

        // Get booking status history
        public async Task<(decimal TotalPaid, decimal AdvancePaid, decimal Discount, bool PendingAdvance)> GetPaymentTotalsAsync(int farmHouseId, long bookingId)
        {
            var paid = _context.PaymentTransactions.AsNoTracking().Where(p =>
                p.FarmHouseId == farmHouseId && p.BookingId == bookingId && !p.IsDelete && p.PaymentStatus == "Success");
            var total = await paid.SumAsync(p => (decimal?)p.Amount) ?? 0;
            var advance = await paid.Where(p => p.PaymentType == "Advance").SumAsync(p => (decimal?)p.Amount) ?? 0;
            var discount = await _context.CheckoutDiscounts.AsNoTracking().Where(d =>
                d.FarmHouseId == farmHouseId && d.BookingId == bookingId && !d.IsDelete)
                .SumAsync(d => (decimal?)d.Amount) ?? 0;
            var pendingAdvance = await _context.PaymentTransactions.AsNoTracking().AnyAsync(p =>
                p.FarmHouseId == farmHouseId && p.BookingId == bookingId && !p.IsDelete &&
                p.PaymentType == "Advance" && p.PaymentMethod == "Online" && p.PaymentStatus == "Pending");
            return (total, advance, discount, pendingAdvance);
        }

        public async Task<List<BookingStatusHistory>>
            GetStatusHistoryAsync(
                int farmHouseId,
                List<long> bookingIds)
        {
            if (bookingIds == null || bookingIds.Count == 0)
                return new List<BookingStatusHistory>();

            return await _context.BookingStatusHistories
                .AsNoTracking()
                .Where(h =>
                    h.FarmHouseId == farmHouseId &&
                    bookingIds.Contains(h.BookingId))
                .OrderByDescending(h => h.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<FarmStay.Domain.Entities.Booking>> GetMyBookingsAsync(
     int customerUserId,
     int farmHouseId,
     int page,
     int pageSize, string? status = null)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            return await _context.Bookings
                .AsNoTracking()
                .Where(b =>
                    b.CustomerUserId == customerUserId &&
                    b.FarmHouseId == farmHouseId &&
                    !b.IsDelete &&
                    (status == null || b.Status == status))
                .OrderByDescending(b => b.CreatedDate)
                .ThenByDescending(b => b.BookingId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
