using System.Security.Claims;
using TMS.Domain;

namespace TMS.Application
{
    public interface IAuthService
    {
        Task<string> GenerateJwtTokenAsync(ApplicationUser user);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        Task<AuthResponse> BuildAuthResponseAsync(ApplicationUser user);
        DateTime GetAccessTokenExpiration();
        DateTime GetRefreshTokenExpiration();
    }
}
