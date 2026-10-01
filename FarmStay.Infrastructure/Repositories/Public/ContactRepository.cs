using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Domain.Entities;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmStay.Infrastructure.Repositories.Public
{
    public class ContactRepository : IContactRepository
    {
        private readonly AppDbContext _context;

        public ContactRepository(AppDbContext context)
        {
            _context = context;
        }


        public async Task AddAsync(ContactUs contact)
        {
            await _context.ContactDetail.AddAsync(contact);
        }

        public async Task<List<ContactUs>> GetAllAsync(int farmHouseId)
        {
            return await _context.ContactDetail
                .AsNoTracking()
                .Where(x =>
                    x.FarmHouseId == farmHouseId &&
                    !x.IsDelete)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();
        }

        // SOFT DELETE

       
public async Task DeleteAsync(int contactId)
        {
            var contact = await _context.ContactDetail
                .FirstOrDefaultAsync(x =>
                    x.ContactId == contactId &&
                    !x.IsDelete);

            if (contact != null)
            {
                contact.IsDelete = true;
            }
        }


    }
}