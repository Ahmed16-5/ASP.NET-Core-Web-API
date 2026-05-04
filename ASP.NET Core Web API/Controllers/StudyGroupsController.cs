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
    [Authorize]
    public class StudyGroupsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;

        public StudyGroupsController(AppDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        /// <summary>
        /// Get all approved study groups
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllStudyGroups()
        {
            var groups = await _context.StudyGroups
                .Where(sg => sg.IsApproved)
                .Include(sg => sg.User)
                .Include(sg => sg.GroupMembers)
                .ToListAsync();

            return Ok(groups);
        }

        /// <summary>
        /// Get study group by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStudyGroupById(int id)
        {
            var group = await _context.StudyGroups
                .Include(sg => sg.User)
                .Include(sg => sg.GroupMembers)
                .Include(sg => sg.Materials)
                .Include(sg => sg.Comments)
                .FirstOrDefaultAsync(sg => sg.ID == id);

            if (group == null)
                return NotFound(new { message = "Study group not found" });

            // Only allow viewing unapproved groups if user is owner or admin
            if (!group.IsApproved)
            {
                var isAuthenticated = User.Identity?.IsAuthenticated == true;
                if (!isAuthenticated)
                    return NotFound(new { message = "Study group not found or not approved yet" });

                var currentUserId = _authService.GetUserIdFromClaims(User);
                var userRole = _authService.GetUserRoleFromClaims(User);

                if (group.UserID != currentUserId && userRole != "Admin")
                    return NotFound(new { message = "Study group not found or not approved yet" });
            }

            return Ok(group);
        }

        /// <summary>
        /// Create new study group (only approved users)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateStudyGroup([FromBody] CreateStudyGroupDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = _authService.GetUserIdFromClaims(User);
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            // Only approved users can create study groups
            if (!user.IsApproved)
                return Forbid();

            var studyGroup = new StudyGroup
            {
                Subject = createDto.Subject,
                Description = createDto.Description,
                Location = createDto.Location,
                MeetingTime = createDto.MeetingTime,
                MeetingType = createDto.MeetingType,
                MaxMembers = createDto.MaxMembers,
                UserID = userId,
                CreatedAt = DateTime.UtcNow,
                IsApproved = false // Needs admin approval
            };

            _context.StudyGroups.Add(studyGroup);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStudyGroupById), new { id = studyGroup.ID }, studyGroup);
        }

        /// <summary>
        /// Update study group (only owner or admin)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateStudyGroup(int id, [FromBody] UpdateStudyGroupDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = _authService.GetUserIdFromClaims(User);
            var userRole = _authService.GetUserRoleFromClaims(User);

            var group = await _context.StudyGroups.FindAsync(id);
            if (group == null)
                return NotFound(new { message = "Study group not found" });

            // Only owner or admin can update
            if (group.UserID != currentUserId && userRole != "Admin")
                return Forbid();

            group.Subject = updateDto.Subject ?? group.Subject;
            group.Description = updateDto.Description ?? group.Description;
            group.Location = updateDto.Location ?? group.Location;
            group.MeetingTime = updateDto.MeetingTime != default ? updateDto.MeetingTime : group.MeetingTime;
            group.MeetingType = updateDto.MeetingType ?? group.MeetingType;
            group.MaxMembers = updateDto.MaxMembers > 0 ? updateDto.MaxMembers : group.MaxMembers;

            _context.StudyGroups.Update(group);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Study group updated successfully", group });
        }

        /// <summary>
        /// Approve/Reject study group (Admin only)
        /// </summary>
        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveStudyGroup(int id, [FromBody] ApproveStudyGroupDto approveDto)
        {
            var group = await _context.StudyGroups.FindAsync(id);
            if (group == null)
                return NotFound(new { message = "Study group not found" });

            group.IsApproved = approveDto.IsApproved;
            _context.StudyGroups.Update(group);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Study group {(approveDto.IsApproved ? "approved" : "rejected")}", group });
        }

        /// <summary>
        /// Get study groups by owner (current user's groups)
        /// </summary>
        [HttpGet("owner/my-groups")]
        [Authorize]
        public async Task<IActionResult> GetMyStudyGroups()
        {
            var userId = _authService.GetUserIdFromClaims(User);

            var groups = await _context.StudyGroups
                .Where(sg => sg.UserID == userId)
                .Include(sg => sg.GroupMembers)
                .ToListAsync();

            return Ok(groups);
        }

        /// <summary>
        /// Search study groups by subject, location, or meeting time
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchStudyGroups(
            [FromQuery] string? subject,
            [FromQuery] string? location,
            [FromQuery] DateTime? meetingTime)
        {
            var query = _context.StudyGroups
                .Where(sg => sg.IsApproved);

            if (!string.IsNullOrWhiteSpace(subject))
                query = query.Where(sg => sg.Subject != null && sg.Subject.Contains(subject));

            if (!string.IsNullOrWhiteSpace(location))
                query = query.Where(sg => sg.Location != null && sg.Location.Contains(location));

            if (meetingTime.HasValue)
            {
                var dateOnly = meetingTime.Value.Date;
                query = query.Where(sg => sg.MeetingTime.Date == dateOnly);
            }

            var results = await query
                .Include(sg => sg.User)
                .Include(sg => sg.GroupMembers)
                .ToListAsync();

            return Ok(results);
        }

        /// <summary>
        /// Delete study group (owner or admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteStudyGroup(int id)
        {
            var currentUserId = _authService.GetUserIdFromClaims(User);
            var userRole = _authService.GetUserRoleFromClaims(User);

            var group = await _context.StudyGroups
                .Include(sg => sg.GroupMembers)
                .Include(sg => sg.JoinRequests)
                .Include(sg => sg.Materials)
                .Include(sg => sg.Comments)
                .FirstOrDefaultAsync(sg => sg.ID == id);

            if (group == null)
                return NotFound(new { message = "Study group not found" });

            // Only owner or admin can delete
            if (group.UserID != currentUserId && userRole != "Admin")
                return Forbid();

            // Remove all related data
            _context.GroupMembers.RemoveRange(group.GroupMembers);
            _context.JoinRequests.RemoveRange(group.JoinRequests);
            _context.Materials.RemoveRange(group.Materials);
            _context.Comments.RemoveRange(group.Comments);

            _context.StudyGroups.Remove(group);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Study group deleted successfully" });
        }

        /// <summary>
        /// Get study group members
        /// </summary>
        [HttpGet("{id}/members")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStudyGroupMembers(int id)
        {
            var group = await _context.StudyGroups.FindAsync(id);
            if (group == null)
                return NotFound(new { message = "Study group not found" });

            var members = await _context.GroupMembers
                .Where(gm => gm.StudyGroupID == id)
                .Include(gm => gm.User)
                .ToListAsync();

            return Ok(members);
        }
    }

    public class ApproveStudyGroupDto
    {
        public bool IsApproved { get; set; }
    }
}
