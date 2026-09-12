using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.Public;


namespace FarmStay.Application.Interfaces.Services.Public
{
    public interface IFeedbackService
    {
        Task<ApiResponse<FeedbackResponseDto>> SaveFeedbackAsync(FeedbackRequestDto dto);

        Task<ApiResponse<FeedbackResponseDto>> GetFeedbackByIdAsync(int feedbackId);

        Task<ApiResponse<List<FeedbackResponseDto>>> GetFeedbackAllAsync();

        Task<ApiResponse<FeedbackResponseDto>> DeleteFeedbackAsync(int feedbackId);
    }
}
