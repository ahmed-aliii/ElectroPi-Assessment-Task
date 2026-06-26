using AutoMapper;
using TMS.Domain;

namespace TMS.Application
{
    public interface IUpdateProjectUseCase
    {
        System.Threading.Tasks.Task<ServiceResult<ProjectResponse>> ExecuteAsync(Guid id, UpdateProjectRequest request);
    }

    public class UpdateProjectUseCase : IUpdateProjectUseCase
    {
        private readonly IProjectService _projectService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public UpdateProjectUseCase(
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

        public async System.Threading.Tasks.Task<ServiceResult<ProjectResponse>> ExecuteAsync(Guid id, UpdateProjectRequest request)
        {
            var ownerId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(ownerId))
                return ServiceResult<ProjectResponse>.Unauthorized();

            var getResult = await _projectService.GetTrackedByIdAndOwnerAsync(id, ownerId);
            if (!getResult.Success || getResult.Data is null)
                return ServiceResult<ProjectResponse>.NotFound(getResult.Messages);

            var project = getResult.Data;
            project.Update(request.Name, request.Description, ProjectAuditHelper.TryParseAuditUserId(ownerId));

            var result = await _projectService.UpdateAsync(project);
            if (!result.Success || result.Data is null)
                return ServiceResult<ProjectResponse>.BadRequest(result.Messages);

            await _cacheService.RemoveAsync(CacheKeys.ProjectById(ownerId, id));
            await _cacheService.RemoveByPrefixAsync(CacheKeys.ProjectsForUserPrefix(ownerId));

            return ServiceResult<ProjectResponse>.Ok(_mapper.Map<ProjectResponse>(result.Data));
        }
    }
}
