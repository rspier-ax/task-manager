# TaskManager

Read **`docs/README.md`** for architecture, standards, and ADRs.

## Product

Simple **task manager** for authenticated users: create, list, update, and delete personal tasks (title, description, status, due date). Demo seed credentials are documented in the root README.

## Stack

- .NET 10, ASP.NET Core Web API, Clean Architecture
- EF Core + SQLite (demo)
- JWT Bearer authentication
- React + Vite + TypeScript (frontend — scaffolded in a later phase)
- xUnit for tests

## Structure

```
src/TaskManager.Domain/          entities, enums
src/TaskManager.Application/     contracts, services, validation
src/TaskManager.Infrastructure/  EF Core, repos, JWT, seed
src/TaskManager.Api/             HTTP endpoints, DI, CORS
tests/TaskManager.Tests/         unit + integration tests
frontend/                        React + Vite + TypeScript
docs/                            architecture, standards, ADRs
```

## Domain boundaries

- **Domain** has no dependencies on EF Core or ASP.NET.
- **Application** owns business rules and validation; it depends only on Domain and abstractions.
- **Infrastructure** implements persistence and token issuance.
- **API** is a thin HTTP adapter; keep controllers free of business rules.
- Task queries and commands are always scoped to the authenticated **UserId**. Do not expose cross-user data without a new ADR.

## Quality expectations

- Prefer TDD: failing test → implementation → refactor.
- Contract or schema changes need tests; architectural shifts need an ADR in `docs/decisions/`.
- Public endpoints: auth register/login and health. Task CRUD requires `[Authorize]`.
- Repository documentation (`README.md`, `docs/`, this file) is **English only**.
- Never commit `_local/` (private interview notes). See `.gitignore`.

## Validation

```bash
dotnet restore
dotnet test
# Frontend (when present)
cd frontend && npm ci && npm run build
```

## Further reading

| Document | Topic |
|----------|--------|
| [docs/guides/demo-guide.md](./docs/guides/demo-guide.md) | Demo walkthrough |
| [docs/architecture/ARCHITECTURE.md](./docs/architecture/ARCHITECTURE.md) | Layers and auth |
| [docs/WORKFLOW.md](./docs/WORKFLOW.md) | Branches and PRs |
| [docs/engineering-practice.md](./docs/engineering-practice.md) | Implementation order |
| [docs/decisions/](./docs/decisions/) | ADRs |
