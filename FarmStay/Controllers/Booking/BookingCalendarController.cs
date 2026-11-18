using FarmStay.Application.DTOs.Booking;
using FarmStay.Application.Interfaces.Services.Booking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FarmStay.Controllers.Booking
{
    [Route("api/booking-calendar")]
    [ApiController]
    public class BookingCalendarController : ControllerBase
    {
        private readonly IBookingCalendarService _calendarService;

        public BookingCalendarController( IBookingCalendarService calendarService)
        {
            _calendarService = calendarService;
        }

        [HttpGet("availability")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailability(
            [FromQuery, BindRequired] DateTime fromDate,
            [FromQuery, BindRequired] DateTime toDate)
        {
            if (fromDate == default || toDate == default)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Both fromDate and toDate are required.",
                    data = (object?)null
                });
            }

            var response = await _calendarService.GetAvailabilityAsync(
                fromDate,
                toDate);

            return Ok(response);
        }

        [HttpGet("month")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMonthlyCalendar(
            [FromQuery, BindRequired] int month,
            [FromQuery, BindRequired] int year)
        {
            if (month < 1 || month > 12 || year < 1 || year > 9999)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Valid month and year are required.",
                    data = (object?)null
                });
            }

            var response = await _calendarService.GetMonthlyCalendarAsync(
                month,
                year);

            return Ok(response);
        }

        [HttpPost("reserve")]
        [Authorize(Policy = FarmStay.API.Authorization.BookingCustomerRequirement.Policy)]
        public async Task<IActionResult> CreateReservation(
            [FromBody] CreateBookingDateReservationRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Reservation request is required.",
                    data = (object?)null
                });
            }

            var response = await _calendarService.CreateReservationAsync(
                request);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpDelete("reserve")]
        [Authorize(Policy = FarmStay.API.Authorization.BookingCustomerRequirement.Policy)]
        public async Task<IActionResult> DeleteReservation(
                    [FromQuery, BindRequired] DateTime fromDate,
                    [FromQuery] DateTime? toDate)
        {
            // Both single date and range are supported.
            // If toDate is not provided, only fromDate is released.
            if (fromDate == default)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "fromDate is required.",
                    data = (object?)null
                });
            }

            var request = new DeleteBookingDateReservationRequestDto
            {
                FromDate = fromDate,
                ToDate = toDate
            };

            var response = await _calendarService.DeleteReservationAsync(request);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }


        [HttpGet("reserve/active")]
        [Authorize(Policy = FarmStay.API.Authorization.BookingCustomerRequirement.Policy)]
        public async Task<IActionResult> GetActiveUserReservations()
        {
            var response = await _calendarService
                .GetActiveUserReservationsAsync();

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

    }
}
