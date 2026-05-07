using Microsoft.EntityFrameworkCore;
using ASP.NET_Core_Web_API.Data;
using ASP.NET_Core_Web_API.Interfaces;
using ASP.NET_Core_Web_API.DTOs;
using ASP.NET_Core_Web_API.models;

namespace ASP.NET_Core_Web_API.Services
{
     
    /// Study group service implementation for study group-related business logic
     
    public class StudyGroupService : IStudyGroupService
    {
        private readonly AppDbContext _context;
        private readonly IGenericRepository<StudyGroup> _repository;

        public StudyGroupService(AppDbContext context, IGenericRepository<StudyGroup> repository)
        {
            _context = context;
            _repository = repository;
        }

         
        /// Get all approved study groups
         
        public async Task<IEnumerable<StudyGroup>> GetAllApprovedStudyGroupsAsync()
        {
            return await _context.StudyGroups
                .Where(sg => sg.IsApproved)
                .Include(sg => sg.User)
                .Include(sg => sg.GroupMembers)
                .ToListAsync();
        }

         
        /// Get study group by ID
         
        public async Task<StudyGroup> GetStudyGroupByIdAsync(int id)
        {
            return await _context.StudyGroups
                .Include(sg => sg.User)
                .Include(sg => sg.GroupMembers)
                .Include(sg => sg.Materials)
                .Include(sg => sg.Comments)
                .FirstOrDefaultAsync(sg => sg.ID == id);
        }

         
        /// Get study group by ID with authorization check
         
        public async Task<StudyGroup> GetStudyGroupByIdWithAuthAsync(int id, int? currentUserId, string? userRole)
        {
            var group = await GetStudyGroupByIdAsync(id);
            if (group == null)
                return null;

            // If not approved, only owner or admin can view
            if (!group.IsApproved && currentUserId.HasValue)
            {
                if (group.UserID != currentUserId && userRole != "Admin")
                    return null;
            }

            return group;
        }

         
        /// Create new study group
         
        public async Task<StudyGroup> CreateStudyGroupAsync(CreateStudyGroupDto createDto, int userId)
        {
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

            return await _repository.AddAsync(studyGroup);
        }

         
        /// Update study group
         
        public async Task<StudyGroup> UpdateStudyGroupAsync(int id, UpdateStudyGroupDto updateDto, int currentUserId, string userRole)
        {
            var group = await _repository.GetByIdAsync(id);
            if (group == null)
                return null;

            // Only owner or admin can update
            if (group.UserID != currentUserId && userRole != "Admin")
                return null;

            group.Subject = updateDto.Subject ?? group.Subject;
            group.Description = updateDto.Description ?? group.Description;
            group.Location = updateDto.Location ?? group.Location;
            group.MeetingTime = updateDto.MeetingTime != default ? updateDto.MeetingTime : group.MeetingTime;
            group.MeetingType = updateDto.MeetingType ?? group.MeetingType;
            group.MaxMembers = updateDto.MaxMembers > 0 ? updateDto.MaxMembers : group.MaxMembers;

            return await _repository.UpdateAsync(group);
        }

         
        /// Approve or reject study group (Admin only)
         
        public async Task<StudyGroup> ApproveStudyGroupAsync(int id, bool isApproved)
        {
            var group = await _repository.GetByIdAsync(id);
            if (group == null)
                return null;

            group.IsApproved = isApproved;
            return await _repository.UpdateAsync(group);
        }

         
        /// Get current user's study groups
         
        public async Task<IEnumerable<StudyGroup>> GetUserStudyGroupsAsync(int userId)
        {
            return await _context.StudyGroups
                .Where(sg => sg.UserID == userId)
                .Include(sg => sg.GroupMembers)
                .ToListAsync();
        }

         
        /// Search study groups by criteria
         
        public async Task<IEnumerable<StudyGroup>> SearchStudyGroupsAsync(string? subject, string? location, DateTime? meetingTime)
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

            return await query
                .Include(sg => sg.User)
                .Include(sg => sg.GroupMembers)
                .ToListAsync();
        }

         
        /// Delete study group
         
        public async Task<bool> DeleteStudyGroupAsync(int id, int currentUserId, string userRole)
        {
            var group = await _context.StudyGroups
                .Include(sg => sg.GroupMembers)
                .Include(sg => sg.JoinRequests)
                .Include(sg => sg.Materials)
                .Include(sg => sg.Comments)
                .FirstOrDefaultAsync(sg => sg.ID == id);

            if (group == null)
                return false;

            // Only owner or admin can delete
            if (group.UserID != currentUserId && userRole != "Admin")
                return false;

            // Remove all related data
            _context.GroupMembers.RemoveRange(group.GroupMembers);
            _context.JoinRequests.RemoveRange(group.JoinRequests);
            _context.Materials.RemoveRange(group.Materials);
            _context.Comments.RemoveRange(group.Comments);

            _context.StudyGroups.Remove(group);
            await _context.SaveChangesAsync();

            return true;
        }

         
        /// Get study group members
         
        public async Task<IEnumerable<GroupMember>> GetStudyGroupMembersAsync(int id)
        {
            return await _context.GroupMembers
                .Where(gm => gm.StudyGroupID == id)
                .Include(gm => gm.User)
                .ToListAsync();
        }

         
        /// Check if study group exists
         
        public async Task<bool> StudyGroupExistsAsync(int id)
        {
            return await _repository.ExistsAsync(id);
        }
    }
}
