# Sales Management System

A .NET 9.0 API for managing products, sales orders, and user authentication with JWT. Built with Clean Architecture, Repository/Unit of Work patterns, and integrated logging (Serilog).

## Project Structure
SalesManagementSystem
├── SalesManagementSystem.API       # Main API (Controllers, Services)
├── SalesManagementSystem.Core      # Domain  Interfaces
├── SalesManagementSystem.EF        # EF Core (DbContext, Migrations)
└── SalesManagementSystem.Shared     # Shread Data like Dtos

## Features
- **Product Management**: CRUD operations for products.
- **Sales Management**: Create and view orders with order items.
- **JWT Authentication**: Secure user registration/login.
- **Logging**: Serilog with SQL Server sink and async logging.
- **Bonus**: Stock management (reduces stock on order, logs warnings when stock < 5).

## Technologies
- **.NET 9.0**
- **Entity Framework Core** (SQL Server)
- **JWT Authentication**
- **Serilog** (Logging)
- **Swagger** (API Documentation)
- **Scutor** (Dependency Injection)

---

## Setup Instructions

### 1. Clone the Repository
```bash
git clone https://github.com/Abuzaki718/SalesManagementSystem.git
```

### 2. Update appsettings.json with your SQL Server connection string
```
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=SalesManagementDB;Trusted_Connection=True;"
}
```

### 3. Configure JWT Secrets
```
"Jwt": {
  "Key": "your-256-bit-secret-key",
  "Issuer": "SalesManagementSystem",
  "Audience": "SalesManagementSystemUsers"
}
```
### 4. Run the API
```
dotnet run --project SalesManagementSystem.API
```
