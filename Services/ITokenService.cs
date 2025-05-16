using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using User_Authapi.DTO_s;
using User_Authapi.Entities;

namespace User_Authapi.Services
{
    public interface ITokenService
    {
        string CreateAccessToken(Person person);
        RefreshTokens CreateRefreshToken(string ipAddress);
    }
    public class TokenService : ITokenService 
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreateAccessToken(Person person) 
        {

            if (person.Id == default)
                throw new ArgumentException("User ID is invalid", nameof(person.Id));

            if (person == null)
                throw new ArgumentException("The User is empty from the database", nameof(person));

            if (string.IsNullOrWhiteSpace(person.UserName))
                throw new ArgumentException("Username is required", nameof(person.UserName));

            if (string.IsNullOrWhiteSpace(person.Email))
                throw new ArgumentException("Email is required", nameof(person.Email));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, person.Id.ToString()),
                new Claim(ClaimTypes.Name, person.UserName),
                new Claim(ClaimTypes.Email, person.Email)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration.GetValue<String>("JwtSettings:AccessToken")!))
                              ?? throw new InvalidOperationException("JWT Accesstoken key is not configured.");

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration.GetValue<String>("JwtSettings:Issuer")
                             ?? throw new InvalidOperationException("JWT issuer is not configured."),
                audience: _configuration.GetValue<String>("JwtSettings:Audience")
                             ?? throw new InvalidOperationException("JWT audience is not configured."),
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        public RefreshTokens CreateRefreshToken(string ipAddress)
        {
            return new RefreshTokens
            {
                RefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };
        }

    }
}
