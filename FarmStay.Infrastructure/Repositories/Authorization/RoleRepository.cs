using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Domain.Entities;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FarmStay.Infrastructure.Repositories.Auth
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Role?> GetByIdAsync(int roleId)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(x =>
                    x.RoleId == roleId &&
                    !x.IsDeleted);
        }

        public async Task<Role?> GetByRoleNameAsync(string roleName)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(x =>
                    x.RoleName == roleName &&
                    !x.IsDeleted);
        }

        public async Task<List<Role>> GetAllAsync()
        {
            return await _context.Roles
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.RoleName)
                .ToListAsync();
        }

        public async Task AddAsync(Role role)
        {
            await _context.Roles.AddAsync(role);
        }

        public Task UpdateAsync(Role role)
        {
            _context.Roles.Update(role);
            return Task.CompletedTask;
        }
    }
}