using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.Auth;
using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Application.Interfaces.Services.Auth;
using FarmStay.Application.Interfaces.Services.Booking;
namespace FarmStay.Application.Services.Booking;
public sealed class AdminBookingCustomers(IUserRepository users, IUserMembershipRepository memberships, IUserService auth, FarmStay.Application.Interfaces.Common.IUnitOfWork unitOfWork)
    : IAdminBookingCustomers
{
    public async Task<AdminCustomer?> FindAsync(int farmhouse, string mobile)
    {
        var user = await users.GetByMobileNumberAsync(mobile.Trim(), farmhouse);
        if (user == null || !user.IsActive || user.IsDeleted) return null;
        var membership = await memberships.GetMembershipAsync(user.UserId, farmhouse);
        var customer = membership is { IsActive: true, IsDeleted: false, Role: { RoleName: "Customer", IsActive: true, IsDeleted: false } };
        return new(user.UserId, farmhouse, user.FullName, user.MobileNumber, user.Email,
            user.IsMobileVerified, user.IsEmailVerified, customer,
            customer && user.IsMobileVerified && (string.IsNullOrWhiteSpace(user.Email) || user.IsEmailVerified));
    }
    public async Task<ApiResponse<AdminCustomer>> RegisterAsync(int farmhouse, int actor, AdminCustomerRegistration request)
    {
        var existed = await users.GetByMobileNumberAsync(request.MobileNumber.Trim(), farmhouse);
        var result = await auth.RegisterAsync(new RegisterRequestDto {
            FullName=request.FullName, MobileNumber=request.MobileNumber, Email=request.Email,
            Password="Aa1!"+Guid.NewGuid().ToString("N")+Guid.NewGuid().ToString("N")
        });
        if (!result.Success) return new() { Message=result.Message };
        var user = await users.GetByIdAsync(result.Data!.UserId);
        if (user != null && existed == null) { user.CreatedBy=actor; await users.UpdateAsync(user); await unitOfWork.SaveChangesAsync(); }
        return new() { Success=true, Message=result.Message, Data=await FindAsync(farmhouse,request.MobileNumber) };
    }
    public async Task<ApiResponse<AdminCustomer>> VerifyAsync(int farmhouse,int userId,string otp)
    {
        var user = await users.GetByIdAsync(userId);
        if (user == null || user.FarmHouseId != farmhouse) return new() { Message="User not found." };
        var result = await auth.VerifyOtpAsync(new VerifyOtpRequestDto { FarmHouseId=farmhouse,UserId=userId,OtpCode=otp });
        return new() { Success=result.Success,Message=result.Message,Data=await FindAsync(farmhouse,user.MobileNumber) };
    }
    public async Task<ApiResponse<bool>> ResendAsync(int farmhouse,int userId)
    {
        var user=await users.GetByIdAsync(userId);
        if(user==null || user.FarmHouseId!=farmhouse) return new(){Message="User not found."};
        return await auth.ResendOtpAsync(new ResendOtpRequestDto { FarmHouseId=farmhouse,UserId=userId });
    }
}
