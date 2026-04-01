# WorkFlowHub

A production-quality project and task management web application built with **ASP.NET Core 8**, **React/TypeScript**, **SQL Server**, and **Azure** services.

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | C# / ASP.NET Core 8 Web API |
| Frontend | React.js + TypeScript |
| Database | SQL Server + Entity Framework Core (Code First) |
| Auth | JWT Bearer + BCrypt |
| Storage | Azure Blob Storage |
| Background Jobs | Azure Functions (Timer + Queue triggers) |
| Containerisation | Docker + docker-compose |
| CI/CD | GitHub Actions |
| Testing | xUnit + EF InMemory + integration tests |
| API Docs | Swagger / OpenAPI |

## Project Structure

```
WorkFlowHub/
├── src/
│   ├── WorkFlowHub.API/          # ASP.NET Core Web API (controllers, middleware)
│   ├── WorkFlowHub.Core/         # Domain models, DTOs, interfaces
│   ├── WorkFlowHub.Infrastructure/ # EF Core, repositories, services
│   └── WorkFlowHub.Functions/    # Azure Functions (cleanup, notifications)
├── tests/
│   ├── WorkFlowHub.UnitTests/    # xUnit unit tests (service layer)
│   └── WorkFlowHub.IntegrationTests/ # Integration tests (EF InMemory + TestServer)
├── frontend/                     # React + TypeScript SPA
├── docker-compose.yml
├── .github/workflows/ci.yml
└── README.md
```

## Running Locally with Docker

```bash
# Copy and configure environment
cp .env.example .env
# Edit .env with your SA_PASSWORD and JWT_KEY

# Start API + SQL Server
docker-compose up --build

# API available at: http://localhost:8080
# Swagger UI: http://localhost:8080
```

## Running Tests

```bash
# Unit tests
dotnet test tests/WorkFlowHub.UnitTests

# Integration tests
dotnet test tests/WorkFlowHub.IntegrationTests

# All tests
dotnet test WorkFlowHub.sln
```

## API Documentation

Swagger UI is available at `/` (root) when running the API.

Key endpoints:
- `POST /api/auth/register` — create account
- `POST /api/auth/login` — get JWT
- `GET  /api/projects` — list projects (paginated, auth required)
- `POST /api/projects` — create project
- `GET  /api/projects/{id}/tasks` — list tasks (paginated)
- `POST /api/projects/{id}/tasks` — create task
- `POST /api/files/upload` — upload to Azure Blob Storage
- `GET  /api/users/me` — current user profile

## Architecture Decisions

- **Clean Architecture** — Core has no infrastructure dependencies; Infrastructure depends on Core only; API depends on both.
- **JWT stored in memory** (frontend) — avoids XSS risks of localStorage.
- **Soft deletes** on Projects and Tasks — records are flagged `IsDeleted`, permanently purged by Azure Function after 30 days.
- **EF Core query filters** — soft-deleted records are transparently excluded from all queries.
- **Pagination** on all list endpoints via `PaginationParams`.
- **Azure Blob Storage** for file uploads — connection string swapped to Azurite (`UseDevelopmentStorage=true`) locally.
