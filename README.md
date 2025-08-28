# Library Management System

A modern library management system with RESTful API backend and Blazor WebAssembly frontend. The system allows for book management, user authentication, order processing, and cover image storage.

## Installation

### Prerequisites
- .NET 7.0 or later
- SQL Server (or Azure SQL Database)
- Azure Blob Storage account
- Visual Studio 2022 or VS Code

### Setup Instructions
1. Clone the repository
   ```
   git clone https://github.com/CompilationErrror/Library.git
   ```

2. Configure connection strings in `appsettings.json` for both API and Web projects
   - SQL Database connection
   - Azure Blob Storage connection

3. Run database migrations
   ```
   dotnet ef database update
   ```

4. Run the projects
   ```
   dotnet run --project LibraryApi
   dotnet run --project LibraryWeb
   ```

## Tech Stack

### Backend (LibraryApi)
- **ASP.NET Core Web API** - RESTful API framework
- **Entity Framework Core** - ORM for database operations
- **Azure SQL Database** - Data storage
- **Azure Blob Storage** - For storing book cover images
- **JWT Authentication** - Secure user authentication
- **Rate Limiting** - To prevent abuse of login endpoints
- **CORS** - Configured for Blazor frontend

### Frontend (LibraryWeb)
- **Blazor WebAssembly** - Client-side web framework
- **MudBlazor** - Material Design component library
- **Blazored.LocalStorage** - Client-side storage for tokens
- **Custom Authentication Handler** - For JWT token management

## Functionality

### Core Features
- **Admin Book Management**
  - Add, update, delete, and search books
  ![image](https://github.com/user-attachments/assets/3600700f-7c98-4ea3-93fc-fa9cacd67133)
  ![image](https://github.com/user-attachments/assets/33a26af8-dd49-48d0-b40d-438d4d7354f7)

  - Add and delete book cover image
  ![image](https://github.com/user-attachments/assets/33d64012-597c-4730-bdb7-700f9e298a07)

- **User Book Management**
  - User registration and authentication
  ![image](https://github.com/user-attachments/assets/d39d0460-eb6d-44ad-a4c4-9f345adc4a1f)

  - View, order books
  ![image](https://github.com/user-attachments/assets/16aa307f-8573-4471-9b8f-dda4c3fe0773)

  - View, return ordered books
  ![image](https://github.com/user-attachments/assets/57df9016-bdb0-4120-97d0-9fe51260cd25)

    
- **Security**
  - JWT token-based authentication
  - Token expiration and cleanup

## Coming Updates

- [ ] User Info Page
- [ ] Role-based functionality improvement
- [ ] Test eviroment
- [ ] Email service

## 📄 License

This project is available for educational and commercial use.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
