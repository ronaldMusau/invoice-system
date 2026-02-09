# Invoice Management System

A full-featured invoice management system built with ASP.NET Core Web API, featuring role-based access control, real-time notifications, and PDF generation capabilities.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [API Documentation](#api-documentation)
- [Database Schema](#database-schema)
- [Authentication & Authorization](#authentication--authorization)
- [Real-time Notifications](#real-time-notifications)
- [PDF Generation](#pdf-generation)

---

## Overview

This Invoice Management System is designed to streamline invoice creation, management, and tracking. It supports two user roles: **Admin** and **User**. Admins can create and assign invoices to users, while users can view their assigned invoices, update payment statuses, and download PDF copies.

### Key Capabilities

- **User Registration & Authentication**: Secure JWT-based authentication
- **Role-Based Access Control**: Admin and User roles with different permissions
- **Invoice Management**: Create, read, update, and delete invoices
- **Real-time Notifications**: SignalR-powered instant notifications
- **PDF Generation**: Generate professional invoice PDFs
- **Payment Tracking**: Track payment status and history

---

## Features

### For Admins
- ✅ Create and assign invoices to users
- ✅ View all invoices across the system
- ✅ Update invoice details and status
- ✅ Delete invoices
- ✅ Monitor payment statuses
- ✅ Access analytics and statistics

### For Users
- ✅ View assigned invoices
- ✅ Update payment status (mark as paid)
- ✅ Download invoice PDFs
- ✅ Receive real-time notifications for new invoices
- ✅ View payment history
- ✅ Track pending invoices

---

## Technology Stack

### Backend
- **Framework**: ASP.NET Core Web API (.NET 10.0)
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: JWT (JSON Web Tokens)
- **Real-time Communication**: SignalR
- **PDF Generation**: QuestPDF
- **Object Mapping**: AutoMapper
- **Password Hashing**: BCrypt.Net

### Frontend (Designed for)
- **Framework**: Angular
- **Port**: http://localhost:4200

---

## Architecture

### Project Structure

```
InvoiceSystem/
├── Controllers/
│   ├── AuthController.cs          # Authentication endpoints
│   ├── InvoicesController.cs      # Invoice CRUD operations
│   ├── NotificationsController.cs # Notification management
│   ├── PaymentsController.cs      # Payment tracking
│   └── StatisticsController.cs    # Analytics and reports
├── Services/
│   ├── AuthService.cs             # Authentication logic
│   └── PdfService.cs              # PDF generation
├── Hubs/
│   └── NotificationHub.cs         # SignalR hub for real-time notifications
├── Data/
│   └── ApplicationDbContext.cs    # EF Core database context
├── Models/                        # Database entities
├── DTOs/                          # Data Transfer Objects
└── Program.cs                     # Application configuration
```

### Design Patterns
- **Repository Pattern**: Via Entity Framework Core DbContext
- **Dependency Injection**: ASP.NET Core DI container
- **DTO Pattern**: Separate data transfer objects from entities
- **Service Layer**: Business logic separated from controllers

---

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later
- SQL Server (LocalDB, Express, or Full)
- Visual Studio 2022 or VS Code
- Angular CLI (for frontend)

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd InvoiceSystem
   ```

2. **Configure Database Connection**
   
   Update `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=InvoiceSystemDb;Trusted_Connection=true;MultipleActiveResultSets=true"
     },
     "Jwt": {
       "Key": "hxnqaaHtwP3h2XxAoSQs+A2yxlA5Kgx7rZGvqLbWL/U=",
       "Issuer": "InvoiceSystemAPI",
       "Audience": "InvoiceSystemClient"
     }
   }
   ```

3. **Apply Database Migrations**
   ```bash
   dotnet ef database update
   ```

4. **Run the Application**
   ```bash
   dotnet run
   ```

5. **Access Swagger Documentation**
   
   Navigate to: `https://localhost:5001/swagger`

---

## API Documentation

### Base URL
```
https://localhost:5001/api
```

All endpoints (except authentication) require a valid JWT token in the Authorization header:
```
Authorization: Bearer <your-jwt-token>
```

---

## 1. Authentication APIs

### 1.1 Register User

**Endpoint**: `POST /api/auth/register`

**Description**: Register a new user account (default role: User)

**Request Body**:
```json
{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "SecurePass123!"
}
```

**Response** (200 OK):
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "john_doe",
  "role": "User"
}
```

**Error Responses**:
- `400 Bad Request`: Username or email already exists
- `400 Bad Request`: Invalid input data

---

### 1.2 Login

**Endpoint**: `POST /api/auth/login`

**Description**: Authenticate user and receive JWT token

**Request Body**:
```json
{
  "username": "john_doe",
  "password": "SecurePass123!"
}
```

**Response** (200 OK):
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Error Responses**:
- `401 Unauthorized`: Invalid username or password

---

### 1.3 Register Admin

**Endpoint**: `POST /api/auth/register-admin`

**Description**: Register a new admin account (role: Admin)

**Request Body**:
```json
{
  "username": "admin_user",
  "email": "admin@example.com",
  "password": "AdminPass123!"
}
```

**Response** (200 OK):
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "admin_user",
  "role": "Admin"
}
```

---

## 2. Invoice APIs

### 2.1 Get All Invoices

**Endpoint**: `GET /api/invoices`

**Description**: 
- **Admins**: Retrieve all invoices in the system
- **Users**: Retrieve only invoices assigned to them

**Headers**:
```
Authorization: Bearer <token>
```

**Response** (200 OK):
```json
[
  {
    "id": 1,
    "invoiceNumber": "INV-20250209-001",
    "customerName": "Acme Corporation",
    "issueDate": "2025-02-09T10:30:00Z",
    "dueDate": "2025-03-09T10:30:00Z",
    "totalAmount": 1500.00,
    "status": "Pending",
    "assignedUserId": 2,
    "assignedUserName": "john_doe",
    "items": [
      {
        "id": 1,
        "description": "Web Development Services",
        "quantity": 10,
        "unitPrice": 100.00,
        "totalPrice": 1000.00
      },
      {
        "id": 2,
        "description": "Hosting Services",
        "quantity": 5,
        "unitPrice": 100.00,
        "totalPrice": 500.00
      }
    ]
  }
]
```

---

### 2.2 Get Invoice by ID

**Endpoint**: `GET /api/invoices/{id}`

**Description**: Retrieve a specific invoice by ID

**Authorization**:
- **Admins**: Can view any invoice
- **Users**: Can only view invoices assigned to them

**Response** (200 OK):
```json
{
  "id": 1,
  "invoiceNumber": "INV-20250209-001",
  "customerName": "Acme Corporation",
  "issueDate": "2025-02-09T10:30:00Z",
  "dueDate": "2025-03-09T10:30:00Z",
  "totalAmount": 1500.00,
  "status": "Pending",
  "assignedUserId": 2,
  "assignedUserName": "john_doe",
  "items": [...]
}
```

**Error Responses**:
- `404 Not Found`: Invoice doesn't exist
- `403 Forbidden`: User not authorized to view this invoice

---

### 2.3 Create Invoice

**Endpoint**: `POST /api/invoices`

**Description**: Create a new invoice (Admin only)

**Required Role**: Admin

**Request Body**:
```json
{
  "customerName": "Acme Corporation",
  "dueDate": "2025-03-09T10:30:00Z",
  "assignedUserId": 2,
  "items": [
    {
      "description": "Web Development Services",
      "quantity": 10,
      "unitPrice": 100.00
    },
    {
      "description": "Hosting Services",
      "quantity": 5,
      "unitPrice": 100.00
    }
  ]
}
```

**Response** (201 Created):
```json
{
  "id": 1,
  "invoiceNumber": "INV-20250209-001",
  "customerName": "Acme Corporation",
  "issueDate": "2025-02-09T10:30:00Z",
  "dueDate": "2025-03-09T10:30:00Z",
  "totalAmount": 1500.00,
  "status": "Pending",
  "assignedUserId": 2,
  "items": [...]
}
```

**Behavior**:
- Auto-generates invoice number (format: INV-YYYYMMDD-XXX)
- Automatically calculates total amount
- Sets initial status to "Pending"
- Creates a notification for the assigned user
- Sends real-time notification via SignalR

**Error Responses**:
- `400 Bad Request`: Assigned user not found
- `401 Unauthorized`: Missing or invalid token
- `403 Forbidden`: User is not an admin

---

### 2.4 Update Invoice

**Endpoint**: `PUT /api/invoices/{id}`

**Description**: Update an existing invoice (Admin only)

**Required Role**: Admin

**Request Body**:
```json
{
  "customerName": "Acme Corporation Updated",
  "dueDate": "2025-03-15T10:30:00Z",
  "status": "Paid",
  "assignedUserId": 2,
  "items": [
    {
      "description": "Web Development Services",
      "quantity": 15,
      "unitPrice": 100.00
    }
  ]
}
```

**Response** (204 No Content)

**Error Responses**:
- `404 Not Found`: Invoice doesn't exist
- `400 Bad Request`: Invalid data

---

### 2.5 Delete Invoice

**Endpoint**: `DELETE /api/invoices/{id}`

**Description**: Delete an invoice (Admin only)

**Required Role**: Admin

**Response** (204 No Content)

**Error Responses**:
- `404 Not Found`: Invoice doesn't exist

---

### 2.6 Download Invoice PDF

**Endpoint**: `GET /api/invoices/{id}/pdf`

**Description**: Download invoice as PDF

**Authorization**:
- **Admins**: Can download any invoice
- **Users**: Can only download invoices assigned to them

**Response**: Binary PDF file

**Headers**:
```
Content-Type: application/pdf
Content-Disposition: attachment; filename=Invoice-INV-20250209-001.pdf
```

**Error Responses**:
- `404 Not Found`: Invoice doesn't exist
- `403 Forbidden`: User not authorized to download this invoice

---

## 3. Payment APIs

### 3.1 Get All Payments

**Endpoint**: `GET /api/payments`

**Description**: 
- **Admins**: Get all payments in the system
- **Users**: Get only their payments

**Response** (200 OK):
```json
[
  {
    "id": 1,
    "invoiceId": 1,
    "invoiceNumber": "INV-20250209-001",
    "amount": 1500.00,
    "paymentDate": "2025-02-15T14:30:00Z",
    "paymentMethod": "Bank Transfer",
    "transactionId": "TXN-123456",
    "notes": "Payment received via wire transfer"
  }
]
```

---

### 3.2 Record Payment

**Endpoint**: `POST /api/payments`

**Description**: Record a payment for an invoice

**Request Body**:
```json
{
  "invoiceId": 1,
  "amount": 1500.00,
  "paymentDate": "2025-02-15T14:30:00Z",
  "paymentMethod": "Bank Transfer",
  "transactionId": "TXN-123456",
  "notes": "Payment received via wire transfer"
}
```

**Response** (201 Created):
```json
{
  "id": 1,
  "invoiceId": 1,
  "amount": 1500.00,
  "paymentDate": "2025-02-15T14:30:00Z",
  "paymentMethod": "Bank Transfer",
  "transactionId": "TXN-123456"
}
```

**Behavior**:
- Automatically updates invoice status to "Paid" if full amount is paid
- Creates a notification for the admin who created the invoice
- Sends real-time notification via SignalR

**Error Responses**:
- `404 Not Found`: Invoice doesn't exist
- `400 Bad Request`: Payment amount exceeds invoice total
- `403 Forbidden`: User not authorized to record payment for this invoice

---

### 3.3 Get Invoice Payment History

**Endpoint**: `GET /api/payments/invoice/{invoiceId}`

**Description**: Get all payments for a specific invoice

**Response** (200 OK):
```json
[
  {
    "id": 1,
    "amount": 750.00,
    "paymentDate": "2025-02-10T14:30:00Z",
    "paymentMethod": "Credit Card",
    "transactionId": "TXN-111111"
  },
  {
    "id": 2,
    "amount": 750.00,
    "paymentDate": "2025-02-15T14:30:00Z",
    "paymentMethod": "Bank Transfer",
    "transactionId": "TXN-222222"
  }
]
```

---

## 4. Notification APIs

### 4.1 Get User Notifications

**Endpoint**: `GET /api/notifications`

**Description**: Get all notifications for the current user

**Response** (200 OK):
```json
[
  {
    "id": 1,
    "message": "New invoice #INV-20250209-001 has been assigned to you.",
    "createdAt": "2025-02-09T10:30:00Z",
    "isRead": false
  },
  {
    "id": 2,
    "message": "Invoice #INV-20250208-005 has been marked as paid.",
    "createdAt": "2025-02-08T15:45:00Z",
    "isRead": true
  }
]
```

---

### 4.2 Mark Notification as Read

**Endpoint**: `PUT /api/notifications/{id}/read`

**Description**: Mark a specific notification as read

**Response** (204 No Content)

**Error Responses**:
- `404 Not Found`: Notification doesn't exist
- `403 Forbidden`: User not authorized to modify this notification

---

### 4.3 Mark All Notifications as Read

**Endpoint**: `PUT /api/notifications/mark-all-read`

**Description**: Mark all user's notifications as read

**Response** (204 No Content)

---

### 4.4 Get Unread Count

**Endpoint**: `GET /api/notifications/unread-count`

**Description**: Get count of unread notifications

**Response** (200 OK):
```json
{
  "count": 5
}
```

---

## 5. Statistics APIs (Admin Only)

### 5.1 Get Dashboard Statistics

**Endpoint**: `GET /api/statistics/dashboard`

**Description**: Get overall statistics for admin dashboard

**Required Role**: Admin

**Response** (200 OK):
```json
{
  "totalInvoices": 150,
  "pendingInvoices": 45,
  "paidInvoices": 95,
  "overdueInvoices": 10,
  "totalRevenue": 125000.00,
  "pendingRevenue": 35000.00,
  "collectedRevenue": 90000.00
}
```

---

### 5.2 Get Revenue by Period

**Endpoint**: `GET /api/statistics/revenue`

**Description**: Get revenue statistics by time period

**Query Parameters**:
- `startDate` (required): Start date (YYYY-MM-DD)
- `endDate` (required): End date (YYYY-MM-DD)

**Example**: `GET /api/statistics/revenue?startDate=2025-01-01&endDate=2025-02-09`

**Response** (200 OK):
```json
{
  "period": {
    "startDate": "2025-01-01",
    "endDate": "2025-02-09"
  },
  "totalRevenue": 45000.00,
  "invoiceCount": 30,
  "averageInvoiceAmount": 1500.00,
  "byStatus": {
    "paid": 35000.00,
    "pending": 10000.00,
    "overdue": 0.00
  }
}
```

---

### 5.3 Get User Performance

**Endpoint**: `GET /api/statistics/user-performance`

**Description**: Get performance statistics for all users

**Response** (200 OK):
```json
[
  {
    "userId": 2,
    "username": "john_doe",
    "assignedInvoices": 25,
    "paidInvoices": 20,
    "pendingInvoices": 5,
    "totalAmount": 37500.00,
    "collectedAmount": 30000.00,
    "paymentRate": 80.0
  }
]
```

---

## Database Schema

### Users Table
```sql
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY,
    Username NVARCHAR(50) UNIQUE NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    Role NVARCHAR(20) NOT NULL DEFAULT 'User',
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
)
```

### Invoices Table
```sql
CREATE TABLE Invoices (
    Id INT PRIMARY KEY IDENTITY,
    InvoiceNumber NVARCHAR(50) UNIQUE NOT NULL,
    CustomerName NVARCHAR(200) NOT NULL,
    IssueDate DATETIME2 NOT NULL,
    DueDate DATETIME2 NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(20) NOT NULL,
    AssignedUserId INT NOT NULL,
    CreatedByAdminId INT NOT NULL,
    FOREIGN KEY (AssignedUserId) REFERENCES Users(Id),
    FOREIGN KEY (CreatedByAdminId) REFERENCES Users(Id)
)
```

### InvoiceItems Table
```sql
CREATE TABLE InvoiceItems (
    Id INT PRIMARY KEY IDENTITY,
    InvoiceId INT NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    TotalPrice DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE CASCADE
)
```

### Payments Table
```sql
CREATE TABLE Payments (
    Id INT PRIMARY KEY IDENTITY,
    InvoiceId INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    PaymentDate DATETIME2 NOT NULL,
    PaymentMethod NVARCHAR(50),
    TransactionId NVARCHAR(100),
    Notes NVARCHAR(MAX),
    RecordedByUserId INT NOT NULL,
    FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id),
    FOREIGN KEY (RecordedByUserId) REFERENCES Users(Id)
)
```

### Notifications Table
```sql
CREATE TABLE Notifications (
    Id INT PRIMARY KEY IDENTITY,
    Message NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    UserId INT NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
)
```

---

## Authentication & Authorization

### JWT Token Structure

The system uses JWT tokens for authentication. Each token contains:

```json
{
  "sub": "2",                    // User ID
  "unique_name": "john_doe",     // Username
  "role": "User",                // User role
  "nbf": 1707475200,             // Not before timestamp
  "exp": 1707561600,             // Expiration timestamp (24 hours)
  "iat": 1707475200              // Issued at timestamp
}
```

### Token Lifetime
- **Duration**: 24 hours
- **Refresh**: Not implemented (user must re-login after expiration)

### Role-Based Permissions

| Action | Admin | User |
|--------|-------|------|
| View all invoices | ✅ | ❌ (only assigned) |
| View specific invoice | ✅ | ✅ (if assigned) |
| Create invoice | ✅ | ❌ |
| Update invoice | ✅ | ❌ |
| Delete invoice | ✅ | ❌ |
| Download PDF | ✅ | ✅ (if assigned) |
| Record payment | ✅ | ✅ (if assigned) |
| View statistics | ✅ | ❌ |

---

## Real-time Notifications

The system uses **SignalR** for real-time notifications.

### SignalR Hub Endpoint
```
wss://localhost:5001/notificationHub
```

### Connection (JavaScript Example)
```javascript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://localhost:5001/notificationHub", {
    accessTokenFactory: () => localStorage.getItem("jwt-token")
  })
  .build();

// Start connection
await connection.start();

// Listen for notifications
connection.on("ReceiveNotification", (message) => {
  console.log("New notification:", message);
  // Update UI with notification
});
```

### Events

#### ReceiveNotification
Triggered when a new notification is created for the user.

**Payload**:
```json
{
  "message": "New invoice #INV-20250209-001 has been assigned to you.",
  "createdAt": "2025-02-09T10:30:00Z"
}
```

**Triggers**:
- When an admin creates a new invoice (sent to assigned user)
- When a user records a payment (sent to admin who created the invoice)

---

## PDF Generation

The system uses **QuestPDF** to generate professional invoice PDFs.

### PDF Features
- ✅ Professional layout with header and footer
- ✅ Company branding area
- ✅ Invoice details (number, dates, status)
- ✅ Customer information
- ✅ Itemized table with quantities and prices
- ✅ Total amount calculation
- ✅ Page numbering
- ✅ Notes section

### Generated PDF Structure

```
┌─────────────────────────────────────┐
│            INVOICE                  │
├─────────────────────────────────────┤
│ Invoice #: INV-20250209-001         │
│ Issue Date: 09/02/2025              │
│ Due Date: 09/03/2025                │
│ Status: Pending                     │
│                                     │
│ Bill To:                            │
│ Acme Corporation                    │
├─────────────────────────────────────┤
│ # │ Description    │ Qty │ Price   │
├───┼────────────────┼─────┼─────────┤
│ 1 │ Web Dev        │ 10  │ $100.00 │
│ 2 │ Hosting        │ 5   │ $100.00 │
├─────────────────────────────────────┤
│         Total Amount: $1,500.00     │
├─────────────────────────────────────┤
│ Notes:                              │
│ Thank you for your business!        │
└─────────────────────────────────────┘
```

---

## Error Handling

### Standard Error Response Format

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "errors": {
    "CustomerName": [
      "The CustomerName field is required."
    ]
  }
}
```

### Common HTTP Status Codes

| Code | Meaning | When Used |
|------|---------|-----------|
| 200 | OK | Successful GET request |
| 201 | Created | Successful POST (resource created) |
| 204 | No Content | Successful PUT/DELETE |
| 400 | Bad Request | Invalid input data |
| 401 | Unauthorized | Missing or invalid token |
| 403 | Forbidden | User lacks permission |
| 404 | Not Found | Resource doesn't exist |
| 500 | Internal Server Error | Server-side error |

---

## CORS Configuration

The API is configured to accept requests from the Angular frontend.

**Allowed Origin**: `http://localhost:4200`

**Allowed Methods**: GET, POST, PUT, DELETE

**Allowed Headers**: All

**Credentials**: Enabled (for SignalR)

---

## Testing the API

### Using Swagger UI

1. Start the application
2. Navigate to `https://localhost:5001/swagger`
3. Use the "Authorize" button to add your JWT token
4. Test endpoints directly from the browser

### Using Postman

1. **Register/Login** to get a JWT token
2. Add token to Authorization header:
   ```
   Authorization: Bearer <your-token>
   ```
3. Make requests to various endpoints

### Example Workflow

```bash
# 1. Register admin
POST /api/auth/register-admin
Body: {"username": "admin", "email": "admin@test.com", "password": "Admin123!"}

# 2. Register user
POST /api/auth/register
Body: {"username": "user1", "email": "user1@test.com", "password": "User123!"}

# 3. Login as admin
POST /api/auth/login
Body: {"username": "admin", "password": "Admin123!"}
Response: {"token": "eyJ..."}

# 4. Create invoice (use admin token)
POST /api/invoices
Headers: Authorization: Bearer eyJ...
Body: {
  "customerName": "Test Company",
  "dueDate": "2025-03-09",
  "assignedUserId": 2,
  "items": [{"description": "Service", "quantity": 1, "unitPrice": 100}]
}

# 5. Login as user
POST /api/auth/login
Body: {"username": "user1", "password": "User123!"}

# 6. View assigned invoices (use user token)
GET /api/invoices
Headers: Authorization: Bearer eyJ...

# 7. Download invoice PDF
GET /api/invoices/1/pdf
Headers: Authorization: Bearer eyJ...
```

---

## Security Considerations

### Implemented Security Features

1. **Password Hashing**: Using BCrypt with salt rounds
2. **JWT Authentication**: Secure token-based authentication
3. **Role-Based Authorization**: Attribute-based access control
4. **HTTPS**: Enforced in production
5. **CORS**: Restricted to specific origin
6. **Input Validation**: DTOs with data annotations
7. **SQL Injection Protection**: Entity Framework parameterized queries

### Recommended Enhancements

- [ ] Implement refresh tokens
- [ ] Add rate limiting
- [ ] Implement account lockout after failed login attempts
- [ ] Add email verification for new accounts
- [ ] Implement password reset functionality
- [ ] Add audit logging for sensitive operations
- [ ] Implement two-factor authentication (2FA)

---

## Future Enhancements

- [ ] Email notifications for invoice creation and payment
- [ ] Recurring invoices
- [ ] Multi-currency support
- [ ] Invoice templates customization
- [ ] Bulk operations (create/update multiple invoices)
- [ ] Export to Excel functionality
- [ ] Advanced reporting and analytics
- [ ] Invoice reminders for overdue payments
- [ ] Client portal for customers to view invoices
- [ ] Integration with payment gateways

---

## Troubleshooting

### Common Issues

**Issue**: "Cannot connect to SQL Server"
- **Solution**: Ensure SQL Server is running and connection string is correct

**Issue**: "Unauthorized" when accessing endpoints
- **Solution**: Ensure JWT token is included in Authorization header

**Issue**: "SignalR connection failed"
- **Solution**: Check CORS settings and ensure WebSocket support is enabled

**Issue**: "PDF generation fails"
- **Solution**: Ensure QuestPDF license is properly configured

---

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## License

This project is licensed under the MIT License.

---

## Support

For issues, questions, or contributions, please contact the development team or create an issue in the repository.

---

## Acknowledgments

- **ASP.NET Core Team** - For the excellent framework
- **QuestPDF** - For the PDF generation library
- **SignalR** - For real-time communication capabilities
- **AutoMapper** - For object mapping
- **BCrypt.Net** - For secure password hashing

---

**Version**: 1.0.0  
**Last Updated**: February 2025  
**Developed with**: ASP.NET Core 10.0