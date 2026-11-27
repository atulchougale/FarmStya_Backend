using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Domain.Entities;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FarmStay.Infrastructure.Repositories.Admin
{
    public class FarmHouseRepository : IFarmHouseRepository
    {
        private readonly AppDbContext _context;

        public FarmHouseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<FarmHouse?> GetByDomainAsync(string domainName)
        {
            return await _context.FarmHouses
                .Include(f => f.OwnerUser)
                .FirstOrDefaultAsync(f =>
                    f.DomainName == domainName &&
                    f.IsActive &&
                    !f.IsDeleted);
        }

        public async Task<FarmHouse?> GetCurrentAsync()
        {
            return await _context.FarmHouses
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.FarmHouseId)
                .FirstOrDefaultAsync();
        }

        public async Task<FarmHouse?> GetByIdAsync(int id)
        {
            return await _context.FarmHouses
                .FirstOrDefaultAsync(f =>
                    f.FarmHouseId == id &&
                    f.IsActive &&
                    !f.IsDeleted);
        }

        public async Task<List<FarmHouse>> GetAllAsync()
        {
            return await _context.FarmHouses
                .Where(f => !f.IsDeleted)
                .OrderBy(f => f.FarmHouseName)
                .ToListAsync();
        }

        public async Task AddAsync(FarmHouse farmHouse)
        {
            await _context.FarmHouses.AddAsync(farmHouse);
        }

        public Task UpdateAsync(FarmHouse farmHouse)
        {
            _context.FarmHouses.Update(farmHouse);
            return Task.CompletedTask;
        }
    }

}
