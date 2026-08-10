using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Domain.Entities;
using FarmStay.Domain.Enums;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FarmStay.Infrastructure.Repositories.Auth
{
    public class UserOtpRepository : IUserOtpRepository
    {
        private readonly AppDbContext _context;

        public UserOtpRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(UserOtp userOtp)
        {
            await _context.UserOtps.AddAsync(userOtp);
        }

        public async Task<UserOtp?> GetActiveOtpAsync(int userId, OtpPurpose purpose)
        {
            return await _context.UserOtps
                .Where(x =>
                    x.UserId == userId &&
                    x.Purpose == purpose &&
                    x.IsActive &&
                    !x.IsDeleted &&
                    !x.IsUsed)
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync();
        }

        public async Task<UserOtp?> GetByOtpAsync(int userId, string otpCode, OtpPurpose purpose)
        {
            return await _context.UserOtps
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.OtpCode == otpCode &&
                    x.Purpose == purpose &&
                    x.IsActive &&
                    !x.IsDeleted &&
                    !x.IsUsed);
        }


        public Task UpdateAsync(UserOtp userOtp)
        {
            _context.UserOtps.Update(userOtp);
            return Task.CompletedTask;
        }
    }
}