using Microsoft.EntityFrameworkCore;
using ASP.NET_Core_Web_API.Data;
using ASP.NET_Core_Web_API.Interfaces;
using ASP.NET_Core_Web_API.models;
using ASP.NET_Core_Web_API.Enums;

namespace ASP.NET_Core_Web_API.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IGenericRepository<User> _repository;

        public UserService(AppDbContext context, IGenericRepository<User> repository)
        {
            _context = context;
            _repository = repository;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<User> GetCurrentUserProfileAsync(int userId)
        {
            return await _repository.GetByIdAsync(userId);
        }

        public async Task<User> UpdateUserAsync(int id, User updateDto, int currentUserId, UserRole userRole)
        {
            var user = await _repository.GetByIdAsync(id);

            if (user == null)
                return null;

            user.Name = updateDto.Name ?? user.Name;
            user.Email = updateDto.Email ?? user.Email;

            // Only admins can change role
            if (userRole == UserRole.Admin && updateDto.Role != user.Role)
                user.Role = updateDto.Role;

            return await _repository.UpdateAsync(user);
        }

        public async Task<User> ApproveUserAsync(int id, bool isApproved)
        {
            var user = await _repository.GetByIdAsync(id);

            if (user == null)
                return null;

            user.IsApproved = isApproved;
            return await _repository.UpdateAsync(user);
        }

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

            _context.StudyGroups.RemoveRange(user.StudyGroups);
            _context.JoinRequests.RemoveRange(user.JoinRequests);
            _context.GroupMembers.RemoveRange(user.GroupMembers);
            _context.Materials.RemoveRange(user.Materials);
            _context.Comments.RemoveRange(user.Comments);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UserExistsAsync(int id)
        {
            return await _repository.ExistsAsync(id);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }
    }
}