using TMS.Domain;

namespace TMS.Application
{
    public interface IProjectRepository : IGenericRepository<Project>
    {
        System.Threading.Tasks.Task<Project?> GetByIdAndOwnerAsync(Guid id, string ownerId);
        System.Threading.Tasks.Task<Project?> GetTrackedByIdAndOwnerAsync(Guid id, string ownerId);
    }
}
