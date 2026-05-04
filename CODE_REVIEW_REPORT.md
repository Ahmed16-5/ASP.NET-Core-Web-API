# 📋 PROJECT CODE REVIEW - CHECKLIST

## 1. USER APPROVAL ✅

### Requirement: Admin can approve/reject users

**Status:** ✅ **IMPLEMENTED CORRECTLY**

- ✅ PUT `/api/users/{id}/approve` - **Implemented**
- ✅ Only Admin can access - **Verified with `[Authorize]` + role check**
- ✅ `ApproveUserDto` with `IsApproved` flag - **Implemented**
- ✅ Proper error handling - **404 for user not found, 403 Forbid for non-admin**

**Code Reference:**
```csharp
[HttpPut("{id}/approve")]
[Authorize]
public async Task<IActionResult> ApproveUser(int id, [FromBody] ApproveUserDto approveDto)
{
	var userRole = _authService.GetUserRoleFromClaims(User);
	if (userRole != "Admin")
		return Forbid(); // 403
	// ... updates IsApproved
}
```

**HTTP Status Codes:**
- ✅ 200 OK - User approved/rejected
- ✅ 403 Forbidden - Non-admin access
- ✅ 404 Not Found - User not found

---

## 2. STUDY GROUP APPROVAL ✅

### Requirement: Admin can approve/reject study groups

**Status:** ✅ **IMPLEMENTED CORRECTLY**

- ✅ StudyGroup model has `IsApproved` property - **Verified**
- ✅ PUT `/api/studygroups/{id}/approve` - **Implemented**
- ✅ Only Admin can access - **Verified with role check**
- ✅ New groups default to `IsApproved = false` - **Verified in CreateStudyGroup**

**Code Reference:**
```csharp
[HttpPut("{id}/approve")]
[Authorize]
public async Task<IActionResult> ApproveStudyGroup(int id, [FromBody] ApproveStudyGroupDto approveDto)
{
	var userRole = _authService.GetUserRoleFromClaims(User);
	if (userRole != "Admin")
		return Forbid();
	group.IsApproved = approveDto.IsApproved;
	// ... saves changes
}
```

---

## 3. PUBLIC BROWSING ✅

### Requirement: GET StudyGroups endpoints allow anonymous access

**Status:** ✅ **IMPLEMENTED CORRECTLY**

- ✅ `GET /api/studygroups` - **`[AllowAnonymous]` applied**
- ✅ `GET /api/studygroups/{id}` - **`[AllowAnonymous]` applied**
- ✅ Only approved groups returned - **`.Where(sg => sg.IsApproved)`**

**Code Reference:**
```csharp
[HttpGet]
[AllowAnonymous]
public async Task<IActionResult> GetAllStudyGroups()
{
	var groups = await _context.StudyGroups
		.Where(sg => sg.IsApproved)  // ✅ Only approved groups
		.Include(sg => sg.User)
		.Include(sg => sg.GroupMembers)
		.ToListAsync();
}
```

**Endpoints Available Anonymously:**
- ✅ GET `/api/studygroups` - Browse approved groups
- ✅ GET `/api/studygroups/{id}` - View group details
- ✅ GET `/api/studygroups/{id}/members` - View members
- ✅ GET `/api/materials/group/{groupId}` - View materials
- ✅ GET `/api/comments/group/{groupId}` - View comments

---

## 4. SEARCH & FILTERING ❌

### Requirement: Filter by Subject, Location, Meeting Time

**Status:** ❌ **MISSING - CRITICAL**

**Problem:** 
- ❌ No query parameters for subject, location, meeting time
- ❌ No filtering endpoint
- ❌ Only materials have basic search (by filename)

**Missing Endpoint:**
```
GET /api/studygroups?subject=Math&location=Cairo&meetingTime=2024-01-15
```

**Required Implementation:**

Add this search endpoint to **StudyGroupsController.cs**:

```csharp
/// <summary>
/// Search study groups with filters
/// </summary>
[HttpGet("search")]
[AllowAnonymous]
public async Task<IActionResult> SearchStudyGroups(
	[FromQuery] string? subject,
	[FromQuery] string? location,
	[FromQuery] DateTime? meetingTime)
{
	var query = _context.StudyGroups
		.Where(sg => sg.IsApproved);

	if (!string.IsNullOrWhiteSpace(subject))
		query = query.Where(sg => sg.Subject.Contains(subject));

	if (!string.IsNullOrWhiteSpace(location))
		query = query.Where(sg => sg.Location.Contains(location));

	if (meetingTime.HasValue)
	{
		var dateOnly = meetingTime.Value.Date;
		query = query.Where(sg => sg.MeetingTime.Date == dateOnly);
	}

	var results = await query
		.Include(sg => sg.User)
		.Include(sg => sg.GroupMembers)
		.ToListAsync();

	return Ok(results);
}
```

**Usage Examples:**
```
GET /api/studygroups/search?subject=Math
GET /api/studygroups/search?location=Cairo
GET /api/studygroups/search?subject=Physics&location=Alexandria
GET /api/studygroups/search?meetingTime=2024-01-20
```

---

## 5. BUSINESS LOGIC VALIDATION ✅

### 5.1 Prevent joining a full group ✅

**Status:** ✅ **IMPLEMENTED CORRECTLY**

- ✅ Member limit checked in `ApproveJoinRequest`
- ✅ Returns BadRequest if group is full

**Code Reference:**
```csharp
var memberCount = await _context.GroupMembers
	.CountAsync(gm => gm.StudyGroupID == joinRequest.StudyGroupID);

if (memberCount >= joinRequest.StudyGroup.MaxMembers)
	return BadRequest(new { message = "Group is full, cannot add more members" });
```

**Status Code:** ✅ 400 Bad Request

---

### 5.2 Prevent duplicate join requests ✅

**Status:** ✅ **IMPLEMENTED CORRECTLY**

- ✅ Checks existing pending requests
- ✅ Checks if already a member

**Code Reference:**
```csharp
// Check if already a member
var isMember = await _context.GroupMembers
	.AnyAsync(gm => gm.UserID == userId && gm.StudyGroupID == sendDto.StudyGroupID);
if (isMember)
	return BadRequest(new { message = "You are already a member of this group" });

// Check if already requested
var existingRequest = await _context.JoinRequests
	.FirstOrDefaultAsync(jr => jr.UserID == userId && jr.StudyGroupID == sendDto.StudyGroupID);
if (existingRequest != null && existingRequest.Status == "Pending")
	return BadRequest(new { message = "You have already sent a request to this group" });
```

**Status Code:** ✅ 400 Bad Request

---

### 5.3 Only approved users can create study groups ✅

**Status:** ✅ **IMPLEMENTED CORRECTLY**

- ✅ Checks `user.IsApproved` before creating group
- ✅ Returns Forbid (403) if not approved

**Code Reference:**
```csharp
if (!user.IsApproved)
	return Forbid();

var studyGroup = new StudyGroup
{
	// ... properties
	IsApproved = false // Needs admin approval
};
```

**Status Code:** ✅ 403 Forbidden

---

### 5.4 Only group members can add materials ✅

**Status:** ✅ **IMPLEMENTED CORRECTLY**

- ✅ Checks membership with `GroupMembers` table
- ✅ Allows group owner or members

**Code Reference:**
```csharp
var isMember = await _context.GroupMembers
	.AnyAsync(gm => gm.UserID == userId && gm.StudyGroupID == studyGroupId);

if (!isMember && studyGroup.UserID != userId)
	return Forbid();
```

**Status Code:** ✅ 403 Forbidden

---

### 5.5 Only owner/admin can delete material ✅

**Status:** ✅ **IMPLEMENTED CORRECTLY**

- ✅ Checks material creator, group owner, or admin

**Code Reference:**
```csharp
bool canDelete = material.UserID == currentUserId || 
			   material.StudyGroup.UserID == currentUserId || 
			   userRole == "Admin";

if (!canDelete)
	return Forbid();
```

**Status Code:** ✅ 403 Forbidden

---

### 5.6 Only comment owner can delete comment ✅

**Status:** ✅ **IMPLEMENTED CORRECTLY**

- ✅ Checks comment owner, group owner, or admin

**Code Reference:**
```csharp
bool canDelete = comment.UserID == currentUserId || 
			   comment.StudyGroup.UserID == currentUserId || 
			   userRole == "Admin";

if (!canDelete)
	return Forbid();
```

**Status Code:** ✅ 403 Forbidden

---

## 6. SECURITY ✅

### 6.1 Proper use of [Authorize] ✅

**Status:** ✅ **IMPLEMENTED CORRECTLY**

- ✅ Controllers/endpoints properly protected
- ✅ `[Authorize]` on protected endpoints
- ✅ `[AllowAnonymous]` on public endpoints

**Code Verification:**
```csharp
[Authorize]                    // Protected
public async Task<IActionResult> CreateStudyGroup(...)

[AllowAnonymous]               // Public
public async Task<IActionResult> GetAllStudyGroups()
```

---

### 6.2 Proper use of [Authorize(Roles = "Admin")] ⚠️

**Status:** ⚠️ **NEEDS IMPROVEMENT**

**Issue:** Role-based authorization uses manual string checking instead of attribute:

**Current Implementation:**
```csharp
[Authorize]
public async Task<IActionResult> ApproveUser(int id, ...)
{
	var userRole = _authService.GetUserRoleFromClaims(User);
	if (userRole != "Admin")
		return Forbid();
}
```

**Better Implementation:**
```csharp
[Authorize(Roles = "Admin")]  // ✅ Cleaner, more declarative
public async Task<IActionResult> ApproveUser(int id, ...)
{
	// No need for manual role check
}
```

**Recommended Fix:** Add this attribute to admin-only endpoints in all controllers:
- `PUT /api/users/{id}/approve` 
- `PUT /api/studygroups/{id}/approve`

**Code to Update:**

**UsersController.cs - Line 131:**
```csharp
// BEFORE:
[HttpPut("{id}/approve")]
[Authorize]
public async Task<IActionResult> ApproveUser(int id, [FromBody] ApproveUserDto approveDto)
{
	var userRole = _authService.GetUserRoleFromClaims(User);
	if (userRole != "Admin")
		return Forbid();

// AFTER:
[HttpPut("{id}/approve")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> ApproveUser(int id, [FromBody] ApproveUserDto approveDto)
{
	// Remove the manual role check
```

**StudyGroupsController.cs - Line 130:**
```csharp
// BEFORE:
[HttpPut("{id}/approve")]
[Authorize]
public async Task<IActionResult> ApproveStudyGroup(int id, [FromBody] ApproveStudyGroupDto approveDto)
{
	var userRole = _authService.GetUserRoleFromClaims(User);
	if (userRole != "Admin")
		return Forbid();

// AFTER:
[HttpPut("{id}/approve")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> ApproveStudyGroup(int id, [FromBody] ApproveStudyGroupDto approveDto)
{
	// Remove the manual role check
```

---

### 6.3 JWT claims extraction ✅

**Status:** ✅ **IMPLEMENTED CORRECTLY**

- ✅ `GetUserIdFromClaims()` - Extracts UserId
- ✅ `GetUserRoleFromClaims()` - Extracts Role
- ✅ Claims set during token generation

**Code Reference:**
```csharp
var claims = new[]
{
	new System.Security.Claims.Claim("UserId", user.ID.ToString()),
	new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email ?? ""),
	new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, user.Role ?? "User"),
	new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.Name ?? "")
};
```

---

## 7. DATA EXPOSURE ✅

### Requirement: Unapproved groups NOT returned in public APIs

**Status:** ✅ **IMPLEMENTED CORRECTLY**

- ✅ `.Where(sg => sg.IsApproved)` in public endpoints
- ✅ Verified in:
  - `GET /api/studygroups` 
  - `GET /api/studygroups/{id}` - ✅ Actually, this needs review...

**⚠️ ISSUE FOUND:** GetStudyGroupById doesn't filter by IsApproved!

**Current Code:**
```csharp
[HttpGet("{id}")]
[AllowAnonymous]
public async Task<IActionResult> GetStudyGroupById(int id)
{
	var group = await _context.StudyGroups
		.FirstOrDefaultAsync(sg => sg.ID == id);  // ❌ No IsApproved check!

	return Ok(group);
}
```

**Recommendation:** Add approval check for unapproved group access:

**Fix for StudyGroupsController.cs - Line 47:**
```csharp
[HttpGet("{id}")]
[AllowAnonymous]
public async Task<IActionResult> GetStudyGroupById(int id)
{
	var group = await _context.StudyGroups
		.Include(sg => sg.User)
		.Include(sg => sg.GroupMembers)
		.Include(sg => sg.Materials)
		.Include(sg => sg.Comments)
		.FirstOrDefaultAsync(sg => sg.ID == id);

	if (group == null)
		return NotFound(new { message = "Study group not found" });

	// Allow viewing unapproved groups only if owner or admin
	if (!group.IsApproved)
	{
		var user = User.Identity?.IsAuthenticated == true 
			? _authService.GetUserIdFromClaims(User) 
			: 0;
		var userRole = User.Identity?.IsAuthenticated == true 
			? _authService.GetUserRoleFromClaims(User) 
			: "User";

		if (group.UserID != user && userRole != "Admin")
			return NotFound(new { message = "Study group not found or not approved yet" });
	}

	return Ok(group);
}
```

Or simpler alternative:
```csharp
// Only return if approved OR current user is owner/admin
if (!group.IsApproved && group.UserID != currentUserId && userRole != "Admin")
	return NotFound(new { message = "Study group not found" });
```

---

## 8. ERROR HANDLING ✅

### Requirement: Proper HTTP status codes (400, 401, 403, 404)

**Status:** ✅ **IMPLEMENTED CORRECTLY**

| Status Code | Used For | Example |
|---|---|---|
| **400** | Bad Request | Duplicate requests, validation errors, password mismatch |
| **401** | Unauthorized | Invalid login, not approved user |
| **403** | Forbidden | Not authorized (non-admin accessing admin endpoint) |
| **404** | Not Found | User/group/material not found |
| **200** | Success | Normal operations |
| **201** | Created | Resource created successfully |

**Verification Examples:**

✅ **400 Bad Request:**
```csharp
return BadRequest(new { message = "You have already sent a request to this group" });
return BadRequest(new { message = "Passwords do not match" });
return BadRequest(new { message = "Group is full, cannot add more members" });
```

✅ **401 Unauthorized:**
```csharp
return Unauthorized(new { message = "Invalid email or password" });
return Unauthorized(new { message = "Your account is not approved by admin yet" });
```

✅ **403 Forbidden:**
```csharp
return Forbid(); // Non-admin accessing admin endpoint
```

✅ **404 Not Found:**
```csharp
return NotFound(new { message = "User not found" });
return NotFound(new { message = "Study group not found" });
```

---

## SUMMARY SCORECARD

| Requirement | Status | Comments |
|---|---|---|
| 1. User Approval | ✅ | Fully implemented with proper authorization |
| 2. Study Group Approval | ✅ | Fully implemented, IsApproved property used |
| 3. Public Browsing | ✅ | Anonymous access works, only approved groups visible |
| 4. Search & Filtering | ❌ | **MISSING - Needs implementation** |
| 5.1 Full Group Prevention | ✅ | Member limit enforced |
| 5.2 Duplicate Requests | ✅ | Prevented with checks |
| 5.3 Approved Users Only | ✅ | Creating groups restricted to approved users |
| 5.4 Members Add Materials | ✅ | Membership verified |
| 5.5 Owner Delete Materials | ✅ | Proper permission checks |
| 5.6 Owner Delete Comments | ✅ | Proper permission checks |
| 6.1 [Authorize] Usage | ✅ | Correctly applied |
| 6.2 [Authorize(Roles)] | ⚠️ | Could be improved - use attribute instead of manual checks |
| 6.3 JWT Claims | ✅ | Properly extracted and used |
| 7. Data Exposure | ⚠️ | GetStudyGroupById needs approval filter |
| 8. Error Handling | ✅ | All proper status codes used |

---

## CRITICAL FIXES NEEDED

### 🔴 **Priority 1 - CRITICAL:**

1. **Add Search/Filter Endpoint**
   - Location: `StudyGroupsController.cs`
   - Add `GET /api/studygroups/search?subject=Math&location=Cairo`
   - See Section 4 for implementation

### 🟡 **Priority 2 - IMPORTANT:**

2. **Fix GetStudyGroupById Data Exposure**
   - Location: `StudyGroupsController.cs` Line 47
   - Add approval check to prevent unauthorized viewing of unapproved groups
   - See Section 7 for implementation

3. **Improve Security with Role Attributes**
   - Location: Multiple controllers
   - Use `[Authorize(Roles = "Admin")]` instead of manual role checks
   - See Section 6.2 for implementation

---

## NEXT STEPS

1. ✅ Run migrations: `dotnet ef migrations add InitialCreate`
2. ✅ Update database: `dotnet ef database update`
3. ✅ Apply critical fixes from Priority 1
4. ✅ Apply important fixes from Priority 2
5. ✅ Test endpoints with Postman or Swagger
6. ✅ Verify all business logic with test cases

