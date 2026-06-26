using MediatR;

namespace TMS.Application.Features.Project.Queries
{
    public record GetProjectByIdQuery(Guid Id) : IRequest<ServiceResult<ProjectResponse>>;

    public class GetProjectByIdHandler : IRequestHandler<GetProjectByIdQuery, ServiceResult<ProjectResponse>>
    {
        private readonly IGetProjectByIdUseCase _useCase;

        public GetProjectByIdHandler(IGetProjectByIdUseCase useCase) => _useCase = useCase;

        public System.Threading.Tasks.Task<ServiceResult<ProjectResponse>> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
            => _useCase.ExecuteAsync(request.Id);
    }
}
