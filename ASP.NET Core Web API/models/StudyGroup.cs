using ASP.NET_Core_Web_API.Enums;

namespace ASP.NET_Core_Web_API.models
{
    public class StudyGroup
    {
        public int ID { get; set; }
        public DateTime CreatedAt { get; set; }
        public GroupApprovalStatus ApprovalStatus { get; set; }
        public DateTime MeetingTime { get; set; }
        public MeetingType MeetingType { get; set; }
        public int MaxMembers { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public string? Subject { get; set; }
        public int UserID { get; set; }

        // Navigation Properties
        public User? User { get; set; }
        public ICollection<GroupMember>? GroupMembers { get; set; } = new List<GroupMember>();
        public ICollection<JoinRequest>? JoinRequests { get; set; } = new List<JoinRequest>();
        public ICollection<Material>? Materials { get; set; } = new List<Material>();
        public ICollection<Comment>? Comments { get; set; } = new List<Comment>();
    }
}
