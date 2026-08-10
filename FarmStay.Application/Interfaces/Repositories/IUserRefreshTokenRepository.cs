using FarmStay.Domain.Entities;

namespace FarmStay.Application.Interfaces.Repositories
{
    public interface IUserRefreshTokenRepository
    {
        Task<UserRefreshToken?> GetByIdAsync(int userRefreshTokenId);

        Task<UserRefreshToken?> GetByTokenHashAsync(string refreshTokenHash);

        Task<List<UserRefreshToken>> GetByUserIdAsync(int userId);

        Task AddAsync(UserRefreshToken refreshToken);

        Task UpdateAsync(UserRefreshToken refreshToken);
    }
}