using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.Public;
using FarmStay.Application.Interfaces.Common;
using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Application.Interfaces.Services.Public;
using FarmStay.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmStay.Application.Services.Public
{
    public class ContactService : IContactService
    {
        private readonly IContactRepository _contactRepository;
        private readonly ILogger<ContactService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFarmHouseRepository _farmHouseRepository;
        private readonly IUserMembershipRepository _userMembershipRepository;

        public ContactService(
      IContactRepository contactRepository,
      ILogger<ContactService> logger,
      IHttpContextAccessor httpContextAccessor,
      IUnitOfWork unitOfWork,
      IFarmHouseRepository farmHouseRepository,
      IUserMembershipRepository userMembershipRepository)
        {
            _contactRepository = contactRepository;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
            _farmHouseRepository = farmHouseRepository;
            _userMembershipRepository = userMembershipRepository;
        }

        public async Task<ApiResponse<ContactResponseDto>> SaveContactAsync(ContactRequestDto dto)
        {
            try
            {
                _logger.LogInformation(
                   "Contact  Save request received.");

                // Get FarmHouseId from Header

                var farmHouseIdHeader = _httpContextAccessor.HttpContext?
                    .Request
                    .Headers["FarmHouseId"]
                    .FirstOrDefault();

                if (!int.TryParse(farmHouseIdHeader, out int farmHouseId))
                {
                    _logger.LogWarning(
                        "FarmHouseId header is missing or invalid.");

                    return new ApiResponse<ContactResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                // Step 3
                // Validate FarmHouse

                var farmHouse =
                    await _farmHouseRepository.GetByIdAsync(farmHouseId);

                if (farmHouse == null)
                {
                    _logger.LogWarning(
                        "Invalid FarmHouse. FarmHouseId: {FarmHouseId}",
                        farmHouseId);

                    return new ApiResponse<ContactResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                _logger.LogInformation(
                    "FarmHouse validated successfully. FarmHouseId: {FarmHouseId}",
                    farmHouseId);


                //Create Contact 


                var contact = new ContactUs
                {
                    FarmHouseId = farmHouseId,

                    FullName = dto.FullName,

                    Email = dto.Email,

                    Message = dto.Message,
                    CreatedDate = DateTime.Now,

                    IsDelete = false,
                };

                //Save Contact

                await _contactRepository.AddAsync(contact);

                await _unitOfWork.SaveChangesAsync();


                _logger.LogInformation(
                  "Feedback created successfully. FarmHouseId: {FarmHouseId}, ContactId: {ContactId}",
                  farmHouseId,
                  contact.ContactId);


                //Get Response

                var response = new ContactResponseDto
                {
                    ContactId = dto.ContactId,
                    FarmHouseId = farmHouseId,

                    FullName = dto.FullName,

                    Email = dto.Email,

                    Message = dto.Message,


                };


                return new ApiResponse<ContactResponseDto>
                {
                    Success = true,
                    Message = "Feedback saved successfully.",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while saving Contact.");

                return new ApiResponse<ContactResponseDto>
                {
                    Success = false,
                    Message = "Something went wrong while saving Contact."
                };
            }
        }


        public async Task<ApiResponse<List<ContactResponseDto>>> GetContactAllAsync()
        {
            try
            {
                _logger.LogInformation("Get all Contact request received.");

                // Get FarmHouseId from Header
                var farmHouseIdHeader = _httpContextAccessor.HttpContext?
                    .Request
                    .Headers["FarmHouseId"]
                    .FirstOrDefault();

                if (!int.TryParse(farmHouseIdHeader, out int farmHouseId))
                {
                    _logger.LogWarning("FarmHouseId header is missing or invalid.");

                    return new ApiResponse<List<ContactResponseDto>>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                // Validate FarmHouse
                var farmHouse = await _farmHouseRepository
                    .GetByIdAsync(farmHouseId);

                if (farmHouse == null)
                {
                    return new ApiResponse<List<ContactResponseDto>>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                // Get Contacts
                var contacts = await _contactRepository
                    .GetAllAsync(farmHouseId);

                var response = contacts.Select(x => new ContactResponseDto
                {
                    ContactId = x.ContactId,
                    FarmHouseId = x.FarmHouseId,
                    FullName = x.FullName,
                    Email = x.Email,
                    Message = x.Message
                }).ToList();

                return new ApiResponse<List<ContactResponseDto>>
                {
                    Success = true,
                    Message = "Contacts retrieved successfully.",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting Contacts.");

                return new ApiResponse<List<ContactResponseDto>>
                {
                    Success = false,
                    Message = "Something went wrong while getting Contacts."
                };
            }
        }



      


public async Task<ApiResponse<ContactResponseDto>> DeleteContactAsync(int contactId)
        {
            try
            {
                _logger.LogInformation(
                    "Delete Contact request received. ContactId: {ContactId}",
                    contactId);

                // Get FarmHouseId from Header
                var farmHouseIdHeader = _httpContextAccessor.HttpContext?
                    .Request
                    .Headers["FarmHouseId"]
                    .FirstOrDefault();

                if (!int.TryParse(farmHouseIdHeader, out int farmHouseId))
                {
                    _logger.LogWarning(
                        "FarmHouseId header is missing or invalid.");

                    return new ApiResponse<ContactResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                // Validate FarmHouse
                var farmHouse = await _farmHouseRepository
                    .GetByIdAsync(farmHouseId);

                if (farmHouse == null)
                {
                    _logger.LogWarning(
                        "Invalid FarmHouse. FarmHouseId: {FarmHouseId}",
                        farmHouseId);

                    return new ApiResponse<ContactResponseDto>
                    {
                        Success = false,
                        Message = "Invalid FarmHouse."
                    };
                }

                // Soft Delete Contact
                await _contactRepository.DeleteAsync(contactId);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Contact deleted successfully. ContactId: {ContactId}",
                    contactId);

                return new ApiResponse<ContactResponseDto>
                {
                    Success = true,
                    Message = "Contact deleted successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while deleting Contact. ContactId: {ContactId}",
                    contactId);

                return new ApiResponse<ContactResponseDto>
                {
                    Success = false,
                    Message = "Something went wrong while deleting Contact."
                };
            }
        }
    }
}
