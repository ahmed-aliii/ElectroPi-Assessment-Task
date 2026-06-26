using AutoMapper;
using TMS.Domain;

namespace TMS.Application
{
    public interface ICreateProjectUseCase
    {
        System.Threading.Tasks.Task<ServiceResult<ProjectResponse>> ExecuteAsync(CreateProjectRequest request);
    }

    public class CreateProjectUseCase : ICreateProjectUseCase
    {
        private readonly IProjectService _projectService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public CreateProjectUseCase(
            IProjectService projectService,
            ICurrentUserService currentUserService,
            IMapper mapper,
            ICacheService cacheService)
        {
            _projectService = projectService;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async System.Threading.Tasks.Task<ServiceResult<ProjectResponse>> ExecuteAsync(CreateProjectRequest request)
        {
            var ownerId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(ownerId))
                return ServiceResult<ProjectResponse>.Unauthorized();

            var project = Project.Create(
                request.Name,
                request.Description,
                ownerId,
                ProjectAuditHelper.TryParseAuditUserId(ownerId));

            var result = await _projectService.CreateAsync(project);
            if (!result.Success || result.Data is null)
                return ServiceResult<ProjectResponse>.BadRequest(result.Messages);

            await _cacheService.RemoveByPrefixAsync(CacheKeys.ProjectsForUserPrefix(ownerId));

            return ServiceResult<ProjectResponse>.Created(_mapper.Map<ProjectResponse>(result.Data));
        }
    }
}
