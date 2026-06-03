# Real-Time Chat Application

Distributed chat app with an ASP.NET Web Forms UI, a .NET 8 API layer, and a shared core library.

## Solution layout

```
real-time-chat-application/
├── chatapp.csproj              # Web Forms UI (ASP.NET 4.7.2)
├── ChatApp.sln
├── ChatApp.Core/               # Domain entities, DTOs, interfaces
│   ├── Entities/
│   ├── Interfaces/
│   └── DTOs/
│       ├── Auth/
│       ├── Messages/
│       └── Chat/
├── ChatApp.Infrastructure/     # EF Core, repositories, services
│   ├── Data/
│   ├── Repositories/
│   └── Services/
└── ChatApp.API/                # ASP.NET Core host (SignalR, REST)
    ├── Controllers/
    └── Hubs/
```

## Projects

| Project | Role |
|---------|------|
| **chatapp** | Login, register, and chat UI |
| **ChatApp.Core** | Shared contracts and domain model |
| **ChatApp.Infrastructure** | Database and external integrations |
| **ChatApp.API** | Real-time hub and HTTP API |

## Build

Open `ChatApp.sln` in Visual Studio 2022 (with the .NET 8 SDK). Restore NuGet packages, then build the solution.
