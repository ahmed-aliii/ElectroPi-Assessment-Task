using MediatR;

namespace TMS.Application.Features
{
    public record LoginCommand(LoginRequest Request) : IRequest<ServiceResult<AuthResponse>>;

    public class LoginHandler : IRequestHandler<LoginCommand, ServiceResult<AuthResponse>>
    {
        private readonly ILoginUseCase _loginUseCase;

        public LoginHandler(ILoginUseCase loginUseCase)
        {
            _loginUseCase = loginUseCase;
        }

        public async Task<ServiceResult<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            return await _loginUseCase.ExecuteAsync(request.Request);
        }
    }
}
