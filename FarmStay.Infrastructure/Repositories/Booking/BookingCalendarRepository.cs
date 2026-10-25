
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
                        stayDate.Booking.Status == "Confirmed"
                    ))
                .ToListAsync();
        }

        public async Task<List<BookingDateReservation>> GetActiveHoldsAsync(
            int farmHouseId,
            DateTime fromDate,
            DateTime toDate,
            DateTime currentUtc)
        {
            var startDate = fromDate.Date;
            var endDate = toDate.Date;

            return await _context.BookingDateReservations
                .AsNoTracking()
                .Where(reservation =>
                    reservation.FarmHouseId == farmHouseId &&
                    reservation.ReservationDate >= startDate &&
                    reservation.ReservationDate < endDate &&
                    reservation.ExpiresAtUtc > currentUtc)
                .ToListAsync();
        }


        public async Task AddReservationsAsync( List<BookingDateReservation> reservations)
        {
            await _context.BookingDateReservations.AddRangeAsync(reservations);
        }
    }
}
