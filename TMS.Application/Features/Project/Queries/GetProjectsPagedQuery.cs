using MediatR;

namespace TMS.Application.Features.Project.Queries
{
    public record GetProjectsPagedQuery(GetProjectsPagedRequest Request) : IRequest<ServiceResult<PagedResult<ProjectResponse>>>;

    public class GetProjectsPagedHandler : IRequestHandler<GetProjectsPagedQuery, ServiceResult<PagedResult<ProjectResponse>>>
    {
        private readonly IGetProjectsPagedUseCase _useCase;

        public GetProjectsPagedHandler(IGetProjectsPagedUseCase useCase) => _useCase = useCase;

        public System.Threading.Tasks.Task<ServiceResult<PagedResult<ProjectResponse>>> Handle(GetProjectsPagedQuery request, CancellationToken cancellationToken)
            => _useCase.ExecuteAsync(request.Request);
    }
}
