using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TMS.Domain;

namespace TMS.Application
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthService(IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        public async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            IList<Claim> userClaims =
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            ];

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                userClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            userClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

            var key = _configuration["jwt:key"] ?? throw new InvalidOperationException("JWT Key is missing in configuration.");
            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256);

            var issuer = _configuration["jwt:issuer"];
            var audience = _configuration["jwt:audience"];
            var expirationMinutes = Convert.ToInt32(_configuration["jwt:accessTokenExpirationMinutes"] ?? "60");

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                claims: userClaims,
                signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var key = _configuration["jwt:key"] ?? throw new InvalidOperationException("JWT Key is missing in configuration.");

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        public DateTime GetAccessTokenExpiration()
        {
            var expirationMinutes = Convert.ToInt32(_configuration["jwt:accessTokenExpirationMinutes"] ?? "60");
            return DateTime.UtcNow.AddMinutes(expirationMinutes);
        }

        public DateTime GetRefreshTokenExpiration()
        {
            var refreshDays = Convert.ToInt32(_configuration["jwt:refreshTokenExpirationDays"] ?? "7");
            return DateTime.UtcNow.AddDays(refreshDays);
        }

        public async Task<AuthResponse> BuildAuthResponseAsync(ApplicationUser user)
        {
            var token = await GenerateJwtTokenAsync(user);
            var refreshToken = GenerateRefreshToken();
            var refreshExpiry = GetRefreshTokenExpiration();

            user.UpdateRefreshToken(refreshToken, refreshExpiry);
            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);

            return new AuthResponse(
                token,
                refreshToken,
                GetAccessTokenExpiration(),
                refreshExpiry,
                AuthenticatedUserDto.FromUser(user, roles));
        }
    }
}
