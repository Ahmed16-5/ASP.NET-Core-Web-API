namespace ASP.NET_Core_Web_API.DTOs
{
    public class UpdateStudyGroupDto
    {
        public string? Subject { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public DateTime MeetingTime { get; set; }
        public string? MeetingType { get; set; }
        public int MaxMembers { get; set; }
    }
}
