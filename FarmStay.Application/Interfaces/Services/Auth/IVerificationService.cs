using FarmStay.Application.DTOs.Auth;

namespace FarmStay.Application.Interfaces.Services.Auth
{
    public interface IVerificationService
    {
        Task<RegisterResponseDto> StartVerificationFlowAsync(
            int userId,
            int farmHouseId,
            int roleId);

        Task<VerificationStatusResponseDto> VerifyEmailAsync(
            VerifyEmailRequestDto dto);

        Task<VerificationStatusResponseDto> VerifyOtpAsync(
            VerifyOtpRequestDto dto);
    }
}