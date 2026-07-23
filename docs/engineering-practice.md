# Engineering practice

How we implement and review changes on TaskManager.

## Implementation order

1. Domain entities and enums (`TaskManager.Domain`)
2. Application contracts, services, and validation + unit tests
3. Infrastructure (EF Core, repositories, JWT, seed)
4. API controllers, auth middleware, CORS, Swagger
5. API / integration tests (`WebApplicationFactory`)
6. Frontend (React + Vite + TypeScript)
7. Living docs / demo guide when behavior is stable

Prefer extending existing files over parallel implementations.

## Before proposing changes

State explicitly:

- What exists today in the relevant area
- What will be reused vs added
- Which files will change

Do not add a new service or component if an equivalent already covers the use case.

## Review standards

Changes that typically require revision:

- Business rules inside controllers instead of Application
- EF Core types leaking into Domain
- Task access without filtering by authenticated `UserId`
- Missing tests for validation or authorization boundaries
- Architectural shifts without an ADR

## Manual verification

```bash
dotnet test
cd frontend && npm run build
```

Smoke path (after API + UI): register/login with seed → list tasks → create → update status → delete → confirm 401 without token.

## Tests expected by change type

| Change | Minimum |
|--------|---------|
| Application validation / rules | Unit test |
| Repository / EF mapping | Unit or integration with in-memory/SQLite |
| Auth / task HTTP endpoints | Integration test |
| Frontend CRUD flows | Component or manual smoke + no console errors |
