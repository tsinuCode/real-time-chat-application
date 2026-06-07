# Real-Time Chat Application

A university group project built with ASP.NET Core 8, SignalR, Entity Framework Core, and SQL Server.

## Tech Stack

- ASP.NET Core 8 MVC
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- SignalR
- ASP.NET Identity
- GitHub Actions

## User Directory

Browse and search registered users in the application.

### Features

- **View users** — See all registered users on the Directory page
- **Search users** — Find users by username or email
- **View profile summary** — Open a user's profile to see username, email, and join date

### Routes

- `/Directory` — User list and search
- `/Directory/Details/{id}` — User profile summary

See [docs/user-directory.md](docs/user-directory.md) for API endpoints, architecture, and testing instructions.

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server or LocalDB

### Run the API

```bash
cd ChatApp.API
dotnet run
```

### Run the Web App

```bash
cd ChatApp.Web
dotnet run
```

### Run Tests

```bash
dotnet test
```

## Project Structure

```
ChatApp.Core/           Domain entities, DTOs, interfaces
ChatApp.Infrastructure/ EF Core, repositories, Identity
ChatApp.API/            REST API + SignalR hub
ChatApp.Web/            MVC frontend
tests/ChatApp.Tests/    Unit tests
docs/                   Feature documentation
```
