# TaskManager

A small full-stack **task manager** built for a .NET technical interview exercise: Clean Architecture backend, JWT auth, CRUD tasks per user, and a React frontend (coming in a later phase).

> **Status:** scaffolding phase. Solution layout and documentation are in place; domain, API, and UI land in follow-up PRs.

## User story

As an authenticated user, I want to create, read, update, and delete my tasks (title, description, status, due date) so I can organize my day. Unauthenticated callers can reach health/login/register only; task endpoints require a valid JWT.

## What it shows

- Clean Architecture (.NET): Domain → Application → Infrastructure → Api
- Auth: register, login, authorized vs public endpoints
- Data: users + tasks (EF Core + SQLite — planned)
- Frontend: React + Vite + TypeScript CRUD (planned)
- Docs: `AGENTS.md`, ADRs, workflow — same spirit as a portfolio case study
- GenAI fluency: prompt craft, validation, and corrections (see below)

## Demo workflow

See [docs/guides/demo-guide.md](./docs/guides/demo-guide.md) (stub until the API exists).

**Planned seed credentials**

| Field | Value |
|-------|--------|
| Email | `demo@taskmanager.local` |
| Password | `Demo123!` |

## Architecture

```
React (Vite)  --JWT-->  TaskManager.Api
                           ↓
                    TaskManager.Application
                           ↓
                    TaskManager.Infrastructure → SQLite
                           ↑
                    TaskManager.Domain
```

Details: [docs/architecture/ARCHITECTURE.md](./docs/architecture/ARCHITECTURE.md).

## Tech stack

| Layer | Choice |
|-------|--------|
| API | ASP.NET Core Web API (.NET 8) |
| Architecture | Clean Architecture |
| Data | EF Core + SQLite |
| Auth | JWT Bearer |
| Frontend | React + Vite + TypeScript |
| Tests | xUnit |

## Run locally

> Commands will work fully once Application/Infrastructure/Api are implemented.

```bash
dotnet restore
dotnet run --project src/TaskManager.Api
# later:
cd frontend && npm ci && npm run dev
```

## Test strategy

- Application: unit tests for validation and ownership rules
- API: integration tests with `WebApplicationFactory`
- Prefer TDD per slice (see [docs/engineering-practice.md](./docs/engineering-practice.md))

## Thought process

I treated this as a **phased** delivery instead of one monolithic drop:

1. **Clarify the informal user story** — Task Manager matches both the CRUD requirement and the GenAI assessment (tasks + users).
2. **Choose boring, reviewable tech** — Clean Architecture + JWT + SQLite so a reviewer can clone and reason about boundaries quickly.
3. **Document like a real team** — `AGENTS.md`, ADRs, and workflow so decisions are visible without digging through chat history.
4. **Keep interview rehearsal private** — notes live in a local `_local/` folder that is gitignored and never pushed.
5. **Ship in small PRs** — scaffold → domain → application/tests → infrastructure → API → frontend → docs polish.

Inspiration for the docs layout came from earlier portfolio projects (`decision-desk`, `dispatch-lab`): public living docs + numbered ADRs + a short agent/dev entrypoint.

## Generative AI tools

### Tool

[Cursor](https://cursor.com) (Composer + Plan mode) as the GenAI coding assistant.

### Prompt I would use to scaffold the task API

```text
Generate a RESTful API for a simple task management system in C# / ASP.NET Core (.NET 8).

Functionality:
- CRUD tasks with title, description, status, and due_date
- Tasks are associated with a User (id, email, password hash, display name)
- JWT: register + login; task endpoints require authorization
- Clean Architecture: Domain, Application, Infrastructure, Api
- Application holds validation and business rules; Domain has no EF/ASP.NET refs
- Every task query/command must be scoped to the authenticated user's id
- Include xUnit stubs for Application services

Do not put EF Core entities configuration inside Domain.
Prefer repository interfaces in Application, implementations in Infrastructure.
```

### How I used GenAI on this project so far

Planning happened in Cursor Plan mode with iterative refinement:

- Locked the domain to **Task Manager** and frontend to **React + Vite + TypeScript**
- Chose **SQLite + JWT** and a **Clean Architecture** split (captured as ADR-0001…0003)
- Adopted a **docs-first scaffold** (this PR) before writing business code
- Defined a **phase → PR → merge → new plan** cadence so each slice stays reviewable

### How I validate AI suggestions

- Map every suggestion back to a requirement (auth, ownership, layer independence, tests)
- Reject “god controllers” and framework leaks into Domain
- Require an explicit checklist: password hashing, `UserId` filters, public vs `[Authorize]`, seed demo user
- Prefer failing tests before accepting Application/API behavior

### Corrections I expect (and will apply) vs naive AI output

| Risk | Correction |
|------|------------|
| Get/Update/Delete by id without user scope | Always filter by authenticated `UserId` |
| Plaintext or weak password storage | Hash with a standard algorithm (e.g. ASP.NET Identity hasher / bcrypt) |
| Free-text status | Enum + validation |
| Missing demo credentials | DbSeeder with documented seed user |
| Skipping tests | TDD on Application; integration tests on API |

### Representative sample

A code sample from the generated/implemented Application or API layer will be pasted here in a later docs PR once those projects contain real logic. Until then, the prompt and validation checklist above are the GenAI deliverable for this scaffold phase.

## Limitations (v1 scaffold)

- No runnable API or UI yet in this PR
- ADRs are `proposed` until the matching code merges
- Demo guide is a stub

## Docs

Start at [docs/README.md](./docs/README.md). Agent/dev entrypoint: [AGENTS.md](./AGENTS.md).

## License

MIT — see [LICENSE](./LICENSE).
