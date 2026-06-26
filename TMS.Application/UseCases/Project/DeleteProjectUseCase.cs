using AutoMapper;
using TMS.Domain;

namespace TMS.Application
{
    public interface IDeleteProjectUseCase
    {
        System.Threading.Tasks.Task<ServiceResult<ProjectResponse>> ExecuteAsync(Guid id);
    }

    public class DeleteProjectUseCase : IDeleteProjectUseCase
    {
        private readonly IProjectService _projectService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public DeleteProjectUseCase(
            IProjectService projectService,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _projectService = projectService;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async System.Threading.Tasks.Task<ServiceResult<ProjectResponse>> ExecuteAsync(Guid id)
        {
            var ownerId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(ownerId))
                return ServiceResult<ProjectResponse>.Unauthorized();

            var getResult = await _projectService.GetTrackedByIdAndOwnerAsync(id, ownerId);
            if (!getResult.Success || getResult.Data is null)
                return ServiceResult<ProjectResponse>.NotFound(getResult.Messages);

            var project = getResult.Data;
            project.SoftDelete(ProjectAuditHelper.TryParseAuditUserId(ownerId) ?? Guid.Empty);

            var result = await _projectService.UpdateAsync(project);
            if (!result.Success || result.Data is null)
                return ServiceResult<ProjectResponse>.BadRequest(result.Messages);

            return ServiceResult<ProjectResponse>.Ok(_mapper.Map<ProjectResponse>(result.Data));
        }
    }
}
