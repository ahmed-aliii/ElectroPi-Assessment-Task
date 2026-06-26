using Microsoft.AspNetCore.Identity;
using TMS.Domain;

namespace TMS.Application
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(IUserRepository userRepository, UserManager<ApplicationUser> userManager)
        {
            _userRepository = userRepository;
            _userManager = userManager;
        }

        public async Task<ServiceResult<ApplicationUser>> GetByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user is null)
                return ServiceResult<ApplicationUser>.NotFound("User not found.");

            return ServiceResult<ApplicationUser>.Ok(user);
        }

        public async Task<ServiceResult<bool>> CheckPasswordAsync(ApplicationUser user, string password)
        {
            var isValid = await _userManager.CheckPasswordAsync(user, password);
            return ServiceResult<bool>.Ok(isValid);
        }

        public async Task<ServiceResult<ApplicationUser>> RegisterAsync(
            string email,
            string firstName,
            string lastName,
            string password)
        {
            var user = ApplicationUser.Register(email, firstName, lastName);

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                return ServiceResult<ApplicationUser>.BadRequest("Failed to register user.");

            await _userManager.AddToRoleAsync(user, AppRoles.User);

            return ServiceResult<ApplicationUser>.Ok(user);
        }
    }
}
