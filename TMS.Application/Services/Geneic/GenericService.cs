using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Application
{
    public class GenericService<T> : IGenericService<T> where T : class
    {
        private readonly IGenericRepository<T> _repository;

        public GenericService(IGenericRepository<T> repository)
        {
            _repository = repository;
        }

        public async Task<ServiceResult<IEnumerable<T>>> GetAllAsync()
        {
            var query = _repository.GetAllAsync();
            var list = query != null ? await query.ToListAsync() : new List<T>();
            return ServiceResult<IEnumerable<T>>.Ok(list);
        }

        public async Task<ServiceResult<IEnumerable<T>>> GetAllWithIncludeAsync(params Expression<Func<T, object>>[] includes)
        {
            var query = _repository.GetAllWithIncludeAsync(includes);
            var list = query != null ? await query.ToListAsync() : new List<T>();
            return ServiceResult<IEnumerable<T>>.Ok(list);
        }

        public async Task<ServiceResult<T>> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return ServiceResult<T>.BadRequest("Entity not found.");

            return ServiceResult<T>.Ok(entity);
        }

        public async Task<ServiceResult<T>> GetByIdWithIncludeAsync(Guid id, params Expression<Func<T, object>>[] includes)
        {
            var entity = await _repository.GetByIdWithIncludeAsync(id, includes);
            if (entity == null)
                return ServiceResult<T>.BadRequest("Entity not found.");

            return ServiceResult<T>.Ok(entity);
        }

        public async Task<ServiceResult<T>> CreateAsync(T entity)
        {
            var created = await _repository.AddAsync(entity);
            if (created == null)
                return ServiceResult<T>.BadRequest("Failed to create entity.");

            return ServiceResult<T>.Ok(created);
        }

        public async Task<ServiceResult<T>> UpdateAsync(T entity)
        {
            var updated = await _repository.UpdateAsync(entity);
            return ServiceResult<T>.Ok(updated);
        }

        public async Task<ServiceResult<T>> DeleteAsync(Guid id)
        {
            var deleted = await _repository.DeleteAsync(id);
            if (deleted == null)
                return ServiceResult<T>.BadRequest("Entity not found.");

            return ServiceResult<T>.Ok(deleted);
        }

        public async Task<ServiceResult<bool>> IsExistAsync(Guid id)
        {
            var exists = await _repository.IsExistAsync(id);
            return ServiceResult<bool>.Ok(exists);
        }

        public async Task<ServiceResult<PagedResult<T>>> GetAllPagedAsync
            (
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            params Expression<Func<T, object>>[]? includes
            )
        {
            var pagedResult = await _repository.GetAllPagedAsync(pageNumber, pageSize, filter, includes);
            return ServiceResult<PagedResult<T>>.Ok(pagedResult);
        }
    }
}
