using ASP.NET_Core_Web_API.DTOs;
using ASP.NET_Core_Web_API.models;

namespace ASP.NET_Core_Web_API.Interfaces
{
     
    /// User service interface for user-related business logic
     
    public interface IUserService
    {
         
        /// Get all users
         
        Task<IEnumerable<User>> GetAllUsersAsync();

         
        /// Get user by ID
         
        Task<User> GetUserByIdAsync(int id);

         
        /// Get current user profile
         
        Task<User> GetCurrentUserProfileAsync(int userId);

         
        /// Update user profile
         
        Task<User> UpdateUserAsync(int id, User updateDto, int currentUserId, string userRole);

         
        /// Approve or reject user (Admin only)
         
        Task<User> ApproveUserAsync(int id, bool isApproved);

         
        /// Delete user and all related data
         
        Task<bool> DeleteUserAsync(int id);

         
        /// Check if user exists
         
        Task<bool> UserExistsAsync(int id);

         
        /// Check if email exists
         
        Task<bool> EmailExistsAsync(string email);
    }
}
