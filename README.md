# Study Group API - ASP.NET Core Web API

![.NET 8](https://img.shields.io/badge/.NET-8.0-blue) ![SQL Server](https://img.shields.io/badge/Database-SQL%20Server-red) ![License](https://img.shields.io/badge/License-MIT-green)

## 📌 Project Overview

**Study Group API** is a backend REST API service designed to facilitate the creation and management of study groups. It provides features for user authentication, authorization, and study group management with an admin approval system.

### Main Features

✅ **User Authentication** - Secure registration and login with JWT tokens  
✅ **Role-Based Authorization** - Admin and User roles with different permissions  
✅ **Admin Approval System** - New users and study groups require admin approval  
✅ **Study Group Management** - Create, update, delete, and search study groups  
✅ **Study Group Members** - Join requests and member management  
✅ **Materials & Comments** - Attach study materials and leave comments in study groups  
✅ **Secure API** - Protected endpoints using JWT Bearer tokens  

---

## 🛠️ Technologies Used

| Technology | Version | Purpose |
|-----------|---------|---------|
| .NET | 8.0 | Framework |
| ASP.NET Core | 8.0 | Web API Framework |
| Entity Framework Core | Latest | ORM (Object-Relational Mapping) |
| SQL Server | 2019+ | Database |
| JWT (JSON Web Tokens) | System.IdentityModel.Tokens.Jwt | Authentication |
| Swagger/Swashbuckle | Latest | API Documentation |

---

## ⚙️ Installation & Setup

### Prerequisites

Before you start, ensure you have installed:

- **Visual Studio 2022+** or **Visual Studio Code**
- **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server 2019+** - [Download here](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- **Git** - [Download here](https://git-scm.com/)

### Step-by-Step Setup

#### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/study-group-api.git
cd study-group-api
```

#### 2. Open the Project

**Using Visual Studio:**
- Open Visual Studio
- Click `File` → `Open` → `Project/Solution`
- Navigate to the project folder and select `ASP.NET Core Web API.sln`

**Using Command Line:**
```bash
dotnet sln open
```

#### 3. Configure Database Connection

Edit `appsettings.json` in the project root:

```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=YOUR_SERVER_NAME;Database=StudyGroupDB;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "Jwt": {
	"SecretKey": "your-secret-key-min-32-chars-long-change-this",
	"Issuer": "StudyGroupAPI",
	"Audience": "StudyGroupAPIUsers",
	"ExpiryMinutes": 60
  }
}
```

**Important Notes:**
- Replace `YOUR_SERVER_NAME` with your SQL Server instance name
- Change the `SecretKey` to a secure random string (minimum 32 characters)
- Keep these values in a secure location, never commit to GitHub

#### 4. Install Dependencies

The NuGet packages will be restored automatically when you open the project in Visual Studio. To manually restore:

```bash
dotnet restore
```

#### 5. Run Database Migrations

**Using Package Manager Console in Visual Studio:**
```powershell
Update-Database
```

**Using Command Line:**
```bash
dotnet ef database update
```

This will create all tables in SQL Server based on the Entity Framework Core migrations.

#### 6. Run the Project

**Using Visual Studio:**
- Press `F5` or click the `Play` button
- The API will open at `https://localhost:7259`
- Swagger UI will be available at `https://localhost:7259/swagger`

**Using Command Line:**
```bash
dotnet run
```

---

## 🔐 Authentication Guide

### User Registration Flow

1. **New users register** with email and password
2. **Admin approval required** - User account is created but not approved
3. **Admin approves user** - User receives approval notification
4. **User can now login** and obtain JWT token

### How to Register

**Request:**
```bash
POST /api/Users/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "name": "John Doe"
}
```

**Response (201 Created):**
```json
{
  "message": "User registered successfully. Awaiting admin approval.",
  "user": {
	"id": 1,
	"name": "John Doe",
	"email": "user@example.com",
	"isApproved": false
  }
}
```

### Admin Approval System

- New users cannot perform authenticated actions until approved
- Admins can approve/reject users via `/api/Users/{id}/approve` endpoint
- Once approved, users can login and receive JWT tokens

### How to Login

**Request:**
```bash
POST /api/Users/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Response (200 OK):**
```json
{
  "message": "Login successful",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
	"id": 1,
	"name": "John Doe",
	"email": "user@example.com",
	"role": "User"
  }
}
```

### Using JWT Token in Swagger

1. Click the **"Authorize"** button in Swagger UI (top-right)
2. Paste your token in the format: `Bearer YOUR_TOKEN_HERE`
3. Click **"Authorize"** and then **"Close"**
4. All subsequent requests will include the token automatically

### Using JWT Token in Postman

1. Create a new request in Postman
2. Go to the **"Authorization"** tab
3. Select **"Bearer Token"** from the Type dropdown
4. Paste your token in the **"Token"** field
5. Send the request

### Using JWT Token with Fetch API (Frontend)

```javascript
const token = "your-jwt-token-here";

fetch('https://localhost:7259/api/StudyGroups', {
  method: 'GET',
  headers: {
	'Authorization': `Bearer ${token}`,
	'Content-Type': 'application/json'
  }
})
.then(response => response.json())
.then(data => console.log(data));
```

---

## 👨‍💻 API Endpoints Overview

### Authentication Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---|
| POST | `/api/Users/register` | Register new user | ❌ No |
| POST | `/api/Users/login` | Login and get JWT token | ❌ No |

### User Management Endpoints

| Method | Endpoint | Description | Auth Required | Role |
|--------|----------|-------------|---|---|
| GET | `/api/Users` | Get all users | ✅ Yes | Admin |
| GET | `/api/Users/{id}` | Get user by ID | ✅ Yes | - |
| PUT | `/api/Users/{id}/approve` | Approve/reject user | ✅ Yes | Admin |

### Study Groups Endpoints

| Method | Endpoint | Description | Auth Required | Role |
|--------|----------|-------------|---|---|
| GET | `/api/StudyGroups` | Get all approved study groups | ❌ No | - |
| GET | `/api/StudyGroups/{id}` | Get study group by ID | ❌ No | - |
| POST | `/api/StudyGroups` | Create new study group | ✅ Yes | User |
| PUT | `/api/StudyGroups/{id}` | Update study group | ✅ Yes | Owner/Admin |
| PUT | `/api/StudyGroups/{id}/approve` | Approve study group | ✅ Yes | Admin |
| DELETE | `/api/StudyGroups/{id}` | Delete study group | ✅ Yes | Owner/Admin |
| GET | `/api/StudyGroups/owner/my-groups` | Get current user's groups | ✅ Yes | User |
| GET | `/api/StudyGroups/search` | Search study groups | ❌ No | - |
| GET | `/api/StudyGroups/{id}/members` | Get group members | ❌ No | - |

### Study Group Members Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---|
| POST | `/api/JoinRequests` | Request to join group | ✅ Yes |
| GET | `/api/JoinRequests` | Get join requests | ✅ Yes |
| PUT | `/api/JoinRequests/{id}/approve` | Approve join request | ✅ Yes |

### Materials Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---|
| POST | `/api/Materials` | Add material to study group | ✅ Yes |
| GET | `/api/Materials/{groupId}` | Get materials for group | ❌ No |
| DELETE | `/api/Materials/{id}` | Delete material | ✅ Yes |

### Comments Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---|
| POST | `/api/Comments` | Add comment to group | ✅ Yes |
| GET | `/api/Comments/{groupId}` | Get comments for group | ❌ No |
| DELETE | `/api/Comments/{id}` | Delete comment | ✅ Yes |

---

## 🧩 Project Structure

```
ASP.NET Core Web API/
├── Controllers/                    # API endpoint handlers
│   ├── UsersController.cs
│   ├── StudyGroupsController.cs
│   ├── JoinRequestsController.cs
│   ├── MaterialsController.cs
│   └── CommentsController.cs
│
├── Models/                         # Entity Framework Core Models
│   ├── User.cs                     # User entity with navigation properties
│   ├── StudyGroup.cs               # Study group entity
│   ├── GroupMember.cs              # Group membership tracking
│   ├── JoinRequest.cs              # Join request entity
│   ├── Material.cs                 # Study materials
│   └── Comment.cs                  # Comments on groups
│
├── DTOs/                           # Data Transfer Objects (for API requests/responses)
│   ├── CreateStudyGroupDto.cs
│   ├── UpdateStudyGroupDto.cs
│   ├── LoginDto.cs
│   ├── RegisterDto.cs
│   ├── AuthResponseDto.cs
│   └── JoinRequestResponseDto.cs
│
├── Data/                           # Database configuration
│   └── AppDbContext.cs             # Entity Framework DbContext
│
├── Migrations/                     # Database migrations
│   └── [Migration files]
│
├── Services/                       # Business logic services
│   └── AuthService.cs              # JWT and authentication logic
│
├── Program.cs                      # Application startup configuration
├── appsettings.json               # Configuration file
├── appsettings.Development.json   # Development settings
└── README.md                       # This file

```

### Folder Descriptions

- **Controllers** - Handle HTTP requests and responses. Each controller manages a specific domain (Users, StudyGroups, etc.)
- **Models** - Entity Framework Core entity classes that represent database tables
- **DTOs** - Data Transfer Objects used for API requests and responses. These protect internal models from external exposure
- **Data** - Contains AppDbContext which configures the database and entity relationships
- **Services** - Contains business logic, authentication logic, and utility functions
- **Migrations** - Contains database migration files for version control of database schema changes

---

## 🧪 Example Requests

### 1. Register a New User

**Request:**
```bash
curl -X POST "https://localhost:7259/api/Users/register" \
  -H "Content-Type: application/json" \
  -d '{
	"email": "newuser@example.com",
	"password": "SecurePass123!",
	"name": "Jane Smith"
  }'
```

**Response:**
```json
{
  "message": "User registered successfully. Awaiting admin approval.",
  "user": {
	"id": 2,
	"name": "Jane Smith",
	"email": "newuser@example.com",
	"isApproved": false,
	"role": "User",
	"createdAt": "2026-05-04T20:00:00Z"
  }
}
```

### 2. Login User

**Request:**
```bash
curl -X POST "https://localhost:7259/api/Users/login" \
  -H "Content-Type: application/json" \
  -d '{
	"email": "newuser@example.com",
	"password": "SecurePass123!"
  }'
```

**Response:**
```json
{
  "message": "Login successful",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJVc2VySWQiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoibmV3dXNlckBleGFtcGxlLmNvbSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlVzZXIiLCJleHAiOjE3NzgwMTI4NDF9.SIGNATURE",
  "user": {
	"id": 2,
	"name": "Jane Smith",
	"email": "newuser@example.com",
	"role": "User"
  }
}
```

### 3. Create Study Group (Authenticated)

**Request:**
```bash
curl -X POST "https://localhost:7259/api/StudyGroups" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
	"subject": "Mathematics",
	"description": "Learn advanced calculus methods",
	"location": "Library Room 101",
	"meetingTime": "2026-05-15T18:00:00Z",
	"meetingType": "In-Person",
	"maxMembers": 10
  }'
```

**Response:**
```json
{
  "id": 1,
  "subject": "Mathematics",
  "description": "Learn advanced calculus methods",
  "location": "Library Room 101",
  "meetingTime": "2026-05-15T18:00:00Z",
  "meetingType": "In-Person",
  "maxMembers": 10,
  "userId": 2,
  "isApproved": false,
  "createdAt": "2026-05-04T20:30:00Z"
}
```

### 4. Get All Study Groups

**Request:**
```bash
curl -X GET "https://localhost:7259/api/StudyGroups" \
  -H "Content-Type: application/json"
```

**Response:**
```json
[
  {
	"id": 1,
	"subject": "Mathematics",
	"description": "Learn advanced calculus methods",
	"location": "Library Room 101",
	"meetingTime": "2026-05-15T18:00:00Z",
	"meetingType": "In-Person",
	"maxMembers": 10,
	"isApproved": true,
	"user": {
	  "id": 2,
	  "name": "Jane Smith",
	  "email": "newuser@example.com"
	}
  }
]
```

### 5. Search Study Groups

**Request:**
```bash
curl -X GET "https://localhost:7259/api/StudyGroups/search?subject=Math&location=Library" \
  -H "Content-Type: application/json"
```

---

## 🚀 How Frontend Should Connect

### Base URL

```
Development: https://localhost:7259
Production: https://your-production-url.com
```

### Authentication Flow (Frontend)

1. User clicks "Register"
2. Frontend sends registration data to `/api/Users/register`
3. Wait for admin approval (show message)
4. User clicks "Login"
5. Frontend sends credentials to `/api/Users/login`
6. Store returned JWT token in `localStorage` or `sessionStorage`
7. Include token in all subsequent requests

### Example: Axios Configuration

```javascript
import axios from 'axios';

const api = axios.create({
  baseURL: 'https://localhost:7259/api',
  headers: {
	'Content-Type': 'application/json'
  }
});

// Add token to every request
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('jwt_token');
  if (token) {
	config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default api;
```

### Example: React Login Component

```javascript
import React, { useState } from 'react';
import api from './api';

function LoginComponent() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const handleLogin = async (e) => {
	e.preventDefault();
	try {
	  const response = await api.post('/Users/login', {
		email,
		password
	  });

	  // Store token
	  localStorage.setItem('jwt_token', response.data.token);

	  // Redirect to dashboard
	  window.location.href = '/dashboard';
	} catch (error) {
	  console.error('Login failed:', error.response.data);
	  alert('Login failed: ' + error.response.data.message);
	}
  };

  return (
	<form onSubmit={handleLogin}>
	  <input 
		type="email" 
		value={email} 
		onChange={(e) => setEmail(e.target.value)} 
		placeholder="Email"
	  />
	  <input 
		type="password" 
		value={password} 
		onChange={(e) => setPassword(e.target.value)} 
		placeholder="Password"
	  />
	  <button type="submit">Login</button>
	</form>
  );
}

export default LoginComponent;
```

### Example: Vue.js Fetch Study Groups

```javascript
<template>
  <div>
	<h1>Study Groups</h1>
	<ul>
	  <li v-for="group in groups" :key="group.id">
		{{ group.subject }} - {{ group.location }}
	  </li>
	</ul>
  </div>
</template>

<script>
import api from './api';

export default {
  data() {
	return {
	  groups: []
	};
  },
  mounted() {
	this.fetchGroups();
  },
  methods: {
	async fetchGroups() {
	  try {
		const response = await api.get('/StudyGroups');
		this.groups = response.data;
	  } catch (error) {
		console.error('Error fetching groups:', error);
	  }
	}
  }
};
</script>
```

---

## ⚠️ Common Issues & Fixes

### 1. **401 Unauthorized Error**

**Problem:** Getting a 401 response when making authenticated requests

**Causes:**
- JWT token is missing or invalid
- Token has expired
- Token format is incorrect

**Solution:**
```javascript
// Check token is included in header
headers: {
  'Authorization': `Bearer ${token}`  // Correct format
}

// NOT this:
headers: {
  'Authorization': token  // ❌ Wrong
}

// Check token expiration
const decoded = jwt_decode(token);
if (decoded.exp * 1000 < Date.now()) {
  console.log('Token expired, redirect to login');
}
```

### 2. **Invalid Token Error**

**Problem:** "invalid_token" error in response

**Causes:**
- Token format is incorrect
- Token signature doesn't match (wrong secret key)
- Token was tampered with

**Solution:**
- Ensure the secret key in `appsettings.json` matches across all environments
- Don't modify the token value
- Regenerate token if needed

### 3. **403 Forbidden - No Permission**

**Problem:** Getting 403 error on allowed endpoints

**Causes:**
- User role doesn't have required permission
- Only owner or admin can perform this action
- User is not approved by admin

**Solution:**
```
- Admin actions: Only Admin role can access
- Owner actions: Only study group owner or Admin can access
- Wait for admin approval if not approved
```

### 4. **405 Method Not Allowed**

**Problem:** Method not allowed for this endpoint

**Causes:**
- Using wrong HTTP method (GET instead of POST)
- Endpoint doesn't exist

**Solution:**
- Check API documentation for correct HTTP method
- Verify endpoint URL spelling

### 5. **500 Internal Server Error - Object Cycle Detection**

**Problem:** "A possible object cycle was detected" error

**Causes:**
- Circular references in Entity Framework navigation properties
- EF Core trying to serialize `User.StudyGroups.User.StudyGroups...`

**Solution:**
- ✅ Already fixed in this project - configured `ReferenceHandler.IgnoreCycles` in `Program.cs`
- Use DTOs instead of returning entity models directly
- Implement `[JsonIgnore]` attributes on circular navigation properties

### 6. **Database Connection Failed**

**Problem:** "Cannot open server requested by the login" or connection timeout

**Causes:**
- SQL Server is not running
- Connection string is incorrect
- Server name is wrong
- Credentials are incorrect

**Solution:**
```bash
# Verify SQL Server is running
# Windows: Check Services (services.msc) for SQL Server
# Update connection string in appsettings.json:

"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=StudyGroupDB;Trusted_Connection=true;TrustServerCertificate=true;"
}

# Test connection in Visual Studio's SQL Server Object Explorer
```

### 7. **Migration Failed**

**Problem:** "There are pending model changes" when running migrations

**Causes:**
- Changes to models without creating migration

**Solution:**
```bash
# Create a new migration
dotnet ef migrations add YourMigrationName

# Apply all pending migrations
dotnet ef database update
```

### 8. **JWT Token Malformed**

**Problem:** Error parsing JWT token

**Causes:**
- Token is incomplete (only 1 or 2 parts instead of 3)
- Token contains invalid characters

**Solution:**
- Ensure full token is copied (should have 2 dots: `xxx.yyy.zzz`)
- Don't include "Bearer" prefix when storing token
- Store only the token value

---

## 👥 Team Workflow (Git Best Practices)

### Branch Strategy

We use **Git Flow** with feature branches:

```
main (production)
  ↓
develop (staging)
  ↓
feature/* (feature branches)
```

### Step-by-Step Workflow

#### 1. Pull Latest Code

Always start by pulling the latest changes from the team:

```bash
git checkout develop
git pull origin develop
```

#### 2. Create Feature Branch

Create a new branch for your feature with a descriptive name:

```bash
# Format: feature/what-you-are-doing
git checkout -b feature/add-study-group-comments
```

**Branch Naming Convention:**
- Features: `feature/description`
- Bug fixes: `bugfix/description`
- Hotfixes: `hotfix/description`

#### 3. Make Changes Locally

Write your code following the project structure and coding standards:

```bash
# Check status
git status

# See changes
git diff

# Stage specific files
git add Controllers/MyController.cs
git add DTOs/MyDto.cs

# Or stage all changes
git add .
```

#### 4. Commit Changes

Write clear, descriptive commit messages:

```bash
# Good commit messages
git commit -m "Add study group comments feature"
git commit -m "Fix JWT expiration validation"
git commit -m "Refactor authentication service"

# Poor commit messages (avoid)
git commit -m "fix stuff"
git commit -m "update"
git commit -m "changes"
```

**Commit Message Format:**
```
[TYPE] Brief description

Detailed explanation if needed

- Bullet point 1
- Bullet point 2
```

**Types:**
- `feat:` New feature
- `fix:` Bug fix
- `refactor:` Code refactoring
- `docs:` Documentation updates
- `style:` Code style changes
- `test:` Adding or modifying tests

**Example:**
```bash
git commit -m "feat: Add comment functionality to study groups

- Create Comment model and DbContext mapping
- Add CommentsController with CRUD endpoints
- Create CommentDto for API responses
- Add authorization checks for comment deletion"
```

#### 5. Push Changes to Remote

Push your feature branch to GitHub:

```bash
git push origin feature/add-study-group-comments

# If branch doesn't exist on remote yet:
git push -u origin feature/add-study-group-comments
```

#### 6. Create Pull Request (PR)

On GitHub:

1. Go to your repository
2. Click "Pull Requests" tab
3. Click "New Pull Request"
4. Select:
   - Base branch: `develop`
   - Compare branch: `feature/your-branch`
5. Add title: `Add study group comments feature`
6. Add description:
   ```
   ## What does this PR do?
   Adds the ability for users to comment on study groups

   ## Related Issue
   Closes #123

   ## Testing
   - [x] Tested login functionality
   - [x] Tested comment creation
   - [x] Tested authorization

   ## Checklist
   - [x] Code follows project style
   - [x] No breaking changes
   - [x] Updated documentation
   ```

7. Click "Create Pull Request"

#### 7. Code Review

- Team members review your code
- Address feedback and make changes:
  ```bash
  # Make requested changes
  git add .
  git commit -m "refactor: Improve comment validation logic"
  git push origin feature/add-study-group-comments
  ```

#### 8. Merge to Develop

Once approved:

1. Click "Merge Pull Request" on GitHub
2. Delete the feature branch
3. Pull changes locally:
   ```bash
   git checkout develop
   git pull origin develop
   ```

#### 9. Deploy to Main (Release)

When ready for production:

```bash
git checkout main
git pull origin main
git merge develop
git push origin main
git tag v1.0.0
git push origin v1.0.0
```

### Daily Workflow Checklist

```bash
# Morning: Get latest code
git checkout develop
git pull origin develop

# Work on your feature
git checkout feature/your-branch
# ... write code ...

# Evening: Commit and push
git add .
git commit -m "feat: your description"
git push origin feature/your-branch

# Before going home: Check status
git status  # Should be clean
git log --oneline -5  # See your commits
```

### Handling Merge Conflicts

If you get a merge conflict:

```bash
# Get latest develop
git fetch origin
git rebase origin/develop

# Fix conflicts in your editor
# Look for: <<<<<<< HEAD, =======, >>>>>>>

# After fixing:
git add .
git rebase --continue
git push origin feature/your-branch -f
```

---

## 📦 Required NuGet Packages

All required packages are already included in the project file. Here's what's installed:

| Package | Version | Purpose |
|---------|---------|---------|
| **Microsoft.AspNetCore.App** | 8.0.0 | ASP.NET Core framework |
| **Microsoft.EntityFrameworkCore** | 8.0.0 | ORM for database access |
| **Microsoft.EntityFrameworkCore.SqlServer** | 8.0.0 | SQL Server provider for EF Core |
| **Microsoft.EntityFrameworkCore.Tools** | 8.0.0 | EF Core command-line tools (migrations) |
| **System.IdentityModel.Tokens.Jwt** | Latest | JWT token handling |
| **Microsoft.IdentityModel.Tokens** | Latest | Token validation |
| **Microsoft.AspNetCore.Authentication.JwtBearer** | 8.0.0 | JWT authentication middleware |
| **Swashbuckle.AspNetCore** | Latest | Swagger documentation |

### To Install a Package

```bash
# Using Package Manager Console
Install-Package PackageName

# Using dotnet CLI
dotnet add package PackageName
```

### To Update a Package

```bash
# Using dotnet CLI
dotnet add package PackageName --version 8.0.5

# Update all packages to latest
dotnet add package --upgrade
```

---

## 💡 Best Practices

### 1. **Always Use DTOs for API Requests/Responses**

✅ **Good:**
```csharp
[HttpPost]
public async Task<IActionResult> CreateStudyGroup([FromBody] CreateStudyGroupDto createDto)
{
	var studyGroup = new StudyGroup
	{
		Subject = createDto.Subject,
		Description = createDto.Description
	};
}
```

❌ **Bad:**
```csharp
[HttpPost]
public async Task<IActionResult> CreateStudyGroup([FromBody] StudyGroup studyGroup)
{
	_context.StudyGroups.Add(studyGroup);
}
```

**Why?** DTOs protect internal entity structure and prevent exposing database details.

### 2. **Never Expose Navigation Properties in Response**

✅ **Good:** Hide circular references
```csharp
public class StudyGroupDto
{
	public int ID { get; set; }
	public string Subject { get; set; }
	[JsonIgnore]
	public User User { get; set; }  // Don't expose
}
```

❌ **Bad:** Include all navigation properties
```csharp
public async Task<IActionResult> GetStudyGroup(int id)
{
	var group = await _context.StudyGroups
		.Include(sg => sg.User)
		.Include(sg => sg.User.StudyGroups)  // Circular!
		.FirstOrDefaultAsync(sg => sg.ID == id);
	return Ok(group);
}
```

### 3. **Keep Secrets Out of Code**

✅ **Good:** Use configuration
```csharp
var secretKey = builder.Configuration["Jwt:SecretKey"];
```

❌ **Bad:** Hardcoded secrets
```csharp
var secretKey = "MySecretKey123"; // Never do this!
```

### 4. **Use Async/Await for Database Operations**

✅ **Good:**
```csharp
var user = await _context.Users.FindAsync(userId);
var groups = await _context.StudyGroups.ToListAsync();
```

❌ **Bad:**
```csharp
var user = _context.Users.Find(userId);  // Synchronous
var groups = _context.StudyGroups.ToList();  // Blocks thread
```

### 5. **Always Validate Input**

✅ **Good:**
```csharp
if (!ModelState.IsValid)
	return BadRequest(ModelState);
```

❌ **Bad:**
```csharp
// Trust all input without validation
var studyGroup = new StudyGroup { ... };
```

### 6. **Use Role-Based Authorization**

✅ **Good:**
```csharp
[Authorize(Roles = "Admin")]
public async Task<IActionResult> ApproveStudyGroup(int id)
{
	// Only admins can access
}
```

❌ **Bad:**
```csharp
[Authorize]
public async Task<IActionResult> ApproveStudyGroup(int id)
{
	// Any authenticated user can access
}
```

### 7. **Handle Exceptions Properly**

✅ **Good:**
```csharp
try
{
	var user = await _context.Users.FindAsync(userId);
	if (user == null)
		return NotFound(new { message = "User not found" });
}
catch (Exception ex)
{
	_logger.LogError($"Error fetching user: {ex.Message}");
	return StatusCode(500, new { message = "Internal server error" });
}
```

❌ **Bad:**
```csharp
var user = await _context.Users.FindAsync(userId);
return Ok(user);  // Crashes if user is null
```

### 8. **Use Dependency Injection**

✅ **Good:**
```csharp
public class StudyGroupsController : ControllerBase
{
	private readonly AppDbContext _context;

	public StudyGroupsController(AppDbContext context)
	{
		_context = context;
	}
}
```

❌ **Bad:**
```csharp
public class StudyGroupsController : ControllerBase
{
	private readonly AppDbContext _context = new AppDbContext();  // Hard dependency
}
```

### 9. **Use HTTPS in Production**

✅ Always use HTTPS for JWT tokens  
✅ Set secure cookie flags  
✅ Implement CORS correctly

### 10. **Validate JWT Token Properly**

✅ **Good:**
```csharp
options.TokenValidationParameters = new TokenValidationParameters
{
	ValidateIssuerSigningKey = true,
	IssuerSigningKey = new SymmetricSecurityKey(key),
	ValidateIssuer = true,
	ValidIssuer = "StudyGroupAPI",
	ValidateAudience = true,
	ValidAudience = "StudyGroupAPIUsers",
	ValidateLifetime = true,  // Check expiration
	ClockSkew = TimeSpan.Zero
};
```

---

## 🔒 Security Checklist

- [ ] JWT Secret key is changed and secured
- [ ] SQL connection string has no exposed credentials
- [ ] HTTPS is enabled in production
- [ ] CORS is properly configured
- [ ] Sensitive data is not logged
- [ ] Input validation is implemented
- [ ] Authorization checks are in place
- [ ] Secrets are in environment variables, not in code
- [ ] Database migrations are run before deployment
- [ ] Admin approval is required for user activation

---

## 🤝 Contributing

1. Create a feature branch
2. Make your changes
3. Push and create a Pull Request
4. Wait for code review
5. Address feedback
6. Merge when approved

---

## 📞 Support & Contact

For questions or issues:
- Create an issue on GitHub
- Contact the development team
- Review API documentation at `/swagger`

---

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

---

## 🎯 Roadmap

- [ ] Email verification for user registration
- [ ] Study group ratings and reviews
- [ ] File upload for study materials
- [ ] Real-time notifications
- [ ] Mobile app integration
- [ ] Advanced search filters
- [ ] Study group calendar integration

---

**Last Updated:** May 2026  
**Maintained By:** Development Team
