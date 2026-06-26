using DomainTask = TMS.Domain.Task;

namespace TMS.Application
{
    public class TaskService : GenericService<DomainTask>, ITaskService
    {
        private readonly ITaskRepository _taskRepository;

        public TaskService(ITaskRepository repository) : base(repository)
        {
            _taskRepository = repository;
        }

        public async System.Threading.Tasks.Task<ServiceResult<DomainTask>> GetByIdAndOwnerAsync(Guid taskId, string ownerId)
        {
            var task = await _taskRepository.GetByIdAndOwnerAsync(taskId, ownerId);
            if (task is null)
                return ServiceResult<DomainTask>.NotFound("Task not found.");

            return ServiceResult<DomainTask>.Ok(task);
        }

        public async System.Threading.Tasks.Task<ServiceResult<DomainTask>> GetTrackedByIdAndOwnerAsync(Guid taskId, string ownerId)
        {
            var task = await _taskRepository.GetTrackedByIdAndOwnerAsync(taskId, ownerId);
            if (task is null)
                return ServiceResult<DomainTask>.NotFound("Task not found.");

            return ServiceResult<DomainTask>.Ok(task);
        }

        public async System.Threading.Tasks.Task<ServiceResult<IReadOnlyList<DomainTask>>> GetByProjectIdAndOwnerAsync(Guid projectId, string ownerId)
        {
            var tasks = await _taskRepository.GetByProjectIdAndOwnerAsync(projectId, ownerId);
            return ServiceResult<IReadOnlyList<DomainTask>>.Ok(tasks);
        }
    }
}
