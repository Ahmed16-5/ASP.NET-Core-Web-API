# Study Group API - ASP.NET Core Web API

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
