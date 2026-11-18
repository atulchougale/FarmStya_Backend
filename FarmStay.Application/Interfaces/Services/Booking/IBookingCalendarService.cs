
using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.Booking;

namespace FarmStay.Application.Interfaces.Services.Booking
{
    public interface IBookingCalendarService
    {
        Task<ApiResponse<BookingCalendarAvailabilityResponseDto>>
            GetAvailabilityAsync(
                DateTime fromDate,
                DateTime toDate);

        Task<ApiResponse<BookingCalendarMonthResponseDto>>
            GetMonthlyCalendarAsync(
                int month,
                int year);

        Task<ApiResponse<CreateBookingDateReservationsDto>> CreateReservationAsync(
                CreateBookingDateReservationRequestDto request);

        Task<ApiResponse<DeleteBookingDateReservationResponseDto>> DeleteReservationAsync(
                DeleteBookingDateReservationRequestDto request);

        Task<ApiResponse<List<ActiveBookingDateReservationDto>>> GetActiveUserReservationsAsync();
    }
}
