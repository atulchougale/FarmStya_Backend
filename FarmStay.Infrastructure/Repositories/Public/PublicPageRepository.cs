using FarmStay.Application.DTOs.Admin;
using FarmStay.Application.DTOs.Public;
using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FarmStay.Infrastructure.Repositories.Public
{
    public class PublicPageRepository : IPublicPageRepository
    {
        private readonly AppDbContext _context;

        public PublicPageRepository(AppDbContext context)
        {
            _context = context;
        }

        // Amenities
        public async Task<List<AmenityResponseDto>> GetAmenitiesAsync(int farmHouseId)
        {
            return await _context.Amenities
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.FarmHouseId == farmHouseId &&
                    x.IsAmenity)
                .Select(x => new AmenityResponseDto
                {
                    ImageId = x.ImageId,
                    ImageUrl = x.ImageUrl,
                    Title = x.Title,
                    Description = x.Description,
                    FarmHouseId = x.FarmHouseId
                })
                .ToListAsync();
        }

        // Carousel
        public async Task<List<AmenityResponseDto>> GetCarsolesAsync(int farmHouseId)
        {
            return await _context.Amenities
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.FarmHouseId == farmHouseId &&
                    x.IsCarasoul)
                .Select(x => new AmenityResponseDto
                {
                    ImageId = x.ImageId,
                    ImageUrl = x.ImageUrl,
                    Title = x.Title,
                    Description = x.Description,
                    FarmHouseId = x.FarmHouseId
                })
                .ToListAsync();
        }

        // Feedback
        public async Task<List<FeedbackResponseDto>> GetFeedbacksAsync(int farmHouseId)
        {
            return await _context.FeedBacks
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.FarmHouseId == farmHouseId)
                .Join(
                        _context.Users.AsNoTracking(),
                        feedback => feedback.CreatedBy,   // Feedback table मधील column
                        user => user.UserId,              // Users table मधील column
                        (feedback, user) => new { feedback, user } // दोन्ही एकत्र करणे
                    )
                .OrderByDescending(x => x.feedback.Rating)
                .ThenByDescending(x => x.feedback.CreatedDate)
                .Take(10)
                .Select(x => new FeedbackResponseDto
                {
                    FeedbackId = x.feedback.FeedBackId,
                    FarmHouseId = x.feedback.FarmHouseId,
                    Review = x.feedback.Review,
                    Rating = x.feedback.Rating,
                    CreatedDate = x.feedback.CreatedDate,
                    CreatedBy = x.feedback.CreatedBy,
                    CreatedByName = x.user.FullName
                })
                .ToListAsync();
        }

        // Gallery
        public async Task<List<GalleryResponseDto>> GetGalleryAync(int farmHouseId)
        {
            return await _context.Galleries
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.FarmHouseId == farmHouseId)
                .OrderBy(x => x.Category)
                .ThenBy(x => x.DisplayOrder)
                .Select(x => new GalleryResponseDto
                {
                    ImageId = x.ImageId,
                    ImageUrl = x.ImageUrl,
                    ImageName = x.ImageName,
                    Category = x.Category,
                    Description = x.Description,
                    DisplayOrder = x.DisplayOrder,
                    FarmHouseId = x.FarmHouseId
                })
                .ToListAsync();
        }
    }
}