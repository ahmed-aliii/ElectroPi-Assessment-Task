using MediatR;

namespace TMS.Application
{
    public record RegisterUserCommand(RegisterRequest Request) : IRequest<ServiceResult<AuthResponse>>;

    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, ServiceResult<AuthResponse>>
    {
        private readonly IRegisterUserUseCase _registerUseCase;

        public RegisterUserHandler(IRegisterUserUseCase registerUseCase)
        {
            _registerUseCase = registerUseCase;
        }

        public async Task<ServiceResult<AuthResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            return await _registerUseCase.ExecuteAsync(request.Request);
        }
    }
}
