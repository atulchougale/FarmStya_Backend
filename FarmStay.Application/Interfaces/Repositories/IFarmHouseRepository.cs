using FarmStay.Domain.Entities;

namespace FarmStay.Application.Interfaces.Repositories
{
    public interface IFarmHouseRepository
    {
        Task<FarmHouse?> GetByIdAsync(int id);

        Task<FarmHouse?> GetByDomainAsync(string domainName);

        Task<List<FarmHouse>> GetAllAsync();

        Task<FarmHouse?> GetCurrentAsync();
        Task AddAsync(FarmHouse farmHouse);

        Task UpdateAsync(FarmHouse farmHouse);
    }
}