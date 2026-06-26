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

        public GetProjectByIdUseCase(
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

            var result = await _projectService.GetByIdAndOwnerAsync(id, ownerId);
            if (!result.Success || result.Data is null)
                return ServiceResult<ProjectResponse>.NotFound(result.Messages);

            return ServiceResult<ProjectResponse>.Ok(_mapper.Map<ProjectResponse>(result.Data));
        }
    }
}
