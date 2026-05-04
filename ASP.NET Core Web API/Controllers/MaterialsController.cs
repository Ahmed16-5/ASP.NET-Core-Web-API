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
    public class MaterialsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;

        public MaterialsController(AppDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        /// <summary>
        /// Get all materials for a study group
        /// </summary>
        [HttpGet("group/{studyGroupId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMaterialsByGroup(int studyGroupId)
        {
            var group = await _context.StudyGroups.FindAsync(studyGroupId);
            if (group == null)
                return NotFound(new { message = "Study group not found" });

            var materials = await _context.Materials
                .Where(m => m.StudyGroupID == studyGroupId)
                .Include(m => m.User)
                .ToListAsync();

            return Ok(materials);
        }

        /// <summary>
        /// Get material by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMaterialById(int id)
        {
            var material = await _context.Materials
                .Include(m => m.User)
                .Include(m => m.StudyGroup)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (material == null)
                return NotFound(new { message = "Material not found" });

            return Ok(material);
        }

        /// <summary>
        /// Create new material (only group members)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateMaterial([FromBody] CreateMaterialDto createDto, [FromQuery] int studyGroupId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = _authService.GetUserIdFromClaims(User);
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            var studyGroup = await _context.StudyGroups.FindAsync(studyGroupId);
            if (studyGroup == null)
                return NotFound(new { message = "Study group not found" });

            // Check if user is a member of the group
            var isMember = await _context.GroupMembers
                .AnyAsync(gm => gm.UserID == userId && gm.StudyGroupID == studyGroupId);

            if (!isMember && studyGroup.UserID != userId)
                return Forbid();

            var material = new Material
            {
                FileName = createDto.FileName,
                FileUrl = createDto.FileUrl,
                UserID = userId,
                StudyGroupID = studyGroupId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Materials.Add(material);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMaterialById), new { id = material.ID }, material);
        }

        /// <summary>
        /// Update material (only creator or group owner or admin)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateMaterial(int id, [FromBody] CreateMaterialDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = _authService.GetUserIdFromClaims(User);
            var userRole = _authService.GetUserRoleFromClaims(User);

            var material = await _context.Materials
                .Include(m => m.StudyGroup)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (material == null)
                return NotFound(new { message = "Material not found" });

            // Only creator, group owner, or admin can update
            bool canUpdate = material.UserID == currentUserId || 
                           material.StudyGroup.UserID == currentUserId || 
                           userRole == "Admin";

            if (!canUpdate)
                return Forbid();

            material.FileName = updateDto.FileName ?? material.FileName;
            material.FileUrl = updateDto.FileUrl ?? material.FileUrl;

            _context.Materials.Update(material);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Material updated successfully", material });
        }

        /// <summary>
        /// Delete material (only creator, group owner, or admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteMaterial(int id)
        {
            var currentUserId = _authService.GetUserIdFromClaims(User);
            var userRole = _authService.GetUserRoleFromClaims(User);

            var material = await _context.Materials
                .Include(m => m.StudyGroup)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (material == null)
                return NotFound(new { message = "Material not found" });

            // Only creator, group owner, or admin can delete
            bool canDelete = material.UserID == currentUserId || 
                           material.StudyGroup.UserID == currentUserId || 
                           userRole == "Admin";

            if (!canDelete)
                return Forbid();

            _context.Materials.Remove(material);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Material deleted successfully" });
        }

        /// <summary>
        /// Get materials uploaded by current user
        /// </summary>
        [HttpGet("user/my-materials")]
        [Authorize]
        public async Task<IActionResult> GetMyMaterials()
        {
            var userId = _authService.GetUserIdFromClaims(User);

            var materials = await _context.Materials
                .Where(m => m.UserID == userId)
                .Include(m => m.StudyGroup)
                .ToListAsync();

            return Ok(materials);
        }

        /// <summary>
        /// Search materials by filename
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchMaterials([FromQuery] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest(new { message = "Search term cannot be empty" });

            var materials = await _context.Materials
                .Where(m => m.FileName.Contains(searchTerm))
                .Include(m => m.User)
                .Include(m => m.StudyGroup)
                .ToListAsync();

            return Ok(materials);
        }
    }
}
