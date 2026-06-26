using MediatR;

namespace TMS.Application.Features.Project.Commands
{
    public record DeleteProjectCommand(Guid Id) : IRequest<ServiceResult<ProjectResponse>>;

    public class DeleteProjectHandler : IRequestHandler<DeleteProjectCommand, ServiceResult<ProjectResponse>>
    {
        private readonly IDeleteProjectUseCase _useCase;

        public DeleteProjectHandler(IDeleteProjectUseCase useCase) => _useCase = useCase;

        public System.Threading.Tasks.Task<ServiceResult<ProjectResponse>> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
            => _useCase.ExecuteAsync(request.Id);
    }
}
