using FarmStay.Application.DTOs.Public;
using FarmStay.Application.Interfaces.Services.Public;
using FarmStay.Application.Services.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmStay.API.Controllers.Public
{
     [Route("api/[controller]")]
        [ApiController]
        public class ContactController : ControllerBase
        {

            private readonly IContactService _contactService;

            public ContactController(IContactService contactService)
            {
                _contactService = contactService;
            }



        [HttpPost("save-contact")]

        public async Task<IActionResult> SaveContactAsync(ContactRequestDto dto)
        {
            var result = await _contactService.SaveContactAsync(dto);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }


        [HttpGet]

        public async Task<IActionResult> GetContactAllAsync()
        {
            var result = await _contactService.GetContactAllAsync();

            return Ok(result);
        }


        
        [HttpDelete("{contactId:int}")]
        public async Task<IActionResult> DeleteContactAsync(int contactId)
        {
            var result = await _contactService.DeleteContactAsync(contactId);

            return Ok(result);
        }
    }


}
    
