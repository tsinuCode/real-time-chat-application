# User Directory

The User Directory feature allows authenticated users to browse, search, and view profiles of registered users in the Real-Time Chat Application.

## Features

- **View users** — Browse all registered users at `/Directory`
- **Search users** — Filter by username or email using the search box
- **View profile summary** — See username, email, and join date at `/Directory/Details/{id}`

## Architecture

```
MVC (ChatApp.Web)          API (ChatApp.API)           Data Layer
DirectoryController  -->   DirectoryController   -->   IUserRepository
     |                          |                          |
  ApiClient (JWT)           [Authorize]              UserRepository
                                                       EF Core / SQL Server
```

The MVC layer calls the API using `ApiClient` with the JWT stored in session. The API uses the repository pattern to query ASP.NET Identity users via Entity Framework Core.

## API Endpoints

All endpoints require authentication (`Authorization: Bearer {token}`). Responses are wrapped in `ApiResponse<T>`:

```json
{
  "success": true,
  "message": null,
  "data": { ... },
  "errors": null
}
```

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/directory/users` | List all users (excludes current user) |
| GET | `/api/directory/search?query={text}` | Search by username or email |
| GET | `/api/directory/user/{id}` | Get user profile detail |

### Example: List users

**Request:** `GET /api/directory/users`

**Response data:**

```json
[
  {
    "id": "abc123",
    "username": "abdi",
    "email": "abdi@test.com"
  }
]
```

> **Note:** User IDs are strings (ASP.NET Identity), not integers.

### Example: User detail

**Request:** `GET /api/directory/user/abc123`

**Response data:**

```json
{
  "id": "abc123",
  "username": "abdi",
  "email": "abdi@test.com",
  "joinedDate": "2026-05-01T00:00:00Z"
}
```

## MVC Routes

| Route | Page |
|-------|------|
| `/Directory` | User list with search |
| `/Directory/Details/{id}` | User profile summary |

## Navigation

A shared partial view (`Views/Shared/_SidebarNav.cshtml`) provides consistent navigation across Chat, Profile, and Directory pages:

- Chats
- Groups
- Directory
- Profile

## Screenshots

<!-- Add screenshots after running the application locally -->

| Page | Screenshot |
|------|------------|
| Directory list | `docs/screenshots/directory-index.png` |
| User search | `docs/screenshots/directory-search.png` |
| User details | `docs/screenshots/directory-details.png` |

## Testing

Run the directory controller unit tests:

```bash
dotnet test tests/ChatApp.Tests --filter "FullyQualifiedName~DirectoryControllerTests"
```

Tests cover:

1. Get all users
2. Search users by query
3. Get user by ID
4. Invalid ID returns NotFound
