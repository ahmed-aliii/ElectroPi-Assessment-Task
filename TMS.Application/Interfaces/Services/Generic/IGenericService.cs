using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Application
{
    public interface IGenericService<T> where T : class
    {
        Task<ServiceResult<IEnumerable<T>>> GetAllAsync();
        Task<ServiceResult<IEnumerable<T>>> GetAllWithIncludeAsync(params Expression<Func<T, object>>[] includes);
        Task<ServiceResult<T>> GetByIdAsync(Guid id);
        Task<ServiceResult<T>> GetByIdWithIncludeAsync(Guid id, params Expression<Func<T, object>>[] includes);
        Task<ServiceResult<T>> CreateAsync(T entity);
        Task<ServiceResult<T>> UpdateAsync(T entity);
        Task<ServiceResult<T>> DeleteAsync(Guid id);
        Task<ServiceResult<bool>> IsExistAsync(Guid id);
        Task<ServiceResult<PagedResult<T>>> GetAllPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>>? filter = null, params Expression<Func<T, object>>[]? includes);
    }
}
