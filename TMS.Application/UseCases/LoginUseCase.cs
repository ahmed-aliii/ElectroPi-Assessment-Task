using Microsoft.AspNetCore.Identity;
using TMS.Domain;

namespace TMS.Application
{
    public interface ILoginUseCase
    {
        Task<ServiceResult<AuthResponse>> ExecuteAsync(LoginRequest request);
    }

    public class LoginUseCase : ILoginUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthService _authService;

        public LoginUseCase(
            IUserRepository userRepository,
            UserManager<ApplicationUser> userManager,
            IAuthService authService)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _authService = authService;
        }

        public async Task<ServiceResult<AuthResponse>> ExecuteAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null || !user.IsActive)
            {
                return ServiceResult<AuthResponse>.Unauthorized("Invalid credentials.");
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
            {
                return ServiceResult<AuthResponse>.Unauthorized("Invalid credentials.");
            }

            var authResponse = await _authService.BuildAuthResponseAsync(user);
            return ServiceResult<AuthResponse>.Ok(authResponse);
        }
    }
}
