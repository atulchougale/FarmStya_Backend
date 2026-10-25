
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
            DateTime currentUtc);

        Task AddReservationsAsync( List<BookingDateReservation> reservations);

    }
}
