using ASP.NET_Core_Web_API.DTOs;
using ASP.NET_Core_Web_API.models;
using ASP.NET_Core_Web_API.Enums;

namespace ASP.NET_Core_Web_API.Interfaces
{
    
    /// Comment service interface for comment-related business logic
    
    public interface ICommentService
    {
        
        /// Get all comments for a study group

        Task<IEnumerable<Comment>> GetCommentsByGroupAsync(int studyGroupId);


        /// Get comment by ID

        Task<Comment> GetCommentByIdAsync(int id);


        /// Create new comment
        Task<Comment> CreateCommentAsync(CreateCommentDto createDto, int studyGroupId, int userId);


        /// Update comment

        Task<Comment> UpdateCommentAsync(int id, CreateCommentDto updateDto, int currentUserId, UserRole userRole);


        /// Delete comment

        Task<bool> DeleteCommentAsync(int id, int currentUserId, UserRole userRole);


        /// Get comments by current user

        Task<IEnumerable<Comment>> GetUserCommentsAsync(int userId);


        /// Get recent comments across all groups

        Task<IEnumerable<Comment>> GetRecentCommentsAsync(int limit = 10);


        /// Get comment count for a study group

        Task<int> GetCommentCountForGroupAsync(int studyGroupId);


        /// Check if comment exists

        Task<bool> CommentExistsAsync(int id);
    }
}
