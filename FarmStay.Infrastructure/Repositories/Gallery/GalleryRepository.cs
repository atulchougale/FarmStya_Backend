using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Domain.Entities;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace FarmStay.Infrastructure.Repositories.Admin
{
    public class GalleryRepository : IGalleryRepository
    {
        private readonly AppDbContext _context;
        public GalleryRepository(AppDbContext context) 
        {
            _context = context;
        }
        public async Task AddAsync(Gallery gallery)
        {
            await _context.Galleries.AddAsync(gallery);
        }

        public async Task<Gallery?> GetByIdAsync(int imageId)
        {
            return await _context.Galleries
                .FirstOrDefaultAsync(x =>
                    x.ImageId == imageId &&
                    !x.IsDelete);
        }

        public async Task<List<Gallery>> GetAllAsync(int farmHouseId)
        {
            return await _context.Galleries
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.FarmHouseId == farmHouseId)

                .OrderBy(x => x.Category)
                .ThenBy(x=> x.DisplayOrder)
                .ToListAsync();
        }

        public Task UpdateAsync(Gallery gallery)
        {
            _context.Galleries.Update(gallery);
            return Task.CompletedTask;
        }
        public async Task<List<string>> GetCategoryListAsync(int farmHouseId)
        {

            return await _context.Galleries
                 .Where(x => !string.IsNullOrEmpty(x.Category) && x.FarmHouseId == farmHouseId)
                .Select(x=> x.Category)
                .Distinct()
              
           .ToListAsync();


        }

        public async Task<Gallery?> GetByImageNameAsync(int farmHouseId, string imageName)
        {
            return await _context.Galleries
                    .FirstOrDefaultAsync(x =>
                   x.ImageName == imageName &&
                   x.FarmHouseId == farmHouseId &&
                   !x.IsDelete);
        }

       

    }
}
