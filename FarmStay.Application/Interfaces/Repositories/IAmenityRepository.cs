using FarmStay.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmStay.Application.Interfaces.Repositories
{
   public interface IAmenityRepository
    {
        Task AddAsync(Amenity amenity);

        Task<Amenity?>GetById(int imageId);

        Task<List<Amenity>> GetAllAsync(int farmHouseId);

        Task<Amenity?> GetByTitleAsync(int farmHouseId, string title);

        Task UpdateAsync(Amenity amenity);

        //Task DeleteAsync(int ImageId, int userId);

    }
}
