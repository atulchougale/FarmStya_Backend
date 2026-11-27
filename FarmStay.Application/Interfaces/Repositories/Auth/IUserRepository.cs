using FarmStay.Domain.Entities;

namespace FarmStay.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task AddAsync(User user);

        Task<User?> GetByIdAsync(int userId);

        Task<User?> GetByEmailAsync(string email, int farmHouseId);

        Task<User?> GetByMobileNumberAsync(string mobileNumber, int farmHouseId);

        Task<User?> GetByEmailVerificationTokenAsync(
            int userId,
            int farmHouseId,
            string token);

        Task<List<User>> GetAllAsync();

        Task UpdateAsync(User user);
    }

}

