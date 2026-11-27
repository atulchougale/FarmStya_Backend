using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Domain.Entities;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmStay.Infrastructure.Repositories.Admin
{

    public class AmenityRepository : IAmenityRepository
    {
        private readonly AppDbContext _context;
        public AmenityRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Amenity amenity)
        {
            await _context.Amenities.AddAsync(amenity);
        }

        // 2. GET ALL
        public async Task<List<Amenity>> GetAllAsync(int farmHouseId)
        {
            return await _context.Amenities
                .Where(x => x.FarmHouseId == farmHouseId && !x.IsDelete)
                .ToListAsync();
        }

        // 3. GET BY ID
        public async Task<Amenity?> GetById(int imageId)
        {
            return await _context.Amenities
                .FirstOrDefaultAsync(x =>
                    x.ImageId == imageId &&
                    !x.IsDelete);
        }

        // 4. UPDATE
        public async Task UpdateAsync(Amenity amenity)
        {
            _context.Amenities.Update(amenity);
            await Task.CompletedTask;
        }

        public async Task<Amenity?> GetByTitleAsync(int farmHouseId, string title)
        {
            return await _context.Amenities
                    .FirstOrDefaultAsync(x =>
                   x.Title == title &&
                   x.FarmHouseId == farmHouseId &&
                   !x.IsDelete);
        }

        //// 5. DELETE - Soft Delete
        //public async Task DeleteAsync(int ImageId, int userId)
        //{
        //    var amenity = await _context.Amenities
        //        .FirstOrDefaultAsync(x => x.ImageId == ImageId);

        //    if (amenity != null)
        //    {
        //        amenity.IsDelete = true;
        //        amenity.ModifyBy = userId;
        //        amenity.ModifyDate = DateTime.Now;
        //    }
        //}
    }
}