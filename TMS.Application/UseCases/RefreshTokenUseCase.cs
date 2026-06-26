using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TMS.Domain;

namespace TMS.Application
{
    public interface IRefreshTokenUseCase
    {
        Task<ServiceResult<AuthResponse>> ExecuteAsync(RefreshTokenRequest request);
    }

    public class RefreshTokenUseCase : IRefreshTokenUseCase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthService _authService;

        public RefreshTokenUseCase(
            UserManager<ApplicationUser> userManager,
            IAuthService authService)
        {
            _userManager = userManager;
            _authService = authService;
        }

        public async Task<ServiceResult<AuthResponse>> ExecuteAsync(RefreshTokenRequest request)
        {
            var principal = _authService.GetPrincipalFromExpiredToken(request.AccessToken);
            var email = principal.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
            {
                return ServiceResult<AuthResponse>.Unauthorized("Invalid access token.");
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null || !user.IsRefreshTokenValid(request.RefreshToken))
            {
                return ServiceResult<AuthResponse>.Unauthorized("Invalid refresh token.");
            }

            var authResponse = await _authService.BuildAuthResponseAsync(user);
            return ServiceResult<AuthResponse>.Ok(authResponse);
        }
    }
}
