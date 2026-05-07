namespace ASP.NET_Core_Web_API.Interfaces
{
   
    /// Generic repository interface for common CRUD operations
    
    public interface IGenericRepository<T> where T : class
    {
        
        /// Get all entities
        
        Task<IEnumerable<T>> GetAllAsync();

        
        /// Get entity by ID
        
        Task<T> GetByIdAsync(int id);

        
        /// Add a new entity
        
        Task<T> AddAsync(T entity);

        
        /// Update an existing entity
        
        Task<T> UpdateAsync(T entity);

        
        /// Delete an entity by ID
        
        Task<bool> DeleteAsync(int id);

        
        /// Check if entity exists by ID
        
        Task<bool> ExistsAsync(int id);
    }
}
