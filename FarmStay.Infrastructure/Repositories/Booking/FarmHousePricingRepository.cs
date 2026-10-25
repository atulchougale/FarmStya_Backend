using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Domain.Entities;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FarmStay.Infrastructure.Repositories.Booking
{
    public class FarmHousePricingRepository : IFarmHousePricingRepository
    {
        private readonly AppDbContext _context;

        public FarmHousePricingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<FarmHousePricing>>
            GetActivePricingByFarmHouseIdAsync(int farmHouseId)
        {
            return await _context.FarmHousePricings
                .AsNoTracking()
                .Where(p =>
                    p.FarmHouseId == farmHouseId &&
                    p.IsActive &&
                    !p.IsDelete)
                .OrderBy(p => p.PricingName)
                .ToListAsync();
        }
    }
}