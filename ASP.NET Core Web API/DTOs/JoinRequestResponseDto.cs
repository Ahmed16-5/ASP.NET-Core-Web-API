namespace ASP.NET_Core_Web_API.DTOs
{
    public class JoinRequestResponseDto
    {
        public int Id { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int StudyGroupId { get; set; }
        public string? StudyGroupSubject { get; set; }
    }
}
