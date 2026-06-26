using Microsoft.EntityFrameworkCore;
using TMS.Application;
using DomainTask = TMS.Domain.Task;

namespace TMS.Infrastructure
{
    public class TaskRepository : GenericRepository<DomainTask>, ITaskRepository
    {
        private readonly ApplicationDbContext _context;

        public TaskRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async System.Threading.Tasks.Task<DomainTask?> GetByIdAndOwnerAsync(Guid taskId, string ownerId)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t =>
                    t.Id == taskId &&
                    t.Project.OwnerId == ownerId &&
                    !t.IsDeleted &&
                    !t.Project.IsDeleted);
        }

        public async System.Threading.Tasks.Task<DomainTask?> GetTrackedByIdAndOwnerAsync(Guid taskId, string ownerId)
        {
            return await _context.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t =>
                    t.Id == taskId &&
                    t.Project.OwnerId == ownerId &&
                    !t.IsDeleted &&
                    !t.Project.IsDeleted);
        }

        public async System.Threading.Tasks.Task<IReadOnlyList<DomainTask>> GetByProjectIdAndOwnerAsync(Guid projectId, string ownerId)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Include(t => t.Project)
                .Where(t =>
                    t.ProjectId == projectId &&
                    t.Project.OwnerId == ownerId &&
                    !t.IsDeleted &&
                    !t.Project.IsDeleted)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
    }
}
