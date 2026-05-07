using ASP.NET_Core_Web_API.DTOs;
using ASP.NET_Core_Web_API.models;

namespace ASP.NET_Core_Web_API.Interfaces
{
    
    /// Material service interface for material-related business logic
    
    public interface IMaterialService
    {
        
        /// Get all materials for a study group
        
        Task<IEnumerable<Material>> GetMaterialsByGroupAsync(int studyGroupId);

        
        /// Get material by ID
        
        Task<Material> GetMaterialByIdAsync(int id);

        
        /// Create new material
        
        Task<Material> CreateMaterialAsync(CreateMaterialDto createDto, int studyGroupId, int userId);

        
        /// Update material
        
        Task<Material> UpdateMaterialAsync(int id, CreateMaterialDto updateDto, int currentUserId, string userRole);

        
        /// Delete material
        
        Task<bool> DeleteMaterialAsync(int id, int currentUserId, string userRole);

        
        /// Get materials uploaded by user
       
        Task<IEnumerable<Material>> GetUserMaterialsAsync(int userId);

        
        /// Search materials by filename
        
        Task<IEnumerable<Material>> SearchMaterialsAsync(string searchTerm);

         
        /// Check if material exists
          
        Task<bool> MaterialExistsAsync(int id);
    }
}
