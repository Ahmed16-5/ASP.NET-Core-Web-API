using Microsoft.EntityFrameworkCore;
using ASP.NET_Core_Web_API.Data;
using ASP.NET_Core_Web_API.Interfaces;
using ASP.NET_Core_Web_API.DTOs;
using ASP.NET_Core_Web_API.models;

namespace ASP.NET_Core_Web_API.Services
{
     
    /// Material service implementation for material-related business logic
     
    public class MaterialService : IMaterialService
    {
        private readonly AppDbContext _context;
        private readonly IGenericRepository<Material> _repository;

        public MaterialService(AppDbContext context, IGenericRepository<Material> repository)
        {
            _context = context;
            _repository = repository;
        }

         
        /// Get all materials for a study group
         
        public async Task<IEnumerable<Material>> GetMaterialsByGroupAsync(int studyGroupId)
        {
            return await _context.Materials
                .Where(m => m.StudyGroupID == studyGroupId)
                .Include(m => m.User)
                .ToListAsync();
        }

         
        /// Get material by ID
         
        public async Task<Material> GetMaterialByIdAsync(int id)
        {
            return await _context.Materials
                .Include(m => m.User)
                .Include(m => m.StudyGroup)
                .FirstOrDefaultAsync(m => m.ID == id);
        }

         
        /// Create new material
         
        public async Task<Material> CreateMaterialAsync(CreateMaterialDto createDto, int studyGroupId, int userId)
        {
            var material = new Material
            {
                FileName = createDto.FileName,
                FileUrl = createDto.FileUrl,
                UserID = userId,
                StudyGroupID = studyGroupId,
                CreatedAt = DateTime.UtcNow
            };

            return await _repository.AddAsync(material);
        }

         
        /// Update material
         
        public async Task<Material> UpdateMaterialAsync(int id, CreateMaterialDto updateDto, int currentUserId, string userRole)
        {
            var material = await _context.Materials
                .Include(m => m.StudyGroup)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (material == null)
                return null;

            // Only creator, group owner, or admin can update
            bool canUpdate = material.UserID == currentUserId ||
                           material.StudyGroup.UserID == currentUserId ||
                           userRole == "Admin";

            if (!canUpdate)
                return null;

            material.FileName = updateDto.FileName ?? material.FileName;
            material.FileUrl = updateDto.FileUrl ?? material.FileUrl;

            return await _repository.UpdateAsync(material);
        }

         
        /// Delete material
         
        public async Task<bool> DeleteMaterialAsync(int id, int currentUserId, string userRole)
        {
            var material = await _context.Materials
                .Include(m => m.StudyGroup)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (material == null)
                return false;

            // Only creator, group owner, or admin can delete
            bool canDelete = material.UserID == currentUserId ||
                           material.StudyGroup.UserID == currentUserId ||
                           userRole == "Admin";

            if (!canDelete)
                return false;

            _context.Materials.Remove(material);
            await _context.SaveChangesAsync();

            return true;
        }

         
        /// Get materials uploaded by user
         
        public async Task<IEnumerable<Material>> GetUserMaterialsAsync(int userId)
        {
            return await _context.Materials
                .Where(m => m.UserID == userId)
                .Include(m => m.StudyGroup)
                .ToListAsync();
        }

         
        /// Search materials by filename
         
        public async Task<IEnumerable<Material>> SearchMaterialsAsync(string searchTerm)
        {
            return await _context.Materials
                .Where(m => m.FileName.Contains(searchTerm))
                .Include(m => m.User)
                .Include(m => m.StudyGroup)
                .ToListAsync();
        }

         
        /// Check if material exists
         
        public async Task<bool> MaterialExistsAsync(int id)
        {
            return await _repository.ExistsAsync(id);
        }
    }
}
