using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Domain.Entities;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FarmStay.Infrastructure.Repositories.Auth
{
    public class UserRefreshTokenRepository : IUserRefreshTokenRepository
    {
        private readonly AppDbContext _context;


        public UserRefreshTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserRefreshToken?> GetByIdAsync(int userRefreshTokenId)
        {
            return await _context.UserRefreshTokens
                .FirstOrDefaultAsync(x =>
                    x.UserRefreshTokenId == userRefreshTokenId &&
                    !x.IsDeleted);
        }

        public async Task<UserRefreshToken?> GetByTokenHashAsync(string refreshTokenHash)
        {
            return await _context.UserRefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.RefreshTokenHash == refreshTokenHash &&
                    x.IsActive &&
                    !x.IsDeleted);
        }

        public async Task<List<UserRefreshToken>> GetByUserIdAsync(int userId)
        {
            return await _context.UserRefreshTokens
                .Where(x =>
                    x.UserId == userId &&
                    x.IsActive &&
                    !x.IsDeleted)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();
        }

        public async Task AddAsync(UserRefreshToken refreshToken)
        {
            await _context.UserRefreshTokens.AddAsync(refreshToken);
        }

        public Task UpdateAsync(UserRefreshToken refreshToken)
        {
            _context.UserRefreshTokens.Update(refreshToken);
            return Task.CompletedTask;
        }
    }

}
