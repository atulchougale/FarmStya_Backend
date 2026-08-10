using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.DTOs.Public;

namespace FarmStay.Application.Interfaces.Services.Public
{
    public interface IPublicSiteService
    {
        Task<ApiResponse<PublicSiteDto>> GetSiteAsync();
    }
}