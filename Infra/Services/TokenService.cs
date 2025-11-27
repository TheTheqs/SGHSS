// Infra/Services/TokenService.cs

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SGHSS.Application.Interfaces.Services;
using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SGHSS.Infra.Services
{
    /// <summary>
    /// Implementação concreta responsável por gerar tokens JWT
    /// contendo as informações essenciais para autenticação e autorização.
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Gera um token JWT para o usuário informado.
        /// </summary>
        /// <param name="user">Usuário autenticado.</param>
        /// <param name="accessLevel">Nível de acesso efetivo do usuário.</param>
        /// <returns>Token JWT e sua expiração.</returns>
        public (string Token, DateTimeOffset ExpiresAt) GenerateToken(User user, AccessLevel accessLevel)
        {
            var secret = _configuration["Jwt:SecretKey"]
                ?? throw new InvalidOperationException("Jwt:SecretKey não configurada no appsettings.json.");

            var issuer = _configuration["Jwt:Issuer"] ?? "sghss-api";
            var audience = _configuration["Jwt:Audience"] ?? "sghss-users";

            var expiresInMinutes = int.TryParse(_configuration["Jwt:ExpiresInMinutes"], out int minutes)
                ? minutes : 120;

            var expiresAt = DateTimeOffset.UtcNow.AddMinutes(minutes);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Claims essenciais
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),
                new Claim("user_type", user.GetType().Name),
                new Claim("access_level", accessLevel.ToString())
            };

            var tokenDescriptor = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt.UtcDateTime,
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            string tokenString = tokenHandler.WriteToken(tokenDescriptor);

            return (tokenString, expiresAt);
        }
    }
}
