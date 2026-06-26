using MediatR;

namespace TMS.Application.Features.Project.Commands
{
    public record CreateProjectCommand(CreateProjectRequest Request) : IRequest<ServiceResult<ProjectResponse>>;

    public class CreateProjectHandler : IRequestHandler<CreateProjectCommand, ServiceResult<ProjectResponse>>
    {
        private readonly ICreateProjectUseCase _useCase;

        public CreateProjectHandler(ICreateProjectUseCase useCase) => _useCase = useCase;

        public System.Threading.Tasks.Task<ServiceResult<ProjectResponse>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
            => _useCase.ExecuteAsync(request.Request);
    }
}
