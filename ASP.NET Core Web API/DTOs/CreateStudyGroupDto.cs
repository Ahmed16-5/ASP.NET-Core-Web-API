using System.ComponentModel.DataAnnotations;

namespace ASP.NET_Core_Web_API.DTOs
{
    public class CreateStudyGroupDto
    {
        [Required(ErrorMessage = "Subject is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Subject must be between 1 and 200 characters")]
        public string? Subject { get; set; }

        [StringLength(500, ErrorMessage = "Description must not exceed 500 characters")]
        public string? Description { get; set; }

        [StringLength(200, ErrorMessage = "Location must not exceed 200 characters")]
        public string? Location { get; set; }

        [Required(ErrorMessage = "Meeting time is required")]
        public DateTime MeetingTime { get; set; }

        [Required(ErrorMessage = "Meeting type is required")]
        [RegularExpression("^(Online|Offline)$", ErrorMessage = "Meeting type must be 'Online' or 'Offline'")]
        public string? MeetingType { get; set; }

        [Required(ErrorMessage = "Max members is required")]
        [Range(1, 10000, ErrorMessage = "Max members must be between 1 and 10000")]
        public int MaxMembers { get; set; }
    }
}
