using ASP.NET_Core_Web_API.DTOs;
using ASP.NET_Core_Web_API.models;
using ASP.NET_Core_Web_API.Enums;

namespace ASP.NET_Core_Web_API.Interfaces
{
    public interface IMaterialService
    {
        Task<IEnumerable<Material>> GetMaterialsByGroupAsync(int studyGroupId);

        Task<Material> GetMaterialByIdAsync(int id);

        Task<Material> CreateMaterialAsync(CreateMaterialDto createDto, int studyGroupId, int userId);

        Task<Material> UpdateMaterialAsync(int id, CreateMaterialDto updateDto, int currentUserId, UserRole userRole);

        Task<bool> DeleteMaterialAsync(int id, int currentUserId, UserRole userRole);

        Task<IEnumerable<Material>> GetUserMaterialsAsync(int userId);

        Task<IEnumerable<Material>> SearchMaterialsAsync(string searchTerm);

        Task<bool> MaterialExistsAsync(int id);
    }
}