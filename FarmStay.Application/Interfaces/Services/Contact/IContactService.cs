using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmStay.Application.Interfaces.Services.Public
{
  public interface IContactService
    {

        Task<ApiResponse<ContactResponseDto>> SaveContactAsync(ContactRequestDto dto); 
        Task<ApiResponse<List<ContactResponseDto>>> GetContactAllAsync(); 
        Task<ApiResponse<ContactResponseDto>> DeleteContactAsync(int contactId);
    }
}
