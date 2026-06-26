namespace TMS.Application
{
    public interface IRegisterUserUseCase
    {
        Task<ServiceResult<AuthResponse>> ExecuteAsync(RegisterRequest request);
    }

    public class RegisterUserUseCase : IRegisterUserUseCase
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        public RegisterUserUseCase(IUserService userService, IAuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        public async Task<ServiceResult<AuthResponse>> ExecuteAsync(RegisterRequest request)
        {
            var existingUser = await _userService.GetByEmailAsync(request.Email);
            if (existingUser.Success)
            {
                return ServiceResult<AuthResponse>.Conflict("User already exists.");
            }

            var registerResult = await _userService.RegisterAsync(
                request.Email,
                request.FirstName,
                request.LastName,
                request.Password);

            if (!registerResult.Success || registerResult.Data is null)
            {
                return ServiceResult<AuthResponse>.BadRequest(registerResult.Messages);
            }

            var authResponse = await _authService.BuildAuthResponseAsync(registerResult.Data);
            return ServiceResult<AuthResponse>.Ok(authResponse);
        }
    }
}
