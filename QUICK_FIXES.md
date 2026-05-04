# QUICK FIXES - CODE SNIPPETS

## FIX #1: Add Search/Filter Endpoint (CRITICAL)

**File:** `ASP.NET Core Web API\Controllers\StudyGroupsController.cs`

**Location:** Add this method after `GetMyStudyGroups()` method (before `DeleteStudyGroup()`)

```csharp
/// <summary>
/// Search study groups by subject, location, or meeting time
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
		query = query.Where(sg => sg.Subject != null && sg.Subject.Contains(subject));

	if (!string.IsNullOrWhiteSpace(location))
		query = query.Where(sg => sg.Location != null && sg.Location.Contains(location));

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
GET /api/studygroups/search?subject=English&location=Giza&meetingTime=2024-01-15
```

---

## FIX #2: Secure GetStudyGroupById (IMPORTANT)

**File:** `ASP.NET Core Web API\Controllers\StudyGroupsController.cs`

**Location:** Replace the `GetStudyGroupById` method (lines 47-60)

**BEFORE:**
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

	return Ok(group);
}
```

**AFTER:**
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

	// Only allow viewing unapproved groups if user is owner or admin
	if (!group.IsApproved)
	{
		var isAuthenticated = User.Identity?.IsAuthenticated == true;
		if (!isAuthenticated)
			return NotFound(new { message = "Study group not found or not approved yet" });

		var currentUserId = _authService.GetUserIdFromClaims(User);
		var userRole = _authService.GetUserRoleFromClaims(User);

		if (group.UserID != currentUserId && userRole != "Admin")
			return NotFound(new { message = "Study group not found or not approved yet" });
	}

	return Ok(group);
}
```

---

## FIX #3: Use [Authorize(Roles = "Admin")] (OPTIONAL - Best Practice)

**File:** `ASP.NET Core Web API\Controllers\UsersController.cs`

**Location:** Line 131 - Update `ApproveUser` method signature

**BEFORE:**
```csharp
[HttpPut("{id}/approve")]
[Authorize]
public async Task<IActionResult> ApproveUser(int id, [FromBody] ApproveUserDto approveDto)
{
	var userRole = _authService.GetUserRoleFromClaims(User);
	if (userRole != "Admin")
		return Forbid();
	// ... rest of code
}
```

**AFTER:**
```csharp
[HttpPut("{id}/approve")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> ApproveUser(int id, [FromBody] ApproveUserDto approveDto)
{
	// Manual role check not needed anymore
	var user = await _context.Users.FindAsync(id);
	if (user == null)
		return NotFound(new { message = "User not found" });

	user.IsApproved = approveDto.IsApproved;
	_context.Users.Update(user);
	await _context.SaveChangesAsync();

	return Ok(new { message = $"User {(approveDto.IsApproved ? "approved" : "rejected")}", user });
}
```

---

## FIX #4: Use [Authorize(Roles = "Admin")] (OPTIONAL - Best Practice)

**File:** `ASP.NET Core Web API\Controllers\StudyGroupsController.cs`

**Location:** Line 130 - Update `ApproveStudyGroup` method signature

**BEFORE:**
```csharp
[HttpPut("{id}/approve")]
[Authorize]
public async Task<IActionResult> ApproveStudyGroup(int id, [FromBody] ApproveStudyGroupDto approveDto)
{
	var userRole = _authService.GetUserRoleFromClaims(User);
	if (userRole != "Admin")
		return Forbid();
	// ... rest of code
}
```

**AFTER:**
```csharp
[HttpPut("{id}/approve")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> ApproveStudyGroup(int id, [FromBody] ApproveStudyGroupDto approveDto)
{
	// Manual role check not needed anymore
	var group = await _context.StudyGroups.FindAsync(id);
	if (group == null)
		return NotFound(new { message = "Study group not found" });

	group.IsApproved = approveDto.IsApproved;
	_context.StudyGroups.Update(group);
	await _context.SaveChangesAsync();

	return Ok(new { message = $"Study group {(approveDto.IsApproved ? "approved" : "rejected")}", group });
}
```

---

## IMPLEMENTATION ORDER

1. **Apply FIX #1 FIRST** (Add Search Endpoint) - This is CRITICAL
2. **Apply FIX #2 SECOND** (Secure GetStudyGroupById) - This is IMPORTANT  
3. **Apply FIX #3 & #4 OPTIONAL** (Use Roles attribute) - Best practice, optional

## TESTING CHECKLIST

After applying fixes, test these endpoints:

### Test Search Endpoint
```
GET /api/studygroups/search?subject=Math HTTP/1.1
GET /api/studygroups/search?location=Cairo HTTP/1.1
GET /api/studygroups/search?meetingTime=2024-01-20 HTTP/1.1
```

### Test Unapproved Group Security
```
GET /api/studygroups/999 (unapproved group)
Expected: 404 Not Found (if anonymous)
Expected: Group data (if user is owner)
```

### Test Admin Approval
```
PUT /api/users/1/approve HTTP/1.1
PUT /api/studygroups/1/approve HTTP/1.1
Expected: 200 OK (if Admin)
Expected: 403 Forbidden (if non-Admin)
```

