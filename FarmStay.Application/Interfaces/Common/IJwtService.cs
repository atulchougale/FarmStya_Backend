using FarmStay.Domain.Entities;
using System.Security.Claims;

namespace FarmStay.Application.Interfaces.Common
{
    public interface IJwtService
    {
        string GenerateAccessToken(UserMembership membership);

        string GenerateRefreshToken();

        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}