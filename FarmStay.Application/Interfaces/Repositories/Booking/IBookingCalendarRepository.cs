
using FarmStay.Domain.Entities;

namespace FarmStay.Application.Interfaces.Repositories
{
    public interface IBookingCalendarRepository
    {
        Task<List<BookingStayDate>> GetPermanentDatesAsync(
            int farmHouseId,
            DateTime fromDate,
            DateTime toDate);

        Task<List<BookingDateReservation>> GetActiveHoldsAsync(
            int farmHouseId,
            DateTime fromDate,
            DateTime toDate,
            DateTime current);

        Task AddReservationsAsync(List<BookingDateReservation> reservations);

        // Returns the tracked holds owned by the user in the given date range.
        Task<List<BookingDateReservation>> GetUserHoldsForDeleteAsync(
            int farmHouseId,
            int userId,
            DateTime fromDate,
            DateTime toDate);

        void RemoveReservations(List<BookingDateReservation> reservations);

        Task<List<BookingDateReservation>> GetActiveUserReservationsAsync(
            int farmHouseId,
            int userId,
            DateTime current);

    }
}
