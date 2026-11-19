using FarmStay.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmStay.Application.Interfaces.Repositories
{
    public interface IBookingListRepository
    {
        Task<(decimal TotalPaid, decimal AdvancePaid, decimal Discount, bool PendingAdvance)> GetPaymentTotalsAsync(int farmHouseId, long bookingId);
        Task<List<Booking>> GetAllBookingsAsync(int farmHouseId,int page,int pageSize);

        Task<Booking?> GetBookingByIdAsync(long bookingId, int farmHouseId, int? customerUserId = null);

        Task<List<BookingEventDate>> GetEventDatesAsync( int farmHouseId, List<long> bookingIds);

        Task<List<BookingStatusHistory>> GetStatusHistoryAsync(int farmHouseId, List<long> bookingIds);


        Task<List<Booking>> GetMyBookingsAsync(
      int customerUserId,
      int farmHouseId,
      int page,
      int pageSize);
    }
}
