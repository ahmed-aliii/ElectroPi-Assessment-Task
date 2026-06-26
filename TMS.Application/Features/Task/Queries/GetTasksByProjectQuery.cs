using MediatR;

namespace TMS.Application.Features.Task.Queries
{
    public record GetTasksByProjectQuery(Guid ProjectId) : IRequest<ServiceResult<IReadOnlyList<TaskResponse>>>;

    public class GetTasksByProjectHandler : IRequestHandler<GetTasksByProjectQuery, ServiceResult<IReadOnlyList<TaskResponse>>>
    {
        private readonly IGetTasksByProjectUseCase _useCase;

        public GetTasksByProjectHandler(IGetTasksByProjectUseCase useCase) => _useCase = useCase;

        public System.Threading.Tasks.Task<ServiceResult<IReadOnlyList<TaskResponse>>> Handle(GetTasksByProjectQuery request, CancellationToken cancellationToken)
            => _useCase.ExecuteAsync(request.ProjectId);
    }
}
