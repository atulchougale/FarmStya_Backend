
using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.Booking;
using FarmStay.Application.Interfaces.Common;
using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Application.Interfaces.Services.Booking;
using FarmStay.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace FarmStay.Application.Services.Booking
{
    public class BookingCalendarService : IBookingCalendarService
    {
        private readonly IBookingCalendarRepository _calendarRepository;
        private readonly IFarmHouseRepository _farmHouseRepository;
        private readonly ILogger<BookingCalendarService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;

        public BookingCalendarService(
            IBookingCalendarRepository calendarRepository,
            IFarmHouseRepository farmHouseRepository,
            IUnitOfWork unitOfWork,
            ILogger<BookingCalendarService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _calendarRepository = calendarRepository;
            _farmHouseRepository = farmHouseRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApiResponse<BookingCalendarAvailabilityResponseDto>>
            GetAvailabilityAsync(
                DateTime fromDate,
                DateTime toDate)
        {
            var farmHouseIdHeader = _httpContextAccessor.HttpContext?
                .Request
                .Headers["FarmHouseId"]
                .FirstOrDefault();

            if (!int.TryParse(farmHouseIdHeader, out int farmHouseId)
                || farmHouseId <= 0)
            {
                _logger.LogWarning(
                    "FarmHouseId header is missing or invalid.");

                return new ApiResponse<BookingCalendarAvailabilityResponseDto>
                {
                    Success = false,
                    Message = "Invalid FarmHouse.",
                    Data = null
                };
            }

            fromDate = fromDate.Date;
            toDate = toDate.Date;

            if (fromDate >= toDate)
            {
                _logger.LogWarning(
                    "Invalid calendar date range. FarmHouseId: {FarmHouseId}, FromDate: {FromDate}, ToDate: {ToDate}",
                    farmHouseId,
                    fromDate,
                    toDate);

                return new ApiResponse<BookingCalendarAvailabilityResponseDto>
                {
                    Success = false,
                    Message = "FromDate must be earlier than ToDate.",
                    Data = null
                };
            }

            _logger.LogInformation(
                "GetAvailability request received. FarmHouseId: {FarmHouseId}, FromDate: {FromDate}, ToDate: {ToDate}",
                farmHouseId,
                fromDate,
                toDate);

            try
            {
                var farmHouse = await _farmHouseRepository
                    .GetByIdAsync(farmHouseId);

                if (farmHouse == null)
                {
                    _logger.LogWarning(
                        "Farmhouse not found. FarmHouseId: {FarmHouseId}",
                        farmHouseId);

                    return new ApiResponse<BookingCalendarAvailabilityResponseDto>
                    {
                        Success = false,
                        Message = "Farmhouse not found.",
                        Data = null
                    };
                }

                var current = DateTime.Now;

                var permanentDates = await _calendarRepository
                    .GetPermanentDatesAsync(
                        farmHouseId,
                        fromDate,
                        toDate);

                var activeHolds = await _calendarRepository
                    .GetActiveHoldsAsync(
                        farmHouseId,
                        fromDate,
                        toDate,
                        current);

                var permanentUnavailableDates = permanentDates
                    .Select(x => x.StayDate.Date)
                    .ToHashSet();

                var temporaryUnavailableDates = activeHolds
                    .Select(x => x.ReservationDate.Date)
                    .ToHashSet();

                var availableDates = new List<DateTime>();
                var unavailableDates = new List<DateTime>();

                // Checkout date is exclusive.
                for (var date = fromDate;
                     date < toDate;
                     date = date.AddDays(1))
                {
                    if (permanentUnavailableDates.Contains(date) ||
                        temporaryUnavailableDates.Contains(date))
                    {
                        unavailableDates.Add(date);
                    }
                    else
                    {
                        availableDates.Add(date);
                    }
                }

                var response = new BookingCalendarAvailabilityResponseDto
                {
                    FarmHouseId = farmHouseId,
                    FromDate = fromDate,
                    ToDate = toDate,

                    IsAvailable = unavailableDates.Count == 0,

                    AvailableDates = availableDates,
                    UnavailableDates = unavailableDates,

                    PermanentDates = permanentDates
                        .Select(x => new CalendarPermanentDateDto
                        {
                            Date = x.StayDate.Date,
                            Status = x.Booking?.Status ?? "Unavailable"
                        })
                        .GroupBy(x => x.Date)
                        .Select(g => g.First())
                        .OrderBy(x => x.Date)
                        .ToList(),

                    ActiveHolds = activeHolds
                        .Select(x => new CalendarActiveHoldDto
                        {
                            Date = x.ReservationDate.Date,
                            Status = "TemporarilyReserved",
                            ExpiresAt = DateTime.SpecifyKind(x.ExpiresAt, DateTimeKind.Local)
                        })
                        .OrderBy(x => x.Date)
                        .ToList()
                };

                _logger.LogInformation(
                    "Calendar availability retrieved. FarmHouseId: {FarmHouseId}, AvailableDates: {AvailableCount}, UnavailableDates: {UnavailableCount}",
                    farmHouseId,
                    response.AvailableDates.Count,
                    response.UnavailableDates.Count);

                return new ApiResponse<BookingCalendarAvailabilityResponseDto>
                {
                    Success = true,
                    Message = response.IsAvailable
                        ? "Selected dates are available."
                        : "Selected dates are not available.",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving calendar availability. FarmHouseId: {FarmHouseId}",
                    farmHouseId);

                return new ApiResponse<BookingCalendarAvailabilityResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving calendar availability.",
                    Data = null
                };
            }
        }




        public async Task<ApiResponse<BookingCalendarMonthResponseDto>>
            GetMonthlyCalendarAsync(int month, int year)
        {
            var farmHouseIdHeader = _httpContextAccessor.HttpContext?
                .Request
                .Headers["FarmHouseId"]
                .FirstOrDefault();

            if (!int.TryParse(farmHouseIdHeader, out int farmHouseId)
                || farmHouseId <= 0)
            {
                _logger.LogWarning(
                    "FarmHouseId header is missing or invalid.");

                return new ApiResponse<BookingCalendarMonthResponseDto>
                {
                    Success = false,
                    Message = "Invalid FarmHouse.",
                    Data = null
                };
            }

            if (month < 1 || month > 12 || year < 1 || year > 9999)
            {
                _logger.LogWarning(
                    "Invalid calendar month or year. Month: {Month}, Year: {Year}",
                    month,
                    year);

                return new ApiResponse<BookingCalendarMonthResponseDto>
                {
                    Success = false,
                    Message = "Invalid month or year.",
                    Data = null
                };
            }

            _logger.LogInformation(
                "GetMonthlyCalendar request received. FarmHouseId: {FarmHouseId}, Month: {Month}, Year: {Year}",
                farmHouseId,
                month,
                year);

            try
            {
                var farmHouse = await _farmHouseRepository
                    .GetByIdAsync(farmHouseId);

                if (farmHouse == null)
                {
                    _logger.LogWarning(
                        "Farmhouse not found. FarmHouseId: {FarmHouseId}",
                        farmHouseId);

                    return new ApiResponse<BookingCalendarMonthResponseDto>
                    {
                        Success = false,
                        Message = "Farmhouse not found.",
                        Data = null
                    };
                }

                var fromDate = new DateTime(year, month, 1);
                var toDate = fromDate.AddMonths(1);
                var current = DateTime.Now;

                // Reuse existing repository methods.
                var permanentDates = await _calendarRepository
                    .GetPermanentDatesAsync(
                        farmHouseId,
                        fromDate,
                        toDate);

                var activeHolds = await _calendarRepository
                    .GetActiveHoldsAsync(
                        farmHouseId,
                        fromDate,
                        toDate,
                        current);

                // Select the latest active booking for each date.
                // Assumption: higher BookingId means a newer booking.
                var bookedDates = permanentDates
                    .Where(x => x.Booking != null)
                    .GroupBy(x => x.StayDate.Date)
                    .Select(group => group
                        .OrderByDescending(x => x.BookingId)
                        .First())
                    .Select(x => new CalendarBookedDateDto
                    {
                        Date = x.StayDate.Date,
                        Status = x.Booking!.Status
                    })
                    .OrderBy(x => x.Date)
                    .ToList();

                // Return active temporary holds only.
                var reservedDates = activeHolds
                    .GroupBy(x => new
                    {
                        Date = x.ReservationDate.Date,
                        x.ExpiresAt
                    })
                    .Select(group => group.First())
                    .Select(x => new CalendarReservedDateDto
                    {
                        Date = x.ReservationDate.Date,
                        Status = "TemporarilyReserved",
                        //ExpiresAt = x.ExpiresAt
                        ExpiresAt = DateTime.SpecifyKind(
                                        x.ExpiresAt,
                                        DateTimeKind.Local
                                    )
                    })
                    .OrderBy(x => x.Date)
                    .ToList();

                var response = new BookingCalendarMonthResponseDto
                {
                    FarmHouseId = farmHouseId,
                    Month = month,
                    Year = year,
                    BookedDates = bookedDates,
                    ReservedDates = reservedDates
                };

                _logger.LogInformation(
                    "Monthly calendar retrieved successfully. FarmHouseId: {FarmHouseId}, Month: {Month}, Year: {Year}, BookedDates: {BookedCount}, ReservedDates: {ReservedCount}",
                    farmHouseId,
                    month,
                    year,
                    response.BookedDates.Count,
                    response.ReservedDates.Count);

                return new ApiResponse<BookingCalendarMonthResponseDto>
                {
                    Success = true,
                    Message = "Monthly calendar retrieved successfully.",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving monthly calendar. FarmHouseId: {FarmHouseId}, Month: {Month}, Year: {Year}",
                    farmHouseId,
                    month,
                    year);

                return new ApiResponse<BookingCalendarMonthResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving monthly calendar.",
                    Data = null
                };
            }
        }


        public async Task<ApiResponse<CreateBookingDateReservationsDto>> CreateReservationAsync(CreateBookingDateReservationRequestDto request)
        {
            try
            {
                _logger.LogInformation(
                    "CreateReservationAsync started.");

                // 1. Get FarmHouseId from request header
                var farmHouseHeader = _httpContextAccessor.HttpContext?
                    .Request.Headers["FarmHouseId"].FirstOrDefault();

                if (!int.TryParse(farmHouseHeader, out int farmHouseId) ||
                    farmHouseId <= 0)
                {
                    _logger.LogWarning(
                        "Date reservation failed. Invalid or missing FarmHouseId header.");

                    return new ApiResponse<CreateBookingDateReservationsDto>
                    {
                        Success = false,
                        Message = "Valid FarmHouseId header is required.",
                        Data = null
                    };
                }

                _logger.LogInformation(
                    "Creating date reservation for FarmHouseId: {FarmHouseId}",
                    farmHouseId);

                // 2. Get authenticated UserId from JWT claims
                var userIdClaim = _httpContextAccessor.HttpContext?
                    .User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(userIdClaim, out int userId) || userId <= 0)
                {
                    _logger.LogWarning(
                        "Date reservation failed. Authenticated UserId is missing or invalid. FarmHouseId: {FarmHouseId}",
                        farmHouseId);

                    return new ApiResponse<CreateBookingDateReservationsDto>
                    {
                        Success = false,
                        Message = "Authenticated user is required.",
                        Data = null
                    };
                }

                // 3. Validate requested dates
                var fromDate = request.FromDate.Date;
                var toDate = request.ToDate.Date;
                var today = DateTime.Now.Date;

                if (fromDate < today)
                {
                    _logger.LogWarning(
                        "Date reservation rejected because check-in date is in the past. UserId: {UserId}, FromDate: {FromDate}",
                        userId,
                        fromDate);

                    return new ApiResponse<CreateBookingDateReservationsDto>
                    {
                        Success = false,
                        Message = "Check-in date cannot be in the past.",
                        Data = null
                    };
                }

                if (fromDate >= toDate)
                {
                    _logger.LogWarning(
                        "Date reservation rejected because date range is invalid. UserId: {UserId}, FromDate: {FromDate}, ToDate: {ToDate}",
                        userId,
                        fromDate,
                        toDate);

                    return new ApiResponse<CreateBookingDateReservationsDto>
                    {
                        Success = false,
                        Message = "Check-out date must be after check-in date.",
                        Data = null
                    };
                }

                // 4. Validate FarmHouse
                var farmHouse = await _farmHouseRepository
                    .GetByIdAsync(farmHouseId);

                if (farmHouse == null)
                {
                    _logger.LogWarning(
                        "Date reservation rejected. FarmHouse not found. FarmHouseId: {FarmHouseId}",
                        farmHouseId);

                    return new ApiResponse<CreateBookingDateReservationsDto>
                    {
                        Success = false,
                        Message = "FarmHouse not found.",
                        Data = null
                    };
                }

                // 5. Fetch permanent bookings and active temporary holds
                var current = DateTime.Now;

                var permanentDates = await _calendarRepository
                    .GetPermanentDatesAsync(farmHouseId, fromDate, toDate);

                var activeHolds = await _calendarRepository
                    .GetActiveHoldsAsync(
                        farmHouseId,
                        fromDate,
                        toDate,
                        current);

                _logger.LogInformation(
                    "Fetched existing booking dates and active holds. FarmHouseId: {FarmHouseId}, PermanentRecords: {PermanentCount}, ActiveHolds: {HoldCount}",
                    farmHouseId,
                    permanentDates.Count,
                    activeHolds.Count);

                // 6. Identify unavailable dates
                var permanentDateSet = permanentDates
                    .Select(x => x.StayDate.Date)
                    .ToHashSet();

                var activeHoldDateSet = activeHolds
                    .Select(x => x.ReservationDate.Date)
                    .ToHashSet();

                var unavailableDates = new List<DateTime>();

                for (var date = fromDate; date < toDate; date = date.AddDays(1))
                {
                    if (permanentDateSet.Contains(date) ||
                        activeHoldDateSet.Contains(date))
                    {
                        unavailableDates.Add(date);
                    }
                }

                if (unavailableDates.Count > 0)
                {
                    _logger.LogWarning(
                        "Date reservation rejected because selected dates are unavailable. FarmHouseId: {FarmHouseId}, UserId: {UserId}, UnavailableDates: {UnavailableDates}",
                        farmHouseId,
                        userId,
                        string.Join(", ", unavailableDates.Select(d =>
                            d.ToString("yyyy-MM-dd"))));

                    return new ApiResponse<CreateBookingDateReservationsDto>
                    {
                        Success = false,
                        Message = "One or more selected dates are unavailable. Please select different dates.",
                        Data = null
                    };
                }

                // 7. Create temporary reservation records
                var selectionId = Guid.NewGuid();
                var expiresAt = current.AddMinutes(10);

                var reservations = new List<BookingDateReservation>();

                for (var date = fromDate; date < toDate; date = date.AddDays(1))
                {
                    reservations.Add(new BookingDateReservation
                    {
                        FarmHouseId = farmHouseId,
                        UserId = userId,
                        SelectionId = selectionId,
                        ReservationDate = date,
                        ExpiresAt = expiresAt,
                        CreatedDate = current
                    });
                }

                _logger.LogInformation(
                    "Prepared {ReservationCount} temporary reservation records. FarmHouseId: {FarmHouseId}, UserId: {UserId}, SelectionId: {SelectionId}, ExpiresAt: {ExpiresAt}",
                    reservations.Count,
                    farmHouseId,
                    userId,
                    selectionId,
                    expiresAt);

                // 8. Add records through Repository
                await _calendarRepository.AddReservationsAsync(reservations);

                _logger.LogInformation(
                    "Reservation records added to the DbContext. Saving changes. FarmHouseId: {FarmHouseId}, SelectionId: {SelectionId}",
                    farmHouseId,
                    selectionId);

                // 9. Commit through existing Unit of Work
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Temporary date reservation created successfully. FarmHouseId: {FarmHouseId}, UserId: {UserId}, SelectionId: {SelectionId}, ReservedDates: {ReservedDateCount}",
                    farmHouseId,
                    userId,
                    selectionId,
                    reservations.Count);

                // 10. Return response
                var responseDto = new CreateBookingDateReservationsDto
                {
                    FarmHouseId = farmHouseId,
                    SelectionId = selectionId,
                    ReservationIds = reservations.OrderBy(x => x.ReservationDate).Select(x => x.ReservationId).ToList(),
                    FromDate = fromDate,
                    ToDate = toDate,
                    ReservedDates = reservations
                        .Select(x => x.ReservationDate)
                        .ToList(),
                    ExpiresAt = expiresAt
                };

                return new ApiResponse<CreateBookingDateReservationsDto>
                {
                    Success = true,
                    Message = "Dates temporarily reserved for 10 minutes.",
                    Data = responseDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while creating temporary date reservation.");

                return new ApiResponse<CreateBookingDateReservationsDto>
                {
                    Success = false,
                    Message = "An error occurred while reserving the selected dates.",
                    Data = null
                };
            }
        }


        public async Task<ApiResponse<DeleteBookingDateReservationResponseDto>> DeleteReservationAsync(
    DeleteBookingDateReservationRequestDto request)
        {
            try
            {
                _logger.LogInformation("DeleteReservationAsync started.");

                // 1. Get FarmHouseId from request header
                var farmHouseHeader = _httpContextAccessor.HttpContext?
                    .Request.Headers["FarmHouseId"].FirstOrDefault();

                if (!int.TryParse(farmHouseHeader, out int farmHouseId) || farmHouseId <= 0)
                {
                    _logger.LogWarning("Date release failed. Invalid or missing FarmHouseId header.");

                    return new ApiResponse<DeleteBookingDateReservationResponseDto>
                    {
                        Success = false,
                        Message = "Valid FarmHouseId header is required.",
                        Data = null
                    };
                }

                // 2. Get authenticated UserId from JWT claims
                var userIdClaim = _httpContextAccessor.HttpContext?
                    .User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(userIdClaim, out int userId) || userId <= 0)
                {
                    _logger.LogWarning(
                        "Date release failed. Authenticated UserId is missing or invalid. FarmHouseId: {FarmHouseId}",
                        farmHouseId);

                    return new ApiResponse<DeleteBookingDateReservationResponseDto>
                    {
                        Success = false,
                        Message = "Authenticated user is required.",
                        Data = null
                    };
                }

                // 3. Resolve date range. Single date => [FromDate, FromDate + 1 day)
                var fromDate = request.FromDate.Date;
                var toDate = request.ToDate.HasValue
                    ? request.ToDate.Value.Date
                    : fromDate.AddDays(1);

                if (fromDate >= toDate)
                {
                    _logger.LogWarning(
                        "Date release rejected because date range is invalid. UserId: {UserId}, FromDate: {FromDate}, ToDate: {ToDate}",
                        userId,
                        fromDate,
                        toDate);

                    return new ApiResponse<DeleteBookingDateReservationResponseDto>
                    {
                        Success = false,
                        Message = "ToDate must be after FromDate.",
                        Data = null
                    };
                }

                // 4. Validate FarmHouse
                var farmHouse = await _farmHouseRepository.GetByIdAsync(farmHouseId);

                if (farmHouse == null)
                {
                    _logger.LogWarning(
                        "Date release rejected. FarmHouse not found. FarmHouseId: {FarmHouseId}",
                        farmHouseId);

                    return new ApiResponse<DeleteBookingDateReservationResponseDto>
                    {
                        Success = false,
                        Message = "FarmHouse not found.",
                        Data = null
                    };
                }

                // 5. Fetch only the holds owned by this user in the range
                var holds = await _calendarRepository.GetUserHoldsForDeleteAsync(
                    farmHouseId,
                    userId,
                    fromDate,
                    toDate);

                if (holds.Count == 0)
                {
                    _logger.LogInformation(
                        "No reservations found to release. FarmHouseId: {FarmHouseId}, UserId: {UserId}, FromDate: {FromDate}, ToDate: {ToDate}",
                        farmHouseId,
                        userId,
                        fromDate,
                        toDate);

                    return new ApiResponse<DeleteBookingDateReservationResponseDto>
                    {
                        Success = false,
                        Message = "No reserved dates found for the selected date(s).",
                        Data = null
                    };
                }

                var releasedDates = holds
                    .Select(x => x.ReservationDate.Date)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                // 6. Permanently delete and commit through Unit of Work
                _calendarRepository.RemoveReservations(holds);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Reservations released successfully. FarmHouseId: {FarmHouseId}, UserId: {UserId}, DeletedCount: {DeletedCount}",
                    farmHouseId,
                    userId,
                    holds.Count);

                return new ApiResponse<DeleteBookingDateReservationResponseDto>
                {
                    Success = true,
                    Message = "Selected dates released successfully.",
                    Data = new DeleteBookingDateReservationResponseDto
                    {
                        FarmHouseId = farmHouseId,
                        FromDate = fromDate,
                        ToDate = toDate,
                        DeletedCount = holds.Count,
                        ReleasedDates = releasedDates
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while releasing reserved dates.");

                return new ApiResponse<DeleteBookingDateReservationResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while releasing the selected dates.",
                    Data = null
                };
            }
        }




        public async Task<ApiResponse<List<ActiveBookingDateReservationDto>>>
            GetActiveUserReservationsAsync()
        {
            try
            {
                // 1. Get FarmHouseId from request header
                var farmHouseHeader = _httpContextAccessor.HttpContext?
                    .Request.Headers["FarmHouseId"].FirstOrDefault();

                if (!int.TryParse(farmHouseHeader, out int farmHouseId) ||
                    farmHouseId <= 0)
                {
                    _logger.LogWarning(
                        "Active reservation recovery failed. Invalid FarmHouseId header.");

                    return new ApiResponse<List<ActiveBookingDateReservationDto>>
                    {
                        Success = false,
                        Message = "Valid FarmHouseId header is required.",
                        Data = null
                    };
                }

                // 2. Get authenticated UserId from JWT claims
                var userIdClaim = _httpContextAccessor.HttpContext?
                    .User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(userIdClaim, out int userId) ||
                    userId <= 0)
                {
                    _logger.LogWarning(
                        "Active reservation recovery failed. Invalid authenticated UserId.");

                    return new ApiResponse<List<ActiveBookingDateReservationDto>>
                    {
                        Success = false,
                        Message = "Authenticated user is required.",
                        Data = null
                    };
                }

                // 3. Validate FarmHouse
                var farmHouse = await _farmHouseRepository
                    .GetByIdAsync(farmHouseId);

                if (farmHouse == null)
                {
                    return new ApiResponse<List<ActiveBookingDateReservationDto>>
                    {
                        Success = false,
                        Message = "FarmHouse not found.",
                        Data = null
                    };
                }

                // 4. Fetch current user's active reservations
                var current = DateTime.Now;

                var activeReservations = await _calendarRepository
                    .GetActiveUserReservationsAsync(
                        farmHouseId,
                        userId,
                        current);

                // 5. Group reservations by SelectionId
                var response = activeReservations
                    .GroupBy(x => x.SelectionId)
                    .Select(group =>
                    {
                        var reservedDates = group
                            .Select(x => x.ReservationDate.Date)
                            .Distinct()
                            .OrderBy(x => x)
                            .ToList();

                        return new ActiveBookingDateReservationDto
                        {
                            FarmHouseId = farmHouseId,
                            SelectionId = group.Key,
                            ReservationIds = group.OrderBy(x => x.ReservationDate).Select(x => x.ReservationId).ToList(),

                            FromDate = reservedDates.Min(),

                            // Checkout date is exclusive
                            ToDate = reservedDates.Max().AddDays(1),

                            ReservedDates = reservedDates,

                            ExpiresAt = DateTime.SpecifyKind(group.Max(x => x.ExpiresAt), DateTimeKind.Local)
                        };
                    })
                    .OrderBy(x => x.FromDate)
                    .ToList();

                _logger.LogInformation(
                    "Active reservations recovered. FarmHouseId: {FarmHouseId}, UserId: {UserId}, SelectionCount: {SelectionCount}",
                    farmHouseId,
                    userId,
                    response.Count);

                // 6. Return response
                return new ApiResponse<List<ActiveBookingDateReservationDto>>
                {
                    Success = true,
                    Message = response.Count > 0
                        ? "Active reservations retrieved successfully."
                        : "No active reservations found.",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error recovering active date reservations.");

                return new ApiResponse<List<ActiveBookingDateReservationDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving active reservations.",
                    Data = null
                };
            }
        }



    }
}
