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
    public class JoinRequestsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;

        public JoinRequestsController(AppDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        /// <summary>
        /// Get all join requests (Admin only or group owner)
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllJoinRequests()
        {
            var currentUserId = _authService.GetUserIdFromClaims(User);
            var userRole = _authService.GetUserRoleFromClaims(User);

            IQueryable<JoinRequest> query = _context.JoinRequests
                .Include(jr => jr.User)
                .Include(jr => jr.StudyGroup);

            // Admins see all, users see only for their groups
            if (userRole != "Admin")
            {
                query = query.Where(jr => jr.StudyGroup.UserID == currentUserId);
            }

            var requests = await query.ToListAsync();
            return Ok(requests);
        }

        /// <summary>
        /// Get join request by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetJoinRequestById(int id)
        {
            var request = await _context.JoinRequests
                .Include(jr => jr.User)
                .Include(jr => jr.StudyGroup)
                .FirstOrDefaultAsync(jr => jr.ID == id);

            if (request == null)
                return NotFound(new { message = "Join request not found" });

            return Ok(request);
        }

        /// <summary>
        /// Send join request to study group
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendJoinRequest([FromBody] SendJoinRequestDto sendDto)
        {
            var userId = _authService.GetUserIdFromClaims(User);
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            var studyGroup = await _context.StudyGroups.FindAsync(sendDto.StudyGroupID);
            if (studyGroup == null)
                return NotFound(new { message = "Study group not found" });

            // Check if already a member
            var isMember = await _context.GroupMembers
                .AnyAsync(gm => gm.UserID == userId && gm.StudyGroupID == sendDto.StudyGroupID);
            if (isMember)
                return BadRequest(new { message = "You are already a member of this group" });

            // Check if already requested
            var existingRequest = await _context.JoinRequests
                .FirstOrDefaultAsync(jr => jr.UserID == userId && jr.StudyGroupID == sendDto.StudyGroupID);
            if (existingRequest != null && existingRequest.Status == "Pending")
                return BadRequest(new { message = "You have already sent a request to this group" });

            var joinRequest = new JoinRequest
            {
                UserID = userId,
                StudyGroupID = sendDto.StudyGroupID,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.JoinRequests.Add(joinRequest);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetJoinRequestById), new { id = joinRequest.ID }, joinRequest);
        }

        /// <summary>
        /// Approve join request (group owner or admin only)
        /// </summary>
        [HttpPut("{id}/approve")]
        [Authorize]
        public async Task<IActionResult> ApproveJoinRequest(int id)
        {
            var currentUserId = _authService.GetUserIdFromClaims(User);
            var userRole = _authService.GetUserRoleFromClaims(User);

            var joinRequest = await _context.JoinRequests
                .Include(jr => jr.StudyGroup)
                .FirstOrDefaultAsync(jr => jr.ID == id);

            if (joinRequest == null)
                return NotFound(new { message = "Join request not found" });

            // Only group owner or admin can approve
            if (joinRequest.StudyGroup.UserID != currentUserId && userRole != "Admin")
                return Forbid();

            if (joinRequest.Status != "Pending")
                return BadRequest(new { message = "Only pending requests can be approved" });

            // Check if member limit reached
            var memberCount = await _context.GroupMembers
                .CountAsync(gm => gm.StudyGroupID == joinRequest.StudyGroupID);

            if (memberCount >= joinRequest.StudyGroup.MaxMembers)
                return BadRequest(new { message = "Group is full, cannot add more members" });

            joinRequest.Status = "Approved";

            // Add user as group member
            var groupMember = new GroupMember
            {
                UserID = joinRequest.UserID,
                StudyGroupID = joinRequest.StudyGroupID,
                JoinedAt = DateTime.UtcNow
            };

            _context.JoinRequests.Update(joinRequest);
            _context.GroupMembers.Add(groupMember);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Join request approved", joinRequest });
        }

        /// <summary>
        /// Reject join request (group owner or admin only)
        /// </summary>
        [HttpPut("{id}/reject")]
        [Authorize]
        public async Task<IActionResult> RejectJoinRequest(int id)
        {
            var currentUserId = _authService.GetUserIdFromClaims(User);
            var userRole = _authService.GetUserRoleFromClaims(User);

            var joinRequest = await _context.JoinRequests
                .Include(jr => jr.StudyGroup)
                .FirstOrDefaultAsync(jr => jr.ID == id);

            if (joinRequest == null)
                return NotFound(new { message = "Join request not found" });

            // Only group owner or admin can reject
            if (joinRequest.StudyGroup.UserID != currentUserId && userRole != "Admin")
                return Forbid();

            if (joinRequest.Status != "Pending")
                return BadRequest(new { message = "Only pending requests can be rejected" });

            joinRequest.Status = "Rejected";
            _context.JoinRequests.Update(joinRequest);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Join request rejected", joinRequest });
        }

        /// <summary>
        /// Cancel own join request
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> CancelJoinRequest(int id)
        {
            var currentUserId = _authService.GetUserIdFromClaims(User);

            var joinRequest = await _context.JoinRequests.FindAsync(id);
            if (joinRequest == null)
                return NotFound(new { message = "Join request not found" });

            // User can only cancel their own requests
            if (joinRequest.UserID != currentUserId)
                return Forbid();

            if (joinRequest.Status != "Pending")
                return BadRequest(new { message = "Only pending requests can be cancelled" });

            _context.JoinRequests.Remove(joinRequest);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Join request cancelled" });
        }

        /// <summary>
        /// Get pending join requests for a study group
        /// </summary>
        [HttpGet("group/{studyGroupId}/pending")]
        [Authorize]
        public async Task<IActionResult> GetPendingRequestsForGroup(int studyGroupId)
        {
            var currentUserId = _authService.GetUserIdFromClaims(User);
            var userRole = _authService.GetUserRoleFromClaims(User);

            var group = await _context.StudyGroups.FindAsync(studyGroupId);
            if (group == null)
                return NotFound(new { message = "Study group not found" });

            // Only group owner or admin can view pending requests
            if (group.UserID != currentUserId && userRole != "Admin")
                return Forbid();

            var pendingRequests = await _context.JoinRequests
                .Where(jr => jr.StudyGroupID == studyGroupId && jr.Status == "Pending")
                .Include(jr => jr.User)
                .ToListAsync();

            return Ok(pendingRequests);
        }

        /// <summary>
        /// Get user's join requests
        /// </summary>
        [HttpGet("user/my-requests")]
        [Authorize]
        public async Task<IActionResult> GetMyJoinRequests()
        {
            var userId = _authService.GetUserIdFromClaims(User);

            var requests = await _context.JoinRequests
                .Where(jr => jr.UserID == userId)
                .Include(jr => jr.StudyGroup)
                .ToListAsync();

            return Ok(requests);
        }
    }

    public class SendJoinRequestDto
    {
        public int StudyGroupID { get; set; }
    }
}
