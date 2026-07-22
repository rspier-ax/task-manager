# Architecture

Living document. Update when boundaries or major stack choices change (and add/supersede an ADR when needed).

## Goals

- Demonstrate Clean Architecture on a small but complete CRUD + auth system.
- Keep Domain and Application independent of frameworks.
- Support a React SPA against a versioned HTTP API.

## Layers

```
frontend (React)
       ↓ HTTP + JWT
TaskManager.Api
       ↓
TaskManager.Application
       ↓ abstractions
TaskManager.Infrastructure  →  SQLite (EF Core)
       ↑
TaskManager.Domain
```

| Project | Responsibility |
|---------|----------------|
| Domain | `User`, `TaskItem`, status enum — no framework refs |
| Application | Use cases, DTOs, validation, repository/token interfaces |
| Infrastructure | EF Core, repositories, JWT, password hashing, DbSeeder |
| Api | Controllers, auth middleware, DI composition, CORS, Swagger |
| Tests | Unit (Application) + integration (Api) |

## Auth model (planned)

- Public: `POST /api/auth/register`, `POST /api/auth/login`, `GET /api/health`
- Authorized: CRUD `/api/tasks` — scoped to the current user

## Data model (planned)

- **User:** Id, Email, PasswordHash, DisplayName, CreatedAt
- **TaskItem:** Id, UserId, Title, Description, Status, DueDate, CreatedAt, UpdatedAt

## Frontend (planned)

React + Vite + TypeScript: login/register, task list, create/edit. Token stored client-side and sent as `Authorization: Bearer`.

## Related ADRs

- [ADR-0001](../decisions/0001-clean-architecture-layers.md) — layer split
- [ADR-0002](../decisions/0002-sqlite-for-demo.md) — SQLite for demo
- [ADR-0003](../decisions/0003-jwt-bearer-auth.md) — JWT auth
