using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ASP.NET_Core_Web_API.models;
using ASP.NET_Core_Web_API.DTOs;
using ASP.NET_Core_Web_API.Services;
using ASP.NET_Core_Web_API.Interfaces;
using ASP.NET_Core_Web_API.Enums;
using Microsoft.AspNetCore.SignalR;
using ASP.NET_Core_Web_API.Hubs;

namespace ASP.NET_Core_Web_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class JoinRequestsController : ControllerBase
    {
        private readonly IJoinRequestService _joinRequestService;
        private readonly AuthService _authService;
        private readonly IHubContext<StudyGroupHub> _hubContext;

        public JoinRequestsController(IJoinRequestService joinRequestService, AuthService authService, IHubContext<StudyGroupHub> hubContext)
        {
            _joinRequestService = joinRequestService;
            _authService = authService;
            _hubContext = hubContext;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllJoinRequests()
        {
            var currentUserId = _authService.GetUserIdFromClaims(User);
            var userRole = _authService.GetUserRoleFromClaims(User);

            if (userRole != UserRole.Admin)
                return Forbid();

            var requests = await _joinRequestService.GetAllJoinRequestsAsync(currentUserId, userRole);

            return Ok(requests);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetJoinRequestById(int id)
        {
            var request = await _joinRequestService.GetJoinRequestByIdAsync(id);

            if (request == null)
                return NotFound(new { message = "Join request not found" });

            return Ok(request);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendJoinRequest([FromBody] SendJoinRequestDto sendDto)
        {
            var userId = _authService.GetUserIdFromClaims(User);

            var joinRequest = await _joinRequestService.SendJoinRequestAsync(sendDto.StudyGroupID, userId);

            if (joinRequest == null)
                return BadRequest(new
                {
                    message = "You are already a member of this group or have a pending request"
                });

                await _hubContext.Clients.Group(sendDto.StudyGroupID.ToString())
                    .SendAsync("ReceiveJoinRequest", joinRequest);

            return CreatedAtAction(nameof(GetJoinRequestById),
                new { id = joinRequest.ID },
                joinRequest);
        }

        [HttpPut("{id}/approve")]
        [Authorize]
        public async Task<IActionResult> ApproveJoinRequest(int id)
        {
            var currentUserId = _authService.GetUserIdFromClaims(User);
            var userRole = _authService.GetUserRoleFromClaims(User);

            var joinRequest = await _joinRequestService
                .ApproveJoinRequestAsync(id, currentUserId, userRole);

            if (joinRequest == null)
                return NotFound(new
                {
                    message = "Join request not found, cannot be approved, or group is full"
                });

            return Ok(new
            {
                message = "Join request approved successfully",
                joinRequest
            });
        }

        [HttpPut("{id}/reject")]
        [Authorize]
        public async Task<IActionResult> RejectJoinRequest(int id)
        {
            var currentUserId = _authService.GetUserIdFromClaims(User);
            var userRole = _authService.GetUserRoleFromClaims(User);

            var joinRequest = await _joinRequestService
                .RejectJoinRequestAsync(id, currentUserId, userRole);

            if (joinRequest == null)
                return NotFound(new
                {
                    message = "Join request not found, already processed, or you don't have permission"
                });

            return Ok(new
            {
                message = "Join request rejected successfully",
                joinRequest
            });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> CancelJoinRequest(int id)
        {
            var currentUserId = _authService.GetUserIdFromClaims(User);

            var success = await _joinRequestService
                .CancelJoinRequestAsync(id, currentUserId);

            if (!success)
                return NotFound(new
                {
                    message = "Join request not found or cannot be cancelled"
                });

            return Ok(new { message = "Join request cancelled" });
        }

        [HttpGet("group/{studyGroupId}/pending")]
        [Authorize]
        public async Task<IActionResult> GetPendingRequestsForGroup(int studyGroupId)
        {
            var currentUserId = _authService.GetUserIdFromClaims(User);
            var userRole = _authService.GetUserRoleFromClaims(User);

            var pendingRequests = await _joinRequestService
                .GetPendingRequestsForGroupAsync(studyGroupId, currentUserId, userRole);

            return Ok(pendingRequests);
        }

        [HttpGet("user/my-requests")]
        [Authorize]
        public async Task<IActionResult> GetMyJoinRequests()
        {
            var userId = _authService.GetUserIdFromClaims(User);

            var requests = await _joinRequestService
                .GetUserJoinRequestsAsync(userId);

            return Ok(requests);
        }
    }

    public class SendJoinRequestDto
    {
        public int StudyGroupID { get; set; }
    }
}