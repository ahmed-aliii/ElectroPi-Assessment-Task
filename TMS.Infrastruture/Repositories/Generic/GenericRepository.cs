using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TMS.Application;

namespace TMS.Infrastruture
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        #region Dependence Injection

        private readonly ApplicationDbContext _context;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        #endregion Dependence Injection

        public async Task<T?> AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            var numerOfRowAffected = await _context.SaveChangesAsync();
            return numerOfRowAffected > 0 ? entity : null;
        }

        public IQueryable<T>? GetAllAsync()
        {
            return _context.Set<T>().AsNoTracking();
        }

        public IQueryable<T>? GetAllWithIncludeAsync(params Expression<Func<T, object>>[] includes)
        {
            var query = _context.Set<T>().AsQueryable();
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return query.AsNoTracking();
        }

        public async Task<T> GetByIdAsync(Guid id)
        {
            var entity = await _context.Set<T>().AsNoTracking().FirstOrDefaultAsync(model => EF.Property<Guid>(model, "Id") == id);
            return entity!;
        }

        public async Task<T> GetByIdWithIncludeAsync(Guid id, params Expression<Func<T, object>>[] includes)
        {
            var query = _context.Set<T>().AsQueryable();
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            var entity = await query.AsNoTracking().FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);
            return entity!;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<T> DeleteAsync(Guid id)
        {
            var entity = await _context.Set<T>().FindAsync(id);
            if (entity != null)
            {
                _context.Set<T>().Remove(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            return entity;
        }

        public async Task<bool> IsExistAsync(Guid id)
        {
            var exists = await _context.Set<T>().AnyAsync(e => EF.Property<Guid>(e, "Id") == id);
            return exists;
        }

        public async Task<PagedResult<T>> GetAllPagedAsync
            (
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            params Expression<Func<T, object>>[]? includes
            )
        {
            var query = _context.Set<T>().AsNoTracking();

            if (includes != null && includes.Length > 0)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var totalRecords = await query.CountAsync();

            var data = await query
                .OrderBy(e => EF.Property<Guid>(e, "Id"))
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>
            {
                Items = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }
    }
}
