# Study Group API - ASP.NET Core Web API

![.NET 8](https://img.shields.io/badge/.NET-8.0-blue)
![SQL Server](https://img.shields.io/badge/Database-SQL%20Server-red)
![License](https://img.shields.io/badge/License-MIT-green)

---



## 📌 Project Overview

**Study Group API** is a backend REST API service designed to facilitate the creation and management of study groups. It provides features for user authentication, authorization, and study group management with an admin approval system.

### Main Features

✅ **User Authentication** - Secure registration and login with JWT tokens  
✅ **Role-Based Authorization** - Admin, Student, and GroupCreator roles with different permissions 
✅ **Admin Approval System** - New GroupCreators and study groups require admin approval  
✅ **Study Group Management** - Create, update, delete, and search study groups  
✅ **Study Group Members** - Join requests and member management  
✅ **Materials & Comments** - Attach study materials and leave comments in study groups  
✅ **Secure API** - Protected endpoints using JWT Bearer tokens  

---

## 👥 System Actors

### Admin
- Approves/rejects GroupCreators
- Approves/rejects study groups
- Manages users and system data

### GroupCreator
- Creates and manages study groups
- Accepts/rejects join requests
- Uploads materials and manages discussions

### Student
- Browses study groups
- Sends join requests
- Participates in comments and materials

---

## 🔐 Authentication Guide

### User Registration Flow

1. **Students and GroupCreators register** with email and password
2. **Students are approved automatically**
3. **GroupCreators require admin approval**
4. **Admin approves GroupCreator accounts**
5. **Approved users can login and obtain JWT tokens**

### Login Response Example

```json
{
  "message": "Login successful",
  "token": "your-jwt-token",
  "user": {
    "id": 1,
    "name": "Ahmed",
    "email": "student@test.com",
    "role": "Student"
  }
}# Study Group API - ASP.NET Core Web API

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
	"role": "Student"
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
| POST | `/api/StudyGroups` | Create new study group | ✅ Yes | GroupCreator |
| PUT | `/api/StudyGroups/{id}` | Update study group | ✅ Yes | Owner/Admin |
| PUT | `/api/StudyGroups/{id}/approve` | Approve study group | ✅ Yes | Admin |
| DELETE | `/api/StudyGroups/{id}` | Delete study group | ✅ Yes | Owner/Admin |
| GET | `/api/StudyGroups/owner/my-groups` | Get current user's groups | ✅ Yes | GroupCreator |
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

  - GroupCreator
  - Student
- ✅ Admin Approval System
- ✅ Study Group Management
- ✅ Join Requests System
- ✅ Comments & Discussions
- ✅ Study Materials Sharing
- ✅ Protected API Endpoints
- ✅ SQL Server Database
- ✅ Entity Framework Core
- ✅ Swagger API Documentation

---

# 👥 System Actors

## Admin
- Approves/rejects GroupCreators
- Approves/rejects study groups
- Manages users and system data

## GroupCreator
- Creates and manages study groups
- Accepts/rejects join requests
- Uploads study materials
- Manages discussions and comments

## Student
- Browses study groups
- Sends join requests
- Participates in comments
- Shares study materials

---

# 🛠️ Technologies Used

| Technology | Purpose |
|---|---|
| ASP.NET Core Web API (.NET 8) | Backend Framework |
| Entity Framework Core | ORM |
| SQL Server | Database |
| JWT Authentication | Authentication & Authorization |
| Swagger / Swashbuckle | API Documentation |
| LINQ | Querying |
| Dependency Injection | Service Management |

---

# 🧩 Project Architecture

```text
Controller
   ↓
Service Layer
   ↓
Repository Layer
   ↓
DbContext (EF Core)
   ↓
SQL Server
📂 Project Structure
ASP.NET Core Web API/
│
├── Controllers/
├── Services/
├── Repositories/
├── Interfaces/
├── DTOs/
├── Models/
├── Data/
├── Migrations/
│
├── Program.cs
├── appsettings.json
└── README.md
🔐 Authentication & Authorization

The system uses JWT Bearer Tokens for authentication.

Roles
Role	Permissions
Admin	Full access
GroupCreator	Manage study groups
Student	Join groups and interact
🔄 System Workflow
Student Flow
Register
Login
Browse study groups
Send join request
Add comments and materials
GroupCreator Flow
Register
Wait for Admin approval
Login
Create study groups
Approve/reject join requests
Admin Flow
Login
Approve GroupCreators
Approve study groups
Manage users
🚀 Setup & Installation
1) Clone Repository
git clone https://github.com/your-username/study-group-api.git
2) Open Project

Open solution using:

Visual Studio 2022
Visual Studio Code
3) Configure Database

Edit appsettings.json

"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=StudyGroupDB;Trusted_Connection=True;TrustServerCertificate=True"
}
4) Run Migrations

Package Manager Console:

Update-Database
5) Run Project
dotnet run

Swagger UI:

https://localhost:7259/swagger
📌 Important API Endpoints
Authentication
Method	Endpoint
POST	/api/Users/register
POST	/api/Users/login
Study Groups
Method	Endpoint
GET	/api/StudyGroups
POST	/api/StudyGroups
PUT	/api/StudyGroups/{id}
DELETE	/api/StudyGroups/{id}
Join Requests
Method	Endpoint
POST	/api/JoinRequests
PUT	/api/JoinRequests/{id}/approve
Comments
Method	Endpoint
POST	/api/Comments
GET	/api/Comments/group/{studyGroupId}
Materials
Method	Endpoint
POST	/api/Materials
GET	/api/Materials/group/{studyGroupId}
🔒 Security Features
JWT Authentication
Role-Based Authorization
Protected Endpoints
Password Hashing
Input Validation
Admin Approval System
📖 API Documentation

Swagger UI is available at:

https://localhost:7259/swagger
📄 License

MIT License

👨‍💻 Developed Using
ASP.NET Core Web API
Entity Framework Core
SQL Server
JWT Authentication
📅 Last Updated

May 2026# Study Group API - ASP.NET Core Web API

![.NET 8](https://img.shields.io/badge/.NET-8.0-blue)
![SQL Server](https://img.shields.io/badge/Database-SQL%20Server-red)
![License](https://img.shields.io/badge/License-MIT-green)

---

# 📌 Project Overview

Study Group API is a backend REST API built using ASP.NET Core Web API and SQL Server.  
The system allows students to browse study groups, group creators to manage study groups, and admins to manage approvals and system access.

The project implements authentication, authorization, role-based access control, join requests, comments, and study materials management using JWT authentication.

---

# ✨ Main Features

- ✅ JWT Authentication & Authorization
- ✅ Role-Based Access Control
- ✅ Three System Roles:
  - Admin
  - GroupCreator
  - Student
- ✅ Admin Approval System
- ✅ Study Group Management
- ✅ Join Requests System
- ✅ Comments & Discussions
- ✅ Study Materials Sharing
- ✅ Protected API Endpoints
- ✅ SQL Server Database
- ✅ Entity Framework Core
- ✅ Swagger API Documentation

---

# 👥 System Actors

## Admin
- Approves/rejects GroupCreators
- Approves/rejects study groups
- Manages users and system data

## GroupCreator
- Creates and manages study groups
- Accepts/rejects join requests
- Uploads study materials
- Manages discussions and comments

## Student
- Browses study groups
- Sends join requests
- Participates in comments
- Shares study materials

---

# 🛠️ Technologies Used

| Technology | Purpose |
|---|---|
| ASP.NET Core Web API (.NET 8) | Backend Framework |
| Entity Framework Core | ORM |
| SQL Server | Database |
| JWT Authentication | Authentication & Authorization |
| Swagger / Swashbuckle | API Documentation |
| LINQ | Querying |
| Dependency Injection | Service Management |

---

# 🧩 Project Architecture

```text
Controller
   ↓
Service Layer
   ↓
Repository Layer
   ↓
DbContext (EF Core)
   ↓
SQL Server
📂 Project Structure
ASP.NET Core Web API/
│
├── Controllers/
├── Services/
├── Repositories/
├── Interfaces/
├── DTOs/
├── Models/
├── Data/
├── Migrations/
│
├── Program.cs
├── appsettings.json
└── README.md
🔐 Authentication & Authorization

The system uses JWT Bearer Tokens for authentication.

Roles
Role	Permissions
Admin	Full access
GroupCreator	Manage study groups
Student	Join groups and interact
🔄 System Workflow
Student Flow
Register
Login
Browse study groups
Send join request
Add comments and materials
GroupCreator Flow
Register
Wait for Admin approval
Login
Create study groups
Approve/reject join requests
Admin Flow
Login
Approve GroupCreators
Approve study groups
Manage users
🚀 Setup & Installation
1) Clone Repository
git clone https://github.com/your-username/study-group-api.git
2) Open Project

Open solution using:

Visual Studio 2022
Visual Studio Code
3) Configure Database

Edit appsettings.json

"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=StudyGroupDB;Trusted_Connection=True;TrustServerCertificate=True"
}
4) Run Migrations

Package Manager Console:

Update-Database
5) Run Project
dotnet run

Swagger UI:

https://localhost:7259/swagger
📌 Important API Endpoints
Authentication
Method	Endpoint
POST	/api/Users/register
POST	/api/Users/login
Study Groups
Method	Endpoint
GET	/api/StudyGroups
POST	/api/StudyGroups
PUT	/api/StudyGroups/{id}
DELETE	/api/StudyGroups/{id}
Join Requests
Method	Endpoint
POST	/api/JoinRequests
PUT	/api/JoinRequests/{id}/approve
Comments
Method	Endpoint
POST	/api/Comments
GET	/api/Comments/group/{studyGroupId}
Materials
Method	Endpoint
POST	/api/Materials
GET	/api/Materials/group/{studyGroupId}
🔒 Security Features
JWT Authentication
Role-Based Authorization
Protected Endpoints
Password Hashing
Input Validation
Admin Approval System
📖 API Documentation

Swagger UI is available at:

https://localhost:7259/swagger
📄 License

MIT License

👨‍💻 Developed Using
ASP.NET Core Web API
Entity Framework Core
SQL Server
JWT Authentication
📅 Last Updated

May 2026
