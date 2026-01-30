using Kickify.Application.Abstractions.Authentication;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Infrastructure.Database;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BrewView.Infrastructure.Authentication
{
    internal sealed class JwtProvider : IJwtProvider
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _dbContext;

        public JwtProvider(IConfiguration configuration, ApplicationDbContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        public async Task<string> GetForCredentialsAsync(string email)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            return GenerateBackendJwt(user);
        }

        private string GenerateBackendJwt(string email, UserRole role, string name, Guid id, string identityId)
        {
            var secretKey = _configuration["Authentication:SecretKey"]!;
            var issuer = _configuration["Authentication:Issuer"]!;
            var audience = _configuration["Authentication:Audience"]!;
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);

            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role.ToString()),
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.NameIdentifier, id.ToString())
            };
            if (!string.IsNullOrWhiteSpace(identityId))
            {
                claims.Add(new Claim("IdentityId", identityId));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = issuer,
                Audience = audience,
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMonths(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(keyBytes),
                    SecurityAlgorithms.HmacSha256Signature)
            }; 

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        public string GenerateBackendJwt(User user)
        {
            return GenerateBackendJwt(user.Email, user.Role, user.FullName ?? string.Empty, user.UserId, user.IdentityId ?? string.Empty);
        }

        public string GenerateRefreshToken()
        {
            byte[] randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            return WebEncoders.Base64UrlEncode(randomBytes);
        }

        public class AuthToken
        {
            [JsonPropertyName("kind")]
            public string Kind { get; set; }

            [JsonPropertyName("localId")]
            public string LocalId { get; set; }

            [JsonPropertyName("email")]
            public string Email { get; set; }

            [JsonPropertyName("displayName")]
            public string DisplayName { get; set; }

            [JsonPropertyName("idToken")]
            public string IdToken { get; set; }

            [JsonPropertyName("registered")]
            public bool Registered { get; set; }

            [JsonPropertyName("refreshToken")]
            public string RefreshToken { get; set; }

            [JsonPropertyName("expiresIn")]
            public string ExpiresIn { get; set; }
        }
    }
}
