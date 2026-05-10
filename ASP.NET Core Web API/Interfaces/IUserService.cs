using ASP.NET_Core_Web_API.DTOs;
using ASP.NET_Core_Web_API.Enums;
using ASP.NET_Core_Web_API.models;

namespace ASP.NET_Core_Web_API.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();

        Task<User> GetUserByIdAsync(int id);

        Task<User> GetCurrentUserProfileAsync(int userId);

        Task<User> UpdateUserAsync(int id, User updateDto, int currentUserId, UserRole userRole);

        Task<User> ApproveUserAsync(int id, bool isApproved);

        Task<bool> DeleteUserAsync(int id);

        Task<bool> UserExistsAsync(int id);

        Task<bool> EmailExistsAsync(string email);
    }
}