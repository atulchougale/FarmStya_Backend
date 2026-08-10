using FarmStay.Domain.Entities;
using FarmStay.Domain.Enums;

public interface IUserOtpRepository
{
    Task AddAsync(UserOtp userOtp);

    Task<UserOtp?> GetActiveOtpAsync(int userId, OtpPurpose purpose);

    Task<UserOtp?> GetByOtpAsync(int userId, string otpCode, OtpPurpose purpose);

    Task UpdateAsync(UserOtp userOtp);
}