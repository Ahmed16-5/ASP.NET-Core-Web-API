using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ASP.NET_Core_Web_API.models;
using ASP.NET_Core_Web_API.DTOs;
using ASP.NET_Core_Web_API.Services;
using ASP.NET_Core_Web_API.Interfaces;
using ASP.NET_Core_Web_API.Enums;

namespace ASP.NET_Core_Web_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly AuthService _authService;

        public CommentsController(ICommentService commentService, AuthService authService)
        {
            _commentService = commentService;
            _authService = authService;
        }

        
        /// Get all comments for a study group
        [HttpGet("group/{studyGroupId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCommentsByGroup(int studyGroupId)
        {
            var comments = await _commentService.GetCommentsByGroupAsync(studyGroupId);
            return Ok(comments);
        }

        
        /// Get comment by ID
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCommentById(int id)
        {
            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null)
                return NotFound(new { message = "Comment not found" });

            return Ok(comment);
        }

        
        /// Create new comment (authorized users only)
        
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentDto createDto, [FromQuery] int studyGroupId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(createDto.Content))
                return BadRequest(new { message = "Comment content cannot be empty" });

            var userId = _authService.GetUserIdFromClaims(User);
            var comment = await _commentService.CreateCommentAsync(createDto, studyGroupId, userId);

            if (comment == null)
                return BadRequest(new { message = "Failed to create comment" });

            return CreatedAtAction(nameof(GetCommentById), new { id = comment.ID }, comment);
        }

        
        /// Update comment (only comment owner)
        
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

            var comment = await _commentService.UpdateCommentAsync(id, updateDto, currentUserId, userRole);
            if (comment == null)
                return NotFound(new { message = "Comment not found" });

            return Ok(new { message = "Comment updated successfully", comment });
        }

        
        /// Delete comment (only comment owner or group owner or admin)
        
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var currentUserId = _authService.GetUserIdFromClaims(User);
            var userRole = _authService.GetUserRoleFromClaims(User);

            var success = await _commentService.DeleteCommentAsync(id, currentUserId, userRole);
            if (!success)
                return NotFound(new { message = "Comment not found" });

            return Ok(new { message = "Comment deleted successfully" });
        }

        
        /// Get comments by current user
        
        [HttpGet("user/my-comments")]
        [Authorize]
        public async Task<IActionResult> GetMyComments()
        {
            var userId = _authService.GetUserIdFromClaims(User);
            var comments = await _commentService.GetUserCommentsAsync(userId);
            return Ok(comments);
        }

        
        /// Get recent comments across all groups
        
        [HttpGet("recent")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRecentComments([FromQuery] int limit = 10)
        {
            var comments = await _commentService.GetRecentCommentsAsync(limit);
            return Ok(comments);
        }

        /// Get comment count for a study group
        
        [HttpGet("group/{studyGroupId}/count")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCommentCountForGroup(int studyGroupId)
        {
            var count = await _commentService.GetCommentCountForGroupAsync(studyGroupId);
            return Ok(new { studyGroupId, commentCount = count });
        }
    }
}
