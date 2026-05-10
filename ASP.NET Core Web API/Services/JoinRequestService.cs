using Microsoft.EntityFrameworkCore;
using ASP.NET_Core_Web_API.Data;
using ASP.NET_Core_Web_API.Interfaces;
using ASP.NET_Core_Web_API.DTOs;
using ASP.NET_Core_Web_API.models;
using ASP.NET_Core_Web_API.Enums;

namespace ASP.NET_Core_Web_API.Services
{
    public class JoinRequestService : IJoinRequestService
    {
        private readonly AppDbContext _context;
        private readonly IGenericRepository<JoinRequest> _repository;

        public JoinRequestService(AppDbContext context, IGenericRepository<JoinRequest> repository)
        {
            _context = context;
            _repository = repository;
        }

        public async Task<IEnumerable<JoinRequest>> GetAllJoinRequestsAsync(int currentUserId, UserRole userRole)
        {
            IQueryable<JoinRequest> query = _context.JoinRequests
                .Include(jr => jr.User)
                .Include(jr => jr.StudyGroup);

            // Admins see all, others see only their groups
            if (userRole != UserRole.Admin)
            {
                query = query.Where(jr => jr.StudyGroup.UserID == currentUserId);
            }

            return await query.ToListAsync();
        }

        public async Task<JoinRequest> GetJoinRequestByIdAsync(int id)
        {
            return await _context.JoinRequests
                .Include(jr => jr.User)
                .Include(jr => jr.StudyGroup)
                .FirstOrDefaultAsync(jr => jr.ID == id);
        }

        public async Task<JoinRequest> SendJoinRequestAsync(int studyGroupId, int userId)
        {
            // Check if already a member
            var isMember = await _context.GroupMembers
                .AnyAsync(gm => gm.UserID == userId && gm.StudyGroupID == studyGroupId);

            if (isMember)
                return null;

            // Check if already requested
            var existingRequest = await _context.JoinRequests
                .FirstOrDefaultAsync(jr =>
                    jr.UserID == userId &&
                    jr.StudyGroupID == studyGroupId);

            if (existingRequest != null &&
                existingRequest.Status == JoinRequestStatus.Pending)
            {
                return null;
            }

            var joinRequest = new JoinRequest
            {
                UserID = userId,
                StudyGroupID = studyGroupId,
                Status = JoinRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            return await _repository.AddAsync(joinRequest);
        }

        public async Task<JoinRequest> ApproveJoinRequestAsync(int id, int currentUserId, UserRole userRole)
        {
            var joinRequest = await _context.JoinRequests
                .Include(jr => jr.StudyGroup)
                .FirstOrDefaultAsync(jr => jr.ID == id);

            if (joinRequest == null)
                return null;

            // Only group owner or admin can approve
            if (joinRequest.StudyGroup.UserID != currentUserId &&
                userRole != UserRole.Admin)
            {
                return null;
            }

            if (joinRequest.Status != JoinRequestStatus.Pending)
                return null;

            // Check if group is full
            var memberCount = await _context.GroupMembers
                .CountAsync(gm => gm.StudyGroupID == joinRequest.StudyGroupID);

            if (memberCount >= joinRequest.StudyGroup.MaxMembers)
                return null;

            joinRequest.Status = JoinRequestStatus.Accepted;

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

        public async Task<JoinRequest> RejectJoinRequestAsync(int id, int currentUserId, UserRole userRole)
        {
            var joinRequest = await _context.JoinRequests
                .Include(jr => jr.StudyGroup)
                .FirstOrDefaultAsync(jr => jr.ID == id);

            if (joinRequest == null)
                return null;

            // Only group owner or admin can reject
            if (joinRequest.StudyGroup.UserID != currentUserId &&
                userRole != UserRole.Admin)
            {
                return null;
            }

            if (joinRequest.Status != JoinRequestStatus.Pending)
                return null;

            joinRequest.Status = JoinRequestStatus.Rejected;

            return await _repository.UpdateAsync(joinRequest);
        }

        public async Task<bool> CancelJoinRequestAsync(int id, int currentUserId)
        {
            var joinRequest = await _repository.GetByIdAsync(id);

            if (joinRequest == null)
                return false;

            if (joinRequest.UserID != currentUserId)
                return false;

            if (joinRequest.Status != JoinRequestStatus.Pending)
                return false;

            return await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<JoinRequest>> GetPendingRequestsForGroupAsync(int studyGroupId, int currentUserId, UserRole userRole)
        {
            var group = await _context.StudyGroups.FindAsync(studyGroupId);

            if (group == null)
                return new List<JoinRequest>();

            // Only owner or admin can view
            if (group.UserID != currentUserId &&
                userRole != UserRole.Admin)
            {
                return new List<JoinRequest>();
            }

            return await _context.JoinRequests
                .Where(jr =>
                    jr.StudyGroupID == studyGroupId &&
                    jr.Status == JoinRequestStatus.Pending)
                .Include(jr => jr.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<JoinRequest>> GetUserJoinRequestsAsync(int userId)
        {
            return await _context.JoinRequests
                .Where(jr => jr.UserID == userId)
                .Include(jr => jr.StudyGroup)
                .ToListAsync();
        }

        public async Task<bool> JoinRequestExistsAsync(int id)
        {
            return await _repository.ExistsAsync(id);
        }
    }
}