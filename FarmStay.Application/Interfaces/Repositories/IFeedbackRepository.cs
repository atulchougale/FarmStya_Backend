using FarmStay.Application.DTOs.Public;
using FarmStay.Domain.Entities;

namespace FarmStay.Application.Interfaces.Repositories
{
    public interface IFeedbackRepository
    {
        Task AddAsync(FeedBack feedback);

        Task<FeedBack?> GetByIdAsync(int feedbackId);

        Task<List<FeedbackResponseDto>> GetAllAsync(int farmhouseId);

        Task DeleteAsync(int feedbackId, int userId);

    }
}
