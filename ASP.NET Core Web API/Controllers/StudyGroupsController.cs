using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ASP.NET_Core_Web_API.models;
using ASP.NET_Core_Web_API.DTOs;
using ASP.NET_Core_Web_API.Services;
using ASP.NET_Core_Web_API.Interfaces;

namespace ASP.NET_Core_Web_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StudyGroupsController : ControllerBase
    {
        private readonly IStudyGroupService _studyGroupService;
        private readonly AuthService _authService;

        public StudyGroupsController(IStudyGroupService studyGroupService, AuthService authService)
        {
            _studyGroupService = studyGroupService;
            _authService = authService;
        }

        
        /// Get all approved study groups
        
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllStudyGroups()
        {
            var groups = await _studyGroupService.GetAllApprovedStudyGroupsAsync();
            return Ok(groups);
        }

       
        /// Get study group by ID
        
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStudyGroupById(int id)
        {
            var isAuthenticated = User.Identity?.IsAuthenticated == true;
            int? currentUserId = null;
            string? userRole = null;

            if (isAuthenticated)
            {
                currentUserId = _authService.GetUserIdFromClaims(User);
                userRole = _authService.GetUserRoleFromClaims(User);
            }

            var group = await _studyGroupService.GetStudyGroupByIdWithAuthAsync(id, currentUserId, userRole);
            if (group == null)
                return NotFound(new { message = "Study group not found or not approved yet" });

            return Ok(group);
        }

        
        /// Create new study group (only approved users)
       
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateStudyGroup([FromBody] CreateStudyGroupDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = _authService.GetUserIdFromClaims(User);
            var studyGroup = await _studyGroupService.CreateStudyGroupAsync(createDto, userId);

            if (studyGroup == null)
                return BadRequest(new { message = "Failed to create study group" });

            return CreatedAtAction(nameof(GetStudyGroupById), new { id = studyGroup.ID }, studyGroup);
        }

        
        /// Update study group (only owner or admin)
        
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateStudyGroup(int id, [FromBody] UpdateStudyGroupDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = _authService.GetUserIdFromClaims(User);
            var userRole = _authService.GetUserRoleFromClaims(User);

            var group = await _studyGroupService.UpdateStudyGroupAsync(id, updateDto, currentUserId, userRole);
            if (group == null)
                return NotFound(new { message = "Study group not found" });

            return Ok(new { message = "Study group updated successfully", group });
        }

        
        /// Approve/Reject study group (Admin only)
       
        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveStudyGroup(int id, [FromBody] ApproveStudyGroupDto approveDto)
        {
            var group = await _studyGroupService.ApproveStudyGroupAsync(id, approveDto.IsApproved);
            if (group == null)
                return NotFound(new { message = "Study group not found" });

            return Ok(new { message = $"Study group {(approveDto.IsApproved ? "approved" : "rejected")}", group });
        }

        
        /// Get study groups by owner (current user's groups)
        
        [HttpGet("owner/my-groups")]
        [Authorize]
        public async Task<IActionResult> GetMyStudyGroups()
        {
            var userId = _authService.GetUserIdFromClaims(User);
            var groups = await _studyGroupService.GetUserStudyGroupsAsync(userId);
            return Ok(groups);
        }

        
        /// Search study groups by subject, location, or meeting time
        
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchStudyGroups(
            [FromQuery] string? subject,
            [FromQuery] string? location,
            [FromQuery] DateTime? meetingTime)
        {
            var results = await _studyGroupService.SearchStudyGroupsAsync(subject, location, meetingTime);
            return Ok(results);
        }

        
        /// Delete study group (owner or admin only)
        
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteStudyGroup(int id)
        {
            var currentUserId = _authService.GetUserIdFromClaims(User);
            var userRole = _authService.GetUserRoleFromClaims(User);

            var success = await _studyGroupService.DeleteStudyGroupAsync(id, currentUserId, userRole);
            if (!success)
                return NotFound(new { message = "Study group not found" });

            return Ok(new { message = "Study group deleted successfully" });
        }

        
        /// Get study group members
         
        [HttpGet("{id}/members")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStudyGroupMembers(int id)
        {
            var members = await _studyGroupService.GetStudyGroupMembersAsync(id);
            return Ok(members);
        }
    }

    public class ApproveStudyGroupDto
    {
        public bool IsApproved { get; set; }
    }
}
