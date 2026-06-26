using AutoMapper;

namespace TMS.Application
{
    public interface IGetProjectsPagedUseCase
    {
        System.Threading.Tasks.Task<ServiceResult<PagedResult<ProjectResponse>>> ExecuteAsync(GetProjectsPagedRequest request);
    }

    public class GetProjectsPagedUseCase : IGetProjectsPagedUseCase
    {
        private readonly IProjectService _projectService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public GetProjectsPagedUseCase(
            IProjectService projectService,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _projectService = projectService;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async System.Threading.Tasks.Task<ServiceResult<PagedResult<ProjectResponse>>> ExecuteAsync(GetProjectsPagedRequest request)
        {
            var ownerId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(ownerId))
                return ServiceResult<PagedResult<ProjectResponse>>.Unauthorized();

            var result = await _projectService.GetAllPagedAsync(
                request.PageNumber,
                request.PageSize,
                p => p.OwnerId == ownerId && !p.IsDeleted);

            if (!result.Success || result.Data is null)
                return ServiceResult<PagedResult<ProjectResponse>>.BadRequest(result.Messages);

            var mapped = new PagedResult<ProjectResponse>
            {
                Items = _mapper.Map<IEnumerable<ProjectResponse>>(result.Data.Items),
                PageNumber = result.Data.PageNumber,
                PageSize = result.Data.PageSize,
                TotalRecords = result.Data.TotalRecords
            };

            return ServiceResult<PagedResult<ProjectResponse>>.Ok(mapped);
        }
    }
}
