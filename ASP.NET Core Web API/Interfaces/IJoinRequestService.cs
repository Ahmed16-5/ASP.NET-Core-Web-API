using ASP.NET_Core_Web_API.DTOs;
using ASP.NET_Core_Web_API.models;

namespace ASP.NET_Core_Web_API.Interfaces
{
    
    /// Join request service interface for join request-related business logic
    
    public interface IJoinRequestService
    {
        
        /// Get all join requests (Admin only or group owner)
        
        Task<IEnumerable<JoinRequest>> GetAllJoinRequestsAsync(int currentUserId, string userRole);

        
        /// Get join request by ID
        
        Task<JoinRequest> GetJoinRequestByIdAsync(int id);

        
        /// Send join request to study group
        
        Task<JoinRequest> SendJoinRequestAsync(int studyGroupId, int userId);

        
        /// Approve join request
        
        Task<JoinRequest> ApproveJoinRequestAsync(int id, int currentUserId, string userRole);

        
        /// Reject join request
        
        Task<JoinRequest> RejectJoinRequestAsync(int id, int currentUserId, string userRole);

        
        /// Cancel join request
        
        Task<bool> CancelJoinRequestAsync(int id, int currentUserId);

        
        /// Get pending join requests for a study group
        
        Task<IEnumerable<JoinRequest>> GetPendingRequestsForGroupAsync(int studyGroupId, int currentUserId, string userRole);

        
        /// Get user's join requests
        
        Task<IEnumerable<JoinRequest>> GetUserJoinRequestsAsync(int userId);

        
        /// Check if join request exists
        
        Task<bool> JoinRequestExistsAsync(int id);
    }
}
