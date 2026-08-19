using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Domain.Entities;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FarmStay.Infrastructure.Repositories.Auth
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    !x.IsDeleted);
        }

        public async Task<User?> GetByEmailAsync(string email, int farmHouseId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x =>
                    !x.IsDeleted &&
                    x.Email == email &&
                    x.FarmHouseId == farmHouseId);
        }

        public async Task<User?> GetByMobileNumberAsync(string mobileNumber, int farmHouseId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x =>
                    !x.IsDeleted &&
                    x.MobileNumber == mobileNumber &&
                    x.FarmHouseId == farmHouseId);
        }

        public async Task<User?> GetByEmailVerificationTokenAsync(
            int userId,
            int farmHouseId,
            string token)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x =>
                    !x.IsDeleted &&
                    x.UserId == userId &&
                    x.FarmHouseId == farmHouseId &&
                    x.EmailVerificationToken == token);
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.FullName)
                .ToListAsync();
        }

        public Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            return Task.CompletedTask;
        }
    }
}
