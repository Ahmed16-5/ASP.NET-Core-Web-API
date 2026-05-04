using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ASP.NET_Core_Web_API.Data;
using ASP.NET_Core_Web_API.models;
using ASP.NET_Core_Web_API.DTOs;
using ASP.NET_Core_Web_API.Services;

namespace ASP.NET_Core_Web_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;

        public UsersController(AppDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(registerDto.Password) || 
                registerDto.Password != registerDto.ConfirmPassword)
                return BadRequest(new { message = "Passwords do not match" });

            var result = await _authService.RegisterAsync(registerDto);
            if (result == null)
                return BadRequest(new { message = "User registration failed or email already exists" });

            return Ok(new { message = "User registered successfully. Please wait for admin approval to login.", user = result });
        }

        /// <summary>
        /// Login user
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(loginDto);
            if (result == null)
                return Unauthorized(new { message = "Invalid email or password" });

            if (!result.IsApproved)
                return Unauthorized(new { message = "Your account is not approved by admin yet. Please contact administrator." });

            return Ok(result);
        }

        /// <summary>
        /// Get all users (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllUsers()
        {
            var userRole = _authService.GetUserRoleFromClaims(User);
            if (userRole != "Admin")
                return Forbid();

            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(user);
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        [HttpGet("profile/me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUserProfile()
        {
            var userId = _authService.GetUserIdFromClaims(User);
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(user);
        }

        /// <summary>
        /// Update user (only own profile or admin can update others)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User updateDto)
        {
            var currentUserId = _authService.GetUserIdFromClaims(User);
            var userRole = _authService.GetUserRoleFromClaims(User);

            // Only allow users to update their own profile or admins to update anyone
            if (currentUserId != id && userRole != "Admin")
                return Forbid();

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            user.Name = updateDto.Name ?? user.Name;
            user.Email = updateDto.Email ?? user.Email;
            user.Role = userRole == "Admin" ? updateDto.Role ?? user.Role : user.Role;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User updated successfully", user });
        }

        /// <summary>
        /// Approve/Reject user (Admin only)
        /// </summary>
        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveUser(int id, [FromBody] ApproveUserDto approveDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            user.IsApproved = approveDto.IsApproved;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"User {(approveDto.IsApproved ? "approved" : "rejected")}", user });
        }

        /// <summary>
        /// Delete user (Admin only or user can delete own account)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var currentUserId = _authService.GetUserIdFromClaims(User);
            var userRole = _authService.GetUserRoleFromClaims(User);

            if (currentUserId != id && userRole != "Admin")
                return Forbid();

            var user = await _context.Users
                .Include(u => u.StudyGroups)
                .Include(u => u.JoinRequests)
                .Include(u => u.GroupMembers)
                .Include(u => u.Materials)
                .Include(u => u.Comments)
                .FirstOrDefaultAsync(u => u.ID == id);

            if (user == null)
                return NotFound(new { message = "User not found" });

            // Remove user from all relationships
            _context.StudyGroups.RemoveRange(user.StudyGroups);
            _context.JoinRequests.RemoveRange(user.JoinRequests);
            _context.GroupMembers.RemoveRange(user.GroupMembers);
            _context.Materials.RemoveRange(user.Materials);
            _context.Comments.RemoveRange(user.Comments);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User deleted successfully" });
        }
    }

    public class ApproveUserDto
    {
        public bool IsApproved { get; set; }
    }
}
