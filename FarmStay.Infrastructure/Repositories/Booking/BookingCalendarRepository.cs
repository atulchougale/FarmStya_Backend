
using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Domain.Entities;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FarmStay.Infrastructure.Repositories.Booking
{
    public class BookingCalendarRepository : IBookingCalendarRepository
    {
        private readonly AppDbContext _context;

        public BookingCalendarRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<BookingStayDate>> GetPermanentDatesAsync(
            int farmHouseId,
            DateTime fromDate,
            DateTime toDate)
        {
            var startDate = fromDate.Date;
            var endDate = toDate.Date;

            return await _context.BookingStayDates
                .AsNoTracking()
                .Include(stayDate => stayDate.Booking)
                .Where(stayDate =>
                    stayDate.FarmHouseId == farmHouseId &&
                    !stayDate.IsDelete &&
                    stayDate.StayDate >= startDate &&
                    stayDate.StayDate < endDate &&
                    stayDate.Booking != null &&
                    !stayDate.Booking.IsDelete &&
                    (
                        stayDate.Booking.Status == "Pending" ||
                        stayDate.Booking.Status == "Approved" ||
                        stayDate.Booking.Status == "Confirmed" ||
                        stayDate.Booking.Status == "CheckedIn" ||
                        stayDate.Booking.Status == "CheckedOut"
                    ))
                .ToListAsync();
        }

        public async Task<List<BookingDateReservation>> GetActiveHoldsAsync(
            int farmHouseId,
            DateTime fromDate,
            DateTime toDate,
            DateTime current)
        {
            var startDate = fromDate.Date;
            var endDate = toDate.Date;

            return await _context.BookingDateReservations
                .AsNoTracking()
                .Where(reservation =>
                    reservation.FarmHouseId == farmHouseId &&
                    reservation.ReservationDate >= startDate &&
                    reservation.ReservationDate < endDate &&
                    reservation.ExpiresAt > current)
                .ToListAsync();
        }


        public async Task AddReservationsAsync(List<BookingDateReservation> reservations)
        {
            await _context.BookingDateReservations.AddRangeAsync(reservations);
        }

        public async Task<List<BookingDateReservation>> GetUserHoldsForDeleteAsync(
                    int farmHouseId,
                    int userId,
                    DateTime fromDate,
                    DateTime toDate)
        {
            var startDate = fromDate.Date;
            var endDate = toDate.Date;

            // Tracked entities (no AsNoTracking) because they will be removed.
            return await _context.BookingDateReservations
                .Where(reservation =>
                    reservation.FarmHouseId == farmHouseId &&
                    reservation.UserId == userId &&
                    reservation.ReservationDate >= startDate &&
                    reservation.ReservationDate < endDate)
                .ToListAsync();
        }

        public void RemoveReservations(List<BookingDateReservation> reservations)
        {
            _context.BookingDateReservations.RemoveRange(reservations);
        }


        public async Task<List<BookingDateReservation>> GetActiveUserReservationsAsync(
                int farmHouseId,
                int userId,
                DateTime current)
            {
                return await _context.BookingDateReservations
                    .AsNoTracking()
                    .Where(reservation =>
                        reservation.FarmHouseId == farmHouseId &&
                        reservation.UserId == userId &&
                        reservation.ExpiresAt > current)
                    .OrderBy(reservation => reservation.ReservationDate)
                    .ToListAsync();
            }
    }
}
