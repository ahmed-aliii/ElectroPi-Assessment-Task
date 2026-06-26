using TMS.Domain;

namespace TMS.Application
{
    public interface ILoginUseCase
    {
        Task<ServiceResult<AuthResponse>> ExecuteAsync(LoginRequest request);
    }

    public class LoginUseCase : ILoginUseCase
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        public LoginUseCase(IUserService userService, IAuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        public async Task<ServiceResult<AuthResponse>> ExecuteAsync(LoginRequest request)
        {
            var userResult = await _userService.GetByEmailAsync(request.Email);
            if (!userResult.Success || userResult.Data is null || !userResult.Data.IsActive)
            {
                return ServiceResult<AuthResponse>.Unauthorized("Invalid credentials.");
            }

            var passwordResult = await _userService.CheckPasswordAsync(userResult.Data, request.Password);
            if (!passwordResult.Success || passwordResult.Data is not true)
            {
                return ServiceResult<AuthResponse>.Unauthorized("Invalid credentials.");
            }

            var authResponse = await _authService.BuildAuthResponseAsync(userResult.Data);
            return ServiceResult<AuthResponse>.Ok(authResponse);
        }
    }
}
