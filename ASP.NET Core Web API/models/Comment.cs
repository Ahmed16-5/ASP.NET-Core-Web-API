namespace ASP.NET_Core_Web_API.models
{
    public class Comment
    {
        public int ID { get; set; }
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserID { get; set; }
        public int StudyGroupID { get; set; }

        // Navigation Properties
        public User? User { get; set; }
        public StudyGroup? StudyGroup { get; set; }
    }
}
