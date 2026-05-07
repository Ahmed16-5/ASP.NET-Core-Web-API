using Microsoft.EntityFrameworkCore;
using ASP.NET_Core_Web_API.Data;
using ASP.NET_Core_Web_API.Interfaces;
using ASP.NET_Core_Web_API.models;

namespace ASP.NET_Core_Web_API.Services
{
     
    /// User service implementation for user-related business logic
     
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IGenericRepository<User> _repository;

        public UserService(AppDbContext context, IGenericRepository<User> repository)
        {
            _context = context;
            _repository = repository;
        }

         
        /// Get all users
         
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _repository.GetAllAsync();
        }

         
        /// Get user by ID
         
        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

         
        /// Get current user profile
         
        public async Task<User> GetCurrentUserProfileAsync(int userId)
        {
            return await _repository.GetByIdAsync(userId);
        }

         
        /// Update user profile
         
        public async Task<User> UpdateUserAsync(int id, User updateDto, int currentUserId, string userRole)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null)
                return null;

            // Update allowed fields
            user.Name = updateDto.Name ?? user.Name;
            user.Email = updateDto.Email ?? user.Email;

            // Only admins can change role
            if (userRole == "Admin")
                user.Role = updateDto.Role ?? user.Role;

            return await _repository.UpdateAsync(user);
        }

         
        /// Approve or reject user (Admin only)
         
        public async Task<User> ApproveUserAsync(int id, bool isApproved)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null)
                return null;

            user.IsApproved = isApproved;
            return await _repository.UpdateAsync(user);
        }

         
        /// Delete user and all related data
         
        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.StudyGroups)
                .Include(u => u.JoinRequests)
                .Include(u => u.GroupMembers)
                .Include(u => u.Materials)
                .Include(u => u.Comments)
                .FirstOrDefaultAsync(u => u.ID == id);

            if (user == null)
                return false;

            // Remove user from all relationships
            _context.StudyGroups.RemoveRange(user.StudyGroups);
            _context.JoinRequests.RemoveRange(user.JoinRequests);
            _context.GroupMembers.RemoveRange(user.GroupMembers);
            _context.Materials.RemoveRange(user.Materials);
            _context.Comments.RemoveRange(user.Comments);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return true;
        }

         
        /// Check if user exists
         
        public async Task<bool> UserExistsAsync(int id)
        {
            return await _repository.ExistsAsync(id);
        }

         
        /// Check if email exists
         
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }
    }
}
