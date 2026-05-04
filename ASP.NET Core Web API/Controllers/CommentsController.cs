using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ASP.NET_Core_Web_API.Data;
using ASP.NET_Core_Web_API.models;
using ASP.NET_Core_Web_API.DTOs;
using ASP.NET_Core_Web_API.Services;

namespace ASP.NET_Core_Web_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;

        public CommentsController(AppDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        /// <summary>
        /// Get all comments for a study group
        /// </summary>
        [HttpGet("group/{studyGroupId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCommentsByGroup(int studyGroupId)
        {
            var group = await _context.StudyGroups.FindAsync(studyGroupId);
            if (group == null)
                return NotFound(new { message = "Study group not found" });

            var comments = await _context.Comments
                .Where(c => c.StudyGroupID == studyGroupId)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return Ok(comments);
        }

        /// <summary>
        /// Get comment by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCommentById(int id)
        {
            var comment = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.StudyGroup)
                .FirstOrDefaultAsync(c => c.ID == id);

            if (comment == null)
                return NotFound(new { message = "Comment not found" });

            return Ok(comment);
        }

        /// <summary>
        /// Create new comment (authorized users only)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentDto createDto, [FromQuery] int studyGroupId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(createDto.Content))
                return BadRequest(new { message = "Comment content cannot be empty" });

            var userId = _authService.GetUserIdFromClaims(User);
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            var studyGroup = await _context.StudyGroups.FindAsync(studyGroupId);
            if (studyGroup == null)
                return NotFound(new { message = "Study group not found" });

            var comment = new Comment
            {
                Content = createDto.Content,
                UserID = userId,
                StudyGroupID = studyGroupId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCommentById), new { id = comment.ID }, comment);
        }

        /// <summary>
        /// Update comment (only comment owner)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] CreateCommentDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(updateDto.Content))
                return BadRequest(new { message = "Comment content cannot be empty" });

            var currentUserId = _authService.GetUserIdFromClaims(User);
            var userRole = _authService.GetUserRoleFromClaims(User);

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
                return NotFound(new { message = "Comment not found" });

            // Only comment owner or admin can update
            if (comment.UserID != currentUserId && userRole != "Admin")
                return Forbid();

            comment.Content = updateDto.Content;

            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Comment updated successfully", comment });
        }

        /// <summary>
        /// Delete comment (only comment owner or group owner or admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var currentUserId = _authService.GetUserIdFromClaims(User);
            var userRole = _authService.GetUserRoleFromClaims(User);

            var comment = await _context.Comments
                .Include(c => c.StudyGroup)
                .FirstOrDefaultAsync(c => c.ID == id);

            if (comment == null)
                return NotFound(new { message = "Comment not found" });

            // Only comment owner, group owner, or admin can delete
            bool canDelete = comment.UserID == currentUserId || 
                           comment.StudyGroup?.UserID == currentUserId || 
                           userRole == "Admin";

            if (!canDelete)
                return Forbid();

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Comment deleted successfully" });
        }

        /// <summary>
        /// Get comments by current user
        /// </summary>
        [HttpGet("user/my-comments")]
        [Authorize]
        public async Task<IActionResult> GetMyComments()
        {
            var userId = _authService.GetUserIdFromClaims(User);

            var comments = await _context.Comments
                .Where(c => c.UserID == userId)
                .Include(c => c.StudyGroup)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return Ok(comments);
        }

        /// <summary>
        /// Get recent comments across all groups
        /// </summary>
        [HttpGet("recent")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRecentComments([FromQuery] int limit = 10)
        {
            var comments = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.StudyGroup)
                .OrderByDescending(c => c.CreatedAt)
                .Take(limit)
                .ToListAsync();

            return Ok(comments);
        }

        /// <summary>
        /// Get comment count for a study group
        /// </summary>
        [HttpGet("group/{studyGroupId}/count")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCommentCountForGroup(int studyGroupId)
        {
            var group = await _context.StudyGroups.FindAsync(studyGroupId);
            if (group == null)
                return NotFound(new { message = "Study group not found" });

            var count = await _context.Comments
                .CountAsync(c => c.StudyGroupID == studyGroupId);

            return Ok(new { studyGroupId, commentCount = count });
        }
    }
}
