using DomainTask = TMS.Domain.Task;

namespace TMS.Application
{
    public interface ITaskService : IGenericService<DomainTask>
    {
        System.Threading.Tasks.Task<ServiceResult<DomainTask>> GetByIdAndOwnerAsync(Guid taskId, string ownerId);
        System.Threading.Tasks.Task<ServiceResult<DomainTask>> GetTrackedByIdAndOwnerAsync(Guid taskId, string ownerId);
        System.Threading.Tasks.Task<ServiceResult<IReadOnlyList<DomainTask>>> GetByProjectIdAndOwnerAsync(Guid projectId, string ownerId);
    }
}
