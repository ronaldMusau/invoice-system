Invoice Management System Backend
A comprehensive invoice management system backend built with ASP.NET Core Web API, featuring JWT authentication, role-based access control, real-time notifications, and PDF generation.

📋 Table of Contents
Overview

Features

Project Structure

API Documentation

Database Schema

Getting Started

Authentication

Real-time Notifications

Deployment

📖 Overview
This is a robust backend system for managing invoices with support for:

Multi-role system (Admin and User)

JWT-based authentication with refresh tokens

Real-time notifications using SignalR

PDF invoice generation

Comprehensive CRUD operations for invoices

Role-based access control

✨ Features
🔐 Authentication & Authorization
User registration with email and username validation

JWT token-based authentication

Refresh token mechanism

Role-based access control (Admin/User)

Secure password hashing using BCrypt

📊 Invoice Management
Admin can create, read, update, and delete invoices

Users can view assigned invoices

Invoice status tracking (Pending, Paid, Overdue, Cancelled)

PDF generation for invoices

Automatic invoice number generation

🔔 Real-time Notifications
SignalR-based real-time notifications

Notifications for new invoice assignments

Notification read/unread tracking

WebSocket communication for instant updates

📁 File Management
PDF generation for invoices

Automatic invoice download

Professional invoice formatting

📁 Project Structure
text
INVOICE-SYSTEM-BACKEND/
├── Controllers/           # API Controllers
│   ├── AuthController.cs            # Authentication endpoints
│   ├── InvoicesController.cs        # Invoice CRUD operations
│   ├── NotificationController.cs    # Notification management
│   ├── UsersController.cs           # User management
│   └── WeatherForecastController.cs # Sample endpoint
├── Data/                 # Data access layer
│   ├── ApplicationDbContext.cs      # Entity Framework context
│   └── SeedData.cs                  # Database seeding
├── DTOs/                 # Data Transfer Objects
│   ├── InvoiceDto.cs               # Invoice data models
│   └── UserDto.cs                  # User data models
├── Helpers/              # Utilities
│   └── AutoMapperProfile.cs       # Object mapping configuration
├── Hubs/                 # SignalR Hubs
│   └── NotificationHub.cs          # Real-time notifications hub
├── LatoFont/             # Font files for PDF generation
├── Migrations/           # Database migrations
├── Models/               # Database entities
│   ├── Invoice.cs                  # Invoice model
│   ├── InvoiceItem.cs              # Invoice item model
│   ├── Notification.cs             # Notification model
│   └── User.cs                     # User model
├── Services/             # Business logic services
│   ├── AuthService.cs              # Authentication logic
│   └── PdfService.cs               # PDF generation service
├── appsettings.json      # Configuration
├── InvoiceSystem.http    # API test file
├── Program.cs            # Application entry point
└── README.md             # This file
🔌 API Documentation
Base URL
text
http://localhost:5000/api
Authentication Endpoints
1. Register User
POST /api/auth/register

Request Body:

json
{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "password123",
  "userType": "User"  // "User" or "Admin"
}
Response:

json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "refresh_token_string",
  "username": "john_doe",
  "role": "User",
  "accessTokenExpiry": "2024-02-10T12:00:00Z",
  "refreshTokenExpiry": "2024-02-17T12:00:00Z"
}
2. Login
POST /api/auth/login

Request Body:

json
{
  "username": "john_doe",
  "password": "password123",
  "userType": "User"
}
Response: Same as register response

3. Refresh Token
POST /api/auth/refresh-token

Request Body:

json
{
  "refreshToken": "refresh_token_string"
}
Response:

json
{
  "accessToken": "new_access_token",
  "refreshToken": "new_refresh_token",
  "accessTokenExpiry": "2024-02-10T12:00:00Z",
  "refreshTokenExpiry": "2024-02-17T12:00:00Z"
}
4. Logout
POST /api/auth/logout

Request Body:

json
{
  "refreshToken": "refresh_token_string"
}
Invoice Endpoints
1. Get All Invoices
GET /api/invoices

Headers:

text
Authorization: Bearer <access_token>
Permissions:

Admin: Gets all invoices

User: Gets only assigned invoices

Response:

json
[
  {
    "id": 1,
    "invoiceNumber": "INV-20240210120000-1234",
    "customerName": "Acme Corporation",
    "issueDate": "2024-02-10T12:00:00Z",
    "dueDate": "2024-03-10T12:00:00Z",
    "totalAmount": 2500.00,
    "status": "Pending",
    "assignedUserId": 2,
    "assignedUserName": "john_doe",
    "items": [
      {
        "id": 1,
        "description": "Web Development",
        "quantity": 40,
        "unitPrice": 50.00,
        "totalPrice": 2000.00
      },
      {
        "id": 2,
        "description": "Hosting",
        "quantity": 1,
        "unitPrice": 500.00,
        "totalPrice": 500.00
      }
    ]
  }
]
2. Get Invoice by ID
GET /api/invoices/{id}

Permissions:

Admin: Can access any invoice

User: Can only access assigned invoices

3. Create Invoice (Admin Only)
POST /api/invoices

Headers:

text
Authorization: Bearer <admin_access_token>
Request Body:

json
{
  "customerName": "New Customer",
  "dueDate": "2024-03-15T12:00:00Z",
  "assignedUserId": 2,
  "items": [
    {
      "description": "Consulting Services",
      "quantity": 10,
      "unitPrice": 100.00
    }
  ]
}
Features:

Automatically generates invoice number

Calculates total amount

Sends real-time notification to assigned user

Creates notification record

4. Update Invoice Status
PUT /api/invoices/{id}/status

Request Body:

json
{
  "status": "Paid"  // "Pending", "Paid", "Overdue", "Cancelled"
}
Permissions:

Admin: Can update any invoice status

User: Can only update assigned invoice status

5. Download Invoice PDF
GET /api/invoices/{id}/download

Response: PDF file download

Notification Endpoints
1. Get User Notifications
GET /api/notification

Headers:

text
Authorization: Bearer <access_token>
Response:

json
[
  {
    "id": 1,
    "message": "New invoice #INV-20240210-001 has been assigned to you",
    "createdAt": "2024-02-10T12:00:00Z",
    "isRead": false,
    "userId": 2
  }
]
2. Mark Notification as Read
PUT /api/notification/{id}/read

3. Delete Notification
DELETE /api/notification/{id}

User Endpoints
1. Get All Users (Admin Only)
GET /api/users

Headers:

text
Authorization: Bearer <admin_access_token>
Response:

json
[
  {
    "id": 2,
    "username": "john_doe",
    "email": "john@example.com",
    "role": "User"
  }
]
🗃️ Database Schema
Users Table
sql
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(100) UNIQUE NOT NULL,
    Email NVARCHAR(200) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    Role NVARCHAR(50) DEFAULT 'User' NOT NULL,
    RefreshToken NVARCHAR(MAX),
    RefreshTokenExpiry DATETIME2,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
)
Invoices Table
sql
CREATE TABLE Invoices (
    Id INT PRIMARY KEY IDENTITY(1,1),
    InvoiceNumber NVARCHAR(50) NOT NULL,
    CustomerName NVARCHAR(200) NOT NULL,
    IssueDate DATETIME2 NOT NULL,
    DueDate DATETIME2 NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(20) DEFAULT 'Pending' NOT NULL,
    AssignedUserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id),
    CreatedByAdminId INT NULL FOREIGN KEY REFERENCES Users(Id)
)
InvoiceItems Table
sql
CREATE TABLE InvoiceItems (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Description NVARCHAR(200) NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    TotalPrice DECIMAL(18,2) NOT NULL,
    InvoiceId INT NOT NULL FOREIGN KEY REFERENCES Invoices(Id) ON DELETE CASCADE
)
Notifications Table
sql
CREATE TABLE Notifications (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Message NVARCHAR(500) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    IsRead BIT DEFAULT 0,
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE
)
🚀 Getting Started
Prerequisites
.NET 8.0 SDK or later

SQL Server (LocalDB, Express, or Full)

Visual Studio 2022 or VS Code

Installation Steps
Clone the repository

bash
git clone <repository-url>
cd INVOICE-SYSTEM-BACKEND
Configure database connection
Edit appsettings.json:

json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=InvoiceSystemDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "YourVeryLongSecretKeyHereAtLeast32Characters",
    "Issuer": "InvoiceSystem",
    "Audience": "InvoiceSystemUsers"
  }
}
Apply database migrations

bash
dotnet ef database update
Run the application

bash
dotnet run
Access the API

Swagger UI: https://localhost:5000/swagger

API Base URL: https://localhost:5000/api

Database Seeding
The system comes with seeded data:

Admin User: username: admin, password: admin123

Regular Users:

john_doe / user123

jane_smith / user123

Sample Invoices: 3 sample invoices with items

🔐 Authentication Details
JWT Configuration
json
{
  "Jwt": {
    "Key": "64-character-secret-key-for-signing-jwt-tokens",
    "Issuer": "InvoiceSystem",
    "Audience": "InvoiceSystemUsers",
    "ExpireDays": 7
  }
}
Token Structure
Access Token: Valid for 1 hour, used for API authorization

Refresh Token: Valid for 7 days, used to obtain new access tokens

Claims Included: UserId, Username, Email, Role

Password Security
Passwords are hashed using BCrypt with salt

Minimum password length: 6 characters

Username and email must be unique

🔔 Real-time Notifications
SignalR Hub Configuration
Hub Endpoint: /notificationHub

Authentication: Uses JWT tokens

Groups: Supports user-specific and admin group notifications

Notification Flow
Admin creates invoice → Notification sent to assigned user

User updates invoice status → Notification sent to admin

Real-time updates through WebSocket connection

Client Connection Example (JavaScript)
javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub", {
        accessTokenFactory: () => localStorage.getItem('accessToken')
    })
    .build();

connection.on("ReceiveNotification", (message) => {
    console.log("New notification:", message);
    // Update UI
});

await connection.start();
🎯 Role-Based Access Control
Admin Permissions
Create, read, update, delete all invoices

View all users

Assign invoices to users

Access all system data

User Permissions
View assigned invoices only

Update status of assigned invoices

Download PDF of assigned invoices

View personal notifications

📄 PDF Generation
Features
Professional invoice formatting

Automatic calculation of totals

Company branding support

Itemized listing

Download as PDF file

Technology
Uses QuestPDF library

Supports custom fonts (Lato)

Generates PDF in memory

Returns as downloadable file

🛠️ Development
Adding New Features
Create model in Models/ folder

Create DTO in DTOs/ folder

Add mapping in AutoMapperProfile.cs

Create service in Services/ folder

Create controller in Controllers/ folder

Add database migration if needed

Testing
Use the provided InvoiceSystem.http file for API testing

Swagger UI for interactive testing

Postman collection available

Logging
Comprehensive logging throughout the application

Log levels: Information, Warning, Error

Console and debug output

🚀 Deployment
Production Considerations
Update JWT Key: Generate a secure random key

Configure HTTPS: Enable in production

Database Backup: Set up regular backups

Environment Variables: Move sensitive data to environment variables

CORS: Configure for your frontend domain

Docker Support (Future)
dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["InvoiceSystem.csproj", "."]
RUN dotnet restore "InvoiceSystem.csproj"
COPY . .
RUN dotnet build "InvoiceSystem.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InvoiceSystem.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InvoiceSystem.dll"]
📞 Support
For issues or questions:

Check the API documentation

Review the database seeding for default credentials

Ensure proper JWT configuration

Verify database connection string

📄 License
This project is for educational and demonstration purposes.

🔄 Version History
v1.0.0 (February 2024)

Initial release

Complete authentication system

Invoice CRUD operations

Real-time notifications

PDF generation

Role-based access control

Happy Coding! 🚀