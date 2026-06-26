using Microsoft.EntityFrameworkCore;
using TMS.Application;
using TMS.Domain;

namespace TMS.Infrastructure
{
    public class ProjectRepository : GenericRepository<Project>, IProjectRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async System.Threading.Tasks.Task<Project?> GetByIdAndOwnerAsync(Guid id, string ownerId)
        {
            return await _context.Projects
                .AsNoTracking()
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == ownerId && !p.IsDeleted);
        }

        public async System.Threading.Tasks.Task<Project?> GetTrackedByIdAndOwnerAsync(Guid id, string ownerId)
        {
            return await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == ownerId && !p.IsDeleted);
        }
    }
}
