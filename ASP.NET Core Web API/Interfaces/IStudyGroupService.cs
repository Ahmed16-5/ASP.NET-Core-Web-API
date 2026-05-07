using ASP.NET_Core_Web_API.DTOs;
using ASP.NET_Core_Web_API.models;

namespace ASP.NET_Core_Web_API.Interfaces
{
     
    /// Study group service interface for study group-related business logic
     
    public interface IStudyGroupService
    {
         
        /// Get all approved study groups
         
        Task<IEnumerable<StudyGroup>> GetAllApprovedStudyGroupsAsync();

         
        /// Get study group by ID
         
        Task<StudyGroup> GetStudyGroupByIdAsync(int id);

         
        /// Get study group by ID with authorization check
         
        Task<StudyGroup> GetStudyGroupByIdWithAuthAsync(int id, int? currentUserId, string? userRole);

         
        /// Create new study group
         
        Task<StudyGroup> CreateStudyGroupAsync(CreateStudyGroupDto createDto, int userId);

         
        /// Update study group
         
        Task<StudyGroup> UpdateStudyGroupAsync(int id, UpdateStudyGroupDto updateDto, int currentUserId, string userRole);

         
        /// Approve or reject study group (Admin only)
         
        Task<StudyGroup> ApproveStudyGroupAsync(int id, bool isApproved);

         
        /// Get current user's study groups
         
        Task<IEnumerable<StudyGroup>> GetUserStudyGroupsAsync(int userId);

         
        /// Search study groups by criteria
         
        Task<IEnumerable<StudyGroup>> SearchStudyGroupsAsync(string? subject, string? location, DateTime? meetingTime);

         
        /// Delete study group
         
        Task<bool> DeleteStudyGroupAsync(int id, int currentUserId, string userRole);

         
        /// Get study group members
         
        Task<IEnumerable<GroupMember>> GetStudyGroupMembersAsync(int id);

         
        /// Check if study group exists
         
        Task<bool> StudyGroupExistsAsync(int id);
    }
}
