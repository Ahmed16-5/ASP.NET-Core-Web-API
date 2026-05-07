using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ASP.NET_Core_Web_API.models;
using ASP.NET_Core_Web_API.DTOs;
using ASP.NET_Core_Web_API.Services;
using ASP.NET_Core_Web_API.Interfaces;

namespace ASP.NET_Core_Web_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly AuthService _authService;

        public UsersController(IUserService userService, AuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        
        /// Register a new user
        
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

        
        /// Login user
        
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

        
        /// Get all users (Admin only)
        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllUsers()
        {
            var userRole = _authService.GetUserRoleFromClaims(User);
            if (userRole != "Admin")
                return Forbid();

            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        
        /// Get user by ID
       
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(user);
        }

        
        /// Get current user profile
        
        [HttpGet("profile/me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUserProfile()
        {
            var userId = _authService.GetUserIdFromClaims(User);
            var user = await _userService.GetCurrentUserProfileAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(user);
        }

        
        /// Update user (only own profile or admin can update others)
        
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User updateDto)
        {
            var currentUserId = _authService.GetUserIdFromClaims(User);
            var userRole = _authService.GetUserRoleFromClaims(User);

            // Only allow users to update their own profile or admins to update anyone
            if (currentUserId != id && userRole != "Admin")
                return Forbid();

            var user = await _userService.UpdateUserAsync(id, updateDto, currentUserId, userRole);
            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(new { message = "User updated successfully", user });
        }

        
        /// Approve/Reject user (Admin only)
        
        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveUser(int id, [FromBody] ApproveUserDto approveDto)
        {
            var user = await _userService.ApproveUserAsync(id, approveDto.IsApproved);
            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(new { message = $"User {(approveDto.IsApproved ? "approved" : "rejected")}", user });
        }

        
        /// Delete user (Admin only or user can delete own account)
        
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var currentUserId = _authService.GetUserIdFromClaims(User);
            var userRole = _authService.GetUserRoleFromClaims(User);

            if (currentUserId != id && userRole != "Admin")
                return Forbid();

            var success = await _userService.DeleteUserAsync(id);
            if (!success)
                return NotFound(new { message = "User not found" });

            return Ok(new { message = "User deleted successfully" });
        }
    }

    public class ApproveUserDto
    {
        public bool IsApproved { get; set; }
    }
}
