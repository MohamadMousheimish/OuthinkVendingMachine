using Outhink.Db.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Outhink.Db.Repositories
{

    /// <summary>
    /// Base Repository Interface
    /// </summary>
    /// <typeparam name="T">Generic type extending <see cref="BaseEntity"/></typeparam>
    /// <remarks>
    /// <see cref="IEnumerable{T}"/> is used here because we're using an in memory database
    /// No SQL servers are used to use <see cref="IQueryable{T}"/>
    /// </remarks>
    public interface IBaseRepository<T> where T : BaseEntity
    {
        Task<T> GetByIdAsync(int id);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<int> CountAsync(Expression<Func<T, bool>> expression);
        Task<T> FirstAsync(Expression<Func<T, bool>> expression);
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> expression);
        Task<IEnumerable<T>> ListAllAsync();
        Task<IEnumerable<T>> ListAsync(Expression<Func<T, bool>> expression);
        Task AddRangeAsync(IEnumerable<T> entities);
    }
}
