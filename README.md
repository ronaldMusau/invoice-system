# Invoice Management System - Backend API

A comprehensive invoice management system built with ASP.NET Core Web API (.NET 8.0), featuring JWT authentication, role-based access control, real-time notifications via SignalR, and PDF generation capabilities.

![.NET Version](https://img.shields.io/badge/.NET-8.0-blue)
![License](https://img.shields.io/badge/license-MIT-green)

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Database Configuration](#database-configuration)
- [API Documentation](#api-documentation)
- [Authentication & Authorization](#authentication--authorization)
- [Real-time Notifications](#real-time-notifications)
- [PDF Generation](#pdf-generation)
- [Error Handling](#error-handling)
- [Security](#security)
- [Testing](#testing)

---

## 🎯 Overview

This Invoice Management System streamlines invoice creation, assignment, and payment tracking with distinct roles for administrators and regular users. The system provides real-time notifications, secure authentication, and professional PDF invoice generation.

### Key Capabilities

- **JWT-based Authentication** with refresh token support
- **Role-Based Access Control** (Admin & User roles)
- **Real-time Notifications** using SignalR
- **PDF Invoice Generation** with QuestPDF
- **Payment Tracking** and history
- **Automatic Status Updates** based on payments

---

## ✨ Features

### For Administrators
- ✅ Create and assign invoices to users
- ✅ View all invoices across the system
- ✅ Monitor all payments and transactions
- ✅ Access user performance statistics
- ✅ Receive payment notifications

### For Users
- ✅ View assigned invoices only
- ✅ Update invoice payment status
- ✅ Download invoice PDFs
- ✅ Receive real-time notifications for new assignments
- ✅ Track payment history
- ✅ Manage personal notifications

---

## 🛠 Technology Stack

### Core Technologies
- **Framework**: ASP.NET Core Web API 8.0
- **Language**: C# 12.0
- **Database**: SQL Server (LocalDB/Express/Full)
- **ORM**: Entity Framework Core 8.0

### Libraries & Packages
- **Authentication**: Microsoft.AspNetCore.Authentication.JwtBearer
- **Password Hashing**: BCrypt.Net-Next
- **Object Mapping**: AutoMapper
- **PDF Generation**: QuestPDF
- **Real-time Communication**: SignalR
- **Database**: Microsoft.EntityFrameworkCore.SqlServer
- **Validation**: System.ComponentModel.DataAnnotations

---

## 📁 Project Structure

```
INVOICE-SYSTEM-BACKEND/
│
├── Controllers/
│   ├── AuthController.cs              # Authentication & user registration
│   ├── InvoicesController.cs          # Invoice CRUD operations
│   ├── NotificationController.cs      # Notification management
│   └── UsersController.cs             # User management (admin)
│
├── Data/
│   ├── ApplicationDbContext.cs        # EF Core DbContext
│   └── SeedData.cs                    # Database seeding
│
├── DTOs/
│   ├── InvoiceDto.cs                  # Invoice data transfer objects
│   └── UserDto.cs                     # User & auth data transfer objects
│
├── Helpers/
│   └── AutoMapperProfile.cs           # AutoMapper configuration
│
├── Hubs/
│   └── NotificationHub.cs             # SignalR hub for real-time notifications
│
├── LatoFont/                          # Font files for PDF generation
│
├── Migrations/                        # EF Core database migrations
│
├── Models/
│   ├── Invoice.cs                     # Invoice entity
│   ├── InvoiceItem.cs                 # Invoice item entity
│   ├── Notification.cs                # Notification entity
│   └── User.cs                        # User entity
│
├── Services/
│   ├── AuthService.cs                 # Authentication business logic
│   └── PdfService.cs                  # PDF generation service
│
├── appsettings.json                   # Application configuration
├── Program.cs                         # Application entry point
└── README.md                          # This file
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB, Express, or Full)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [SQL Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms) (optional)

### Installation Steps

1. **Clone the Repository**
   ```bash
   git clone <repository-url>
   cd InvoiceSystem
   ```

2. **Configure Database Connection**
   
   Update `appsettings.json` with your SQL Server connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=InvoiceSystemDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
     }
   }
   ```

   **Common Connection Strings:**
   
   - **SQL Server LocalDB (Windows)**:
     ```
     Server=(localdb)\\mssqllocaldb;Database=InvoiceSystemDB;Trusted_Connection=true;
     ```
   
   - **SQL Server Express**:
     ```
     Server=localhost\\SQLEXPRESS;Database=InvoiceSystemDB;Trusted_Connection=true;
     ```
   
   - **SQL Server with Authentication**:
     ```
     Server=localhost;Database=InvoiceSystemDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;
     ```

3. **Install Dependencies**
   ```bash
   dotnet restore
   ```

4. **Apply Database Migrations**
   ```bash
   dotnet ef database update
   ```
   
   If you don't have EF Core tools installed:
   ```bash
   dotnet tool install --global dotnet-ef
   ```

5. **Run the Application**
   ```bash
   dotnet run
   ```

6. **Access the API**
   - **Swagger UI**: https://localhost:5001/swagger
   - **Base API URL**: https://localhost:5001/api

---

## 💾 Database Configuration

### Database Schema

The system uses 4 main tables with well-defined relationships:

#### Users Table
```sql
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY,
    Username NVARCHAR(100) UNIQUE NOT NULL,
    Email NVARCHAR(200) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    Role NVARCHAR(50) NOT NULL DEFAULT 'User',
    RefreshToken NVARCHAR(MAX),
    RefreshTokenExpiry DATETIME2,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
)
```

#### Invoices Table
```sql
CREATE TABLE Invoices (
    Id INT PRIMARY KEY IDENTITY,
    InvoiceNumber NVARCHAR(50) UNIQUE NOT NULL,
    CustomerName NVARCHAR(200) NOT NULL,
    IssueDate DATETIME2 NOT NULL,
    DueDate DATETIME2 NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(20) NOT NULL,
    AcceptedDate DATETIME2,
    AssignedUserId INT NOT NULL,
    CreatedByAdminId INT,
    FOREIGN KEY (AssignedUserId) REFERENCES Users(Id),
    FOREIGN KEY (CreatedByAdminId) REFERENCES Users(Id)
)
```

#### InvoiceItems Table
```sql
CREATE TABLE InvoiceItems (
    Id INT PRIMARY KEY IDENTITY,
    InvoiceId INT NOT NULL,
    Description NVARCHAR(200) NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    TotalPrice DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE CASCADE
)
```

#### Notifications Table
```sql
CREATE TABLE Notifications (
    Id INT PRIMARY KEY IDENTITY,
    Message NVARCHAR(500) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsRead BIT NOT NULL DEFAULT 0,
    UserId INT NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
)
```

### Entity Relationships

```
Users (1) ─────< (N) Invoices [AssignedUser]
Users (1) ─────< (N) Invoices [CreatedByAdmin]
Users (1) ─────< (N) Notifications

Invoices (1) ──< (N) InvoiceItems
```

### Seeded Data

The database is automatically seeded with test data:

**Default Admin:**
- Username: `admin`
- Password: `admin123`
- Email: `admin@invoicesystem.com`

**Default Users:**
- Username: `john_doe`, Password: `user123`, Email: `john@example.com`
- Username: `jane_smith`, Password: `user123`, Email: `jane@example.com`

**Sample Invoices:** 3 pre-created invoices with various statuses

---

## 📚 API Documentation

### Base URL
```
https://localhost:5001/api
```

### Authentication Header
All endpoints (except registration and login) require a JWT token:
```
Authorization: Bearer <your-jwt-token>
```

---

## 🔐 Authentication APIs

### 1.1 Register User

**Endpoint:** `POST /api/auth/register`

**Description:** Register a new user account with default "User" role

**Request Body:**
```json
{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "SecurePass123!",
  "userType": "User"
}
```

**Validation Rules:**
- `username`: Required, unique
- `email`: Required, valid email format, unique
- `password`: Required, minimum 6 characters
- `userType`: Required, must be "User" or "Admin"

**Success Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4=",
  "username": "john_doe",
  "role": "User",
  "accessTokenExpiry": "2025-02-11T11:00:00Z",
  "refreshTokenExpiry": "2025-02-17T10:00:00Z"
}
```

**Error Responses:**
- `400 Bad Request`: Username or email already exists
- `400 Bad Request`: Invalid input data (validation errors)

---

### 1.2 Login

**Endpoint:** `POST /api/auth/login`

**Description:** Authenticate user and receive JWT tokens

**Request Body:**
```json
{
  "username": "john_doe",
  "password": "SecurePass123!",
  "userType": "User"
}
```

**Validation Rules:**
- `username`: Required
- `password`: Required
- `userType`: Required, must match user's actual role ("User" or "Admin")

**Success Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4=",
  "username": "john_doe",
  "role": "User",
  "accessTokenExpiry": "2025-02-11T11:00:00Z",
  "refreshTokenExpiry": "2025-02-17T10:00:00Z"
}
```

**Error Responses:**
- `401 Unauthorized`: Invalid username or password
- `401 Unauthorized`: User type mismatch (e.g., trying to login as Admin when registered as User)

---

### 1.3 Refresh Token

**Endpoint:** `POST /api/auth/refresh-token`

**Description:** Get new access and refresh tokens using a valid refresh token

**Request Body:**
```json
{
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4="
}
```

**Success Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "bmV3IHJlZnJlc2ggdG9rZW4=",
  "accessTokenExpiry": "2025-02-11T11:00:00Z",
  "refreshTokenExpiry": "2025-02-17T10:00:00Z"
}
```

**Error Responses:**
- `401 Unauthorized`: Invalid or expired refresh token

---

### 1.4 Revoke Token

**Endpoint:** `POST /api/auth/revoke-token`

**Description:** Revoke a refresh token (requires authentication)

**Authorization:** Bearer token required

**Request Body:**
```json
{
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4="
}
```

**Success Response (200 OK):**
```json
{
  "message": "Token revoked successfully"
}
```

**Error Responses:**
- `400 Bad Request`: Failed to revoke token

---

### 1.5 Logout

**Endpoint:** `POST /api/auth/logout`

**Description:** Logout user by revoking refresh token

**Request Body:**
```json
{
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4="
}
```

**Success Response (200 OK):**
```json
{
  "message": "Logged out successfully"
}
```

---

## 📄 Invoice APIs

### 2.1 Get All Invoices

**Endpoint:** `GET /api/invoices`

**Description:** 
- **Admins**: Retrieve all invoices in the system
- **Users**: Retrieve only invoices assigned to them

**Authorization:** Bearer token required

**Success Response (200 OK):**
```json
[
  {
    "id": 1,
    "invoiceNumber": "INV-20260209145321-1234",
    "customerName": "Acme Corporation",
    "issueDate": "2025-02-09T10:30:00Z",
    "dueDate": "2025-03-09T10:30:00Z",
    "totalAmount": 2500.00,
    "status": "Pending",
    "assignedUserId": 2,
    "assignedUserName": "john_doe",
    "items": [
      {
        "id": 1,
        "description": "Web Development Services",
        "quantity": 40,
        "unitPrice": 50.00,
        "totalPrice": 2000.00
      },
      {
        "id": 2,
        "description": "Hosting Services (Annual)",
        "quantity": 1,
        "unitPrice": 500.00,
        "totalPrice": 500.00
      }
    ]
  }
]
```

**Query Parameters:** None

**Behavior:**
- Results are ordered by `IssueDate` descending (newest first)
- Users only see invoices where `assignedUserId` matches their ID
- Admins see all invoices

---

### 2.2 Get Invoice by ID

**Endpoint:** `GET /api/invoices/{id}`

**Description:** Retrieve a specific invoice by its ID

**Authorization:** Bearer token required

**Path Parameters:**
- `id` (integer): Invoice ID

**Success Response (200 OK):**
```json
{
  "id": 1,
  "invoiceNumber": "INV-20260209145321-1234",
  "customerName": "Acme Corporation",
  "issueDate": "2025-02-09T10:30:00Z",
  "dueDate": "2025-03-09T10:30:00Z",
  "totalAmount": 2500.00,
  "status": "Pending",
  "assignedUserId": 2,
  "assignedUserName": "john_doe",
  "items": [
    {
      "id": 1,
      "description": "Web Development Services",
      "quantity": 40,
      "unitPrice": 50.00,
      "totalPrice": 2000.00
    }
  ]
}
```

**Error Responses:**
- `404 Not Found`: Invoice doesn't exist
- `403 Forbidden`: User not authorized (non-admin trying to view another user's invoice)

---

### 2.3 Create Invoice

**Endpoint:** `POST /api/invoices`

**Description:** Create a new invoice and assign it to a user (Admin only)

**Authorization:** Bearer token required (Admin role)

**Request Body:**
```json
{
  "customerName": "Acme Corporation",
  "dueDate": "2025-03-09T10:30:00Z",
  "assignedUserId": 2,
  "items": [
    {
      "description": "Web Development Services",
      "quantity": 40,
      "unitPrice": 50.00
    },
    {
      "description": "Hosting Services (Annual)",
      "quantity": 1,
      "unitPrice": 500.00
    }
  ]
}
```

**Validation Rules:**
- `customerName`: Required, not empty
- `dueDate`: Required, must be in the future
- `assignedUserId`: Required, must be valid user ID
- `items`: Required, at least one item
- Each item must have:
  - `description`: Required, not empty
  - `quantity`: Required, > 0
  - `unitPrice`: Required, > 0

**Success Response (201 Created):**
```json
{
  "id": 5,
  "invoiceNumber": "INV-20260210083045-5678",
  "customerName": "Acme Corporation",
  "issueDate": "2025-02-10T08:30:45Z",
  "dueDate": "2025-03-09T10:30:00Z",
  "totalAmount": 2500.00,
  "status": "Pending",
  "assignedUserId": 2,
  "assignedUserName": "john_doe",
  "items": [...]
}
```

**Automatic Behaviors:**
- Invoice number auto-generated (format: `INV-YYYYMMDDHHMMSS-XXXX`)
- `issueDate` set to current UTC time
- `status` set to "Pending"
- `totalAmount` calculated from items
- `createdByAdminId` set to current admin's ID
- Notification created for assigned user
- Real-time SignalR notification sent to assigned user

**Error Responses:**
- `400 Bad Request`: Validation errors (missing fields, invalid data)
- `400 Bad Request`: Assigned user not found
- `401 Unauthorized`: Missing or invalid token
- `403 Forbidden`: User is not an admin

---

### 2.4 Update Invoice Status

**Endpoint:** `PUT /api/invoices/{id}/status`

**Description:** Update the status of an invoice

**Authorization:** Bearer token required
- **Admins**: Can update any invoice
- **Users**: Can only update invoices assigned to them

**Path Parameters:**
- `id` (integer): Invoice ID

**Request Body:**
```json
{
  "status": "Paid"
}
```

**Valid Status Values:**
- `Pending`
- `Paid`
- `Overdue`
- `Cancelled`

**Success Response (200 OK):**
```json
{
  "message": "Invoice status updated successfully"
}
```

**Error Responses:**
- `404 Not Found`: Invoice doesn't exist
- `400 Bad Request`: Invalid status value
- `403 Forbidden`: User not authorized to update this invoice

---

### 2.5 Download Invoice PDF

**Endpoint:** `GET /api/invoices/{id}/download`

**Description:** Download invoice as a PDF file

**Authorization:** Bearer token required
- **Admins**: Can download any invoice
- **Users**: Can only download invoices assigned to them

**Path Parameters:**
- `id` (integer): Invoice ID

**Success Response (200 OK):**
- **Content-Type**: `application/pdf`
- **Content-Disposition**: `attachment; filename=Invoice_INV-20260209145321-1234.pdf`
- **Body**: Binary PDF data

**Error Responses:**
- `404 Not Found`: Invoice doesn't exist
- `403 Forbidden`: User not authorized to download this invoice

**PDF Features:**
- Professional layout with header
- Company branding area
- Invoice details (number, dates, status)
- Customer information
- Itemized table with quantities, unit prices, and totals
- Grand total calculation
- Page numbering
- Footer with notes

---

## 📬 Notification APIs

### 3.1 Get User Notifications

**Endpoint:** `GET /api/notification`

**Description:** Retrieve all notifications for the authenticated user

**Authorization:** Bearer token required

**Success Response (200 OK):**
```json
[
  {
    "id": 1,
    "message": "New invoice #INV-20260209145321-1234 has been assigned to you for Acme Corporation",
    "createdAt": "2025-02-09T10:30:45Z",
    "isRead": false,
    "userId": 2
  },
  {
    "id": 2,
    "message": "Invoice #INV-20260208142010-8765 status updated",
    "createdAt": "2025-02-08T14:20:10Z",
    "isRead": true,
    "userId": 2
  }
]
```

**Behavior:**
- Results ordered by `createdAt` descending (newest first)
- Only returns notifications for the authenticated user

---

### 3.2 Mark Notification as Read

**Endpoint:** `PUT /api/notification/{id}/read`

**Description:** Mark a specific notification as read

**Authorization:** Bearer token required

**Path Parameters:**
- `id` (integer): Notification ID

**Success Response (200 OK):**
```json
{
  "message": "Notification marked as read"
}
```

**Error Responses:**
- `404 Not Found`: Notification doesn't exist
- `403 Forbidden`: User not authorized (notification belongs to another user)

---

### 3.3 Delete Notification

**Endpoint:** `DELETE /api/notification/{id}`

**Description:** Delete a specific notification

**Authorization:** Bearer token required

**Path Parameters:**
- `id` (integer): Notification ID

**Success Response (200 OK):**
```json
{
  "message": "Notification deleted successfully"
}
```

**Error Responses:**
- `404 Not Found`: Notification doesn't exist
- `403 Forbidden`: User not authorized (notification belongs to another user)

---

## 👥 User APIs (Admin Only)

### 4.1 Get All Users

**Endpoint:** `GET /api/users`

**Description:** Retrieve all users with "User" role (excludes admins)

**Authorization:** Bearer token required (Admin role)

**Success Response (200 OK):**
```json
[
  {
    "id": 2,
    "username": "john_doe",
    "email": "john@example.com",
    "role": "User"
  },
  {
    "id": 3,
    "username": "jane_smith",
    "email": "jane@example.com",
    "role": "User"
  }
]
```

**Purpose:** Used by admins when assigning invoices to users

**Error Responses:**
- `401 Unauthorized`: Missing or invalid token
- `403 Forbidden`: User is not an admin

---

## 🔒 Authentication & Authorization

### JWT Token Structure

Access tokens are JSON Web Tokens (JWT) containing the following claims:

```json
{
  "nameid": "2",                    // User ID
  "unique_name": "john_doe",        // Username
  "email": "john@example.com",      // Email
  "role": "User",                   // User role
  "nbf": 1707561600,                // Not before
  "exp": 1707565200,                // Expiration (1 hour from issue)
  "iat": 1707561600,                // Issued at
  "iss": "InvoiceSystem",           // Issuer
  "aud": "InvoiceSystemUsers"       // Audience
}
```

### Token Lifetimes

| Token Type | Lifetime | Storage |
|------------|----------|---------|
| Access Token | 1 hour | Client memory/localStorage |
| Refresh Token | 7 days | Database + Client storage |

### Token Refresh Flow

```
1. Client → POST /api/auth/refresh-token (with refresh token)
2. Server validates refresh token
3. Server generates new access + refresh tokens
4. Server stores new refresh token in database
5. Server returns both tokens to client
6. Client updates stored tokens
```

### Role-Based Permissions

| Endpoint | Admin | User |
|----------|-------|------|
| `GET /api/invoices` | All invoices | Assigned only |
| `GET /api/invoices/{id}` | Any invoice | Assigned only |
| `POST /api/invoices` | ✅ | ❌ |
| `PUT /api/invoices/{id}/status` | ✅ | Assigned only |
| `GET /api/invoices/{id}/download` | Any invoice | Assigned only |
| `GET /api/users` | ✅ | ❌ |
| `GET /api/notification` | Own notifications | Own notifications |
| `PUT /api/notification/{id}/read` | Own notifications | Own notifications |
| `DELETE /api/notification/{id}` | Own notifications | Own notifications |

### Authorization Header Format

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## 🔔 Real-time Notifications

### SignalR Hub

The system uses **SignalR** for real-time push notifications.

**Hub Endpoint:**
```
wss://localhost:5001/notificationHub
```

### Authentication

SignalR connections require JWT authentication via query string:

```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://localhost:5001/notificationHub", {
    accessTokenFactory: () => localStorage.getItem("accessToken")
  })
  .build();
```

### Events

#### ReceiveNotification

**Description:** Pushed to client when a new notification is created

**Payload:**
```javascript
{
  message: "New invoice #INV-20260209145321-1234 has been assigned to you for Acme Corporation"
}
```

**Client-side Handler:**
```javascript
connection.on("ReceiveNotification", (message) => {
  console.log("Notification received:", message);
  // Update UI, show toast, etc.
});
```

### Notification Triggers

| Action | Recipient | Message |
|--------|-----------|---------|
| Admin creates invoice | Assigned user | "New invoice #{number} has been assigned to you for {customer}" |

### Connection Management

```javascript
// Start connection
await connection.start();
console.log("SignalR connected");

// Handle disconnection
connection.onclose(async () => {
  console.log("SignalR disconnected. Reconnecting...");
  await connection.start();
});

// Stop connection
await connection.stop();
```

---

## 📄 PDF Generation

### Technology

The system uses **QuestPDF** (Community License) for PDF generation.

### PDF Structure

```
┌────────────────────────────────────────────────┐
│                   INVOICE                      │
├────────────────────────────────────────────────┤
│                                                │
│  Invoice #: INV-20260209145321-1234            │
│  Issue Date: 09/02/2025                        │
│  Due Date: 09/03/2025                          │
│  Status: Pending                               │
│                                                │
│  Bill To:                                      │
│  Acme Corporation                              │
│                                                │
├────────────────────────────────────────────────┤
│  #  │ Description          │ Qty │ Unit Price │
├─────┼──────────────────────┼─────┼────────────┤
│  1  │ Web Development      │ 40  │   $50.00   │
│  2  │ Hosting Services     │  1  │  $500.00   │
├────────────────────────────────────────────────┤
│                                                │
│             Total Amount: $2,500.00            │
│                                                │
├────────────────────────────────────────────────┤
│  Notes:                                        │
│  Thank you for your business!                  │
│                                                │
└────────────────────────────────────────────────┘
```

### Features

- ✅ Professional A4 layout
- ✅ Header with "INVOICE" title
- ✅ Invoice metadata (number, dates, status)
- ✅ Customer information section
- ✅ Itemized table with columns:
  - Item number
  - Description
  - Quantity
  - Unit price
  - Line total
- ✅ Grand total with currency formatting
- ✅ Notes section
- ✅ Page numbering in footer

### Customization

PDF generation logic is in `Services/PdfService.cs`. Modify this file to:
- Change layout/styling
- Add company logo
- Include payment terms
- Add custom footer text
- Change currency format

---

## ⚠️ Error Handling

### Standard Error Response Format

```json
{
  "message": "Error description here"
}
```

### Validation Error Response Format

```json
{
  "message": "Invalid input data",
  "errors": [
    "The CustomerName field is required.",
    "The DueDate field must be in the future."
  ]
}
```

### HTTP Status Codes

| Code | Meaning | When Used |
|------|---------|-----------|
| 200 | OK | Successful GET/PUT request |
| 201 | Created | Successful POST (resource created) |
| 204 | No Content | Successful DELETE |
| 400 | Bad Request | Invalid input data, validation errors |
| 401 | Unauthorized | Missing, invalid, or expired token |
| 403 | Forbidden | User lacks permission for action |
| 404 | Not Found | Resource doesn't exist |
| 500 | Internal Server Error | Unexpected server-side error |

### Common Error Scenarios

#### Invalid Token
```json
{
  "message": "Unauthorized"
}
```

#### Validation Errors
```json
{
  "message": "Invalid input data",
  "errors": [
    "The CustomerName field is required.",
    "Item quantity must be greater than zero"
  ]
}
```

#### Permission Denied
```json
{
  "message": "Access denied"
}
```

#### Resource Not Found
```json
{
  "message": "Invoice not found"
}
```

---

## 🔐 Security

### Implemented Security Features

1. **Password Hashing**
   - Algorithm: BCrypt
   - Automatically salted
   - One-way encryption (passwords cannot be decrypted)

2. **JWT Authentication**
   - Industry-standard token format
   - Signed with HMAC SHA256
   - Contains only non-sensitive user data
   - Short expiration (1 hour)

3. **Refresh Tokens**
   - Long-lived (7 days)
   - Stored in database
   - Can be revoked
   - Rotated on each refresh

4. **Role-Based Authorization**
   - Attribute-based access control (`[Authorize(Roles = "Admin")]`)
   - Claims-based authorization
   - Granular permission checks in controllers

5. **CORS Policy**
   - Restricted to specific origin (`http://localhost:4200`)
   - Credentials allowed for SignalR
   - Controlled headers and methods

6. **Input Validation**
   - Data annotations on DTOs
   - Model state validation
   - Additional business rule validation

7. **SQL Injection Protection**
   - Entity Framework Core parameterized queries
   - No raw SQL execution

8. **HTTPS Enforcement**
   - Enabled in production
   - HTTP to HTTPS redirection

### Security Best Practices

✅ **DO:**
- Store JWT secret in environment variables/configuration
- Use HTTPS in production
- Validate all user inputs
- Log security events (failed logins, unauthorized access)
- Implement rate limiting for authentication endpoints
- Use strong password requirements
- Rotate refresh tokens

❌ **DON'T:**
- Store passwords in plain text
- Hardcode secrets in code
- Trust client-side data without validation
- Expose sensitive data in error messages
- Use weak JWT secrets

### Recommended Enhancements

- [ ] Implement account lockout after failed login attempts
- [ ] Add email verification for new accounts
- [ ] Implement password reset functionality
- [ ] Add two-factor authentication (2FA)
- [ ] Implement rate limiting middleware
- [ ] Add audit logging for sensitive operations
- [ ] Implement password complexity requirements
- [ ] Add CSRF protection for non-API endpoints
- [ ] Implement IP whitelisting for admin endpoints

---

## 🧪 Testing

### Manual Testing with Swagger

1. Start the application
2. Navigate to `https://localhost:5001/swagger`
3. Click "Authorize" button
4. Enter JWT token: `Bearer <your-token>`
5. Test endpoints

### Manual Testing with Postman

**Collection Setup:**

1. **Environment Variables:**
   ```
   baseUrl: https://localhost:5001
   accessToken: <obtained-from-login>
   ```

2. **Authorization:**
   - Type: Bearer Token
   - Token: `{{accessToken}}`

**Example Test Flow:**

```http
# 1. Register Admin
POST {{baseUrl}}/api/auth/register
Content-Type: application/json

{
  "username": "testadmin",
  "email": "testadmin@test.com",
  "password": "Admin123!",
  "userType": "Admin"
}

# 2. Login as Admin (save accessToken)
POST {{baseUrl}}/api/auth/login
Content-Type: application/json

{
  "username": "testadmin",
  "password": "Admin123!",
  "userType": "Admin"
}

# 3. Get All Users
GET {{baseUrl}}/api/users
Authorization: Bearer {{accessToken}}

# 4. Create Invoice
POST {{baseUrl}}/api/invoices
Authorization: Bearer {{accessToken}}
Content-Type: application/json

{
  "customerName": "Test Company",
  "dueDate": "2025-03-15T10:00:00Z",
  "assignedUserId": 2,
  "items": [
    {
      "description": "Consulting Services",
      "quantity": 10,
      "unitPrice": 100.00
    }
  ]
}

# 5. Get All Invoices
GET {{baseUrl}}/api/invoices
Authorization: Bearer {{accessToken}}

# 6. Download Invoice PDF
GET {{baseUrl}}/api/invoices/1/download
Authorization: Bearer {{accessToken}}
```

### Testing SignalR Connection

**JavaScript Example:**

```javascript
const signalR = require("@microsoft/signalr");

const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://localhost:5001/notificationHub", {
    accessTokenFactory: () => "your-jwt-token-here"
  })
  .configureLogging(signalR.LogLevel.Information)
  .build();

connection.on("ReceiveNotification", (message) => {
  console.log("Notification:", message);
});

connection.start()
  .then(() => console.log("SignalR Connected"))
  .catch(err => console.error("SignalR Error:", err));
```

---

## 🚀 Deployment

### Prerequisites

- SQL Server database (Azure SQL, AWS RDS, or on-premises)
- .NET 8.0 Runtime
- IIS, Azure App Service, or Docker

### Configuration for Production

1. **Update `appsettings.Production.json`:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-server;Database=InvoiceSystemDB;User Id=prod_user;Password=STRONG_PASSWORD;TrustServerCertificate=False;Encrypt=True;"
  },
  "Jwt": {
    "Key": "GENERATE_STRONG_SECRET_KEY_HERE_AT_LEAST_32_CHARACTERS",
    "Issuer": "YourCompany.InvoiceSystem",
    "Audience": "InvoiceSystemClients",
    "ExpireDays": 7
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "AllowedOrigins": ["https://your-frontend-domain.com"]
}
```

2. **Generate Strong JWT Secret:**

```bash
# PowerShell
$bytes = New-Object byte[] 64
(New-Object Security.Cryptography.RNGCryptoServiceProvider).GetBytes($bytes)
[Convert]::ToBase64String($bytes)
```

3. **Apply Migrations:**

```bash
dotnet ef database update --connection "YourProductionConnectionString"
```

4. **Publish Application:**

```bash
dotnet publish -c Release -o ./publish
```

### Docker Deployment

**Dockerfile:**

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["InvoiceSystem.csproj", "./"]
RUN dotnet restore "InvoiceSystem.csproj"
COPY . .
RUN dotnet build "InvoiceSystem.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InvoiceSystem.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InvoiceSystem.dll"]
```

**Build and Run:**

```bash
docker build -t invoice-system-api .
docker run -d -p 8080:80 -e "ConnectionStrings__DefaultConnection=YOUR_CONNECTION_STRING" invoice-system-api
```

---

## 🛠 Troubleshooting

### Common Issues

#### 1. Database Connection Failed

**Error:** `A network-related or instance-specific error occurred while establishing a connection to SQL Server`

**Solutions:**
- Verify SQL Server is running
- Check connection string in `appsettings.json`
- Ensure firewall allows SQL Server port (1433)
- For LocalDB, ensure LocalDB is installed
- Test connection with SQL Server Management Studio

#### 2. Migration Errors

**Error:** `Unable to create an object of type 'ApplicationDbContext'`

**Solution:**
```bash
# Ensure you're in the project directory
cd InvoiceSystem

# Install EF Core tools if not installed
dotnet tool install --global dotnet-ef

# Add migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

#### 3. JWT Authentication Fails

**Error:** `401 Unauthorized` when accessing protected endpoints

**Solutions:**
- Ensure JWT token is included in Authorization header
- Check token format: `Bearer <token>`
- Verify token hasn't expired (check `exp` claim)
- Ensure JWT secret in `appsettings.json` matches what was used to generate token
- Check `Issuer` and `Audience` configuration

#### 4. SignalR Connection Fails

**Error:** `Failed to connect to SignalR hub`

**Solutions:**
- Ensure CORS is configured correctly
- Check WebSocket support is enabled
- Verify JWT token is passed in connection options
- Check browser console for detailed error messages
- Ensure SignalR endpoint path is correct (`/notificationHub`)

#### 5. PDF Generation Fails

**Error:** `QuestPDF license not configured`

**Solution:**
```csharp
// Add to Program.cs
QuestPDF.Settings.License = LicenseType.Community;
```

#### 6. CORS Errors

**Error:** `Access to XMLHttpRequest has been blocked by CORS policy`

**Solutions:**
- Update `AllowedOrigins` in Program.cs
- Ensure frontend URL matches exactly (including protocol and port)
- For development, use `SetIsOriginAllowed((host) => true)`
- Restart the API after changes

---

## 📝 API Testing Examples

### cURL Examples

```bash
# Register User
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","email":"test@test.com","password":"Test123!","userType":"User"}'

# Login
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","password":"Test123!","userType":"User"}'

# Get Invoices (with token)
curl -X GET https://localhost:5001/api/invoices \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"

# Create Invoice (Admin)
curl -X POST https://localhost:5001/api/invoices \
  -H "Authorization: Bearer ADMIN_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"customerName":"ACME Corp","dueDate":"2025-03-15T00:00:00Z","assignedUserId":2,"items":[{"description":"Service","quantity":1,"unitPrice":100}]}'

# Download Invoice PDF
curl -X GET https://localhost:5001/api/invoices/1/download \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -o invoice.pdf
```

---

## 📋 Environment Variables

### Required Environment Variables (Production)

```bash
# Database
ConnectionStrings__DefaultConnection="Server=...;Database=...;User Id=...;Password=..."

# JWT
Jwt__Key="YOUR_STRONG_SECRET_KEY_AT_LEAST_32_CHARACTERS"
Jwt__Issuer="YourCompany.InvoiceSystem"
Jwt__Audience="InvoiceSystemClients"

# Logging
ASPNETCORE_ENVIRONMENT="Production"
Logging__LogLevel__Default="Warning"

# CORS (comma-separated)
AllowedOrigins="https://your-frontend.com,https://www.your-frontend.com"
```

---

## 🔄 Database Migrations

### Creating Migrations

```bash
# Add new migration
dotnet ef migrations add MigrationName

# View migration SQL
dotnet ef migrations script

# Update database
dotnet ef database update

# Rollback to specific migration
dotnet ef database update PreviousMigrationName

# Remove last migration (if not applied)
dotnet ef migrations remove
```

### Migration History

1. `InitialCreate` - Initial database schema
2. `UpdateInvoiceRelationships` - Updated invoice relationships
3. `InitialDatabaseCorrection` - Corrected database structure
4. `MakeCreatedByAdminIdNull` - Made CreatedByAdminId nullable

---

## 📊 Performance Considerations

### Database Optimization

- **Indexes:** EF Core creates indexes on foreign keys automatically
- **Eager Loading:** Use `.Include()` to prevent N+1 queries
- **Pagination:** Consider adding pagination for large result sets
- **Connection Pooling:** Enabled by default in EF Core

### Recommended Indexes

```sql
-- Speed up invoice queries by status
CREATE INDEX IX_Invoices_Status ON Invoices(Status);

-- Speed up user invoice queries
CREATE INDEX IX_Invoices_AssignedUserId ON Invoices(AssignedUserId);

-- Speed up notification queries
CREATE INDEX IX_Notifications_UserId_IsRead ON Notifications(UserId, IsRead);
```

### Caching Strategies

Consider implementing caching for:
- User list (rarely changes)
- Invoice statistics (compute periodically)
- PDF templates

---

## 📚 Additional Resources

### Official Documentation
- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [JWT Authentication](https://jwt.io/introduction)
- [SignalR](https://docs.microsoft.com/en-us/aspnet/core/signalr/introduction)
- [QuestPDF](https://www.questpdf.com/)

### Related Packages
- [AutoMapper](https://automapper.org/)
- [BCrypt.Net](https://github.com/BcryptNet/bcrypt.net)
- [Swagger/OpenAPI](https://swagger.io/)

---

## 📄 License

This project is licensed under the MIT License.

---

## 👥 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the project
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📞 Support

For issues, questions, or contributions:
- Create an issue on GitHub
- Contact the development team
- Check the documentation

---

## 🙏 Acknowledgments

- **Microsoft** - ASP.NET Core framework
- **QuestPDF** - PDF generation library
- **SignalR Team** - Real-time communication
- **AutoMapper Contributors** - Object mapping
- **BCrypt.Net Team** - Password hashing

---

**Project Version:** 1.0.0  
**Last Updated:** February 2025  
**Framework:** ASP.NET Core 8.0  
**Database:** SQL Server
