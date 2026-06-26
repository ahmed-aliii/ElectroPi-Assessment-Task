using MediatR;

namespace TMS.Application
{
    public record RefreshTokenCommand(RefreshTokenRequest Request) : IRequest<ServiceResult<AuthResponse>>;

    public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, ServiceResult<AuthResponse>>
    {
        private readonly IRefreshTokenUseCase _refreshTokenUseCase;

        public RefreshTokenHandler(IRefreshTokenUseCase refreshTokenUseCase)
        {
            _refreshTokenUseCase = refreshTokenUseCase;
        }

        public async Task<ServiceResult<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            return await _refreshTokenUseCase.ExecuteAsync(request.Request);
        }
    }
}
