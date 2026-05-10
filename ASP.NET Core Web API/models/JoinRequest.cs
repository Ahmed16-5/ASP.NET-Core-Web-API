using ASP.NET_Core_Web_API.Enums;

namespace ASP.NET_Core_Web_API.models
{
    public class JoinRequest
    {
        public int ID { get; set; }
        public JoinRequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserID { get; set; }
        public int StudyGroupID { get; set; }

        // Navigation Properties
        public User? User { get; set; }
        public StudyGroup? StudyGroup { get; set; }
    }
}
