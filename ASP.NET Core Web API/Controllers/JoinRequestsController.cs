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
    public class JoinRequestsController : ControllerBase
    {
        private readonly IJoinRequestService _joinRequestService;
        private readonly AuthService _authService;

        public JoinRequestsController(IJoinRequestService joinRequestService, AuthService authService)
        {
            _joinRequestService = joinRequestService;
            _authService = authService;
        }

        
        /// Get all join requests (Admin only or group owner)
        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllJoinRequests()
        {
            var currentUserId = _authService.GetUserIdFromClaims(User);
            var userRole = _authService.GetUserRoleFromClaims(User);

            var requests = await _joinRequestService.GetAllJoinRequestsAsync(currentUserId, userRole);
            return Ok(requests);
        }

        
        /// Get join request by ID
        
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetJoinRequestById(int id)
        {
            var request = await _joinRequestService.GetJoinRequestByIdAsync(id);
            if (request == null)
                return NotFound(new { message = "Join request not found" });

            return Ok(request);
        }

       
        /// Send join request to study group
        
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendJoinRequest([FromBody] SendJoinRequestDto sendDto)
        {
            var userId = _authService.GetUserIdFromClaims(User);
            var joinRequest = await _joinRequestService.SendJoinRequestAsync(sendDto.StudyGroupID, userId);

            if (joinRequest == null)
                return BadRequest(new { message = "You are already a member of this group or have a pending request" });

            return CreatedAtAction(nameof(GetJoinRequestById), new { id = joinRequest.ID }, joinRequest);
        }

        
        /// Approve join request (group owner or admin only)
        
        [HttpPut("{id}/approve")]
        [Authorize]
        public async Task<IActionResult> ApproveJoinRequest(int id)
        {
            var currentUserId = _authService.GetUserIdFromClaims(User);
            var userRole = _authService.GetUserRoleFromClaims(User);

            var joinRequest = await _joinRequestService.ApproveJoinRequestAsync(id, currentUserId, userRole);
            if (joinRequest == null)
                return NotFound(new { message = "Join request not found or cannot be approved" });

            return Ok(new { message = "Join request approved", joinRequest });
        }

        
        /// Reject join request (group owner or admin only)
        
        [HttpPut("{id}/reject")]
        [Authorize]
        public async Task<IActionResult> RejectJoinRequest(int id)
        {
            var currentUserId = _authService.GetUserIdFromClaims(User);
            var userRole = _authService.GetUserRoleFromClaims(User);

            var joinRequest = await _joinRequestService.RejectJoinRequestAsync(id, currentUserId, userRole);
            if (joinRequest == null)
                return NotFound(new { message = "Join request not found or cannot be rejected" });

            return Ok(new { message = "Join request rejected", joinRequest });
        }

        
        /// Cancel own join request
        
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> CancelJoinRequest(int id)
        {
            var currentUserId = _authService.GetUserIdFromClaims(User);

            var success = await _joinRequestService.CancelJoinRequestAsync(id, currentUserId);
            if (!success)
                return NotFound(new { message = "Join request not found or cannot be cancelled" });

            return Ok(new { message = "Join request cancelled" });
        }

        
        /// Get pending join requests for a study group
        
        [HttpGet("group/{studyGroupId}/pending")]
        [Authorize]
        public async Task<IActionResult> GetPendingRequestsForGroup(int studyGroupId)
        {
            var currentUserId = _authService.GetUserIdFromClaims(User);
            var userRole = _authService.GetUserRoleFromClaims(User);

            var pendingRequests = await _joinRequestService.GetPendingRequestsForGroupAsync(studyGroupId, currentUserId, userRole);
            return Ok(pendingRequests);
        }

        
        /// Get user's join requests
        
        [HttpGet("user/my-requests")]
        [Authorize]
        public async Task<IActionResult> GetMyJoinRequests()
        {
            var userId = _authService.GetUserIdFromClaims(User);
            var requests = await _joinRequestService.GetUserJoinRequestsAsync(userId);
            return Ok(requests);
        }
    }

    public class SendJoinRequestDto
    {
        public int StudyGroupID { get; set; }
    }
}
