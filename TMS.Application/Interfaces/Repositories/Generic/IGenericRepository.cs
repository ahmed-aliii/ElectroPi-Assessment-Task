using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Application
{
    public interface IGenericRepository<T> where T : class
    {
        IQueryable<T>? GetAllAsync();
        IQueryable<T>? GetAllWithIncludeAsync(params Expression<Func<T, object>>[] includes);
        Task<T> GetByIdAsync(Guid id);
        Task<T> GetByIdWithIncludeAsync(Guid id, params Expression<Func<T, object>>[] includes);
        Task<T?> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<T> DeleteAsync(Guid id);
        Task<bool> IsExistAsync(Guid id);
        Task<PagedResult<T>> GetAllPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>>? filter = null, params Expression<Func<T, object>>[]? includes);
    }
}
