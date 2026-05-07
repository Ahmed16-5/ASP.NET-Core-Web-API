using Microsoft.EntityFrameworkCore;
using ASP.NET_Core_Web_API.Data;
using ASP.NET_Core_Web_API.Interfaces;
using ASP.NET_Core_Web_API.DTOs;
using ASP.NET_Core_Web_API.models;

namespace ASP.NET_Core_Web_API.Services
{
     
    /// Join request service implementation for join request-related business logic
     
    public class JoinRequestService : IJoinRequestService
    {
        private readonly AppDbContext _context;
        private readonly IGenericRepository<JoinRequest> _repository;

        public JoinRequestService(AppDbContext context, IGenericRepository<JoinRequest> repository)
        {
            _context = context;
            _repository = repository;
        }

         
        /// Get all join requests (Admin only or group owner)
         
        public async Task<IEnumerable<JoinRequest>> GetAllJoinRequestsAsync(int currentUserId, string userRole)
        {
            IQueryable<JoinRequest> query = _context.JoinRequests
                .Include(jr => jr.User)
                .Include(jr => jr.StudyGroup);

            // Admins see all, users see only for their groups
            if (userRole != "Admin")
            {
                query = query.Where(jr => jr.StudyGroup.UserID == currentUserId);
            }

            return await query.ToListAsync();
        }

         
        /// Get join request by ID
         
        public async Task<JoinRequest> GetJoinRequestByIdAsync(int id)
        {
            return await _context.JoinRequests
                .Include(jr => jr.User)
                .Include(jr => jr.StudyGroup)
                .FirstOrDefaultAsync(jr => jr.ID == id);
        }

         
        /// Send join request to study group
         
        public async Task<JoinRequest> SendJoinRequestAsync(int studyGroupId, int userId)
        {
            // Check if already a member
            var isMember = await _context.GroupMembers
                .AnyAsync(gm => gm.UserID == userId && gm.StudyGroupID == studyGroupId);
            if (isMember)
                return null; // User is already a member

            // Check if already requested
            var existingRequest = await _context.JoinRequests
                .FirstOrDefaultAsync(jr => jr.UserID == userId && jr.StudyGroupID == studyGroupId);
            if (existingRequest != null && existingRequest.Status == "Pending")
                return null; // User has already sent a pending request

            var joinRequest = new JoinRequest
            {
                UserID = userId,
                StudyGroupID = studyGroupId,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            return await _repository.AddAsync(joinRequest);
        }

         
        /// Approve join request
         
        public async Task<JoinRequest> ApproveJoinRequestAsync(int id, int currentUserId, string userRole)
        {
            var joinRequest = await _context.JoinRequests
                .Include(jr => jr.StudyGroup)
                .FirstOrDefaultAsync(jr => jr.ID == id);

            if (joinRequest == null)
                return null;

            // Only group owner or admin can approve
            if (joinRequest.StudyGroup.UserID != currentUserId && userRole != "Admin")
                return null;

            if (joinRequest.Status != "Pending")
                return null; // Only pending requests can be approved

            // Check if member limit reached
            var memberCount = await _context.GroupMembers
                .CountAsync(gm => gm.StudyGroupID == joinRequest.StudyGroupID);

            if (memberCount >= joinRequest.StudyGroup.MaxMembers)
                return null; // Group is full

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

            return joinRequest;
        }

         
        /// Reject join request
         
        public async Task<JoinRequest> RejectJoinRequestAsync(int id, int currentUserId, string userRole)
        {
            var joinRequest = await _context.JoinRequests
                .Include(jr => jr.StudyGroup)
                .FirstOrDefaultAsync(jr => jr.ID == id);

            if (joinRequest == null)
                return null;

            // Only group owner or admin can reject
            if (joinRequest.StudyGroup.UserID != currentUserId && userRole != "Admin")
                return null;

            if (joinRequest.Status != "Pending")
                return null; // Only pending requests can be rejected

            joinRequest.Status = "Rejected";
            return await _repository.UpdateAsync(joinRequest);
        }

         
        /// Cancel join request
         
        public async Task<bool> CancelJoinRequestAsync(int id, int currentUserId)
        {
            var joinRequest = await _repository.GetByIdAsync(id);
            if (joinRequest == null)
                return false;

            // User can only cancel their own requests
            if (joinRequest.UserID != currentUserId)
                return false;

            if (joinRequest.Status != "Pending")
                return false; // Only pending requests can be cancelled

            return await _repository.DeleteAsync(id);
        }

         
        /// Get pending join requests for a study group
         
        public async Task<IEnumerable<JoinRequest>> GetPendingRequestsForGroupAsync(int studyGroupId, int currentUserId, string userRole)
        {
            var group = await _context.StudyGroups.FindAsync(studyGroupId);
            if (group == null)
                return new List<JoinRequest>();

            // Only group owner or admin can view pending requests
            if (group.UserID != currentUserId && userRole != "Admin")
                return new List<JoinRequest>();

            return await _context.JoinRequests
                .Where(jr => jr.StudyGroupID == studyGroupId && jr.Status == "Pending")
                .Include(jr => jr.User)
                .ToListAsync();
        }

         
        /// Get user's join requests
         
        public async Task<IEnumerable<JoinRequest>> GetUserJoinRequestsAsync(int userId)
        {
            return await _context.JoinRequests
                .Where(jr => jr.UserID == userId)
                .Include(jr => jr.StudyGroup)
                .ToListAsync();
        }

         
        /// Check if join request exists
         
        public async Task<bool> JoinRequestExistsAsync(int id)
        {
            return await _repository.ExistsAsync(id);
        }
    }
}
