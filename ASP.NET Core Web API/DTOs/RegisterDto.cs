using ASP.NET_Core_Web_API.Enums;

namespace ASP.NET_Core_Web_API.DTOs
{
    public class RegisterDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public UserRole Role { get; set; }
    }
}