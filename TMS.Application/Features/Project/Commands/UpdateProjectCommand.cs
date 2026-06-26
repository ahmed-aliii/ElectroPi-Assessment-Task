using MediatR;

namespace TMS.Application.Features.Project.Commands
{
    public record UpdateProjectCommand(Guid Id, UpdateProjectRequest Request) : IRequest<ServiceResult<ProjectResponse>>;

    public class UpdateProjectHandler : IRequestHandler<UpdateProjectCommand, ServiceResult<ProjectResponse>>
    {
        private readonly IUpdateProjectUseCase _useCase;

        public UpdateProjectHandler(IUpdateProjectUseCase useCase) => _useCase = useCase;

        public System.Threading.Tasks.Task<ServiceResult<ProjectResponse>> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
            => _useCase.ExecuteAsync(request.Id, request.Request);
    }
}
