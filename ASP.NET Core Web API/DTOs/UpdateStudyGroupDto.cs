using System.ComponentModel.DataAnnotations;

namespace ASP.NET_Core_Web_API.DTOs
{
    public class UpdateStudyGroupDto
    {
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Subject must be between 1 and 200 characters")]
        public string? Subject { get; set; }

        [StringLength(500, ErrorMessage = "Description must not exceed 500 characters")]
        public string? Description { get; set; }

        [StringLength(200, ErrorMessage = "Location must not exceed 200 characters")]
        public string? Location { get; set; }

        public DateTime MeetingTime { get; set; }

        [RegularExpression("^(Online|Offline)$", ErrorMessage = "Meeting type must be 'Online' or 'Offline'")]
        public string? MeetingType { get; set; }

        [Range(1, 10000, ErrorMessage = "Max members must be between 1 and 10000")]
        public int MaxMembers { get; set; }
    }
}
