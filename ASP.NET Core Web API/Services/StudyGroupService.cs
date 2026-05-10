using Microsoft.EntityFrameworkCore;
using ASP.NET_Core_Web_API.Data;
using ASP.NET_Core_Web_API.Interfaces;
using ASP.NET_Core_Web_API.DTOs;
using ASP.NET_Core_Web_API.models;
using ASP.NET_Core_Web_API.Enums;

namespace ASP.NET_Core_Web_API.Services
{
    public class StudyGroupService : IStudyGroupService
    {
        private readonly AppDbContext _context;
        private readonly IGenericRepository<StudyGroup> _repository;

        public StudyGroupService(AppDbContext context, IGenericRepository<StudyGroup> repository)
        {
            _context = context;
            _repository = repository;
        }

        public async Task<IEnumerable<StudyGroup>> GetAllApprovedStudyGroupsAsync()
        {
            return await _context.StudyGroups
                .Where(sg => sg.ApprovalStatus == GroupApprovalStatus.Approved)
                .Include(sg => sg.User)
                .Include(sg => sg.GroupMembers)
                .ToListAsync();
        }

        public async Task<StudyGroup> GetStudyGroupByIdAsync(int id)
        {
            return await _context.StudyGroups
                .Include(sg => sg.User)
                .Include(sg => sg.GroupMembers)
                .Include(sg => sg.Materials)
                .Include(sg => sg.Comments)
                .FirstOrDefaultAsync(sg => sg.ID == id);
        }

        public async Task<StudyGroup> GetStudyGroupByIdWithAuthAsync(int id, int? currentUserId, UserRole? userRole)
        {
            var group = await GetStudyGroupByIdAsync(id);

            if (group == null)
                return null;

            // If not approved, only owner or admin can view
            if (group.ApprovalStatus != GroupApprovalStatus.Approved)
            {
                if (!currentUserId.HasValue)
                    return null;

                if (group.UserID != currentUserId && userRole != UserRole.Admin)
                    return null;
            }

            return group;
        }

        public async Task<StudyGroup> CreateStudyGroupAsync(CreateStudyGroupDto createDto, int userId)
        {
            var studyGroup = new StudyGroup
            {
                Subject = createDto.Subject,
                Description = createDto.Description,
                Location = createDto.Location,
                MeetingTime = createDto.MeetingTime,
                MeetingType = Enum.Parse<MeetingType>(createDto.MeetingType),
                MaxMembers = createDto.MaxMembers,
                UserID = userId,
                CreatedAt = DateTime.UtcNow,
                ApprovalStatus = GroupApprovalStatus.Pending
            };

            return await _repository.AddAsync(studyGroup);
        }

        public async Task<StudyGroup> UpdateStudyGroupAsync(int id, UpdateStudyGroupDto updateDto, int currentUserId, UserRole userRole)
        {
            var group = await _repository.GetByIdAsync(id);

            if (group == null)
                return null;

            // Only owner or admin can update
            if (group.UserID != currentUserId && userRole != UserRole.Admin)
                return null;

            group.Subject = updateDto.Subject ?? group.Subject;
            group.Description = updateDto.Description ?? group.Description;
            group.Location = updateDto.Location ?? group.Location;
            group.MeetingTime = updateDto.MeetingTime != default
                ? updateDto.MeetingTime
                : group.MeetingTime;

            if (!string.IsNullOrWhiteSpace(updateDto.MeetingType))
            {
                group.MeetingType = Enum.Parse<MeetingType>(updateDto.MeetingType);
            }

            group.MaxMembers = updateDto.MaxMembers > 0
                ? updateDto.MaxMembers
                : group.MaxMembers;

            return await _repository.UpdateAsync(group);
        }

        public async Task<StudyGroup> ApproveStudyGroupAsync(int id, bool isApproved)
        {
            var group = await _repository.GetByIdAsync(id);

            if (group == null)
                return null;

            group.ApprovalStatus = isApproved
                ? GroupApprovalStatus.Approved
                : GroupApprovalStatus.Rejected;

            return await _repository.UpdateAsync(group);
        }

        public async Task<IEnumerable<StudyGroup>> GetUserStudyGroupsAsync(int userId)
        {
            return await _context.StudyGroups
                .Where(sg => sg.UserID == userId)
                .Include(sg => sg.GroupMembers)
                .ToListAsync();
        }

        public async Task<IEnumerable<StudyGroup>> SearchStudyGroupsAsync(string? subject, string? location, DateTime? meetingTime)
        {
            var query = _context.StudyGroups
                .Where(sg => sg.ApprovalStatus == GroupApprovalStatus.Approved);

            if (!string.IsNullOrWhiteSpace(subject))
            {
                query = query.Where(sg =>
                    sg.Subject != null &&
                    sg.Subject.Contains(subject));
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                query = query.Where(sg =>
                    sg.Location != null &&
                    sg.Location.Contains(location));
            }

            if (meetingTime.HasValue)
            {
                var dateOnly = meetingTime.Value.Date;

                query = query.Where(sg =>
                    sg.MeetingTime.Date == dateOnly);
            }

            return await query
                .Include(sg => sg.User)
                .Include(sg => sg.GroupMembers)
                .ToListAsync();
        }

        public async Task<bool> DeleteStudyGroupAsync(int id, int currentUserId, UserRole userRole)
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
            if (group.UserID != currentUserId && userRole != UserRole.Admin)
                return false;

            _context.GroupMembers.RemoveRange(group.GroupMembers);
            _context.JoinRequests.RemoveRange(group.JoinRequests);
            _context.Materials.RemoveRange(group.Materials);
            _context.Comments.RemoveRange(group.Comments);

            _context.StudyGroups.Remove(group);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<GroupMember>> GetStudyGroupMembersAsync(int id)
        {
            return await _context.GroupMembers
                .Where(gm => gm.StudyGroupID == id)
                .Include(gm => gm.User)
                .ToListAsync();
        }

        public async Task<bool> StudyGroupExistsAsync(int id)
        {
            return await _repository.ExistsAsync(id);
        }
    }
}