using FarmStay.Application.DTOs.Public;
using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Domain.Entities;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FarmStay.Infrastructure.Repositories.Public
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly AppDbContext _context;

        public Task<bool> CanReviewBookingAsync(long bookingId, int farmHouseId, int userId) =>
            _context.Bookings.AnyAsync(b => b.BookingId == bookingId && b.FarmHouseId == farmHouseId &&
                b.CustomerUserId == userId && !b.IsDelete && b.Status == "CheckedOut" &&
                !_context.FeedBacks.Any(f => f.BookingId == bookingId));

        public FeedbackRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(FeedBack feedback)
        {
            await _context.FeedBacks.AddAsync(feedback);
        }
        // GET BY ID
        public async Task<FeedBack?> GetByIdAsync(int feedbackId)
        {
            return await _context.FeedBacks
                .FirstOrDefaultAsync(x =>
                    x.FeedBackId == feedbackId &&
                    !x.IsDelete);
        }
        // GET ALL
        public async Task<List<FeedbackResponseDto>> GetAllAsync(int farmHouseId)
        {
            return await _context.FeedBacks
                .AsNoTracking()
                .Where(x =>
                    x.FarmHouseId == farmHouseId &&
                    !x.IsDelete)
                .Join(
                    _context.Users,
                    feedback => feedback.CreatedBy,
                    user => user.UserId,
                    (feedback, user) => new FeedbackResponseDto
                    {
                        FeedbackId = feedback.FeedBackId,
                        FarmHouseId = feedback.FarmHouseId,
                        Rating = feedback.Rating,
                        Review = feedback.Review,
                        CreatedBy = feedback.CreatedBy,
                        CreatedByName = user.FullName,
                        CreatedDate = feedback.CreatedDate
                    })
                .OrderByDescending(x => x.Rating)
                .ToListAsync();
        }

        // SOFT DELETE
        public async Task DeleteAsync(int feedbackId, int userId)
        {
            var feedback = await _context.FeedBacks
                .FirstOrDefaultAsync(x =>
                    x.FeedBackId == feedbackId &&
                    !x.IsDelete);

            if (feedback != null)
            {
                feedback.IsDelete = true;
                feedback.ModifyBy = userId;
                feedback.ModifyDate = DateTime.Now;

                await _context.SaveChangesAsync();
            }
        }
    }
}
