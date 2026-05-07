using Microsoft.EntityFrameworkCore;
using ASP.NET_Core_Web_API.Data;
using ASP.NET_Core_Web_API.Interfaces;

namespace ASP.NET_Core_Web_API.Repositories
{
     
    /// Generic repository implementation for all entities
     
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

         
        /// Get all entities
         
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

         
        /// Get entity by ID
         
        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

         
        /// Add a new entity
         
        public async Task<T> AddAsync(T entity)
        {
            _dbSet.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

         
        /// Update an existing entity
         
        public async Task<T> UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

         
        /// Delete an entity by ID
         
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null)
                return false;

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

         
        /// Check if entity exists by ID
         
        public async Task<bool> ExistsAsync(int id)
        {
            return await _dbSet.FindAsync(id) != null;
        }
    }
}
