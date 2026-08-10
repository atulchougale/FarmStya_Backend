using FarmStay.Domain.Entities;

namespace FarmStay.Application.Interfaces.Repositories
{
    public interface IRoleRepository
    {
        Task<Role?> GetByIdAsync(int roleId);

        Task<Role?> GetByRoleNameAsync(string roleName);

        Task<List<Role>> GetAllAsync();

        Task AddAsync(Role role);

        Task UpdateAsync(Role role);
    }
}