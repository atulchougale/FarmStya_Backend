using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Domain.Entities;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FarmStay.Infrastructure.Repositories.Auth
{
    public class UserMembershipRepository : IUserMembershipRepository
    {
        private readonly AppDbContext _context;

        public UserMembershipRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserMembership?> GetMembershipAsync(int userId, int farmHouseId)
        {
            return await _context.UserMemberships
                .Include(x => x.User)
                .Include(x => x.Role)
                .Include(x => x.FarmHouse)
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.FarmHouseId == farmHouseId &&
                    x.IsActive &&
                    !x.IsDeleted);
        }

        public async Task<List<UserMembership>> GetByUserIdAsync(int userId)
        {
            return await _context.UserMemberships
                .Include(x => x.Role)
                .Include(x => x.FarmHouse)
                .Where(x =>
                    x.UserId == userId &&
                    x.IsActive &&
                    !x.IsDeleted)
                .ToListAsync();
        }

        public async Task<List<UserMembership>> GetByFarmHouseIdAsync(int farmHouseId)
        {
            return await _context.UserMemberships
                .Include(x => x.User)
                .Include(x => x.Role)
                .Where(x =>
                    x.FarmHouseId == farmHouseId &&
                    x.IsActive &&
                    !x.IsDeleted)
                .ToListAsync();
        }

        public async Task AddAsync(UserMembership membership)
        {
            await _context.UserMemberships.AddAsync(membership);
        }

        public Task UpdateAsync(UserMembership membership)
        {
            _context.UserMemberships.Update(membership);
            return Task.CompletedTask;
        }
    }
}