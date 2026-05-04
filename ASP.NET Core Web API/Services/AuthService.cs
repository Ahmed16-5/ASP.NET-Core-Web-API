using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ASP.NET_Core_Web_API.Data;
using ASP.NET_Core_Web_API.models;
using ASP.NET_Core_Web_API.DTOs;

namespace ASP.NET_Core_Web_API.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Hash password using SHA256
        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // Verify password
        public bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hash;
        }

        // Register new user
        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            if (string.IsNullOrWhiteSpace(registerDto.Email) || 
                string.IsNullOrWhiteSpace(registerDto.Password) || 
                string.IsNullOrWhiteSpace(registerDto.Name))
            {
                return null;
            }

            // Check if user already exists
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == registerDto.Email);
            if (existingUser != null)
            {
                return null;
            }

            var user = new User
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                PasswordHash = HashPassword(registerDto.Password),
                Role = "User", // Default role
                CreatedAt = DateTime.UtcNow,
                IsApproved = false // New users need admin approval
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                UserId = user.ID,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                IsApproved = user.IsApproved,
                Token = null // No token until approved and login
            };
        }

        // Login user
        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            if (string.IsNullOrWhiteSpace(loginDto.Email) || 
                string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return null;
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == loginDto.Email);
            if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                return null;
            }

            // Only approved users can login
            if (!user.IsApproved)
            {
                return new AuthResponseDto
                {
                    UserId = user.ID,
                    Name = user.Name,
                    Email = user.Email,
                    IsApproved = false,
                    Token = null
                };
            }

            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                UserId = user.ID,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                IsApproved = user.IsApproved,
                Token = token
            };
        }

        // Generate JWT token
        public string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"] ?? "your-secret-key-min-32-chars-long")
            );
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new System.Security.Claims.Claim("UserId", user.ID.ToString()),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email ?? ""),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, user.Role ?? "User"),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.Name ?? "")
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "StudyGroupAPI",
                audience: _configuration["Jwt:Audience"] ?? "StudyGroupAPIUsers",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Get user ID from claims
        public int GetUserIdFromClaims(System.Security.Claims.ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst("UserId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return 0;
        }

        // Get user role from claims
        public string GetUserRoleFromClaims(System.Security.Claims.ClaimsPrincipal user)
        {
            var roleClaim = user.FindFirst(System.Security.Claims.ClaimTypes.Role);
            return roleClaim?.Value ?? "User";
        }
    }
}
