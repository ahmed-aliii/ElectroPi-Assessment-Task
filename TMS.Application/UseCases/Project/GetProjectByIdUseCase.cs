using AutoMapper;

namespace TMS.Application
{
    public interface IGetProjectByIdUseCase
    {
        System.Threading.Tasks.Task<ServiceResult<ProjectResponse>> ExecuteAsync(Guid id);
    }

    public class GetProjectByIdUseCase : IGetProjectByIdUseCase
    {
        private readonly IProjectService _projectService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public GetProjectByIdUseCase(
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

        public async System.Threading.Tasks.Task<ServiceResult<ProjectResponse>> ExecuteAsync(Guid id)
        {
            var ownerId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(ownerId))
                return ServiceResult<ProjectResponse>.Unauthorized();

            var cacheKey = CacheKeys.ProjectById(ownerId, id);
            var cachedProject = await _cacheService.GetAsync<ProjectResponse>(cacheKey);
            if (cachedProject is not null)
                return ServiceResult<ProjectResponse>.Ok(cachedProject);

            var result = await _projectService.GetByIdAndOwnerAsync(id, ownerId);
            if (!result.Success || result.Data is null)
                return ServiceResult<ProjectResponse>.NotFound(result.Messages);

            var mapped = _mapper.Map<ProjectResponse>(result.Data);
            await _cacheService.SetAsync(cacheKey, mapped, TimeSpan.FromMinutes(5));

            return ServiceResult<ProjectResponse>.Ok(mapped);
        }
    }
}
