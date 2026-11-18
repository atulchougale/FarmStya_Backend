
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
        public async Task<(decimal TotalPaid, decimal AdvancePaid, decimal Discount)> GetPaymentTotalsAsync(int farmHouseId, long bookingId)
        {
            var paid = _context.PaymentTransactions.AsNoTracking().Where(p =>
                p.FarmHouseId == farmHouseId && p.BookingId == bookingId && !p.IsDelete && p.PaymentStatus == "Success");
            var total = await paid.SumAsync(p => (decimal?)p.Amount) ?? 0;
            var advance = await paid.Where(p => p.PaymentType == "Advance").SumAsync(p => (decimal?)p.Amount) ?? 0;
            var discount = await _context.CheckoutDiscounts.AsNoTracking().Where(d =>
                d.FarmHouseId == farmHouseId && d.BookingId == bookingId && !d.IsDelete)
                .SumAsync(d => (decimal?)d.Amount) ?? 0;
            return (total, advance, discount);
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
     int pageSize)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            return await _context.Bookings
                .AsNoTracking()
                .Where(b =>
                    b.CustomerUserId == customerUserId &&
                    b.FarmHouseId == farmHouseId &&
                    !b.IsDelete)
                .OrderByDescending(b => b.CreatedDate)
                .ThenByDescending(b => b.BookingId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
