using TMS.Domain;

namespace TMS.Application
{
    public interface IProjectService : IGenericService<Project>
    {
        System.Threading.Tasks.Task<ServiceResult<Project>> GetByIdAndOwnerAsync(Guid id, string ownerId);
        System.Threading.Tasks.Task<ServiceResult<Project>> GetTrackedByIdAndOwnerAsync(Guid id, string ownerId);
    }
}
