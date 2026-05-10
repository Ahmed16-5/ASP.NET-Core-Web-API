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

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllStudyGroups()
        {
            var groups = await _studyGroupService.GetAllApprovedStudyGroupsAsync();
            return Ok(groups);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStudyGroupById(int id)
        {
            var isAuthenticated = User.Identity?.IsAuthenticated == true;
            int? currentUserId = null;
            UserRole? userRole = null;

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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateStudyGroup([FromBody] CreateStudyGroupDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = _authService.GetUserIdFromClaims(User);
            var userRole = _authService.GetUserRoleFromClaims(User);

            if (userRole == UserRole.Student)
                return Forbid();

            var studyGroup = await _studyGroupService.CreateStudyGroupAsync(createDto, userId);

            if (studyGroup == null)
                return BadRequest(new { message = "Failed to create study group" });

            return CreatedAtAction(nameof(GetStudyGroupById), new { id = studyGroup.ID }, studyGroup);
        }

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
                return NotFound(new { message = "Study group not found or you don't have permission to update it" });

            return Ok(new { message = "Study group updated successfully", group });
        }

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveStudyGroup(int id, [FromBody] ApproveStudyGroupDto approveDto)
        {
            var group = await _studyGroupService.ApproveStudyGroupAsync(id, approveDto.IsApproved);

            if (group == null)
                return NotFound(new { message = "Study group not found" });

            return Ok(new { message = $"Study group {(approveDto.IsApproved ? "approved" : "rejected")}", group });
        }

        [HttpGet("owner/my-groups")]
        [Authorize]
        public async Task<IActionResult> GetMyStudyGroups()
        {
            var userId = _authService.GetUserIdFromClaims(User);
            var groups = await _studyGroupService.GetUserStudyGroupsAsync(userId);
            return Ok(groups);
        }

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

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteStudyGroup(int id)
        {
            var currentUserId = _authService.GetUserIdFromClaims(User);
            var userRole = _authService.GetUserRoleFromClaims(User);

            var success = await _studyGroupService.DeleteStudyGroupAsync(id, currentUserId, userRole);

            if (!success)
                return NotFound(new { message = "Study group not found or you don't have permission to delete it" });

            return Ok(new { message = "Study group deleted successfully" });
        }

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