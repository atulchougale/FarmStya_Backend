using FarmStay.Application.Common.Constants;
using FarmStay.Application.Interfaces.Common;
using FarmStay.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FarmStay.Infrastructure.Services.Auth
{
    public class JwtService : IJwtService
    {
        private readonly string _key;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly double _expiryMinutes;


        public JwtService(IConfiguration configuration)
        {
                _key = configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT Key is missing.");

            _issuer = configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("JWT Issuer is missing.");

            _audience = configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("JWT Audience is missing.");

            _expiryMinutes = Convert.ToDouble(
                configuration["Jwt:DurationInMinutes"]
                ?? throw new InvalidOperationException("JWT Duration is missing.")
            );
        }

        public string GenerateAccessToken(
            User user,
            UserMembership membership,
            FarmHouse farmHouse,
            Role role)
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(membership);
            ArgumentNullException.ThrowIfNull(farmHouse);
            ArgumentNullException.ThrowIfNull(role);

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, role.RoleName),

            new Claim(JwtClaimNames.FarmHouseId, farmHouse.FarmHouseId.ToString()),
            new Claim(JwtClaimNames.UserMembershipId, membership.UserMembershipId.ToString()),

            new Claim("RoleId", role.RoleId.ToString()),
            new Claim("FarmHouseName", farmHouse.FarmHouseName)
        };

            var signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_key)
            );

            var credentials = new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(_expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];

            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            return Convert.ToHexString(randomBytes);
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false,

                ValidIssuer = _issuer,
                ValidAudience = _audience,

                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_key)
                )
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(
                    token,
                    validationParameters,
                    out SecurityToken validatedToken);

                if (validatedToken is not JwtSecurityToken jwtToken ||
                    !jwtToken.Header.Alg.Equals(
                        SecurityAlgorithms.HmacSha256,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }

}
