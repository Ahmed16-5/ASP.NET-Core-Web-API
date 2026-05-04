namespace ASP.NET_Core_Web_API.DTOs
{
    public class AuthResponseDto
    {
        public int UserId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? Token { get; set; }
        public bool IsApproved { get; set; }
    }
}
