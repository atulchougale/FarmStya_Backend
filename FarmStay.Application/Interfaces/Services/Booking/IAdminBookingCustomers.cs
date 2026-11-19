using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.Auth;
namespace FarmStay.Application.Interfaces.Services.Booking;
public record AdminCustomer(int UserId, int FarmHouseId, string FullName, string MobileNumber, string? Email,
    bool IsMobileVerified, bool IsEmailVerified, bool HasMembership, bool CanBook);
public record AdminCustomerRegistration(string FullName, string MobileNumber, string? Email);
public interface IAdminBookingCustomers
{
    Task<AdminCustomer?> FindAsync(int farmhouse, string mobile);
    Task<ApiResponse<AdminCustomer>> RegisterAsync(int farmhouse, int actor, AdminCustomerRegistration request);
    Task<ApiResponse<AdminCustomer>> VerifyAsync(int farmhouse, int userId, string otp);
    Task<ApiResponse<bool>> ResendAsync(int farmhouse, int userId);
}
