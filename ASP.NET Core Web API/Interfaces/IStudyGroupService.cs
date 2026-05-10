using ASP.NET_Core_Web_API.DTOs;
using ASP.NET_Core_Web_API.models;
using ASP.NET_Core_Web_API.Enums;

namespace ASP.NET_Core_Web_API.Interfaces
{
    public interface IStudyGroupService
    {
        Task<IEnumerable<StudyGroup>> GetAllApprovedStudyGroupsAsync();

        Task<StudyGroup> GetStudyGroupByIdAsync(int id);

        Task<StudyGroup> GetStudyGroupByIdWithAuthAsync(int id, int? currentUserId, UserRole? userRole);

        Task<StudyGroup> CreateStudyGroupAsync(CreateStudyGroupDto createDto, int userId);

        Task<StudyGroup> UpdateStudyGroupAsync(int id, UpdateStudyGroupDto updateDto, int currentUserId, UserRole userRole);

        Task<StudyGroup> ApproveStudyGroupAsync(int id, bool isApproved);

        Task<IEnumerable<StudyGroup>> GetUserStudyGroupsAsync(int userId);

        Task<IEnumerable<StudyGroup>> SearchStudyGroupsAsync(string? subject, string? location, DateTime? meetingTime);

        Task<bool> DeleteStudyGroupAsync(int id, int currentUserId, UserRole userRole);

        Task<IEnumerable<GroupMember>> GetStudyGroupMembersAsync(int id);

        Task<bool> StudyGroupExistsAsync(int id);
    }
}