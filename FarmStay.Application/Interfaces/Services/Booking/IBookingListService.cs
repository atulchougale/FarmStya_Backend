using FarmStay.Application.DTOs.Booking;

namespace FarmStay.Application.Interfaces.Services.Booking;

public interface IBookingListService
{
    Task<bool> CanViewAsync(int userId, int farmHouseId, bool administration);
    Task<bool> CanDecideAsync(int userId, int farmHouseId);
    Task<bool> CanConfirmAsync(int userId, int farmHouseId);
    Task<bool> IsGlobalAdministratorAsync(int userId);
    Task<IReadOnlyList<BookingListResponseDto>> ListAsync(int farmHouseId, int? customerUserId, int page, int pageSize);
    Task<BookingListResponseDto?> FindAsync(int farmHouseId, int? customerUserId, long bookingId);
}
