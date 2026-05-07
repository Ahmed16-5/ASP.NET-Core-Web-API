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
    public class MaterialsController : ControllerBase
    {
        private readonly IMaterialService _materialService;
        private readonly AuthService _authService;

        public MaterialsController(IMaterialService materialService, AuthService authService)
        {
            _materialService = materialService;
            _authService = authService;
        }


        /// Get all materials for a study group
        [HttpGet("group/{studyGroupId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMaterialsByGroup(int studyGroupId)
        {
            var materials = await _materialService.GetMaterialsByGroupAsync(studyGroupId);
            return Ok(materials);
        }


        /// Get material by ID

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMaterialById(int id)
        {
            var material = await _materialService.GetMaterialByIdAsync(id);
            if (material == null)
                return NotFound(new { message = "Material not found" });

            return Ok(material);
        }


        /// Create new material (only group members)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateMaterial([FromBody] CreateMaterialDto createDto, [FromQuery] int studyGroupId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = _authService.GetUserIdFromClaims(User);
            var material = await _materialService.CreateMaterialAsync(createDto, studyGroupId, userId);

            if (material == null)
                return BadRequest(new { message = "Failed to create material" });

            return CreatedAtAction(nameof(GetMaterialById), new { id = material.ID }, material);
        }


        /// Update material (only creator or group owner or admin)

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateMaterial(int id, [FromBody] CreateMaterialDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = _authService.GetUserIdFromClaims(User);
            var userRole = _authService.GetUserRoleFromClaims(User);

            var material = await _materialService.UpdateMaterialAsync(id, updateDto, currentUserId, userRole);
            if (material == null)
                return NotFound(new { message = "Material not found" });

            return Ok(new { message = "Material updated successfully", material });
        }


        /// Delete material (only creator, group owner, or admin)

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteMaterial(int id)
        {
            var currentUserId = _authService.GetUserIdFromClaims(User);
            var userRole = _authService.GetUserRoleFromClaims(User);

            var success = await _materialService.DeleteMaterialAsync(id, currentUserId, userRole);
            if (!success)
                return NotFound(new { message = "Material not found" });

            return Ok(new { message = "Material deleted successfully" });
        }


        /// Get materials uploaded by current user
        
        [HttpGet("user/my-materials")]
        [Authorize]
        public async Task<IActionResult> GetMyMaterials()
        {
            var userId = _authService.GetUserIdFromClaims(User);
            var materials = await _materialService.GetUserMaterialsAsync(userId);
            return Ok(materials);
        }

       
        /// Search materials by filename
        
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchMaterials([FromQuery] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest(new { message = "Search term cannot be empty" });

            var materials = await _materialService.SearchMaterialsAsync(searchTerm);
            return Ok(materials);
        }
    }
}
