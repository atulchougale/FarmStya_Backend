using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.Auth;

namespace FarmStay.Application.Interfaces.Services.Auth
{
    public interface IUserService
    {
        // ================= Authentication =================


        Task<ApiResponse<RegisterResponseDto>> RegisterAsync(RegisterRequestDto dto);

        Task<ApiResponse<bool>> VerifyEmailAsync(VerifyEmailRequestDto dto);

        Task<ApiResponse<bool>> VerifyOtpAsync(VerifyOtpRequestDto dto);

        //Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto dto);



        //Task<ApiResponse<bool>> ResendVerificationAsync(string emailOrMobile);

        //Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordDto dto);

        //Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordDto dto);

        //Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto dto);

        //Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(string refreshToken);

        //Task<ApiResponse<bool>> LogoutAsync(int userId);
    }


}
