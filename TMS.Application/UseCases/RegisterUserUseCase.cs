using Microsoft.AspNetCore.Identity;
using TMS.Domain;

namespace TMS.Application
{
    public interface IRegisterUserUseCase
    {
        Task<ServiceResult<AuthResponse>> ExecuteAsync(RegisterRequest request);
    }

    public class RegisterUserUseCase : IRegisterUserUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthService _authService;

        public RegisterUserUseCase(
            IUserRepository userRepository,
            UserManager<ApplicationUser> userManager,
            IAuthService authService)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _authService = authService;
        }

        public async Task<ServiceResult<AuthResponse>> ExecuteAsync(RegisterRequest request)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return ServiceResult<AuthResponse>.Conflict("User already exists.");
            }

            var user = ApplicationUser.Register(request.Email, request.FirstName, request.LastName);

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return ServiceResult<AuthResponse>.BadRequest("Failed to register user.");
            }

            await _userManager.AddToRoleAsync(user, AppRoles.User);

            var authResponse = await _authService.BuildAuthResponseAsync(user);
            return ServiceResult<AuthResponse>.Ok(authResponse);
        }
    }
}
