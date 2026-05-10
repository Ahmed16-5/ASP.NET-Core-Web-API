using ASP.NET_Core_Web_API.DTOs;
using ASP.NET_Core_Web_API.models;
using ASP.NET_Core_Web_API.Enums;

namespace ASP.NET_Core_Web_API.Interfaces
{
    public interface IJoinRequestService
    {
        Task<IEnumerable<JoinRequest>> GetAllJoinRequestsAsync(int currentUserId, UserRole userRole);

        Task<JoinRequest> GetJoinRequestByIdAsync(int id);

        Task<JoinRequest> SendJoinRequestAsync(int studyGroupId, int userId);

        Task<JoinRequest> ApproveJoinRequestAsync(int id, int currentUserId, UserRole userRole);

        Task<JoinRequest> RejectJoinRequestAsync(int id, int currentUserId, UserRole userRole);

        Task<bool> CancelJoinRequestAsync(int id, int currentUserId);

        Task<IEnumerable<JoinRequest>> GetPendingRequestsForGroupAsync(int studyGroupId, int currentUserId, UserRole userRole);

        Task<IEnumerable<JoinRequest>> GetUserJoinRequestsAsync(int userId);

        Task<bool> JoinRequestExistsAsync(int id);
    }
}