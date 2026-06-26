using DomainTask = TMS.Domain.Task;

namespace TMS.Application
{
    public interface ITaskRepository : IGenericRepository<DomainTask>
    {
        System.Threading.Tasks.Task<DomainTask?> GetByIdAndOwnerAsync(Guid taskId, string ownerId);
        System.Threading.Tasks.Task<DomainTask?> GetTrackedByIdAndOwnerAsync(Guid taskId, string ownerId);
        System.Threading.Tasks.Task<IReadOnlyList<DomainTask>> GetByProjectIdAndOwnerAsync(Guid projectId, string ownerId);
    }
}
