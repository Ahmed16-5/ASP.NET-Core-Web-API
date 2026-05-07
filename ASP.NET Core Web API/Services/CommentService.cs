using Microsoft.EntityFrameworkCore;
using ASP.NET_Core_Web_API.Data;
using ASP.NET_Core_Web_API.Interfaces;
using ASP.NET_Core_Web_API.DTOs;
using ASP.NET_Core_Web_API.models;

namespace ASP.NET_Core_Web_API.Services
{
    
    /// Comment service implementation 
    public class CommentService : ICommentService
    {
        private readonly AppDbContext _context;
        private readonly IGenericRepository<Comment> _repository;

        public CommentService(AppDbContext context, IGenericRepository<Comment> repository)
        {
            _context = context;
            _repository = repository;
        }


        /// Get all comments for a study group
        public async Task<IEnumerable<Comment>> GetCommentsByGroupAsync(int studyGroupId)
        {
            return await _context.Comments
                .Where(c => c.StudyGroupID == studyGroupId)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        
        /// Get comment by ID
        public async Task<Comment> GetCommentByIdAsync(int id)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.StudyGroup)
                .FirstOrDefaultAsync(c => c.ID == id);
        }


        /// Create new comment

        public async Task<Comment> CreateCommentAsync(CreateCommentDto createDto, int studyGroupId, int userId)
        {
            var comment = new Comment
            {
                Content = createDto.Content,
                UserID = userId,
                StudyGroupID = studyGroupId,
                CreatedAt = DateTime.UtcNow
            };

            return await _repository.AddAsync(comment);
        }


        /// Update comment

        public async Task<Comment> UpdateCommentAsync(int id, CreateCommentDto updateDto, int currentUserId, string userRole)
        {
            var comment = await _repository.GetByIdAsync(id);
            if (comment == null)
                return null;

            // Only comment owner or admin can update
            if (comment.UserID != currentUserId && userRole != "Admin")
                return null;

            comment.Content = updateDto.Content;

            return await _repository.UpdateAsync(comment);
        }

        /// Delete comment
        public async Task<bool> DeleteCommentAsync(int id, int currentUserId, string userRole)
        {
            var comment = await _context.Comments
                .Include(c => c.StudyGroup)
                .FirstOrDefaultAsync(c => c.ID == id);

            if (comment == null)
                return false;

            // Only comment owner, group owner, or admin can delete
            bool canDelete = comment.UserID == currentUserId ||
                           comment.StudyGroup?.UserID == currentUserId ||
                           userRole == "Admin";

            if (!canDelete)
                return false;

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return true;
        }

        
        /// Get comments by current user
        
        public async Task<IEnumerable<Comment>> GetUserCommentsAsync(int userId)
        {
            return await _context.Comments
                .Where(c => c.UserID == userId)
                .Include(c => c.StudyGroup)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        
        /// Get recent comments across all groups
        
        public async Task<IEnumerable<Comment>> GetRecentCommentsAsync(int limit = 10)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.StudyGroup)
                .OrderByDescending(c => c.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        
        /// Get comment count for a study group
        
        public async Task<int> GetCommentCountForGroupAsync(int studyGroupId)
        {
            return await _context.Comments
                .CountAsync(c => c.StudyGroupID == studyGroupId);
        }

        
        /// Check if comment exists
        
        public async Task<bool> CommentExistsAsync(int id)
        {
            return await _repository.ExistsAsync(id);
        }
    }
}
