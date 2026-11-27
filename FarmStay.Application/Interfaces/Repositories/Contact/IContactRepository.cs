using FarmStay.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmStay.Application.Interfaces.Repositories
{
   public interface IContactRepository
    {

        Task AddAsync(ContactUs contact);

        Task<List<ContactUs>> GetAllAsync(int farmHouseId);

        Task DeleteAsync(int contactId);


        
    }
}
