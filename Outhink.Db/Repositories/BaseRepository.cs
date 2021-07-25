using Microsoft.EntityFrameworkCore;
using Outhink.Db.Context;
using Outhink.Db.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Outhink.Db.Repositories
{
    /// <summary>
    /// Base Repository Class
    /// </summary>
    /// <typeparam name="T">Generic type extending <see cref="BaseEntity"/></typeparam>
    /// <remarks>
    /// <see cref="IEnumerable{T}"/> is used here because we're using an in memory database
    /// No SQL servers are used to use <see cref="IQueryable{T}"/>
    /// </remarks>
    public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
    {
        protected readonly OuthinkContext _context;

        public BaseRepository(OuthinkContext dbContext)
        {
            _context = dbContext;
        }

        public virtual async Task<T> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<IEnumerable<T>> ListAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<IEnumerable<T>> ListAsync(Expression<Func<T, bool>> expression)
        {
            return await _context.Set<T>().Where(expression).ToListAsync();
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> expression)
        {
            return await _context.Set<T>().CountAsync(expression);
        }

        public async Task<T> AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _context.Set<T>().AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            var attachedEntity = _context.Set<T>().Local.FirstOrDefault(entry => entry.Id.Equals(entity.Id));
            if (attachedEntity != null)
            {
                _context.Entry(attachedEntity).State = EntityState.Detached;
            }
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<T> FirstAsync(Expression<Func<T, bool>> expression)
        {
            return await _context.Set<T>().FirstAsync(expression);
        }

        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> expression)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(expression);
        }
    }
}
