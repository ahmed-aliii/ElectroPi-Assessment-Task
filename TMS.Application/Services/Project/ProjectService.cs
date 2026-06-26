using TMS.Domain;

namespace TMS.Application
{
    public class ProjectService : GenericService<Project>, IProjectService
    {
        private readonly IProjectRepository _projectRepository;

        public ProjectService(IProjectRepository repository) : base(repository)
        {
            _projectRepository = repository;
        }

        public async System.Threading.Tasks.Task<ServiceResult<Project>> GetByIdAndOwnerAsync(Guid id, string ownerId)
        {
            var project = await _projectRepository.GetByIdAndOwnerAsync(id, ownerId);
            if (project is null)
                return ServiceResult<Project>.NotFound("Project not found.");

            return ServiceResult<Project>.Ok(project);
        }

        public async System.Threading.Tasks.Task<ServiceResult<Project>> GetTrackedByIdAndOwnerAsync(Guid id, string ownerId)
        {
            var project = await _projectRepository.GetTrackedByIdAndOwnerAsync(id, ownerId);
            if (project is null)
                return ServiceResult<Project>.NotFound("Project not found.");

            return ServiceResult<Project>.Ok(project);
        }
    }
}
