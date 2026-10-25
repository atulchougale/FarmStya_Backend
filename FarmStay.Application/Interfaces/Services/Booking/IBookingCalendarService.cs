
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

        Task<ApiResponse<CreateBookingDateReservationResponseDto>> CreateReservationAsync(
                CreateBookingDateReservationRequestDto request);
    }
}
