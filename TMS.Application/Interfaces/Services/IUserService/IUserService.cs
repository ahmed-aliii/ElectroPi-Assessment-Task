using TMS.Domain;

namespace TMS.Application
{
    public interface IUserService
    {
        Task<ServiceResult<ApplicationUser>> GetByEmailAsync(string email);
        Task<ServiceResult<bool>> CheckPasswordAsync(ApplicationUser user, string password);
        Task<ServiceResult<ApplicationUser>> RegisterAsync(string email, string firstName, string lastName, string password);
    }
}
