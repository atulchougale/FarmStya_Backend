using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.Auth;

namespace FarmStay.Application.Interfaces.Services.Auth
{
    public interface IUserService
    {
        // ================= Authentication =================


        Task<ApiResponse<RegisterResponseDto>> RegisterAsync(RegisterRequestDto dto);

        Task<ApiResponse<VerificationStatusResponseDto>> VerifyEmailAsync(VerifyEmailRequestDto dto);

        Task<ApiResponse<VerificationStatusResponseDto>> VerifyOtpAsync(VerifyOtpRequestDto dto);

        Task<ApiResponse<bool>> ResendOtpAsync(ResendOtpRequestDto dto);

        Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto dto);

        Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordDto dto);

        Task<ApiResponse<bool>> ResetPasswordEmailAsync(ResetPasswordByEmailDto dto);

        Task<ApiResponse<bool>> ResetPasswordOtpAsync(ResetPasswordOtpDto dto);

        Task<ApiResponse<bool>> ChangePasswordAsync(ChangePasswordDto dto);

        Task<ApiResponse<ProfileDto>> GetProfileAsync();

        Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(RefreshTokenDto dto);

        Task<ApiResponse<bool>> LogoutAsync(LogoutDto dto);
    }


}
