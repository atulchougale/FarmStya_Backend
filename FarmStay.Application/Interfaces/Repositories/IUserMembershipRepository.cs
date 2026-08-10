using FarmStay.Domain.Entities;

namespace FarmStay.Application.Interfaces.Repositories
{
    public interface IUserMembershipRepository
    {
        Task<UserMembership?> GetMembershipAsync(int userId, int farmHouseId);

        Task<List<UserMembership>> GetByUserIdAsync(int userId);

        Task<List<UserMembership>> GetByFarmHouseIdAsync(int farmHouseId);

        Task AddAsync(UserMembership membership);

        Task UpdateAsync(UserMembership membership);
    }
}