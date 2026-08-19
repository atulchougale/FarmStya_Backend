using FarmStay.Domain.Entities;
using System.Security.Claims;

namespace FarmStay.Application.Interfaces.Common
{
    public interface IJwtService
    {
        string GenerateAccessToken(
        User user,
        UserMembership membership,
        FarmHouse farmHouse,
        Role role);

    string GenerateRefreshToken();

        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }

}
