namespace ASP.NET_Core_Web_API.models
{
    public class User
    {
        public int ID { get; set; }
        public string? Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Role { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public bool IsApproved { get; set; }

        // Navigation Properties

        public ICollection<StudyGroup> StudyGroups { get; set; } = new List<StudyGroup>();
        public ICollection<JoinRequest> JoinRequests { get; set; } = new List<JoinRequest>();
        public ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();
        public ICollection<Material> Materials { get; set; } = new List<Material>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
