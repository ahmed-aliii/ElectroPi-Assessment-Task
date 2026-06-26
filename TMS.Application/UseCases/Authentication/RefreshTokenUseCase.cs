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
        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        public RefreshTokenUseCase(IUserService userService, IAuthService authService)
        {
            _userService = userService;
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

            var userResult = await _userService.GetByEmailAsync(email);

            if (!userResult.Success || userResult.Data is null ||
                !userResult.Data.IsRefreshTokenValid(request.RefreshToken))
            {
                return ServiceResult<AuthResponse>.Unauthorized("Invalid refresh token.");
            }

            var authResponse = await _authService.BuildAuthResponseAsync(userResult.Data);
            return ServiceResult<AuthResponse>.Ok(authResponse);
        }
    }
}
